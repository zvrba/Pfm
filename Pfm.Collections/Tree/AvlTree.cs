using System;
using System.Runtime.CompilerServices;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Provides default implementations of static methods in <see cref="ITagTraits{TTag}"/> that are suitable for AVL tree.
/// </summary>
/// <typeparam name="TTag">Concrete tag type.</typeparam>
public interface IAvlTagTraits<TTag> : ITagTraits<TTag>
    where TTag : struct, ITagTraits<TTag>
{
    static TTag ITagTraits<TTag>.Nil => default;

    static void ITagTraits<TTag>.Combine(TTag left, ref TTag result, TTag right) {
        result.Rank = 1 + (left.Rank > right.Rank ? left.Rank : right.Rank);
        result.Size = 1 + left.Size + right.Size;
    }
}


/// <summary>
/// Specialization of <see cref="JoinableTree{TTag, TValue, TValueTraits}"/> to AVL trees.
/// </summary>
public sealed class AvlTree<TTag, TValue, TValueTraits> : JoinableTree<TTag, TValue, TValueTraits>
    where TTag : struct, IAvlTagTraits<TTag>
    where TValueTraits : struct, IValueTraits<TValue>
{
    /// <inheritdoc/>
    public AvlTree(ulong transient) : base(transient) { }

    // UTILITIES

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int H(JoinableTreeNode<TTag, TValue> n) => n is null ? 0 : n.T.Rank;

    private struct JoinData
    {
        public JoinableTreeNode<TTag, TValue> Middle, Other;
    }

    /// <inheritdoc/>
    public override JoinableTreeNode<TTag, TValue> Join
        (
        JoinableTreeNode<TTag, TValue> left,
        JoinableTreeNode<TTag, TValue> middle,
        JoinableTreeNode<TTag, TValue> right
        )
    {
        if (H(left) > H(right) + 1) {
            var jd = new JoinData { Middle = middle, Other = right, };
            return JoinR(left, ref jd);
        }
        if (H(right) > H(left) + 1) {
            var jd = new JoinData { Middle = middle, Other = left, };
            return JoinL(right, ref jd);
        }
        return this.JoinBalanced(left, middle, right);
    }

    // Search along the right spine of tl ...
    private JoinableTreeNode<TTag, TValue> JoinR(JoinableTreeNode<TTag, TValue> tl, ref JoinData jd)
    {
        var (l, c) = (tl.L, tl.R);
        if (H(c) <= H(jd.Other) + 1) {
            var t1 = this.JoinBalanced(c, jd.Middle, jd.Other);
            tl = tl.Clone<TValueTraits>(Transient);
            tl.R = t1;
            tl.Update();
            if (t1.T.Rank > H(l) + 1)
                tl = this.RotLL(tl);
        } else {
            var t1 = JoinR(c, ref jd);
            tl = tl.Clone<TValueTraits>(Transient);
            tl.R = t1;
            tl.Update();
            if (t1.T.Rank > H(l) + 1)
                tl = this.RotL(tl);
        }
        return tl;
    }

    // Search along the left spine of tr...
    private JoinableTreeNode<TTag, TValue> JoinL(JoinableTreeNode<TTag, TValue> tr, ref JoinData jd)
    {
        var (c, r) = (tr.L, tr.R);
        if (H(c) <= H(jd.Other) + 1) {
            var t1 = this.JoinBalanced(jd.Other, jd.Middle, c);
            tr = tr.Clone<TValueTraits>(Transient);
            tr.L = t1;
            tr.Update();
            if (t1.T.Rank > H(r) + 1)
                tr = this.RotRR(tr);
        } else {
            var t1 = JoinL(c, ref jd);
            tr = tr.Clone<TValueTraits>(Transient);
            tr.L = t1;
            tr.Update();
            if (t1.T.Rank > H(r) + 1)
                tr = this.RotR(tr);
        }
        return tr;
    }


    /// <inheritdoc/>
    public override void ValidateStructure() => ValidateHeights(Root);

    private static int ValidateHeights(JoinableTreeNode<TTag, TValue> node) {
        if (node == null)
            return 0;
        var l = ValidateHeights(node.L);
        var r = ValidateHeights(node.R);
        var h = 1 + (l > r ? l : r);
        var b = r - l;

        if (node.T.Rank != h)
            throw new NotImplementedException();
        if (b < -1 || b > 1)
            throw new NotImplementedException();

        return h;
    }
}
