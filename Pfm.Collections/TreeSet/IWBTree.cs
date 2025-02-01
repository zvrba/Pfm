using System;
using System.Runtime.CompilerServices;

namespace Podaga.PersistentCollections.TreeSet;

public interface IWBTree<TSelf, TValue> : IBalanceTraits<TSelf, TValue>
    where TSelf : struct, IValueTraits<TValue>, IWBTree<TSelf, TValue>
{
    // NB: CombineBalanceTags would normally perform some calculations, but in WB tree, the balance is
    // also the size, which is automatically maintained by the tree itself.
    static int IBalanceTraits<TSelf, TValue>.NilBalance => 0;
    static int IBalanceTraits<TSelf, TValue>.CombineBalanceTags(int left, int right) => 0;

    private const float Alpha = 0.25f;  // Alpha
    private const float AlphaC = 1 - Alpha;

    static TreeNode<TValue> IBalanceTraits<TSelf, TValue>.Join(
        JoinTree<TValue, TSelf> transient,
        TreeNode<TValue> left,
        TreeNode<TValue> middle,
        TreeNode<TValue> right)
    {
        if (LeftHeavy(S(left), S(right))) {
            var jd = new JoinData { Middle = middle, Other = right, Tree = transient };
            return JoinR(left, ref jd);
        }
        if (LeftHeavy(S(right), S(left))) {
            var jd = new JoinData { Middle = middle, Other = left, Tree = transient };
            return JoinL(right, ref jd);
        }
        return transient.JoinBalanced(left, middle, right);
    }

    private struct JoinData
    {
        public TreeNode<TValue> Middle;
        public TreeNode<TValue> Other;
        public JoinTree<TValue, TSelf> Tree;
    }

    // Follow right branch of tl until a TreeNode c is reached with a weight like to tr.
    private static TreeNode<TValue> JoinR(TreeNode<TValue> tl, ref JoinData jd) {
        if (Like(S(tl), S(jd.Other)))             // Base case
            return jd.Tree.JoinBalanced(tl, jd.Middle, jd.Other);

        var t1 = JoinR(tl.R, ref jd);
        tl = tl.Clone<TSelf>(jd.Tree.Transient);
        tl.R = t1;
        tl.Update<TSelf>();

        if (!Like(S(tl.L), S(t1))) {
            if (IsSingleRotation(S(tl.L), S(t1))) tl = jd.Tree.RotL(tl);
            else tl = jd.Tree.RotLL(tl);
        }
        return tl;
    }

    // Follow left branch of tr until a TreeNode c is reached with a weight like to tl.
    private static TreeNode<TValue> JoinL(TreeNode<TValue> tr, ref JoinData jd) {
        if (Like(S(jd.Other), S(tr)))
            return jd.Tree.JoinBalanced(jd.Other, jd.Middle, tr);

        var t1 = JoinL(tr.L, ref jd);
        tr = tr.Clone<TSelf>(jd.Tree.Transient);
        tr.L = t1;
        tr.Update<TSelf>();

        if (!Like(S(t1), S(tr.R))) {
            if (IsSingleRotation(S(t1), S(tr.R))) tr = jd.Tree.RotR(tr);
            else tr = jd.Tree.RotRR(tr);
        }
        return tr;
    }

    static void IBalanceTraits<TSelf, TValue>.ValidateStructure(TreeNode<TValue> root) {
        if (root?.Size > 1)  // Single-element tree cannot be balanced.
            ValidateWeights(root);

        static void ValidateWeights(TreeNode<TValue> TreeNode) {
            if (TreeNode == null)
                return;
            var r = (float)(S(TreeNode.L) + 1) / (S(TreeNode.L) + S(TreeNode.R) + 2);
            if (r < Alpha || r > AlphaC)
                throw new NotImplementedException();
            ValidateWeights(TreeNode.L);
            ValidateWeights(TreeNode.R);
        }
    }

    // NOTE: TreeNode<> restricts sizes to 26 bits: therefore, the calculations below cannot overflow.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int S(TreeNode<TValue> n) => n?.Size ?? 0;

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
}
