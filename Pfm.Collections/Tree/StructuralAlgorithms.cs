#nullable enable
using System;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Structural algorithm extension methods on <see cref="JoinableTree{TTag, TValue, TSelf}"/>.  These algorithms do not
/// depend on value traits, except for cloning.  The implicit <c>this</c> parameter is used as the transient context for
/// cloning during mutations.
/// </summary>
public static class StructuralAlgorithms
{
    /// <summary>
    /// Copies all nodes from <paramref name="other"/> into <c>this</c>, replacing the root.
    /// Each node is copied if the node's transient tag is different from <c>this</c>.
    /// </summary>
    /// <param name="this">The tree instance to copy into.</param>
    /// <param name="other">Root of the (sub)tree to copy; must not be null.</param>
    public static void CopyFrom<TTag, TValue, TSelf>
        (
        this JoinableTree<TTag, TValue, TSelf> @this,
        JoinableTree<TTag, TValue, TSelf> other
        )
        where TTag : struct, ITagTraits<TTag>
        where TSelf : struct, ITreeTraits<TTag, TValue>
    {
        @this.Root = other.Root is null ? null : Copy<TTag, TValue, TSelf>(@this.Transient, other.Root);
    }

    private static JoinableTreeNode<TTag, TValue> Copy<TTag, TValue, TValueTraits>(ulong transient, JoinableTreeNode<TTag, TValue> root)
        where TTag : struct, ITagTraits<TTag>
        where TValueTraits : struct, IValueTraits<TValue>
    {
        root = root.Clone<TValueTraits>(transient);
        if (root.L != null)
            root.L = Copy<TTag, TValue, TValueTraits>(transient, root.L);
        if (root.R != null)
            root.R = Copy<TTag, TValue, TValueTraits>(transient, root.R);
        return root;
    }

    /// <summary>
    /// Returns the n'th element in sorted order in the tree.  Using this method is more efficient than the equivalent iterator methods.
    /// </summary>
    /// <param name="this">The tree instance.</param>
    /// <param name="index">Order of the element to retrieve.</param>
    /// <returns>The found element.</returns>
    /// <exception cref="IndexOutOfRangeException">
    /// Index is outside of range <c>[0, Size-1)</c>, size being the size of the subtree.
    /// </exception>
    public static TValue Nth<TTag, TValue, TSelf>
        (
        this JoinableTree<TTag, TValue, TSelf> @this,
        int index
        )
        where TTag : struct, ITagTraits<TTag>
        where TSelf : struct, ITreeTraits<TTag, TValue>
    {
        var root = @this.Root;
        if (root is null || index < 0 || index >= root.T.Size)
            throw new IndexOutOfRangeException("Invalid tree element index.");

        ++index;    // Makes calculations easier.
    loop:
        var l = root!.L is null ? 0 : root.L.T.Size;
        if (index == l + 1)
            return root.V;
        if (index <= l) {
            root = root.L;
        } else {
            root = root.R;
            index -= l + 1;
        }
        goto loop;
    }

    /// <summary>
    /// Returns a tree for which the in-order values are concatenation of in-order values of <paramref name="left"/>
    /// and <paramref name="right"/>.
    /// </summary>
    /// <param name="this">Tree instance; used for transient context.</param>
    /// <param name="left">Left side of the join.</param>
    /// <param name="right">Right side of the join.</param>
    public static JoinableTreeNode<TTag, TValue>? Join2<TTag, TValue, TSelf>
        (
        this JoinableTree<TTag, TValue, TSelf> @this,
        JoinableTreeNode<TTag, TValue>? left,
        JoinableTreeNode<TTag, TValue>? right
        )
        where TTag : struct, ITagTraits<TTag>
        where TSelf : struct, ITreeTraits<TTag, TValue>
    {
        if (left is null)
            return right;
        var n = SplitLast(left, out var leftlast);
        return TTreeTraits.Join(this, n, leftlast, right);

        JoinableTreeNode<TTag, TValue> SplitLast(JoinableTreeNode<TTag, TValue> node, out JoinableTreeNode<TTag, TValue> rightmost) {
            if (node.R == null) {
                rightmost = node;
                return node.L;
            }
            var n = SplitLast(node.R, out rightmost);
            return TSelf.Join()
            var j = TTreeTraits.Join(this, node.L, node, n);
            return j;
        }
    }


