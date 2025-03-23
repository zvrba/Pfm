using System;
using System.Runtime.CompilerServices;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Traversal methods on <see cref="TreeIterator{TValue}"/>. Every method in
/// this class leaves a "trail" on the iterator which may be used to continue iteration.
/// </summary>
public static class IteratorAlgorithms
{
    /// <summary>
    /// Creates an iterator for the tree and pushes the root node (if the tree is not empty) on top of the stack.
    /// This makes other iterator algorithms operate over the complete tree.
    /// </summary>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <param name="this">The tree for which to create the iterator.</param>
    /// <returns>An iterator instance.</returns>
    public static TreeIterator<TValue> GetIterator<TValue>
        (
        this JoinableTreeNode<TValue> @this
        )
    {
        var it = TreeIterator<TValue>.New();
        if (@this != null)
            it.Push(@this);
        return it;
    }

    /// <summary>
    /// Finds a value equivalent to <paramref name="value"/>.
    /// </summary>
    /// <param name="this">Iterator instance.</param>
    /// <param name="node">
    /// Node to start the search from.  If <c>null</c>, the top node of <paramref name="this"/> is used.
    /// Otherwise, <paramref name="this"/> is cleared and search restarted with the given node.
    /// </param>
    /// <param name="value">Value to look for.  Only the key fields must be initialized.</param>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <typeparam name="TValueTraits">Value traits.</typeparam>
    /// <returns>
    /// The result of the last comparison leading to the top node in <paramref name="this"/>.
    /// Zero means that an equivalent value was found and is on top of the stack.
    /// If <paramref name="node"/> was <c>null</c>, -1 is returned (arbitrarily).
    /// </returns>
    public static int Find<TValue, TValueTraits>
        (
        this ref TreeIterator<TValue> @this,
        JoinableTreeNode<TValue> node,
        TValue value
        )
        where TValueTraits : IValueTraits<TValue>
    {
        if (node != null) @this.Clear();
        else node = @this.TryPop();

        int c = -1;
        while (node != null) {
            @this.Push(node);
            if ((c = TValueTraits.Compare(value, node.Value)) == 0)
                break;
            node = c < 0 ? node.Left : node.Right;
        }
        return c;
    }

    /// <summary>
    /// Moves <c>this</c> to the smallest value in the subtree.
    /// </summary>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <param name="this">Iterator instance.</param>
    /// <param name="node">
    /// Node to start the search from.  If <c>null</c>, the top node of <c>this</c> is used.
    /// Otherwise, <c>this</c> is cleared and search restarted with the given node.
    /// </param>
    /// <returns>
    /// True if a node was found.  False is returned only when both <paramref name="node"/> is <c>null</c> and
    /// <c>this</c> is empty.
    /// </returns>
    public static bool First<TValue>
        (
        this ref TreeIterator<TValue> @this,
        JoinableTreeNode<TValue> node
        )
    {
        if (node != null) @this.Clear();
        else node = @this.TryPop();

        for (; node != null; node = node.Left)
            @this.Push(node);
        return !@this.IsEmpty;
    }

    /// <summary>
    /// Moves <c>this</c> to the largest value in the subtree.
    /// </summary>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <param name="this">Iterator instance.</param>
    /// <param name="node">
    /// Node to start the search from.  If <c>null</c>, the top node of <c>this</c> is used.
    /// Otherwise, <c>this</c> is cleared and search restarted with the given node.
    /// </param>
    /// <returns>
    /// True if a node was found.  False is returned only when both <paramref name="node"/> is <c>null</c> and
    /// <c>this</c> is empty.
    /// </returns>
    public static bool Last<TValue>
        (
        this ref TreeIterator<TValue> @this,
        JoinableTreeNode<TValue> node
        )
    {
        if (node != null) @this.Clear();
        else node = @this.TryPop();

        for (; node != null; node = node.Right)
            @this.Push(node);
        return !@this.IsEmpty;
    }

    /// <summary>
    /// Moves <c>this</c> to the next element in sort order.
    /// </summary>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <param name="this">Iterator instance.</param>
    /// <returns>True if the next element exists, false otherwise.</returns>
    public static bool Succ<TValue>(this ref TreeIterator<TValue> @this)
    {
        var current = @this.TryPop();
        if (current == null)
            return false;

        if (current.Right != null) {
            @this.Push(current);
            for (current = current.Right; current != null; current = current.Left)
                @this.Push(current);
        } else {
            JoinableTreeNode<TValue> y;
            do {
                y = current;
                if ((current = @this.TryPop()) == null)
                    return false;
            } while (y == current.Right);
            @this.Push(current);
        }
        return true;
    }

    /// <summary>
    /// Moves <c>this</c> to the previous element in sort order.
    /// </summary>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <param name="this">Iterator instance.</param>
    /// <returns>True if the next element exists, false otherwise.</returns>
    public static bool Pred<TValue>(this ref TreeIterator<TValue> @this)
    {
        var current = @this.TryPop();
        if (current == null)
            return false;

        if (current.Left != null) {
            @this.Push(current);
            for (current = current.Left; current != null; current = current.Right)
                @this.Push(current);
        } else {
            JoinableTreeNode<TValue> y;
            do {
                y = current;
                if ((current = @this.TryPop()) == null)
                    return false;
            } while (y == current.Left);
            @this.Push(current);
        }
        return true;
    }

    /// <summary>
    /// Sets <c>this</c> to the n'th element in sorted order in the tree.
    /// </summary>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <param name="this">Iterator instance.</param>
    /// <param name="node">
    /// Node from which to start the search.  If <c>null</c>, the top node of <c>this</c> is used.
    /// Otherwise, <c>this</c> is cleared and search restarted with the given node.
    /// </param>
    /// <param name="index">Order of the element to retrieve.</param>
    /// <exception cref="IndexOutOfRangeException">
    /// Index is outside of range <c>[0, Size-1)</c>, size being the size of the subtree.
    /// </exception>
    public static void Nth<TValue>
        (
        this ref TreeIterator<TValue> @this,
        JoinableTreeNode<TValue> node,
        int index
        )
    {
        if (node != null) @this.Clear();
        else if (!@this.IsEmpty) node = @this.Top;

        if (node == null || index < 0 || index >= node.Size)
            throw new IndexOutOfRangeException("Invalid tree element index.");
        ++index;    // Makes calculations easier.

    loop:
        @this.Push(node);
        var l = node.Left?.Size ?? 0;
        if (index == l + 1)
            return;
        if (index <= l) {
            node = node.Left;
        } else {
            node = node.Right;
            index -= l + 1;
        }
        goto loop;
    }
}
