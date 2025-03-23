#nullable enable
using System;
using System.Collections.Generic;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Adapts a joinable tree to <see cref="IReadOnlyList{T}"/>.  The collection is nevertheless modifiable
/// through the inherited <see cref="ICollection{T}"/> methods.
/// </summary>
/// <typeparam name="TValue">Tree element type.</typeparam>
/// <typeparam name="TJoin">Tree join strategy.</typeparam>
public class ReadOnlyListTreeAdapter<TValue, TJoin> : CollectionTreeAdapter<TValue, TJoin>,
    IReadOnlyList<TValue>
    where TJoin : struct, ITreeTraits<TValue>

{
    /// <summary>
    /// Initializes an collection.
    /// </summary>
    /// <param name="root">Tree root from an existing tree, or <c>null</c> to initialize an empty collection.</param>
    /// <param name="transient">Transient tag to reuse, or 0 to create a new one.</param>
    public ReadOnlyListTreeAdapter(JoinableTreeNode<TValue>? root = null, ulong transient = 0) : base(root, transient) { }

    /// <inheritdoc/>
    public TValue this[int index] => Root.Nth(index);
}
