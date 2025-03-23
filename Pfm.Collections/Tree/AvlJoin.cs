using System;
using System.Runtime.CompilerServices;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Implementation of <see cref="ITreeTraits{TValue}"/> for AVL trees.
/// </summary>
/// <typeparam name="TSelf">The most-derived type implementing this interface (CRTP pattern).</typeparam>
/// <typeparam name="TValue">Tree element type.</typeparam>
public interface IAvlJoin<TSelf, TValue> : ITreeTraits<TValue>
    where TSelf : struct, IAvlJoin<TSelf, TValue>
{
    // UTILITIES

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int H(JoinableTreeNode<TValue> n) => n?.Rank ?? 0;

    /// <inheritdoc/>
    static int ITreeTraits<TValue>.NilRank => 0;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int ITreeTraits<TValue>.CombineRanks(int left, int middle, int right) => 1 + (left > right ? left : right);

    /// <inheritdoc/>
    static JoinableTreeNode<TValue> ITreeTraits<TValue>.Join(TreeSection<TValue> jd)
    {
        if (H(jd.Left) > H(jd.Right) + 1)
            return JoinR(jd);
        if (H(jd.Right) > H(jd.Left) + 1)
            return JoinL(jd);
        return jd.JoinBalanced<TSelf>();
    }

    static void ITreeTraits<TValue>.ValidateStructure(JoinableTreeNode<TValue> node) => ValidateHeights(node);

    // Search along the right spine of tl ...
    private static JoinableTreeNode<TValue> JoinR(TreeSection<TValue> jd)
    {
        var tl = jd.Left;
        var (l, c) = (tl.Left, tl.Right);
        if (H(c) <= H(jd.Right) + 1) {
            jd.Left = c;
            var t1 = jd.JoinBalanced<TSelf>();
            tl = tl.Clone<TSelf>(jd.Transient);
            tl.Right = t1;
            tl.Update<TSelf>();
            if (t1.Rank > H(l) + 1)
                tl = tl.RotLL<TValue, TSelf>(jd.Transient);
        } else {
            jd.Left = c;
            var t1 = JoinR(jd);
            tl = tl.Clone<TSelf>(jd.Transient);
            tl.Right = t1;
            tl.Update<TSelf>();
            if (t1.Rank > H(l) + 1)
                tl = tl.RotL<TValue, TSelf>(jd.Transient);
        }
        return tl;
    }

    // Search along the left spine of tr...
    private static JoinableTreeNode<TValue> JoinL(TreeSection<TValue> jd)
    {
        var tr = jd.Right;
        var (c, r) = (tr.Left, tr.Right);
        if (H(c) <= H(jd.Left) + 1) {
            jd.Right = c;
            var t1 = jd.JoinBalanced<TSelf>();
            tr = tr.Clone<TSelf>(jd.Transient);
            tr.Left = t1;
            tr.Update<TSelf>();
            if (t1.Rank > H(r) + 1)
                tr = tr.RotRR<TValue, TSelf>(jd.Transient);
        } else {
            jd.Right = c;
            var t1 = JoinL(jd);
            tr = tr.Clone<TSelf>(jd.Transient);
            tr.Left = t1;
            tr.Update<TSelf>();
            if (t1.Rank > H(r) + 1)
                tr = tr.RotR<TValue, TSelf>(jd.Transient);
        }
        return tr;
    }

    private static int ValidateHeights(JoinableTreeNode<TValue> node) {
        if (node == null)
            return 0;
        var l = ValidateHeights(node.Left);
        var r = ValidateHeights(node.Right);
        var h = 1 + (l > r ? l : r);
        var b = r - l;

        if (node.Rank != h)
            throw new NotImplementedException();
        if (b < -1 || b > 1)
            throw new NotImplementedException();

        return h;
    }
}
