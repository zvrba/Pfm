using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Pfm.Collections.TreeSet;

public struct AvlTree<TValue, TValueTraits, TPersistenceTraits> : ITreeTraits<TValue>
    where TValueTraits : struct, IValueTraits<TValue>
    where TPersistenceTraits : struct, IPersistenceTraits<TValue>
{
    public static int NilBalance => 0;
    public static int CombineBalanceTags(int left, int right) => 1 + (left > right ? left : right);

    public static TreeNode<TValue> Join(
        TreeIterator<TValue> wa,
        TreeNode<TValue> left,
        TreeNode<TValue> middle,
        TreeNode<TValue> right)
    {
        if (H(left) > H(right) + 1)
            return JoinR(wa, left, middle, right);
        if (H(right) > H(left) + 1)
            return JoinL(wa, left, middle, right);
        return Node(left, middle, right);

    }

    private static TreeNode<TValue> JoinR(
        TreeIterator<TValue> wa,
        TreeNode<TValue> left,
        TreeNode<TValue> middle,
        TreeNode<TValue> right)
    {
        TreeNode<TValue> c, t1;


        // Descend along right spine of left until a suitable balance point for joining is found.
        var originalDepth = wa.Depth;
        {
            var threshold = H(right) + 1;   // Local copy for optimization.
    descend:
            c = left.R;
            if (c.Rank > threshold) {
                wa.Push(left);
                left = c;
                goto descend;
            }
        }

        Debug.Assert(H(c) <= H(right) + 1, "Descend terminating condition.");
        Debug.Assert(left.R == c, "Correctness.");

        // Make the join.
        t1 = Node(c, middle, right);
        left = TPersistenceTraits.Clone(left);
        left.R = t1;
        Update(left);
        if (t1.Rank > left.Rank + 1)
            left = JoinTree<TValue, TValueTraits, AvlTree<TValue, TValueTraits, TPersistenceTraits>, TPersistenceTraits>.RotRL(left);

        // Ascend back: t1 is the result of join one level below, left travels up the tree.
        t1 = left;
        while (wa.Depth > originalDepth) {
            left = wa.TryPop();
            left = TPersistenceTraits.Clone(left);
            left.R = t1;
            Update(left);
            if (t1.Rank > left.Rank + 1)
                left = JoinTree<TValue, TValueTraits, AvlTree<TValue, TValueTraits, TPersistenceTraits>, TPersistenceTraits>.RotL(left);
            t1 = left;
        }

        return t1;
    }

    private static TreeNode<TValue> JoinL(
        TreeIterator<TValue> wa,
        TreeNode<TValue> left,
        TreeNode<TValue> middle,
        TreeNode<TValue> right)
    {
        TreeNode<TValue> c, t1;

        // Descend along left spine of right until a suitable balance point for joining is found.
        var originalDepth = wa.Depth;
        {
            var threshold = H(left) + 1;    // Local copy for optimization
        descend:
            c = right.L;
            if (c.Rank > threshold) {
                wa.Push(right);
                right = c;
                goto descend;
            }
        }

        Debug.Assert(H(c) <= H(left) + 1, "Descend terminating condition.");
        Debug.Assert(right.L == c, "Correctness.");

        // Make the join.
        t1 = Node(left, middle, c);
        right = TPersistenceTraits.Clone(right);
        right.L = t1;
        Update(right);
        if (t1.Rank > right.Rank + 1)
            right = JoinTree<TValue, TValueTraits, AvlTree<TValue, TValueTraits, TPersistenceTraits>, TPersistenceTraits>.RotLR(right);

        // Ascend back: t1 is the result of join one level below, right travels up the tree.
        t1 = right;
        while (wa.Depth > originalDepth) {
            right = wa.TryPop();
            right = TPersistenceTraits.Clone(right);
            right.L = t1;
            Update(right);
            if (t1.Rank > right.Rank + 1)
                right = JoinTree<TValue, TValueTraits, AvlTree<TValue, TValueTraits, TPersistenceTraits>, TPersistenceTraits>.RotL(right);
            t1 = right;
        }

        return t1;
    }

    static void ITreeTraits<TValue>.ValidateStructure(TreeNode<TValue> root) {
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
    private static TreeNode<TValue> Node(TreeNode<TValue> l, TreeNode<TValue> m, TreeNode<TValue> r) {
        m = TPersistenceTraits.Clone(m);
        m.L = l; m.R = r;
        Update(m);
        return m;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Update(TreeNode<TValue> n) => n.Update<TValueTraits, AvlTree<TValue, TValueTraits, TPersistenceTraits>>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int H(TreeNode<TValue> n) => n?.Rank ?? 0;
}
