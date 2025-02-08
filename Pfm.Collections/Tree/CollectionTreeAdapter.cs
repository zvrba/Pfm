using System;
using System.Collections;
using System.Collections.Generic;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Adapts an instance of <see cref="JoinableTree{TTag, TValue, TValueTraits}"/> to <see cref="ICollection{T}"/> and
/// <see cref="IReadOnlyList{T}"/> interfaces.
/// </summary>
public class CollectionTreeAdapter<TTag, TValue, TValueTraits> : ICollection<TValue>, IReadOnlyList<TValue>
    where TTag : struct, ITagTraits<TTag>
    where TValueTraits : struct, IValueTraits<TValue>
{
    private readonly JoinableTree<TTag, TValue, TValueTraits> tree;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="tree">Tree instance to adapt.</param>
    public CollectionTreeAdapter(JoinableTree<TTag, TValue, TValueTraits> tree) => this.tree = tree;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public int Count => tree.Root?.T.Size ?? 0;

    /// <inheritdoc/>
    public TValue this[int index] => tree.Nth(index);

    /// <inheritdoc/>
    void ICollection<TValue>.Add(TValue item) => Add(item);

    /// <summary>
    /// Adds an item to <c>this</c>.
    /// </summary>
    /// <param name="item">Item to add.</param>
    /// <returns>
    /// True if the item was added, false if it already exists in this collection.
    /// </returns>
    public bool Add(TValue item) {
        var state = new ValueAlgorithms.SearchState<TTag, TValue, TValueTraits> { Value = item };
        return tree.Insert(ref state);
    }

    /// <inheritdoc/>
    public void Clear() => tree.Root = null;

    /// <inheritdoc/>
    public bool Contains(TValue item) {
        var state = new ValueAlgorithms.SearchState<TTag, TValue, TValueTraits> { Value = item };
        return tree.Find(ref state);
    }

    /// <inheritdoc/>
    public bool Remove(TValue item) {
        var state = new ValueAlgorithms.SearchState<TTag, TValue, TValueTraits> { Value = item };
        return tree.Delete(ref state);
    }

    /// <inheritdoc/>
    public void CopyTo(TValue[] array, int arrayIndex) {
        if (arrayIndex + Count > array.Length)
            throw new ArgumentException("The destination array is too short.");
        foreach (var item in this)
            array[arrayIndex++] = item;
    }

    /// <inheritdoc/>
    public IEnumerator<TValue> GetEnumerator() {
        var it = TreeIterator<TTag, TValue>.New();
        if (tree.Root is null)
            yield break;
        if (it.First(tree.Root)) {
            do {
                yield return it.Top.V;
            } while (it.Succ());
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
