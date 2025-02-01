using System;
using System.Runtime.CompilerServices;

namespace Podaga.PersistentCollections.TreeSet;

public interface IAvlTree<TSelf, TValue> : IBalanceTraits<TSelf, TValue>
    where TSelf : struct, IValueTraits<TValue>, IAvlTree<TSelf, TValue>
{
    static int IBalanceTraits<TSelf, TValue>.NilBalance => 0;
    static int IBalanceTraits<TSelf, TValue>.CombineBalanceTags(int left, int right) => 1 + (left > right ? left : right);

    private struct JoinData {
        public TreeNode<TValue> Middle;
        public TreeNode<TValue> Other;
        public JoinTree<TValue, TSelf> Tree;
    }

    static TreeNode<TValue> IBalanceTraits<TSelf, TValue>.Join(
        JoinTree<TValue, TSelf> transient,
        TreeNode<TValue> left,
        TreeNode<TValue> middle,
        TreeNode<TValue> right)
    {
        if (H(left) > H(right) + 1) {
            var jd = new JoinData { Middle = middle, Other = right, Tree = transient };
            return JoinR(left, ref jd);
        }
        if (H(right) > H(left) + 1) {
            var jd = new JoinData { Middle = middle, Other = left, Tree = transient };
            return JoinL(right, ref jd);
        }
        return transient.JoinBalanced(left, middle, right);
    }

    // Search along the right spine of tl ...
    private static TreeNode<TValue> JoinR(TreeNode<TValue> tl, ref JoinData jd) {
        var (l, c) = (tl.L, tl.R);
        if (H(c) <= H(jd.Other) + 1) {
            var t1 = jd.Tree.JoinBalanced(c, jd.Middle, jd.Other);
            tl = tl.Clone<TSelf>(jd.Tree.Transient);
            tl.R = t1;
            tl.Update<TSelf>();
            if (t1.Rank > H(l) + 1)
                tl = jd.Tree.RotLL(tl);
        } else {
            var t1 = JoinR(c, ref jd);
            tl = tl.Clone<TSelf>(jd.Tree.Transient);
            tl.R = t1;
            tl.Update<TSelf>();
            if (t1.Rank > H(l) + 1)
                tl = jd.Tree.RotL(tl);
        }
        return tl;
    }

    // Search along the left spine of tr...
    private static TreeNode<TValue> JoinL(TreeNode<TValue> tr, ref JoinData jd) {
        var (c, r) = (tr.L, tr.R);
        if (H(c) <= H(jd.Other) + 1) {
            var t1 = jd.Tree.JoinBalanced(jd.Other, jd.Middle, c);
            tr = tr.Clone<TSelf>(jd.Tree.Transient);
            tr.L = t1;
            tr.Update<TSelf>();
            if (t1.Rank > H(r) + 1)
                tr = jd.Tree.RotRR(tr);
        } else {
            var t1 = JoinL(c, ref jd);
            tr = tr.Clone<TSelf>(jd.Tree.Transient);
            tr.L = t1;
            tr.Update<TSelf>();
            if (t1.Rank > H(r) + 1)
                tr = jd.Tree.RotR(tr);
        }
        return tr;
    }

    static void IBalanceTraits<TSelf, TValue>.ValidateStructure(TreeNode<TValue> root) {
        ValidateHeights(root);

        static int ValidateHeights(TreeNode<TValue> node) {
            if (node == null)
                return 0;
            var l = ValidateHeights(node.L);
            var r = ValidateHeights(node.R);
            var h = 1 + (l > r ? l : r);
            var b = r - l;

            if (node.Rank != h)
                throw new NotImplementedException();
            if (b < -1 || b > 1)
                throw new NotImplementedException();

            return h;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int H(TreeNode<TValue> n) => n?.Rank ?? 0;
}
