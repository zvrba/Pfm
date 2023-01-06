using System;
using System.Runtime.CompilerServices;

namespace Pfm.Collections.JoinTree;

/// <summary>
/// <para>
/// Weight-balanced tree as described in the paper.  See also https://github.com/cmuparlay/PAM for the
/// reference code.  Balancing factor has been chosen, and balance conditions rewritten, such that they
/// can be computed exactly for for a couple of million elements.
/// </para>
/// <para>
/// Original paper: J. Nievergelt, E. M. Reingold: Binary search trees of bounded balance.  We use
/// balance factor of 1/4 as that one is most easily computed exactly.
/// </para>
/// </summary>
public struct WBTree<TValue, TNodeTraits> : ITreeTraits<TValue>
    where TNodeTraits : struct, INodeTraits<TValue>
{
    private const float Alpha = 0.25f;  // Alpha
    private const float AlphaC = 1 - Alpha;
    
    public static void Update(Node<TValue> node) {
        var ls = node.L?.Size ?? 0;
        var rs = node.R?.Size ?? 0;
        node.Size = 1 + ls + rs;
    }

    public static Node<TValue> Join(Node<TValue> left, Node<TValue> middle, Node<TValue> right) {
        if (LeftHeavy(S(left), S(right)))
            return JoinR(left, middle, right);
        if (LeftHeavy(S(right), S(left)))
            return JoinL(left, middle, right);
        return Node(left, middle, right);
    }

    // Follow right branch of tl until a node c is reached with a weight like to tr.
    private static Node<TValue> JoinR(Node<TValue> tl, Node<TValue> value, Node<TValue> tr) {
        if (Like(S(tl), S(tr)))             // Base case
            return Node(tl, value, tr);

        var t1 = JoinR(tl.R, value, tr);
        tl = TNodeTraits.Clone(tl);
        tl.R = t1;
        Update(tl);

        if (!Like(S(tl.L), S(t1))) {
            if (IsSingleRotation(S(tl.L), S(t1)))
                tl = Rotations<TValue, TNodeTraits, WBTree<TValue, TNodeTraits>>.RotL(tl);
            else
                tl = Rotations<TValue, TNodeTraits, WBTree<TValue, TNodeTraits>>.RotRL(tl);
        }
        return tl;
    }

    // Follow left branch of tr until a node c is reached with a weight like to tl.
    private static Node<TValue> JoinL(Node<TValue> tl, Node<TValue> value, Node<TValue> tr) {
        if (Like(S(tl), S(tr)))
            return Node(tl, value, tr);

        var t1 = JoinL(tl, value, tr.L);
        tr = TNodeTraits.Clone(tr);
        tr.L = t1;
        Update(tr);

        if (!Like(S(t1), S(tr.R))) {
            if (IsSingleRotation(S(t1), S(tr.R)))
                tr = Rotations<TValue, TNodeTraits, WBTree<TValue, TNodeTraits>>.RotR(tr);
            else
                tr = Rotations<TValue, TNodeTraits, WBTree<TValue, TNodeTraits>>.RotLR(tr);
        }
        return tr;
    }

    // For every T = Node(tl, e, tr) the invariant is Alpha <= W(tl) / W(t) <= 1-Alpha, W(.) being size + 1.
    static void ITreeTraits<TValue>.ValidateStructure(Node<TValue> root) {
        if (root?.Size > 1)  // Single-element tree cannot be balanced.
            ValidateWeights(root);

        static void ValidateWeights(Node<TValue> node) {
            if (node == null)
                return;
            var r = (float)(S(node.L) + 1) / (S(node.L) + S(node.R) + 2);
            if (r < Alpha || r > AlphaC)
                throw new NotImplementedException();
            ValidateWeights(node.L);
            ValidateWeights(node.R);
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
    private static int S(Node<TValue> n) => n?.Size ?? 0;

    // TODO: XXX: Arithmetic below could overflow for large tree sizes.  Could be rewritten with long types instead.

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
