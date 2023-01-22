using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Pfm.Collections.TreeSet;

public interface IAvlTree<TSelf, TValue> : IJoinTree<TSelf, TValue>
    where TSelf : struct, IAvlTree<TSelf, TValue>, IValueTraits<TValue>, IPersistenceTraits<TValue>
{
    static int IJoinTree<TSelf, TValue>.NilBalance => 0;
    static int IJoinTree<TSelf, TValue>.CombineBalanceTags(int left, int right) => 1 + (left > right ? left : right);

    static TreeNode<TValue> IJoinTree<TSelf, TValue>.Join(TreeNode<TValue> left, TreeNode<TValue> middle, TreeNode<TValue> right)
    {
        if (H(left) > H(right) + 1)
            return JoinR(left, middle, right);
        if (H(right) > H(left) + 1)
            return JoinL(left, middle, right);
        return JoinBalanced(left, middle, right);

    }

    // Search along the right spine of tl ...
    private static TreeNode<TValue> JoinR(TreeNode<TValue> tl, TreeNode<TValue> k, TreeNode<TValue> tr) {
        var (l, c) = (tl.L, tl.R);
        if (H(c) <= H(tr) + 1) {
            var t1 = JoinBalanced(c, k, tr);
            tl = TSelf.Clone(tl);
            tl.R = t1;
            tl.Update<TSelf>();
            if (t1.Rank > H(l) + 1)
                tl = RotLL(tl);
        } else {
            var t1 = JoinR(c, k, tr);
            tl = TSelf.Clone(tl);
            tl.R = t1;
            tl.Update<TSelf>();
            if (t1.Rank > H(l) + 1)
                tl = RotL(tl);
        }
        return tl;
    }

    // Search along the left spine of tr...
    private static TreeNode<TValue> JoinL(TreeNode<TValue> tl, TreeNode<TValue> k, TreeNode<TValue> tr) {
        var (c, r) = (tr.L, tr.R);
        if (H(c) <= H(tl) + 1) {
            var t1 = JoinBalanced(tl, k, c);
            tr = TSelf.Clone(tr);
            tr.L = t1;
            tr.Update<TSelf>();
            if (t1.Rank > H(r) + 1)
                tr = RotRR(tr);
        } else {
            var t1 = JoinL(tl, k, c);
            tr = TSelf.Clone(tr);
            tr.L = t1;
            tr.Update<TSelf>();
            if (t1.Rank > H(r) + 1)
                tr = RotR(tr);
        }
        return tr;
    }

    static void IJoinTree<TSelf, TValue>.ValidateStructure(TreeNode<TValue> root) {
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
