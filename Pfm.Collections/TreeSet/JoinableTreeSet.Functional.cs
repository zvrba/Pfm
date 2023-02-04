using System;

namespace Pfm.Collections.TreeSet;

/// <summary>
/// Functional operations over sets: non-destructive to either argument.
/// </summary>
public static class JoinableTreeSet
{
    public static bool
    SetEquals<TValue, TTreeTraits>(JoinableTreeSet<TValue, TTreeTraits> left, JoinableTreeSet<TValue, TTreeTraits> right)
    where TTreeTraits : struct, IValueTraits<TValue>, IBalanceTraits<TTreeTraits, TValue>
    {
        return JoinTree<TValue, TTreeTraits>.SetEquals(left._Root, right._Root);
    }

    public static JoinableTreeSet<TValue, TTreeTraits>
    SetUnion<TValue, TTreeTraits>(JoinableTreeSet<TValue, TTreeTraits> left, JoinableTreeSet<TValue, TTreeTraits> right)
    where TTreeTraits : struct, IValueTraits<TValue>, IBalanceTraits<TTreeTraits, TValue>
    {
        var ret = new JoinableTreeSet<TValue, TTreeTraits>();
        ret._Root = ret._Tree.SetUnion(left._Root, right._Root);
        return ret;
    }

    public static JoinableTreeSet<TValue, TTreeTraits>
    SetIntersection<TValue, TTreeTraits>(JoinableTreeSet<TValue, TTreeTraits> left, JoinableTreeSet<TValue, TTreeTraits> right)
    where TTreeTraits : struct, IValueTraits<TValue>, IBalanceTraits<TTreeTraits, TValue> {
        var ret = new JoinableTreeSet<TValue, TTreeTraits>();
        ret._Root = ret._Tree.SetIntersection(left._Root, right._Root);
        return ret;
    }

    public static JoinableTreeSet<TValue, TTreeTraits>
    SetDifference<TValue, TTreeTraits>(JoinableTreeSet<TValue, TTreeTraits> left, JoinableTreeSet<TValue, TTreeTraits> right)
    where TTreeTraits : struct, IValueTraits<TValue>, IBalanceTraits<TTreeTraits, TValue> {
        var ret = new JoinableTreeSet<TValue, TTreeTraits>();
        ret._Root = ret._Tree.SetDifference(left._Root, right._Root);
        return ret;
    }
}
