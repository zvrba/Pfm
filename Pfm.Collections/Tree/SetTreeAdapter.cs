#if false
#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Adapts a joinable tree to <see cref="ISet{T}"/>.
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="THolder"></typeparam>
public class SetTreeAdapter<TValue, THolder> : CollectionTreeAdapter<TValue, THolder>,
    IAdaptedTree<THolder, TValue>,
    ISet<TValue>
    where THolder : struct, ITaggedValueHolder<THolder, TValue>, ITreeJoin<THolder>
{

    /// <summary>
    /// Initializes an collection.
    /// </summary>
    /// <param name="root">Tree root from an existing tree, or <c>null</c> to initialize an empty collection.</param>
    /// <param name="transient">Transient tag to reuse, or 0 to create a new one.</param>
    public SetTreeAdapter(JoinableTreeNode<THolder>? root = null, ulong transient = 0) : base(root, transient) { }

    /// <inheritdoc/>
    public ulong Transient => Transient;
    private ulong _Transient;

    /// <inheritdoc/>
    public JoinableTreeNode<THolder>? Root { get; private set; }

    /// <inheritdoc/>
    public int Count => Root?.Size ?? 0;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    public bool Add(TValue item) {
        throw new NotImplementedException();
    }

    public void ExceptWith(IEnumerable<TValue> other) {
        throw new NotImplementedException();
    }

    public void IntersectWith(IEnumerable<TValue> other) {
        throw new NotImplementedException();
    }

    public bool IsProperSubsetOf(IEnumerable<TValue> other) {
        throw new NotImplementedException();
    }

    public bool IsProperSupersetOf(IEnumerable<TValue> other) {
        throw new NotImplementedException();
    }

    public bool IsSubsetOf(IEnumerable<TValue> other) {
        throw new NotImplementedException();
    }

    public bool IsSupersetOf(IEnumerable<TValue> other) {
        throw new NotImplementedException();
    }

    public bool Overlaps(IEnumerable<TValue> other) {
        throw new NotImplementedException();
    }

    public bool SetEquals(IEnumerable<TValue> other) {
        throw new NotImplementedException();
    }

    public void SymmetricExceptWith(IEnumerable<TValue> other) {
        throw new NotImplementedException();
    }

    public void UnionWith(IEnumerable<TValue> other) {
        throw new NotImplementedException();
    }

    void ICollection<TValue>.Add(TValue item) {
        throw new NotImplementedException();
    }

    public void Clear() {
        throw new NotImplementedException();
    }

    public bool Contains(TValue item) {
        throw new NotImplementedException();
    }

    public void CopyTo(TValue[] array, int arrayIndex) {
        throw new NotImplementedException();
    }

    public bool Remove(TValue item) {
        throw new NotImplementedException();
    }

    public IEnumerator<TValue> GetEnumerator() {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}
#endif