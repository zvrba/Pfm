#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Adapts a joinable tree to <see cref="ICollection{T}"/>.
/// </summary>
/// <typeparam name="TValue">Tree element type.</typeparam>
/// <typeparam name="TJoin">Tree join strategy.</typeparam>
public class CollectionTreeAdapter<TValue, TJoin> :
    IAdaptedTree<TValue, TJoin>,
    ICollection<TValue>
    where TJoin : struct, ITreeTraits<TValue>
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="root">Tree root from an existing tree, or <c>null</c> to initialize an empty collection.</param>
    /// <param name="transient">Transient tag to reuse, or 0 to create a new one.</param>
    public CollectionTreeAdapter(JoinableTreeNode<TValue>? root = null, ulong transient = 0) {
        Root = root;
        _Transient = TransientSource.NewTransient(transient);
    }

    /// <summary>
    /// Forks the collection.  Ensures that modifications to <c>this</c> and the forked version are invisible to each other.
    /// </summary>
    /// <param name="immediate">
    /// If true, all nodes are copied into the new instance immediately.  Otherwise, nodes are copied only upon modification.
    /// </param>
    /// <returns>
    /// A forked instance that contains the same elements as <c>this</c>.
    /// </returns>
    public CollectionTreeAdapter<TValue, TJoin> Fork(bool immediate) {
        // Original's transient change so that changes in the original do not affect the fork.
        _Transient = TransientSource.NewTransient();

        // Set up the fork.
        var t = TransientSource.NewTransient();
        var f = Root;
        if (immediate && f != null)
            f = f.Copy<TValue, TJoin>(t);

        return new(f, t);
    }

    /// <inheritdoc/>
    public ulong Transient => _Transient;
    private ulong _Transient;

    /// <inheritdoc/>
    public JoinableTreeNode<TValue>? Root { get; protected set; }

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public int Count => Root?.Size ?? 0;


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
        var state = new ModifyState<TValue> { Value = item, Transient = Transient };
        Root = Root.Insert<TValue, TJoin>(ref state);
        return state.Found == null;
    }

    /// <inheritdoc/>
    public void Clear() => Root = null;

    /// <inheritdoc/>
    public bool Contains(TValue item) => Root.Find<TValue, TJoin>(item, out var found) != null && found == 0;

    /// <inheritdoc/>
    public bool Remove(TValue item) {
        var state = new ModifyState<TValue> { Value = item, Transient = Transient };
        var root = Root.Delete<TValue, TJoin>(ref state);
        if (state.Found == null)
            return false;
        Root = root;
        return true;
    }

    internal void CheckCopyLength(Array array, int arrayIndex) {
        ArgumentNullException.ThrowIfNull(array);
        if (arrayIndex < 0 || arrayIndex >= array.Length)
            throw new IndexOutOfRangeException();
        if (arrayIndex + Count > array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
    }

    /// <inheritdoc/>
    public void CopyTo(TValue[] array, int arrayIndex) {
        CheckCopyLength(array, arrayIndex);
        foreach (var item in this)
            array[arrayIndex++] = item;
    }

    /// <inheritdoc/>
    public IEnumerator<TValue> GetEnumerator() {
        var it = TreeIterator<TValue>.New();
        if (Root is null)
            yield break;
        if (it.First(Root)) {
            do {
                yield return it.Top.Value;
            } while (it.Succ());
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
