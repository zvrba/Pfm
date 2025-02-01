using System;
using System.Diagnostics;

namespace Podaga.PersistentCollections.TreeSet;

/// <summary>
/// <para>
/// A light-weight stack of <see cref="TreeNode{TValue}"/> used by iterative tree traversal algorithms.
/// A <c>default</c> instance is unusable (<see cref="IsAllocated"/> is false) and must be allocated with
/// <see cref="Allocate(int)"/>.
/// </para>
/// <para>
/// WARNING: This is a mutable struct.  For performance reasons, the provided methods and properties perform
/// no error checking on their own and rely on the runtime throwing <see cref="NullReferenceException"/> or
/// <see cref="IndexOutOfRangeException"/>.  Copying of an iterator is shallow: the "original" and its "copy"
/// will share the same underlying stack array.  For deep copying, use <see cref="Pred(ref Podaga.Collections.TreeSet.TreeIterator{TValue})"/>.
/// </para>
/// <para>
/// WARNING: Iterating over a non-persistent tree that is being modified yields unspecified results.  The modification
/// does not need to be concurrent either, e.g., using <c>Succ</c> or <c>Pred</c> while inserting or deleting elements
/// will also lead to unspecified results.
/// </para>
/// </summary>
/// <typeparam name="TValue">Value type held by the tree.</typeparam>
[DebuggerDisplay("Depth={Depth}")]
public struct TreeIterator<TValue>
{
    /// <summary>
    /// Default capacity allocated by the stack.  This should be sufficient for any balanced tree of up to 4G elements.
    /// </summary>
    public const int DefaultCapacity = 48;

    /// <summary>
    /// Creates a new allocated instance with default capacity.
    /// </summary>
    public static TreeIterator<TValue> New() => new(DefaultCapacity);

    /// <summary>
    /// Allocating constructor.
    /// </summary>
    /// <param name="capacity">Maximum capacity supported by this stack.</param>
    public TreeIterator(int capacity) => Allocate(capacity);

    /// <summary>
    /// Copy constructor: copies all fields and the stack array, but not the nodes.  The new
    /// stack has the same capacity as <paramref name="other"/>.
    /// </summary>
    /// <param name="other">
    /// An existing, allocated stack to copy.
    /// </param>
    public TreeIterator(TreeIterator<TValue> other) {
        Depth = other.Depth;
        Path = new TreeNode<TValue>[other.Path.Length];
        Array.Copy(other.Path, Path, Depth);
    }

    /// <summary>
    /// Tree traversal stack; root is always at index 0 (bottom) and the current node at <c>Count-1</c>.
    /// </summary>
    /// <seealso cref="Top"/>
    /// <seealso cref="Depth"/>
    public TreeNode<TValue>[] Path;

    /// <summary>
    /// Number of elements on the stack.
    /// </summary>
    public int Depth;

    /// <summary>
    /// Allocates <see cref="Path"/> and sets <see cref="Depth"/> to 0.
    /// </summary>
    /// <param name="capacity">Maximum capacity supported by this stack.</param>
    public void Allocate(int capacity = DefaultCapacity) {
        Path = new TreeNode<TValue>[capacity];
        Depth = 0;
    }

    /// <summary>
    /// False if <c>this</c> is not usable.
    /// </summary>
    public bool IsAllocated => Path != null;

    /// <summary>
    /// True when there are no elements on the stack.
    /// </summary>
    public bool IsEmpty => Depth == 0;

    /// <summary>
    /// A reference to the top node on the stack which is at index <c>Count - 1</c>.
    /// </summary>
    public ref TreeNode<TValue> Top => ref Path[Depth - 1];

    /// <summary>
    /// Removes all elements from the stack.
    /// (NB! Only resets the stack pointer, the contents of <see cref="Path"/> array is unchanged.)
    /// </summary>
    public void Clear() => Depth = 0;

    /// <summary>
    /// Pushes a node onto the stack.  <paramref name="node"/> must not be null (checked only in debug builds).
    /// </summary>
    /// <param name="node"></param>
    public void Push(TreeNode<TValue> node) {
        Debug.Assert(node != null);
        Path[Depth++] = node;
    }

    /// <summary>
    /// Attempts to pop the top node from the stack.
    /// </summary>
    /// <returns>
    /// The popped node or <c>null</c> if the stack was empty.
    /// </returns>
    public TreeNode<TValue> TryPop() => Depth > 0 ? Path[--Depth] : null;

