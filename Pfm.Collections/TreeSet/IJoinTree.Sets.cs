using System;

namespace Pfm.Collections.TreeSet;

public partial interface IJoinTree<TSelf, TValue>
    where TSelf : struct, IJoinTree<TSelf, TValue>, IValueTraits<TValue>, IPersistenceTraits<TValue>
{

    /// <summary>
    /// Set equality: the sets rooted at <paramref name="a"/> and <paramref name="b"/> must contain "same" elements,
    /// where element equality is defined by the traits' comparer.
    /// </summary>
    /// <returns>
    /// True if both trees contain the same elements, false otherwise.
    /// </returns>
    public static bool SetEquals(TreeNode<TValue> a, TreeNode<TValue> b) {
        if (a.Size != b.Size)
            return false;

        var ai = new TreeIterator<TValue>(TreeIterator<TValue>.DefaultCapacity);
        var hasA = ai.First(a);

        var bi = new TreeIterator<TValue>(TreeIterator<TValue>.DefaultCapacity);
        var hasB = bi.First(b);

    loop:
        if (!hasA || !hasB)
            return !hasA && !hasB;
        if (TSelf.CompareKey(ai.Top.V, bi.Top.V) != 0)
            return false;
        hasA = ai.Succ();
        hasB = bi.Succ();
        goto loop;
    }

    /// <summary>
    /// Union of sets in trees rooted at <paramref name="t1"/> and <paramref name="t2"/>.
    /// Without persistence, this operation is destructive to BOTH arguments.
    /// </summary>
    /// <returns>
    /// The root of the tree that is the union of <paramref name="t1"/> and <paramref name="t2"/>.
    /// </returns>
    public static TreeNode<TValue> SetUnion(TreeNode<TValue> t1, TreeNode<TValue> t2) {
        if (t1 == null)
            return t2;
        if (t2 == null)
            return t1;

        var s = Split(t1, t2.V);
        var l = SetUnion(s.L, t2.L);
        var r = SetUnion(s.R, t2.R);
        if (s.M != null) {
            t2 = TSelf.Clone(t2);
            TSelf.CombineValues(s.M.V, ref t2.V, t2.V);
        }
        return TSelf.Join(l, t2, r);
    }

    /// <summary>
    /// Intersection of sets in trees rooted at <paramref name="t1"/> and <paramref name="t2"/>.
    /// Without persistence, this operation is destructive to BOTH arguments.
    /// </summary>
    /// <returns>
    /// The root of the tree that is the union of <paramref name="t1"/> and <paramref name="t2"/>.
    /// </returns>
    public static TreeNode<TValue> SetIntersection(TreeNode<TValue> t1, TreeNode<TValue> t2) {
        if (t1 == null || t2 == null)
            return null;

        var s = Split(t1, t2.V);
        var l = SetIntersection(s.L, t2.L);
        var r = SetIntersection(s.R, t2.R);
        if (s.M != null) {
            t2 = TSelf.Clone(t2);
            TSelf.CombineValues(s.M.V, ref t2.V, t2.V);
            return TSelf.Join(l, t2, r);
        }
        return Join2(l, r);
    }

    /// <summary>
    /// Difference of sets in trees rooted at <paramref name="t1"/> and <paramref name="t2"/>.
    /// Without persistence, this operation is destructive to BOTH arguments.
    /// </summary>
    /// <returns>
    /// The root of the tree that is the union of <paramref name="t1"/> and <paramref name="t2"/>.
    /// </returns>
    public static TreeNode<TValue> SetDifference(TreeNode<TValue> t1, TreeNode<TValue> t2) {
        if (t1 == null)
            return null;
        if (t2 == null)
            return t1;

        var s = Split(t1, t2.V);
        var l = SetDifference(s.L, t2.L);
        var r = SetDifference(s.R, t2.R);
        return Join2(l, r);
    }
}
