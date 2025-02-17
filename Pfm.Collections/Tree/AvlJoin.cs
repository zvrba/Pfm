﻿using System;
using System.Runtime.CompilerServices;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Implementation of <see cref="ITreeJoin{TValue}"/> for AVL trees.
/// </summary>
public struct AvlJoin<TValue> : ITreeJoin<TValue>
    where TValue : struct, ITaggedValue<TValue>
{
    // UTILITIES

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int H(JoinableTreeNode<TValue> n) => n?.Rank ?? 0;

    /// <inheritdoc/>
    public static int Nil => 0;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Combine(int left, int middle, int right) => 1 + (left > right ? left : right);

    /// <inheritdoc/>
    public static JoinableTreeNode<TValue> Join(TreeSection<TValue> jd)
    {
        if (H(jd.Left) > H(jd.Right) + 1)
            return JoinR(jd);
        if (H(jd.Right) > H(jd.Left) + 1)
            return JoinL(jd);
        return jd.JoinBalanced<AvlJoin<TValue>>();
    }

    // Search along the right spine of tl ...
    private static JoinableTreeNode<TValue> JoinR(TreeSection<TValue> jd)
    {
        var tl = jd.Left;
        var (l, c) = (tl.Left, tl.Right);
        if (H(c) <= H(jd.Right) + 1) {
            jd.Left = c;
            var t1 = jd.JoinBalanced<AvlJoin<TValue>>();
            tl = tl.Clone(jd.Transient);
            tl.Right = t1;
            tl.Update<AvlJoin<TValue>>();
            if (t1.Rank > H(l) + 1)
                tl = tl.RotLL<TValue, AvlJoin<TValue>>(jd.Transient);
        } else {
            jd.Left = c;
            var t1 = JoinR(jd);
            tl = tl.Clone(jd.Transient);
            tl.Right = t1;
            tl.Update<AvlJoin<TValue>>();
            if (t1.Rank > H(l) + 1)
                tl = tl.RotL<TValue, AvlJoin<TValue>>(jd.Transient);
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
            var t1 = jd.JoinBalanced<AvlJoin<TValue>>();
            tr = tr.Clone(jd.Transient);
            tr.Left = t1;
            tr.Update<AvlJoin<TValue>>();
            if (t1.Rank > H(r) + 1)
                tr = tr.RotRR<TValue, AvlJoin<TValue>>(jd.Transient);
        } else {
            jd.Right = c;
            var t1 = JoinL(jd);
            tr = tr.Clone(jd.Transient);
            tr.Left = t1;
            tr.Update<AvlJoin<TValue>>();
            if (t1.Rank > H(r) + 1)
                tr = tr.RotR<TValue, AvlJoin<TValue>>(jd.Transient);
        }
        return tr;
    }

    /// <inheritdoc/>
    public static void ValidateStructure(JoinableTreeNode<TValue> node) => ValidateHeights(node);

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
