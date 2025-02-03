using System;
using System.Runtime.CompilerServices;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Traversal methods on <see cref="TreeIterator{TTag, TValue}"/>.  Note that methods take a <see cref="JoinableTreeNode{TTag, TValue}"/>
/// instead of <see cref="JoinableTree{TTag, TValue, TValueTraits}"/> because they may be used to operate on subtrees.  Every method in
/// this class leaves a "trail" on the iterator which may be used to continue iteration.
/// </summary>
public static class IteratorAlgorithms
{

    /// <summary>
    /// Finds a value equivalent to <paramref name="value"/>.
    /// </summary>
    /// <param name="this">Iterator instance.</param>
    /// <param name="node">
    /// Node to start the search from.  If <c>null</c>, the top node of <paramref name="this"/> is used.
    /// Otherwise, <paramref name="this"/> is cleared and search restarted with the given node.
    /// </param>
    /// <param name="value">Value to look for.  Only the key fields must be initialized.</param>
    /// <returns>
    /// The result of the last comparison leading to the top node in <paramref name="iterator"/>.
    /// Zero means that an equivalent value was found and is on top of the stack.
    /// If <paramref name="node"/> was <c>null</c>, -1 is returned (arbitrarily).
    /// </returns>
    public static int Find<TTag, TValue, TValueTraits>
        (
        this ref TreeIterator<TTag, TValue> @this,
        JoinableTreeNode<TTag, TValue> node,
        TValue value
        )
        where TTag : struct, ITagTraits<TTag>
        where TValueTraits : struct, IValueTraits<TValue>
    {
        if (node != null) @this.Clear();
        else node = @this.TryPop();

        int c = -1;
        while (node != null) {
            @this.Push(node);
            if ((c = TValueTraits.Compare(value, node.V)) == 0)
                break;
            node = c < 0 ? node.L : node.R;
        }
        return c;
    }

    /// <summary>
    /// Moves <c>this</c> to the smallest value in the subtree.
    /// </summary>
    /// <param name="this">Iterator instance.</param>
    /// <param name="node">
    /// Node to start the search from.  If <c>null</c>, the top node of <c>this</c> is used.
    /// Otherwise, <c>this</c> is cleared and search restarted with the given node.
    /// </param>
    /// <returns>
    /// True if a node was found.  False is returned only when both <paramref name="node"/> is <c>null</c> and
    /// <c>this</c> is empty.
    /// </returns>
    public static bool First<TTag, TValue>
        (
        this ref TreeIterator<TTag, TValue> @this,
        JoinableTreeNode<TTag, TValue> node
        )
        where TTag : struct, ITagTraits<TTag>
    {
        if (node != null) @this.Clear();
        else node = @this.TryPop();

        for (; node != null; node = node.L)
            @this.Push(node);
        return !@this.IsEmpty;
    }

    /// <summary>
    /// Moves <c>this</c> to the largest value in the subtree.
    /// </summary>
    /// <param name="this">Iterator instance.</param>
    /// <param name="node">
    /// Node to start the search from.  If <c>null</c>, the top node of <c>this</c> is used.
    /// Otherwise, <c>this</c> is cleared and search restarted with the given node.
    /// </param>
    /// <returns>
    /// True if a node was found.  False is returned only when both <paramref name="node"/> is <c>null</c> and
    /// <c>this</c> is empty.
    /// </returns>
    public static bool Last<TTag, TValue>
        (
        this ref TreeIterator<TTag, TValue> @this,
        JoinableTreeNode<TTag, TValue> node
        )
        where TTag : struct, ITagTraits<TTag>
    {
        if (node != null) @this.Clear();
        else node = @this.TryPop();

        for (; node != null; node = node.R)
            @this.Push(node);
        return !@this.IsEmpty;
    }

    /// <summary>
    /// Moves <c>this</c> to the next element in sort order.
    /// </summary>
    /// <param name="this">Iterator instance.</param>
    /// <returns>True if the next element exists, false otherwise.</returns>
    public static bool Succ<TTag, TValue>(this ref TreeIterator<TTag, TValue> @this)
        where TTag : struct, ITagTraits<TTag>
    {
        var current = @this.TryPop();
        if (current == null)
            return false;

        if (current.R != null) {
            @this.Push(current);
            for (current = current.R; current != null; current = current.L)
                @this.Push(current);
        } else {
            JoinableTreeNode<TTag, TValue> y;
            do {
                y = current;
                if ((current = @this.TryPop()) == null)
                    return false;
            } while (y == current.R);
            @this.Push(current);
        }
        return true;
    }

    /// <summary>
    /// Moves <c>this</c> to the previous element in sort order.
    /// </summary>
    /// <param name="this">Iterator instance.</param>
    /// <returns>True if the next element exists, false otherwise.</returns>
    public static bool Pred<TTag, TValue>(this ref TreeIterator<TTag, TValue> @this)
        where TTag : struct, ITagTraits<TTag>
    {
        var current = @this.TryPop();
        if (current == null)
            return false;

        if (current.L != null) {
            @this.Push(current);
            for (current = current.L; current != null; current = current.R)
                @this.Push(current);
        } else {
            JoinableTreeNode<TTag, TValue> y;
            do {
                y = current;
                if ((current = @this.TryPop()) == null)
                    return false;
            } while (y == current.L);
            @this.Push(current);
        }
        return true;
    }

    /// <summary>
    /// Sets <c>this</c> to the n'th element in sorted order in the tree.
    /// </summary>
    /// <param name="this">Iterator instance.</param>
    /// <param name="node">
    /// Node from which to start the search.  If <c>null</c>, the top node of <c>this</c> is used.
    /// Otherwise, <c>this</c> is cleared and search restarted with the given node.
    /// </param>
    /// <param name="index">Order of the element to retrieve.</param>
    /// <exception cref="IndexOutOfRangeException">
    /// Index is outside of range <c>[0, Size-1)</c>, size being the size of the subtree.
    /// </exception>
    public static void Nth<TTag, TValue>
        (
        this ref TreeIterator<TTag, TValue> @this,
        JoinableTreeNode<TTag, TValue> node,
        int index
        )
        where TTag : struct, ITagTraits<TTag>
    {
        if (node != null) @this.Clear();
        else if (!@this.IsEmpty) node = @this.Top;

        if (node == null || index < 0 || index >= node.T.Size)
            throw new IndexOutOfRangeException("Invalid tree element index.");
        ++index;    // Makes calculations easier.

    loop:
        @this.Push(node);
        var l = node.L?.T.Size ?? 0;
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
