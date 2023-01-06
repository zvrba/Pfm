using System;
using System.Runtime.CompilerServices;

namespace Pfm.Collections.IntrusiveTree;

public struct Iterator<TNode, TValue, TTag>
    where TNode : class, INodeTraits<TNode, TValue, TTag>
    where TTag : struct, ITagTraits<TTag>
{
    public const int MaxDepth = 48;                         // Depth of balanced with up to 2^32 elements tree should not exceed this.

    internal Iterator(AbstractTree<TNode, TValue, TTag> tree) {
        this.Tree = tree ?? throw new ArgumentNullException(nameof(tree));
        this.Path = new TNode[MaxDepth];
        this.Count = 0;
    }

    private Iterator(Iterator<TNode, TValue, TTag> other) {
        this.Tree = other.Tree;
        this.Path = new TNode[MaxDepth];
        this.Count = other.Count;
        Array.Copy(other.Path, Path, Count);
    }

    public Iterator<TNode, TValue, TTag> Clone() => new(this);

    public readonly AbstractTree<TNode, TValue, TTag> Tree;
    public readonly TNode[] Path;
    public int Count;

    /// <summary>
    /// False for <c>default</c> instance.
    /// </summary>
    public bool IsAllocated => Tree != null;

    public bool IsEmpty => Count == 0;
    public ref TNode Top => ref Path[Count - 1];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => Count = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(TNode node) => Path[Count++] = node ?? throw new ArgumentNullException(nameof(node));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TNode TryPop() => Count > 0 ? Path[--Count] : null;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public TNode First() {
        Clear();
        for (var n = Tree.Root; n != null; n = n.L)
            Push(n);
        return IsEmpty ? null : Top;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public TNode Last() {
        Clear();
        for (var n = Tree.Root; n != null; n = n.R)
            Push(n);
        return IsEmpty ? null : Top;
    }

    /// <summary>
    /// Searches for <paramref name="value"/> and initializes the iterator to the last visited element, which is
    /// available through <see cref="Top"/> property.
    /// </summary>
    /// <param name="value">Value to search for; only the key fields (as determined by the comparison) must be set.</param>
    /// <returns>
    /// The result of the last comparison that lead to <paramref name="value"/>.
    /// Zero means that an element equal to <paramref name="value"/> has been found.
    /// Non-zero will be returned for an empty tree.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public int Find(TValue value) {
        var n = Tree.Root;
        int c = -1;
        Clear();
        while (n != null) {
            Push(n);
            if ((c = TNode.Compare(value, n.V)) == 0)
                break;
            n = c < 0 ? n.L : n.R;
        }
        return c;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public TNode Succ() {
        var current = TryPop();
        if (current == null)
            return null;
        if (current.R != null) {
            Push(current);
            for (current = current.R; current != null; current = current.L)
                Push(current);
        }
        else {
            TNode y;
            do {
                y = current;
                if ((current = TryPop()) == null)
                    return null;
            } while (y == current.R);
            Push(current);
        }
        return Top;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public TNode Pred() {
        var current = TryPop();
        if (current == null)
            return null;
        if (current.L != null) {
            Push(current);
            for (current = current.L; current != null; current = current.R)
                Push(current);
        }
        else {
            TNode y;
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
