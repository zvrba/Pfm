using Pfm.Collections.JoinTree;

using System;

namespace Pfm.Collections.TreeSet;

public partial interface IJoinTree<TSelf, TValue>
    where TSelf : struct, IJoinTree<TSelf, TValue>, IValueTraits<TValue>, IPersistenceTraits<TValue>
{
    /// <summary>
    /// Single left rotation using <paramref name="n"/> as pivot node.
    /// </summary>
    /// <returns>New subtree root such that <paramref name="n"/> is its left child.</returns>
    protected static TreeNode<TValue> RotL(TreeNode<TValue> n) {
        n = TSelf.Clone(n);
        var y = TSelf.Clone(n.R);
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
    protected static TreeNode<TValue> RotLL(TreeNode<TValue> n) {
        n = TSelf.Clone(n);
        var x = TSelf.Clone(n.R);
        var y = TSelf.Clone(x.L);
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
    protected static TreeNode<TValue> RotR(TreeNode<TValue> n) {
        n = TSelf.Clone(n);
        var x = TSelf.Clone(n.L);
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
    protected static TreeNode<TValue> RotRR(TreeNode<TValue> n) {
        n = TSelf.Clone(n);
        var x = TSelf.Clone(n.L);
        var y = TSelf.Clone(x.R);
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
