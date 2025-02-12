using System;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Describes the result of splitting a tree at a node.
/// </summary>
/// <seealso cref="TreeAlgorithms.Split{TValue}(Podaga.PersistentCollections.Tree.JoinableTreeNode{TValue}, TValue)"/>.
public struct TreeSplit<TValue> where TValue : ITaggedValue<TValue>
{
    public JoinableTreeNode<TValue> Left;
    public JoinableTreeNode<TValue> Middle;
    public JoinableTreeNode<TValue> Right;
}

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
    public static JoinableTreeNode<TValue> Copy<TValue>
        (
        this JoinableTreeNode<TValue> root,
        ulong transient
        )
        where TValue : ITaggedValue<TValue>
    {
        root = root.Clone(transient);
        if (root.Left != null)
            root.Left = Copy(root.Left, transient);
        if (root.Right != null)
            root.Right = Copy(root.Right, transient);
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
    public static TValue Nth<TValue>
        (
        this JoinableTreeNode<TValue> root,
        int index
        )
        where TValue : ITaggedValue<TValue>
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
    /// Splits a tree rooted at <paramref name="this"/> into left and right subtrees 
    /// holding respectively values less than and greater than <paramref name="value"/>.
    /// </summary>
    /// <returns>
    /// A structure containing the left and right subtrees and a flag indicating whether <paramref name="value"/> was
    /// found in the tree under <paramref name="this"/>.
    /// </returns>
    public static TreeSplit<TValue> Split<TValue, TJoin>
        (
        this JoinableTreeNode<TValue> @this,
        TValue value
        )
        where TValue : ITaggedValue<TValue>
        where TJoin : struct, ITreeJoin<TValue>
    {
        if (@this == null)
            return default;
        var c = TValue.Compare(value, @this.Value);
        if (c == 0)
            return new() { Left = @this.Left, Middle = @this, Right = @this.Right };
        
        if (c < 0) {
            var s = Split<TValue, TJoin>(@this.Left, value);
            var jd = new TreeJoin<TValue> { Left = s.Right, Middle = @this, Right = @this.Right };  // XXX: Transient?!
            var j = TJoin.Join(jd);
            return new() { Left = s.Left, Middle = s.Middle, Right = j };
        } else {
            var s = Split<TValue, TJoin>(@this.Right, value);
            var jd = new TreeJoin<TValue> { Left = @this.Left, Middle = @this, Right = s.Left }; // XXX: Transient?!
            var j = TJoin.Join(jd);
            return new() { Left = j, Middle = s.Middle, Right = s.Right };
        }
    }

    /// <summary>
    /// Single left rotation using <paramref name="this"/> as pivot node.
    /// </summary>
    /// <returns>New subtree root such that <paramref name="this"/> is its left child.</returns>
    public static JoinableTreeNode<TValue> RotL<TValue, TJoin>
        (
        this JoinableTreeNode<TValue> @this,
        ulong transient
        )
        where TValue : ITaggedValue<TValue>
        where TJoin : struct, ITreeJoin<TValue>
    {
        @this = @this.Clone(transient);
        var y = @this.Right.Clone(transient);
        @this.Right = y.Left;
        y.Left = @this;
        @this.Update<TJoin>();
        y.Update<TJoin>();
        return y;
    }

    /// <summary>
    /// Double left rotation using <paramref name="this"/> as pivot node.
    /// </summary>
    /// <returns>New subtree root such that <paramref name="this"/> is its left child.</returns>
    public static JoinableTreeNode<TValue> RotLL<TValue, TJoin>
        (
        this JoinableTreeNode<TValue> @this,
        ulong transient
        )
        where TValue : ITaggedValue<TValue>
        where TJoin : struct, ITreeJoin<TValue>
    {
        @this = @this.Clone(transient);
        var x = @this.Right.Clone(transient);
        var y = x.Left.Clone(transient);
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
    /// <returns>New subtree root such that <paramref name="this"/> is its right child.</returns>
    public static JoinableTreeNode<TValue> RotR<TValue, TJoin>
        (
        this JoinableTreeNode<TValue> @this,
        ulong transient
        )
        where TValue : ITaggedValue<TValue>
        where TJoin : struct, ITreeJoin<TValue> 
    {
        @this = @this.Clone(transient);
        var x = @this.Left.Clone(transient);
        @this.Left = x.Right;
        x.Right = @this;
        @this.Update<TJoin>();
        x.Update<TJoin>();
        return x;
    }

    /// <summary>
    /// Double right rotation using <paramref name="this"/> as pivot node.
    /// </summary>
    /// <returns>New subtree root such that <paramref name="this"/> is its right child.</returns>
    public static JoinableTreeNode<TValue> RotRR<TValue, TJoin>
        (
        this JoinableTreeNode<TValue> @this,
        ulong transient
        )
        where TValue : ITaggedValue<TValue>
        where TJoin : struct, ITreeJoin<TValue> 
    {
        @this = @this.Clone(transient);
        var x = @this.Left.Clone(transient);
        var y = x.Right.Clone(transient);
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
