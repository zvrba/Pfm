using System;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Extension methods on <see cref="JoinableTreeNode{TValue}"/>.  These methods allow to treat any tree node as a
/// tree root for further operations.
/// </summary>
public static partial class TreeAlgorithms
{
    /// <summary>
    /// Copies all nodes in the subtree starting from <paramref name="root"/>.
    /// Each node is cloned if the its transient tag is different from <paramref name="transient"/>.
    /// </summary>
    /// <param name="root">Tree node from which to start copying.</param>
    /// <param name="transient">The required transient tag on the copied subtree.</param>
    /// <returns>The root of the copied subtree.</returns>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <typeparam name="TValueTraits">Value traits (determine how the value is cloned).</typeparam>
    public static JoinableTreeNode<TValue> Copy<TValue, TValueTraits>
        (
        this JoinableTreeNode<TValue> root,
        ulong transient
        )
        where TValueTraits : IValueTraits<TValue>
    {
        root = root.Clone<TValueTraits>(transient);
        if (root.Left != null)
            root.Left = Copy<TValue, TValueTraits>(root.Left, transient);
        if (root.Right != null)
            root.Right = Copy<TValue, TValueTraits>(root.Right, transient);
        return root;
    }

    /// <summary>
    /// Returns the n'th element in sorted order in the tree.  Using this method is more efficient than the equivalent iterator methods.
    /// </summary>
    /// <param name="root">Node at which to begin the search.</param>
    /// <param name="index">Order of the element to retrieve.</param>
    /// <returns>The found element.</returns>
    /// <exception cref="IndexOutOfRangeException">
    /// <paramref name="root"/> is null, or <paramref name="index"/> is outside of range <c>[0, Size-1)</c>, size being the size of the subtree.
    /// </exception>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    public static TValue Nth<TValue>
        (
        this JoinableTreeNode<TValue> root,
        int index
        )
    {
        if (root is null || index < 0 || index >= root.Size)
            throw new IndexOutOfRangeException("Invalid tree element index.");

        ++index;    // Makes calculations easier.
    loop:
        var l = root!.Left is null ? 0 : root.Left.Size;
        if (index == l + 1)
            return root.Value;
        if (index <= l) {
            root = root.Left;
        } else {
            root = root.Right;
            index -= l + 1;
        }
        goto loop;
    }

    /// <summary>
    /// Single left rotation using <paramref name="this"/> as pivot node.
    /// </summary>
    /// <param name="this">Pivot node.</param>
    /// <param name="transient">Transient tag to use during modifications.</param>
    /// <returns>New subtree root such that <paramref name="this"/> is its left child.</returns>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <typeparam name="TJoin">Tree join strategy.</typeparam>
    public static JoinableTreeNode<TValue> RotL<TValue, TJoin>
        (
        this JoinableTreeNode<TValue> @this,
        ulong transient
        )
        where TJoin : struct, ITreeTraits<TValue>
    {
        @this = @this.Clone<TJoin>(transient);
        var y = @this.Right.Clone<TJoin>(transient);
        @this.Right = y.Left;
        y.Left = @this;
        @this.Update<TJoin>();
        y.Update<TJoin>();
        return y;
    }

    /// <summary>
    /// Double left rotation using <paramref name="this"/> as pivot node.
    /// </summary>
    /// <param name="this">Pivot node.</param>
    /// <param name="transient">Transient tag to use during modifications.</param>
    /// <returns>New subtree root such that <paramref name="this"/> is its left child.</returns>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <typeparam name="TJoin">Tree join strategy.</typeparam>
    public static JoinableTreeNode<TValue> RotLL<TValue, TJoin>
        (
        this JoinableTreeNode<TValue> @this,
        ulong transient
        )
        where TJoin : struct, ITreeTraits<TValue>
    {
        @this = @this.Clone<TJoin>(transient);
        var x = @this.Right.Clone<TJoin>(transient);
        var y = x.Left.Clone<TJoin>(transient);
        @this.Right = y.Left;
        x.Left = y.Right;
        y.Left = @this;
        y.Right = x;
        @this.Update<TJoin>();
        x.Update<TJoin>();
        y.Update<TJoin>();
        return y;
    }

    /// <summary>
    /// Single right rotation using <paramref name="this"/> as pivot node.
    /// </summary>
    /// <param name="this">Pivot node.</param>
    /// <param name="transient">Transient tag to use during modifications.</param>
    /// <returns>New subtree root such that <paramref name="this"/> is its right child.</returns>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <typeparam name="TJoin">Tree join strategy.</typeparam>
    public static JoinableTreeNode<TValue> RotR<TValue, TJoin>
        (
        this JoinableTreeNode<TValue> @this,
        ulong transient
        )
        where TJoin : struct, ITreeTraits<TValue> 
    {
        @this = @this.Clone<TJoin>(transient);
        var x = @this.Left.Clone<TJoin>(transient);
        @this.Left = x.Right;
        x.Right = @this;
        @this.Update<TJoin>();
        x.Update<TJoin>();
        return x;
    }

    /// <summary>
    /// Double right rotation using <paramref name="this"/> as pivot node.
    /// </summary>
    /// <param name="this">Pivot node.</param>
    /// <param name="transient">Transient tag to use during modifications.</param>
    /// <returns>New subtree root such that <paramref name="this"/> is its right child.</returns>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <typeparam name="TJoin">Tree join strategy.</typeparam>
    public static JoinableTreeNode<TValue> RotRR<TValue, TJoin>
        (
        this JoinableTreeNode<TValue> @this,
        ulong transient
        )
        where TJoin : struct, ITreeTraits<TValue> 
    {
        @this = @this.Clone<TJoin>(transient);
        var x = @this.Left.Clone<TJoin>(transient);
        var y = x.Right.Clone<TJoin>(transient);
        x.Right = y.Left;
        @this.Left = y.Right;
        y.Left = x;
        y.Right = @this;
        x.Update<TJoin>();
        @this.Update<TJoin>();
        y.Update<TJoin>();
        return y;
    }
}
