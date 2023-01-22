using System;

namespace Pfm.Collections.TreeSet;

public partial class JoinTree<TValue, TValueTraits, TTreeTraits, TPersistenceTraits>
    where TValueTraits : struct, IValueTraits<TValue>
    where TTreeTraits : struct, ITreeTraits<TValue>
    where TPersistenceTraits : struct, IPersistenceTraits<TValue>
{
    /// <summary>
    /// Single left rotation using <paramref name="n"/> as pivot node.
    /// </summary>
    /// <returns>New subtree root such that <paramref name="n"/> is its left child.</returns>
    public static TreeNode<TValue> RotL(TreeNode<TValue> n) {
        n = TPersistenceTraits.Clone(n);
        var y = TPersistenceTraits.Clone(n.R);
        n.R = y.L;
        y.L = n;
        Update(n);
        Update(y);
        return y;
    }

    /// <summary>
    /// Single right rotation using <paramref name="n"/> as pivot node.
    /// </summary>
    /// <returns>New subtree root such that <paramref name="n"/> is its right child.</returns>
    public static TreeNode<TValue> RotR(TreeNode<TValue> n) {
        n = TPersistenceTraits.Clone(n);
        var x = TPersistenceTraits.Clone(n.L);
        n.L = x.R;
        x.R = n;
        Update(n);
        Update(x);
        return x;
    }

    /// <summary>
    /// Double left-right rotation using <paramref name="n"/> as pivot node.
    /// </summary>
    /// <returns>New subtree root such that <paramref name="n"/> is its right child.</returns>
    public static TreeNode<TValue> RotLR(TreeNode<TValue> n) {
        n = TPersistenceTraits.Clone(n);
        var x = TPersistenceTraits.Clone(n.L);
        var y = TPersistenceTraits.Clone(x.R);
        x.R = y.L;
        n.L = y.R;
        y.L = x;
        y.R = n;
        Update(x);
        Update(n);
        Update(y);
        return y;
    }

    /// <summary>
    /// Double right-left rotation using <paramref name="n"/> as pivot node.
    /// </summary>
    /// <returns>New subtree root such that <paramref name="n"/> is its left child.</returns>
    public static TreeNode<TValue> RotRL(TreeNode<TValue> n) {
        n = TPersistenceTraits.Clone(n);
        var x = TPersistenceTraits.Clone(n.R);
        var y = TPersistenceTraits.Clone(x.L);
        n.R = y.L;
        x.L = y.R;
        y.L = n;
        y.R = x;
        Update(n);
        Update(x);
        Update(y);
        return y;
    }
}
