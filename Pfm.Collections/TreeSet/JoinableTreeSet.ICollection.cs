using System;
using System.Collections;
using System.Collections.Generic;

namespace Podaga.PersistentCollections.TreeSet;

// This file implements ICollection / IReadOnlyList interfaces.

public partial class JoinableTreeSet<TValue, TTreeTraits> :
    ICollection<TValue>,
    IReadOnlyList<TValue>,
    ISet<TValue>
    where TTreeTraits : struct, IValueTraits<TValue>, IBalanceTraits<TTreeTraits, TValue>
{
    public bool IsReadOnly => false;

    public void Clear() => _Root = null;
    public bool Contains(TValue item) => JoinTree<TValue, TTreeTraits>.Find(_Root, ref item);
    void ICollection<TValue>.Add(TValue item) => TryAdd(ref item);
    public bool Remove(TValue item) => TryRemove(ref item);

    public TValue this[int index] => JoinTree<TValue, TTreeTraits>.Nth(_Root, index);

    public void CopyTo(TValue[] array, int arrayIndex) {
        if (array == null)
            throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0 || arrayIndex + Count > array.Length)
            throw new ArgumentException("Insufficient space in the target array.", nameof(arrayIndex));
        foreach (var item in this)
            array[arrayIndex++] = item;
    }

    public IEnumerator<TValue> GetEnumerator() {
        var iterator = new TreeIterator<TValue>(TreeIterator<TValue>.DefaultCapacity);
        if (iterator.First(_Root)) {
            yield return iterator.Top.V;
            while (iterator.Succ())
                yield return iterator.Top.V;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    /// <summary>
    /// Checks whether <paramref name="other"/> contains the same elements (defined by
    /// <see cref="IValueTraits{TValue}.CompareKey(in TValue, in TValue)"/> returning 0)
    /// as <c>this</c>.
    /// </summary>
    public bool SetEquals(JoinableTreeSet<TValue, TTreeTraits> other)
        => JoinTree<TValue, TTreeTraits>.SetEquals(_Root, other._Root);

    /// <summary>
    /// Sets <c>this</c> to the union of <c>this</c> and <paramref name="other"/>.
    /// </summary>
    public void SetUnion(JoinableTreeSet<TValue, TTreeTraits> other)
        => _Root = _Tree.SetUnion(_Root, other._Root);

    /// <summary>
    /// Sets <c>this</c> to the difference of <c>this</c> and <paramref name="other"/>.
    /// </summary>
    public void SetSubtract(JoinableTreeSet<TValue, TTreeTraits> other)
        => _Root = _Tree.SetDifference(_Root, other._Root);

    /// <summary>
    /// Sets <c>this</c> to the intersection of <c>this</c> and <paramref name="other"/>.
    /// </summary>
    public void SetIntersect(JoinableTreeSet<TValue, TTreeTraits> other)
        => _Root = _Tree.SetIntersection(_Root, other._Root);
}
