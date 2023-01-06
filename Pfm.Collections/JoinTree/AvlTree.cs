using System;
using System.Runtime.CompilerServices;

namespace Pfm.Collections.JoinTree;

public struct AvlTree<TValue, TNodeTraits> : ITreeTraits<TValue>
    where TNodeTraits : struct, INodeTraits<TValue>
{
    public static void Update(Node<TValue> node) {
        int lh, rh, ls, rs;
        
        if (node.L != null) { lh = node.L.Rank; ls = node.L.Size; }
        else { lh = ls = 0; }
        
        if (node.R != null) { rh = node.R.Rank; rs = node.R.Size; }
        else { rh = rs = 0; }

        node.Rank = 1 + (lh > rh ? lh : rh);
        node.Size = 1 + ls + rs;
    }

    public static Node<TValue> Join(Node<TValue> left, Node<TValue> middle, Node<TValue> right) {
        if (H(left)> H(right) + 1)
            return JoinR(left, middle, right);
        if (H(right) > H(left) + 1)
            return JoinL(left, middle, right);
        return Node(left, middle, right);
    }

    // Search along the right spine of tl ...
    private static Node<TValue> JoinR(Node<TValue> tl, Node<TValue> k, Node<TValue> tr) {
        var (l, c) = (tl.L, tl.R);
        if (H(c) <= H(tr) + 1) {
            var t1 = Node(c, k, tr);
            tl = TNodeTraits.Clone(tl);
            tl.R = t1;
            Update(tl);
            if (t1.Rank > H(l) + 1)
                tl = Rotations<TValue, TNodeTraits, AvlTree<TValue, TNodeTraits>>.RotRL(tl);
        }
        else {
            var t1 = JoinR(c, k, tr);
            tl = TNodeTraits.Clone(tl);
            tl.R = t1;
            Update(tl);
            if (t1.Rank > H(l) + 1)
                tl = Rotations<TValue, TNodeTraits, AvlTree<TValue, TNodeTraits>>.RotL(tl);
        }
        return tl;
    }

    // Search along the left spine of tr...
    private static Node<TValue> JoinL(Node<TValue> tl, Node<TValue> k, Node<TValue> tr) {
        var (c, r) = (tr.L, tr.R);
        if (H(c) <= H(tl) + 1) {
            var t1 = Node(tl, k, c);
            tr = TNodeTraits.Clone(tr);
            tr.L = t1;
            Update(tr);
            if (t1.Rank > H(r) + 1)
                tr = Rotations<TValue, TNodeTraits, AvlTree<TValue, TNodeTraits>>.RotLR(tr);
        } else {
            var t1 = JoinL(tl, k, c);
            tr = TNodeTraits.Clone(tr);
            tr.L = t1;
            Update(tr);
            if (t1.Rank > H(r) + 1)
                tr = Rotations<TValue, TNodeTraits, AvlTree<TValue, TNodeTraits>>.RotR(tr);
        }
        return tr;
    }

    static void ITreeTraits<TValue>.ValidateStructure(Node<TValue> root) {
        ValidateHeights(root);

        static int ValidateHeights(Node<TValue> node) {
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
    private static Node<TValue> Node(Node<TValue> l, Node<TValue> m, Node<TValue> r) {
        m = TNodeTraits.Clone(m);
        m.L = l; m.R = r;
        Update(m);
        return m;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int H(Node<TValue> n) => n?.Rank ?? 0;
}