    /// <summary>
    /// Attaches <paramref name="l"/> and <paramref name="r"/> as left and right children of <paramref name="m"/>.
    /// </summary>
    /// <returns>
    /// The updated (possibly cloned) node that was <paramref name="m"/>.
    /// </returns>
    public static JoinableTreeNode<TTag, TValue> JoinBalanced<TTag, TValue, TSelf>
        (
        this JoinableTree<TTag, TValue, TSelf> @this,
        JoinableTreeNode<TTag, TValue> l,
        JoinableTreeNode<TTag, TValue> m,
        JoinableTreeNode<TTag, TValue> r
        )
        where TTag : struct, ITagTraits<TTag>
        where TSelf : struct, ITreeTraits<TTag, TValue>
    {
        m = m.Clone<TSelf>(@this.Transient);
        m.L = l; m.R = r;
        m.Update<TSelf>();
        return m;
    }

    /// <summary>
    /// Single left rotation using <paramref name="n"/> as pivot node.
    /// </summary>
    /// <returns>New subtree root such that <paramref name="n"/> is its left child.</returns>
    public static JoinableTreeNode<TTag, TValue> RotL<TTag, TValue, TSelf>
        (
        this JoinableTree<TTag, TValue, TSelf> @this,
        JoinableTreeNode<TTag, TValue> n
        )
        where TTag : struct, ITagTraits<TTag>
        where TSelf : struct, ITreeTraits<TTag, TValue>
    {
        n = n.Clone<TSelf>(@this.Transient);
        var y = n.R.Clone<TSelf>(@this.Transient);
        n.R = y.L;
        y.L = n;
        n.Update<TSelf>();
        y.Update<TSelf>();
        return y;
    }

    /// <summary>
    /// Double left rotation using <paramref name="n"/> as pivot node.
    /// </summary>
    /// <returns>New subtree root such that <paramref name="n"/> is its left child.</returns>
    public static JoinableTreeNode<TTag, TValue> RotLL<TTag, TValue, TSelf>
        (
        this JoinableTree<TTag, TValue, TSelf> @this,
        JoinableTreeNode<TTag, TValue> n
        )
        where TTag : struct, ITagTraits<TTag>
        where TSelf : struct, ITreeTraits<TTag, TValue> 
    {
        n = n.Clone<TSelf>(@this.Transient);
        var x = n.R.Clone<TSelf>(@this.Transient);
        var y = x.L.Clone<TSelf>(@this.Transient);
        n.R = y.L;
        x.L = y.R;
        y.L = n;
        y.R = x;
        n.Update<TSelf>();
        x.Update<TSelf>();
        y.Update<TSelf>();
        return y;
    }

    /// <summary>
    /// Single right rotation using <paramref name="n"/> as pivot node.
    /// </summary>
    /// <returns>New subtree root such that <paramref name="n"/> is its right child.</returns>
    public static JoinableTreeNode<TTag, TValue> RotR<TTag, TValue, TSelf>
        (
        this JoinableTree<TTag, TValue, TSelf> @this,
        JoinableTreeNode<TTag, TValue> n
        )
        where TTag : struct, ITagTraits<TTag>
        where TSelf : struct, ITreeTraits<TTag, TValue>
    {
        n = n.Clone<TSelf>(@this.Transient);
        var x = n.L.Clone<TSelf>(@this.Transient);
        n.L = x.R;
        x.R = n;
        n.Update<TSelf>();
        x.Update<TSelf>();
        return x;
    }

    /// <summary>
    /// Double right rotation using <paramref name="n"/> as pivot node.
    /// </summary>
    /// <returns>New subtree root such that <paramref name="n"/> is its right child.</returns>
    public static JoinableTreeNode<TTag, TValue> RotRR<TTag, TValue, TSelf>
        (
        this JoinableTree<TTag, TValue, TSelf> @this,
        JoinableTreeNode<TTag, TValue> n
        )
        where TTag : struct, ITagTraits<TTag>
        where TSelf : struct, ITreeTraits<TTag, TValue>
    {
        n = n.Clone<TSelf>(@this.Transient);
        var x = n.L.Clone<TSelf>(@this.Transient);
        var y = x.R.Clone<TSelf>(@this.Transient);
        x.R = y.L;
        n.L = y.R;
        y.L = x;
        y.R = n;
        x.Update<TSelf>();
        n.Update<TSelf>();
        y.Update<TSelf>();
        return y;
    }
}
