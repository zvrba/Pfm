#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Podaga.PersistentCollections.Tree;

public interface ICollectionValueHolder<TSelf, TValue> : ITaggedValue<TSelf>
    where TSelf : struct, ICollectionValueHolder<TSelf, TValue>
{
    abstract static TSelf Create(TValue value);
    TValue Value { get; set; }
}

/// <summary>
/// Adapts a joinable tree to <see cref="ICollection{T}"/> and <see cref="IReadOnlyList{T}"/> interfaces.
/// </summary>
public class CollectionTreeAdapter<TValue, TJoin, THolder> : ICollection<TValue>, IReadOnlyList<TValue>
    where TJoin : struct, ITreeJoin<THolder>
    where THolder : struct, ICollectionValueHolder<THolder, TValue>
{
    private static ulong NextTransient = 0;

    /// <summary>
    /// Initializes an empty collection.
    /// </summary>
    public CollectionTreeAdapter() => this.Transient = Interlocked.Increment(ref NextTransient);

    /// <summary>
    /// Forks the collection.
    /// </summary>
    /// <returns>
    /// A forked instance that contains the same elements as <c>this</c>.
    /// </returns>
    public CollectionTreeAdapter<TValue, TJoin, THolder> Fork() => new() { Root = Root };

    /// <summary>
    /// Transient value for this collection.
    /// </summary>
    public ulong Transient { get; }

    /// <summary>
    /// Root of the tree that represents this collection.
    /// </summary>
    public JoinableTreeNode<THolder>? Root { get; private set; }

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public int Count => Root?.Size ?? 0;

    /// <inheritdoc/>
    public TValue this[int index] => Root.Nth(index).Value;

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
        var state = new ModifyState<THolder> { Value = THolder.Create(item), Transient = Transient };
        Root = Root.Insert<THolder, TJoin>(ref state);
        return state.Found == null;
    }

    /// <inheritdoc/>
    public void Clear() => Root = null;

    /// <inheritdoc/>
    public bool Contains(TValue item) => Root.Find(THolder.Create(item), out var found) != null && found == 0;

    /// <inheritdoc/>
    public bool Remove(TValue item) {
        var state = new ModifyState<THolder> { Value = THolder.Create(item), Transient = Transient };
        var root = Root.Delete<THolder, TJoin>(ref state);
        if (state.Found == null)
            return false;
        Root = root;
        return true;
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
        var it = TreeIterator<THolder>.New();
        if (Root is null)
            yield break;
        if (it.First(Root)) {
            do {
                yield return it.Top.Value.Value;
            } while (it.Succ());
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
