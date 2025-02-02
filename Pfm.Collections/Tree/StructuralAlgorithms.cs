#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Structural algorithm extension methods on <see cref="JoinableTree{TTag, TValue, TSelf}"/>.  These algorithms do not
/// depend on value traits, except for cloning.  The implicit <c>this</c> parameter is used as the transient context for
/// cloning during mutations.
/// </summary>
public static class StructuralAlgorithms
{
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
