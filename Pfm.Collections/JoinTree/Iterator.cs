using System;
using System.Runtime.CompilerServices;

namespace Pfm.Collections.JoinTree;

/// <summary>
/// Forward and backward iterator over tree nodes in in-order.
/// </summary>
public struct Iterator<TValue>
{
    /// <summary>
    /// Maximum path depth supported by the iterator.
    /// Should be sufficient for all balanced trees of up to 2G elements.
    /// </summary>
    public const int MaxDepth = 48;

    /// <summary>
    /// This is the root of the tree being traversed.
    /// </summary>
    public Node<TValue> Root;

    /// <summary>
    /// Traversal stack, "current" node is always at the top.
    /// </summary>
    public Node<TValue>[] Path;
    
    /// <summary>
    /// Number of elements on <see cref="Path"/>.
    /// </summary>
    public int Count;

    internal Iterator(Node<TValue> root) {
        this.Root = root;
        this.Path = new Node<TValue>[MaxDepth];
        this.Count = 0;
    }

    private Iterator(Iterator<TValue> other) {
        this.Root = other.Root;
        this.Path = new Node<TValue>[MaxDepth];
        this.Count = other.Count;
        Array.Copy(other.Path, Path, Count);
    }

    public Iterator<TValue> Clone() => new(this);

    /// <summary>
    /// False for <c>default</c> instance.
    /// </summary>
    public bool IsAllocated => Path != null;

    public bool IsEmpty => Count == 0;
    public ref Node<TValue> Top => ref Path[Count - 1];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => Count = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(Node<TValue> node) => Path[Count++] = node ?? throw new ArgumentNullException(nameof(node));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Node<TValue> TryPop() => Count > 0 ? Path[--Count] : null;

    public Node<TValue> First() {
        Clear();
        for (var n = Root; n != null; n = n.L)
            Push(n);
        return IsEmpty ? null : Top;
    }

    public Node<TValue> Last() {
        Clear();
        for (var n = Root; n != null; n = n.R)
            Push(n);
        return IsEmpty ? null : Top;
    }

    public Node<TValue> Succ() {
        var current = TryPop();
        if (current == null)
            return null;
        if (current.R != null) {
            Push(current);
            for (current = current.R; current != null; current = current.L)
                Push(current);
        } else {
            Node<TValue> y;
            do {
                y = current;
                if ((current = TryPop()) == null)
                    return null;
            } while (y == current.R);
            Push(current);
        }
        return Top;
    }

    public Node<TValue> Pred() {
        var current = TryPop();
        if (current == null)
            return null;
        if (current.L != null) {
            Push(current);
            for (current = current.L; current != null; current = current.R)
                Push(current);
        } else {
            Node<TValue> y;
            do {
                y = current;
                if ((current = TryPop()) == null)
                    return null;
            } while (y == current.L);
            Push(current);
        }
        return Top;
    }
}
