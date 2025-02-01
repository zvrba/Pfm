using System;
using System.Runtime.CompilerServices;

namespace Podaga.PersistentCollections.TreeSet;

public partial class JoinTree<TValue, TTreeTraits>
    where TTreeTraits : struct, IValueTraits<TValue>, IBalanceTraits<TTreeTraits, TValue>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TreeNode<TValue> JoinBalanced(
        TreeNode<TValue> l,
        TreeNode<TValue> m,
        TreeNode<TValue> r)
    {
        m = m.Clone<TTreeTraits>(transient);
        m.L = l; m.R = r;
        m.Update<TTreeTraits>();
        return m;
    }

    /// <summary>
    /// Single left rotation using <paramref name="n"/> as pivot node.
    /// </summary>
    /// <returns>New subtree root such that <paramref name="n"/> is its left child.</returns>
    public TreeNode<TValue> RotL(TreeNode<TValue> n) {
        n = n.Clone<TTreeTraits>(transient);
        var y = n.R.Clone<TTreeTraits>(transient);
        n.R = y.L;
        y.L = n;
        n.Update<TTreeTraits>();
        y.Update<TTreeTraits>();
        return y;
    }

    /// <summary>
    /// Double left rotation using <paramref name="n"/> as pivot node.
    /// </summary>
    /// <returns>New subtree root such that <paramref name="n"/> is its left child.</returns>
    public TreeNode<TValue> RotLL(TreeNode<TValue> n) {
        n = n.Clone<TTreeTraits>(transient);
        var x = n.R.Clone<TTreeTraits>(transient);
        var y = x.L.Clone<TTreeTraits>(transient);
        n.R = y.L;
        x.L = y.R;
        y.L = n;
        y.R = x;
        n.Update<TTreeTraits>();
        x.Update<TTreeTraits>();
        y.Update<TTreeTraits>();
        return y;
    }

    /// <summary>
    /// Single right rotation using <paramref name="n"/> as pivot node.
    /// </summary>
    /// <returns>New subtree root such that <paramref name="n"/> is its right child.</returns>
    public TreeNode<TValue> RotR(TreeNode<TValue> n) {
        n = n.Clone<TTreeTraits>(transient);
        var x = n.L.Clone<TTreeTraits>(transient);
        n.L = x.R;
        x.R = n;
        n.Update<TTreeTraits>();
        x.Update<TTreeTraits>();
        return x;
    }

    /// <summary>
    /// Double right rotation using <paramref name="n"/> as pivot node.
    /// </summary>
    /// <returns>New subtree root such that <paramref name="n"/> is its right child.</returns>
    public TreeNode<TValue> RotRR(TreeNode<TValue> n) {
        n = n.Clone<TTreeTraits>(transient);
        var x = n.L.Clone<TTreeTraits>(transient);
        var y = x.R.Clone<TTreeTraits>(transient);
        x.R = y.L;
        n.L = y.R;
        y.L = x;
        y.R = n;
        x.Update<TTreeTraits>();
        n.Update<TTreeTraits>();
        y.Update<TTreeTraits>();
        return y;
    }

}
