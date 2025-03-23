using System;
using System.Runtime.CompilerServices;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Implementation of <see cref="ITreeTraits{TValue}"/> for WB trees.
/// The balance factor is hard-coded to 1/4.  This value is just below the maximum proven in the paper
/// that makes the tree strongly joinable.
/// </summary>
/// <typeparam name="TSelf">The most-derived type implementing this interface (CRTP pattern).</typeparam>
/// <typeparam name="TValue">Tree element type.</typeparam>
public interface IWBJoin<TSelf, TValue> : ITreeTraits<TValue>
    where TSelf : struct, IWBJoin<TSelf, TValue>
{
    private const float Alpha = 0.25f;  // Alpha
    private const float AlphaC = 1 - Alpha;

    // UTILITIES.  TODO! FIX! The calculations below can overflow when sizes exceed > 2^26 elements.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int S(JoinableTreeNode<TValue> n) => n?.Size ?? 0;

    // Checks whether the left size is overweight using int arithmetic only.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool LeftHeavy(int lsize, int rsize) => lsize + 1 > 3 * (lsize + rsize + 2) / 4;

    // Checks that the balance factor is within [1/4, 3/4] using int arithmetic only.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Like(int lsize, int rsize) {
        rsize += lsize + 2;         // Total tree weight
        lsize = (lsize + 1) * 4;    // Common denominator for lhs and rhs
        return lsize >= rsize && lsize <= 3 * rsize;
    }

    // Checks that the balance factor is within [1/4, 2/3) using int arithmetic only.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSingleRotation(int lsize, int rsize) {
        rsize += lsize + 2;         // Total tree weight
        lsize = (lsize + 1) * 12;   // LCM of 3 and 4
        return lsize >= 3 * rsize && lsize < 8 * rsize;
    }

    /// <inheritdoc/>
    static int ITreeTraits<TValue>.NilRank => 0;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int ITreeTraits<TValue>.CombineRanks(int left, int middle, int right) => 0;

    /// <inheritdoc/>
    static JoinableTreeNode<TValue> ITreeTraits<TValue>.Join(TreeSection<TValue> jd)
    {
        if (LeftHeavy(S(jd.Left), S(jd.Right)))
            return JoinR(jd);
        if (LeftHeavy(S(jd.Right), S(jd.Left)))
            return JoinL(jd);
        return jd.JoinBalanced<TSelf>();
    }

    /// <inheritdoc/>
    static void ITreeTraits<TValue>.ValidateStructure(JoinableTreeNode<TValue> node) {
        if (node?.Size > 1)  // Single-element tree cannot be balanced.
            ValidateWeights(node);
    }

    private static JoinableTreeNode<TValue> JoinR(TreeSection<TValue> jd) {
        if (Like(S(jd.Left), S(jd.Right)))             // Base case
            return jd.JoinBalanced<TSelf>();

        var tl = jd.Left;
        jd.Left = tl.Right;
        var t1 = JoinR(jd);
        tl = tl.Clone<TSelf>(jd.Transient);
        tl.Right = t1;
        tl.Update<TSelf>();

        if (!Like(S(tl.Left), S(t1))) {
            if (IsSingleRotation(S(tl.Left), S(t1))) tl = tl.RotL<TValue, TSelf>(jd.Transient);
            else tl = tl.RotLL<TValue, TSelf>(jd.Transient);
        }
        return tl;
    }

    // Follow left branch of tr until a TreeNode c is reached with a weight like to tl.
    private static JoinableTreeNode<TValue> JoinL(TreeSection<TValue> jd) {
        if (Like(S(jd.Left), S(jd.Right)))
            return jd.JoinBalanced<TSelf>();

        var tr = jd.Right;
        jd.Right = tr.Left;
        var t1 = JoinL(jd);
        tr = tr.Clone<TSelf>(jd.Transient);
        tr.Left = t1;
        tr.Update<TSelf>();

        if (!Like(S(t1), S(tr.Right))) {
            if (IsSingleRotation(S(t1), S(tr.Right))) tr = tr.RotR<TValue, TSelf>(jd.Transient);
            else tr = tr.RotRR<TValue, TSelf>(jd.Transient);
        }
        return tr;
    }

    private static void ValidateWeights(JoinableTreeNode<TValue> node) {
        if (node == null)
            return;
        var r = (float)(S(node.Left) + 1) / (S(node.Left) + S(node.Right) + 2);
        if (r < Alpha || r > AlphaC)
            throw new NotImplementedException();
        ValidateWeights(node.Left);
        ValidateWeights(node.Right);
    }
}