    /// <summary>
    /// Finds a value equivalent to <paramref name="value"/>.
    /// </summary>
    /// <param name="node">
    /// Node to start the search from.  If <c>null</c>, the top node of <paramref name="iterator"/> is used.
    /// Otherwise, <paramref name="iterator"/> is cleared and search restarted with the given node.
    /// </param>
    /// <param name="value">Value to look for.  Only the key fields must be initialized.</param>
    /// <param name="iterator">An allocated iterator holding the path to the last visited node during traversal.</param>
    /// <returns>
    /// The result of the last comparison leading to the top node in <paramref name="iterator"/>.
    /// Zero means that an equivalent value was found and is on top of the stack.
    /// If <paramref name="node"/> was <c>null</c>, -1 is returned (arbitrarily).
    /// </returns>
    /// <typeparam name="TValueTraits">Value traits to use while searching for the value.</typeparam>
    public int Find<TValueTraits>(TreeNode<TValue> node, TValue value)
        where TValueTraits : struct, IValueTraits<TValue>
    {
        if (node != null) Clear();
        else node = TryPop();

        int c = -1;
        while (node != null) {
            Push(node);
            if ((c = TValueTraits.CompareKey(value, node.V)) == 0)
                break;
            node = c < 0 ? node.L : node.R;
        }
        return c;
    }

    /// <summary>
    /// Moves <c>this</c> to the smallest value in the subtree.
    /// </summary>
    /// <param name="node">
    /// Node to start the search from.  If <c>null</c>, the top node of <c>this</c> is used.
    /// Otherwise, <c>this</c> is cleared and search restarted with the given node.
    /// </param>
    /// <returns>
    /// True if a node was found.  False is returned only when both <paramref name="node"/> is <c>null</c> and
    /// <c>this</c> is empty.
    /// </returns>
    public bool First(TreeNode<TValue> node) {
        if (node != null) Clear();
        else node = TryPop();

        for (; node != null; node = node.L)
            Push(node);
        return !IsEmpty;
    }

    /// <summary>
    /// Moves <c>this</c> to the largest value in the subtree.
    /// </summary>
    /// <param name="node">
    /// Node to start the search from.  If <c>null</c>, the top node of <c>this</c> is used.
    /// Otherwise, <c>this</c> is cleared and search restarted with the given node.
    /// </param>
    /// <returns>
    /// True if a node was found.  False is returned only when both <paramref name="node"/> is <c>null</c> and
    /// <c>this</c> is empty.
    /// </returns>
    public bool Last(TreeNode<TValue> node) {
        if (node != null) Clear();
        else node = TryPop();

        for (; node != null; node = node.R)
            Push(node);
        return !IsEmpty;
    }

    /// <summary>
    /// Moves <c>this</c> to the next element in sort order.
    /// </summary>
    /// <returns>True if the next element exists, false otherwise.</returns>
    public bool Succ() {
        var current = TryPop();
        if (current == null)
            return false;

        if (current.R != null) {
            Push(current);
            for (current = current.R; current != null; current = current.L)
                Push(current);
        } else {
            TreeNode<TValue> y;
            do {
                y = current;
                if ((current = TryPop()) == null)
                    return false;
            } while (y == current.R);
            Push(current);
        }
        return true;
    }

    /// <summary>
    /// Moves <c>this</c> to the previous element in sort order.
    /// </summary>
    /// <param name="iterator">
    /// Iterator pointing to an existing node in the tree.
    /// </param>
    /// <returns>True if the next element exists, false otherwise.</returns>
    public bool Pred() {
        var current = TryPop();
        if (current == null)
            return false;

        if (current.L != null) {
            Push(current);
            for (current = current.L; current != null; current = current.R)
                Push(current);
        } else {
            TreeNode<TValue> y;
            do {
                y = current;
                if ((current = TryPop()) == null)
                    return false;
            } while (y == current.L);
            Push(current);
        }
        return true;
    }

    /// <summary>
    /// Sets <c>this</c> to the n'th element in sorted order in the tree.
    /// </summary>
    /// <param name="node">
    /// Node from which to start the search.  If <c>null</c>, the top node of <c>this</c> is used.
    /// Otherwise, <c>this</c> is cleared and search restarted with the given node.
    /// </param>
    /// <param name="index">Order of the element to retrieve.</param>
    /// <exception cref="IndexOutOfRangeException">
    /// Index is outside of range <c>[0, Size-1)</c>, size being the size of the subtree.
    /// </exception>
    public void Nth(TreeNode<TValue> node, int index) {
        if (node != null) Clear();
        else if (!IsEmpty) node = Top;

        if (node == null || index < 0 || index >= node.Size)
            throw new IndexOutOfRangeException("Invalid tree element index.");
        ++index;    // Makes calculations easier.

    loop:
        Push(node);
        var l = node.L?.Size ?? 0;
        if (index == l + 1) 
            return;
        if (index <= l) {
            node = node.L;
        } else {
            node = node.R;
            index -= l + 1;
        }
        goto loop;
    }
}
