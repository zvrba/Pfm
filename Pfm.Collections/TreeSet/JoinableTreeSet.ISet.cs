using System;
using System.Collections.Generic;

namespace Pfm.Collections.TreeSet;

public partial class JoinableTreeSet<TValue, TTreeTraits> :
    ICollection<TValue>,
    IReadOnlyList<TValue>,
    ISet<TValue>
    where TTreeTraits : struct, IValueTraits<TValue>, IBalanceTraits<TTreeTraits, TValue>
{
    private static ISet<TValue> AsISet(IEnumerable<TValue> other) {
        if (other is ISet<TValue> iset)
            return iset;
        throw new NotSupportedException("The enumerable is not an ISet<T>.");
    }

    public bool Add(TValue item) => TryAdd(ref item);
    
    public void UnionWith(IEnumerable<TValue> other) {
        if (other is JoinableTreeSet<TValue, TTreeTraits> jts) {
            SetUnion(jts);
            return;
        }
        foreach (var item in other)
            Add(item);
    }

    public void IntersectWith(IEnumerable<TValue> other) {
        if (other is JoinableTreeSet<TValue, TTreeTraits> jts)  {
            SetIntersect(jts);
            return;
        }

        var iset = AsISet(other);
        if (iset.Count == 0) {
            _Root = null;
            return;
        }

        var it = GetIterator();
        if (!it.First(null))
            return;

        TValue current;
        bool hasNext;

    loop:
        if (iset.Contains(it.Top.V)) {
            if (it.Succ())
                goto loop;
        }

        current = it.Top.V;
        hasNext = it.Succ();
        Remove(current);
        
        // Must reset the iterator as the removed node might be in the middle of the path.
        if (hasNext) {
            it.Find<TTreeTraits>(_Root, it.Top.V);
            goto loop;
        }
    }

    public void ExceptWith(IEnumerable<TValue> other) {
        if (other is JoinableTreeSet<TValue, TTreeTraits> jts) {
            SetSubtract(jts);
            return;
        }
        foreach (var item in other)
            Remove(item);
    }
    
    public void SymmetricExceptWith(IEnumerable<TValue> other) => throw new NotImplementedException();

    public bool SetEquals(IEnumerable<TValue> other) {
        if (other is JoinableTreeSet<TValue, TTreeTraits> jts)
            return SetEquals(jts);
        
        var iset = AsISet(other);
        if (Count != iset.Count)
            return false;

        foreach (var item in iset)
            if (!Contains(item))
                return false;
        return true;
    }

    public bool Overlaps(IEnumerable<TValue> other) {
        if (Count > 0) {
            foreach (var item in other)
                if (Contains(item))
                    return true;
        }
        return false;
    }

    // Empty set is a subset of any other set.
    public bool IsProperSubsetOf(IEnumerable<TValue> other) {
        var iset = AsISet(other);
        var count = SubsetCount(iset);
        return count >= 0 && count < iset.Count;
    }
    
    public bool IsSubsetOf(IEnumerable<TValue> other) {
        var iset = AsISet(other);
        var count = SubsetCount(iset);
        return count >= 0 && count <= iset.Count;
    }
    
    // Counting works only if other is a set (unique elements).
    private int SubsetCount(ISet<TValue> other) {
        var count = 0;
        foreach (var item in this) {
            if (!other.Contains(item))
                return -1;
            ++count;
        }
        return count;
    }

    // Empty set is NOT a superset of any set.
    public bool IsProperSupersetOf(IEnumerable<TValue> other) {
        var iset = AsISet(other);
        var count = SupersetCount(iset);
        return count > iset.Count;
    }

    public bool IsSupersetOf(IEnumerable<TValue> other) {
        var iset = AsISet(other);
        var count = SupersetCount(iset);
        return count >= iset.Count;
    }

    private int SupersetCount(ISet<TValue> other) {
        var count = 0;
        foreach (var item in other) {
            if (!Contains(item))
                return -1;
            ++count;
        }
        return count;
    }
}
