using System;

namespace Podaga.PersistentCollections.TreeSet;

public partial class JoinTree<TValue, TTreeTraits>
    where TTreeTraits : struct, IValueTraits<TValue>, IBalanceTraits<TTreeTraits, TValue>
{

    /// <summary>
    /// Set equality: the sets rooted at <paramref name="a"/> and <paramref name="b"/> must contain "same" elements,
    /// where element equality is defined by the traits' comparer.  Neither argument is allowed to be null.
    /// </summary>
    /// <returns>
    /// True if both trees contain the same elements, false otherwise.
    /// </returns>
    public static bool SetEquals(TreeNode<TValue> a, TreeNode<TValue> b) {
        if (a == null || b == null)
            return (a == null) == (b == null);
        if (a.Size != b.Size)
            return false;

        var ai = new TreeIterator<TValue>(TreeIterator<TValue>.DefaultCapacity);
        var hasA = ai.First(a);

        var bi = new TreeIterator<TValue>(TreeIterator<TValue>.DefaultCapacity);
        var hasB = bi.First(b);

    loop:
        if (!hasA || !hasB)
            return !hasA && !hasB;
        if (TTreeTraits.CompareKey(ai.Top.V, bi.Top.V) != 0)
            return false;
        hasA = ai.Succ();
        hasB = bi.Succ();
        goto loop;
    }

    /// <summary>
    /// Union of sets in trees rooted at <paramref name="t1"/> and <paramref name="t2"/>.
    /// </summary>
    /// <returns>
    /// The root of the tree that is the union of <paramref name="t1"/> and <paramref name="t2"/>.
    /// </returns>
    public TreeNode<TValue> SetUnion(TreeNode<TValue> t1, TreeNode<TValue> t2) {
        if (t1 == null)
            return t2;
        if (t2 == null)
            return t1;

        var s = Split(t1, t2.V);
        var l = SetUnion(s.L, t2.L);
        var r = SetUnion(s.R, t2.R);
        if (s.M != null) {
            t1 = s.M.Clone<TTreeTraits>(transient);
            TTreeTraits.CombineValues(t1.V, ref t1.V, t2.V);
        }
        else {
            t1 = t2;
        }
        return TTreeTraits.Join(this, l, t1, r);
    }

    /// <summary>
    /// Intersection of sets in trees rooted at <paramref name="t1"/> and <paramref name="t2"/>.
    /// </summary>
    /// <returns>
    /// The root of the tree that is the union of <paramref name="t1"/> and <paramref name="t2"/>.
    /// </returns>
    public TreeNode<TValue> SetIntersection(TreeNode<TValue> t1, TreeNode<TValue> t2) {
        if (t1 == null || t2 == null)
            return null;

        var s = Split(t1, t2.V);
        var l = SetIntersection(s.L, t2.L);
        var r = SetIntersection(s.R, t2.R);
        if (s.M != null) {
            t1 = s.M.Clone<TTreeTraits>(transient);
            TTreeTraits.CombineValues(t1.V, ref t1.V, t2.V);
            return TTreeTraits.Join(this, l, t1, r);
        }
        return Join2(l, r);
    }

    /// <summary>
    /// Difference of sets in trees rooted at <paramref name="t1"/> and <paramref name="t2"/>.
    /// </summary>
    /// <returns>
    /// The root of the tree that is the union of <paramref name="t1"/> and <paramref name="t2"/>.
    /// </returns>
    public TreeNode<TValue> SetDifference(TreeNode<TValue> t1, TreeNode<TValue> t2) {
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
