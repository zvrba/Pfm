using System;
using System.Runtime.CompilerServices;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Provides default implementations of static methods in <see cref="ITagTraits{TTag}"/> that are suitable for AVL tree.
/// </summary>
/// <typeparam name="TTag">Concrete tag type.</typeparam>
public interface IWBTagTraits<TTag> : ITagTraits<TTag>
    where TTag : struct, ITagTraits<TTag>
{
    static TTag ITagTraits<TTag>.Nil => default;

    static void ITagTraits<TTag>.Combine(TTag left, ref TTag result, TTag right) {
        result.Size = 1 + left.Size + right.Size;
    }
}

/// <summary>
/// Specialization of <see cref="JoinableTree{TTag, TValue, TValueTraits}"/> to weight-balanced trees.
/// The balance factor is hard-coded to 1/4.
/// </summary>
public sealed class JoinableWBTree<TTag, TValue, TValueTraits> : JoinableTree<TTag, TValue, TValueTraits>
    where TTag : struct, IWBTagTraits<TTag>
    where TValueTraits : struct, IValueTraits<TValue>

{
    private const float Alpha = 0.25f;  // Alpha
    private const float AlphaC = 1 - Alpha;

    /// <inheritdoc/>
    public JoinableWBTree(ulong transient = 0) : base(transient) { }

    // UTILITIES.  TODO! FIX! The calculations below can overflow when sizes exceed > 2^26 elements.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int S(JoinableTreeNode<TTag, TValue> n) => n?.T.Size ?? 0;

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

    private struct JoinData
    {
        public JoinableTreeNode<TTag, TValue> Middle;
        public JoinableTreeNode<TTag, TValue> Other;
    }

    /// <inheritdoc/>
    public override JoinableTree<TTag, TValue, TValueTraits> Fork() {
        _Transient = NewTransient();
        return new JoinableWBTree<TTag, TValue, TValueTraits> { Root = Root };
    }

    /// <inheritdoc/>
    public override JoinableTreeNode<TTag, TValue> Join
        (
        JoinableTreeNode<TTag, TValue> left,
        JoinableTreeNode<TTag, TValue> middle,
        JoinableTreeNode<TTag, TValue> right
        )
    {
        if (LeftHeavy(S(left), S(right))) {
            var jd = new JoinData { Middle = middle, Other = right, };
            return JoinR(left, ref jd);
        }
        if (LeftHeavy(S(right), S(left))) {
            var jd = new JoinData { Middle = middle, Other = left, };
            return JoinL(right, ref jd);
        }
        return this.JoinBalanced(left, middle, right);
    }

    private JoinableTreeNode<TTag, TValue> JoinR(JoinableTreeNode<TTag, TValue> tl, ref JoinData jd) {
        if (Like(S(tl), S(jd.Other)))             // Base case
            return this.JoinBalanced(tl, jd.Middle, jd.Other);

        var t1 = JoinR(tl.R, ref jd);
        tl = tl.Clone<TValueTraits>(Transient);
        tl.R = t1;
        tl.Update();

        if (!Like(S(tl.L), S(t1))) {
            if (IsSingleRotation(S(tl.L), S(t1))) tl = this.RotL(tl);
            else tl = this.RotLL(tl);
        }
        return tl;
    }

    // Follow left branch of tr until a TreeNode c is reached with a weight like to tl.
    private JoinableTreeNode<TTag, TValue> JoinL(JoinableTreeNode<TTag, TValue> tr, ref JoinData jd) {
        if (Like(S(jd.Other), S(tr)))
            return this.JoinBalanced(jd.Other, jd.Middle, tr);

        var t1 = JoinL(tr.L, ref jd);
        tr = tr.Clone<TValueTraits>(Transient);
        tr.L = t1;
        tr.Update();

        if (!Like(S(t1), S(tr.R))) {
            if (IsSingleRotation(S(t1), S(tr.R))) tr = this.RotR(tr);
            else tr = this.RotRR(tr);
        }
        return tr;
    }

    /// <inheritdoc/>
    public override void ValidateStructure() {
        if (Root?.T.Size > 1)  // Single-element tree cannot be balanced.
            ValidateWeights(Root);

        static void ValidateWeights(JoinableTreeNode<TTag, TValue> TreeNode) {
            if (TreeNode == null)
                return;
            var r = (float)(S(TreeNode.L) + 1) / (S(TreeNode.L) + S(TreeNode.R) + 2);
            if (r < Alpha || r > AlphaC)
                throw new NotImplementedException();
            ValidateWeights(TreeNode.L);
            ValidateWeights(TreeNode.R);
        }
    }
}
