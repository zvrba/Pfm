using System;
using System.Collections;
using System.Collections.Generic;

namespace Pfm.Collections.TreeSet;

// This file implements ICollection / IReadOnlyList interfaces.

public partial class JoinableTreeSet<TTree, TValue> :
    ICollection<TValue>,
    IReadOnlyList<TValue>
    where TTree : struct, IJoinTree<TTree, TValue>, IValueTraits<TValue>, IPersistenceTraits<TValue>
{
    public bool IsReadOnly => false;

    public void Clear() => _Root = null;
    public bool Contains(TValue item) => IJoinTree<TTree, TValue>.Find(_Root, ref item);
    public void Add(TValue item) => TryAdd(ref item);
    public bool Remove(TValue item) => TryRemove(ref item);

    public TValue this[int index] => IJoinTree<TTree, TValue>.Nth(_Root, index);

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
    public bool SetEquals(JoinableTreeSet<TTree, TValue> other)
        => IJoinTree<TTree, TValue>.SetEquals(_Root, other._Root);


    /// <summary>
    /// Sets <c>this</c> to the union of <c>this</c> and <paramref name="other"/>.
    /// The operation is destructive to <paramref name="other"/> if this is a mutable tree.
    /// </summary>
    public void SetUnion(JoinableTreeSet<TTree, TValue> other)
        => _Root = IJoinTree<TTree, TValue>.SetUnion(_Root, other._Root);

    /// <summary>
    /// Sets <c>this</c> to the difference of <c>this</c> and <paramref name="other"/>.
    /// The operation is destructive to <paramref name="other"/> if this is a mutable tree.
    /// </summary>
    public void SetSubtract(JoinableTreeSet<TTree, TValue> other)
        => _Root = IJoinTree<TTree, TValue>.SetDifference(_Root, other._Root);

    /// <summary>
    /// Sets <c>this</c> to the intersection of <c>this</c> and <paramref name="other"/>.
    /// The operation is destructive to <paramref name="other"/> if this is a mutable tree.
    /// </summary>
    public void SetIntersect(JoinableTreeSet<TTree, TValue> other)
        => _Root = IJoinTree<TTree, TValue>.SetIntersection(_Root, other._Root);
}
