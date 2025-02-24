#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Adapts a joinable tree to <see cref="IDictionary{TKey, TValue}"/> and <see cref="IReadOnlyDictionary{TKey, TValue}"/>.
/// </summary>
public class DictionaryTreeAdapter<TKey, TValue, TJoin> :
    CollectionTreeAdapter<KeyValuePair<TKey, TValue>, TJoin>,
    IDictionary<TKey, TValue>,
    IReadOnlyDictionary<TKey, TValue>
    where TJoin : struct, ITreeTraits<KeyValuePair<TKey, TValue>>
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="root">Tree root from an existing tree, or <c>null</c> to initialize an empty set.</param>
    /// <param name="transient">Transient tag to reuse, or 0 to create a new one.</param>
    public DictionaryTreeAdapter(JoinableTreeNode<KeyValuePair<TKey, TValue>>? root = null, ulong transient = 0)
        : base(root, transient) { }

    /// <inheritdoc/>
    public TValue this[TKey key] {
        get => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException($"Key `{key}` was not found in the dictionary.");
        set {
            var kv = new KeyValuePair<TKey, TValue>(key, value);
            var n = Root.Find<KeyValuePair<TKey, TValue>, TJoin>(kv, out var found);
            if (n != null && found == 0) n.Value = kv;
            else Add(kv);
        }
    }

    /// <inheritdoc/>
    public ICollection<TKey> Keys => throw new NotImplementedException();
    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

    /// <inheritdoc/>
    public ICollection<TValue> Values => throw new NotImplementedException();
    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    /// <inheritdoc/>
    public void Add(TKey key, TValue value) {
        if (!Add(new KeyValuePair<TKey, TValue>(key, value)))
            throw new ArgumentException($"Key `{key}` already exists in the dictionary.");
    }

    /// <inheritdoc/>
    public bool ContainsKey(TKey key) => Contains(new KeyValuePair<TKey, TValue>(key, default!));

    /// <inheritdoc/>
    public bool Remove(TKey key) => Remove(new KeyValuePair<TKey, TValue>(key, default!));

    /// <inheritdoc/>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) {
        var n = Root.Find<KeyValuePair<TKey, TValue>, TJoin>(new KeyValuePair<TKey, TValue>(key, default!), out var found);
        if (n != null && found == 0) {
            value = n.Value.Value;
            return true;
        }
        value = default;
        return false;
    }
}
