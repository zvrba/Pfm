using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Pfm.Collections.JoinTree;

/// <summary>
/// Common implementation of mutable or immutable dictionaries.  Mutability (<see cref="IsPersistent"/>) is determined
/// by how <typeparamref name="TNodeTraits"/> implement
/// <see cref="INodeTraits{TValue}.Clone(Node{TValue})"/>.  In any case, the implementation gives an illusion of
/// a mutable data structure.  To preserve previous versions, use <see cref="Copy(bool)"/> method which default
/// implementation is extremely efficient for immutable dictionaries (single allocation).
/// </summary>
/// <typeparam name="K">Key type.</typeparam>
/// <typeparam name="V">Value type.</typeparam>
/// <typeparam name="TNodeTraits">
/// Node traits determine how key/value pairs are compared, merged, cloned and whether the tree is persistent.
/// </typeparam>
public class MergeableDictionary<K, V, TNodeTraits> :
    ICloneable,
    ISet<KeyValuePair<K, V>>,
    IReadOnlyList<KeyValuePair<K, V>>,
    IDictionary<K, V>
    where TNodeTraits : struct, INodeTraits<KeyValuePair<K, V>>
{
    private JoinTree<KeyValuePair<K, V>, TNodeTraits, AvlTree<KeyValuePair<K, V>, TNodeTraits>> tree;

    public MergeableDictionary() => tree = default;
    private MergeableDictionary(JoinTree<KeyValuePair<K, V>, TNodeTraits, AvlTree<KeyValuePair<K, V>, TNodeTraits>> other)
        => tree = other;

    /// <summary>
    /// If true, the collection is thread-safe without external locking.  This instance will nevertheless
    /// give an illusion of a mutable data structure by replacing the current version with the new.  To
    /// preserve previous versions, use <see cref="Copy(bool)"/>.
    /// </summary>
    public bool IsPersistent => TNodeTraits.IsPersistent;

    /// <summary>
    /// Number of elements in this dictionary.
    /// </summary>
    public int Count => tree.Count;

    /// <summary>
    /// Always mutable.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Creates a deep copy of elements stored in <c>this</c>.
    /// </summary>
    /// <param name="force">
    /// If true, and the dictionary is persistent, it will force creation of a deep copy instead of 
    /// reusing the same storage.  This may be desirable if <typeparamref name="TNodeTraits"/> also
    /// clones the stored value.
    /// </param>
    /// <returns>
    /// A new instance containing the same elements as <c>this</c>.
    /// </returns>
    public MergeableDictionary<K, V, TNodeTraits> Copy(bool force = false) =>
        IsPersistent && !force ? new(tree) : new(tree.Copy(force));

    /// <summary>
    /// Always makes a deep copy of the dictionary, regardless of persistence.
    /// </summary>
    /// <returns>
    /// A new, deeply-copied instance.  Whether the values are cloned is determined by the implementation
    /// of <typeparamref name="TNodeTraits"/>.
    /// </returns>
    public object Clone() => Copy(true);

    /// <summary>
    /// Gets an iterator for two-way traversal of the dictionary in key order.
    /// </summary>
    /// <returns>
    /// A new iterator instance.
    /// </returns>
    public Iterator<KeyValuePair<K, V>> GetIterator() => tree.GetIterator();

    /// <summary>
    /// Returns the n'th element in the sorted order.
    /// </summary>
    /// <param name="index">
    /// Element index to fetch; smallest is at index 0.
    /// </param>
    /// <returns>
    /// The found key-value pair.
    /// </returns>
    /// <exception cref="IndexOutOfRangeException">When index is out of range <c>[0, Count)</c>.</exception>
    public KeyValuePair<K, V> Nth(int index) => tree.Nth(index);

    // Additional set operations: the algorithms are more efficient than those taking IEnumerable.
    // NB! They are destructive to "other" argument if the tree is not persistent.

    public void UnionWith(MergeableDictionary<K, V, TNodeTraits> other) =>
        tree = JoinTree<KeyValuePair<K, V>, TNodeTraits, AvlTree<KeyValuePair<K, V>, TNodeTraits>>.SetUnion(tree, other.tree);

    public void IntersectWith(MergeableDictionary<K, V, TNodeTraits> other) =>
        tree = JoinTree<KeyValuePair<K, V>, TNodeTraits, AvlTree<KeyValuePair<K, V>, TNodeTraits>>.SetIntersection(tree, other.tree);

    public void ExceptWith(MergeableDictionary<K, V, TNodeTraits> other) =>
        tree = JoinTree<KeyValuePair<K, V>, TNodeTraits, AvlTree<KeyValuePair<K, V>, TNodeTraits>>.SetDifference(tree, other.tree);



    /// <summary>
    /// Explicit implementation, would clash with ordinary indexer for <c>int</c> keys.
    /// </summary>
    /// <seealso cref="Nth(int)"/>
    KeyValuePair<K, V> IReadOnlyList<KeyValuePair<K, V>>.this[int index] => tree.Nth(index);

    bool ISet<KeyValuePair<K, V>>.Add(KeyValuePair<K, V> item) => throw new NotImplementedException();
    void ISet<KeyValuePair<K, V>>.ExceptWith(IEnumerable<KeyValuePair<K, V>> other) => throw new NotImplementedException();
    void ISet<KeyValuePair<K, V>>.IntersectWith(IEnumerable<KeyValuePair<K, V>> other) => throw new NotImplementedException();
    bool ISet<KeyValuePair<K, V>>.IsProperSubsetOf(IEnumerable<KeyValuePair<K, V>> other) => throw new NotImplementedException();
    bool ISet<KeyValuePair<K, V>>.IsProperSupersetOf(IEnumerable<KeyValuePair<K, V>> other) => throw new NotImplementedException();
    bool ISet<KeyValuePair<K, V>>.IsSubsetOf(IEnumerable<KeyValuePair<K, V>> other) => throw new NotImplementedException();
    bool ISet<KeyValuePair<K, V>>.IsSupersetOf(IEnumerable<KeyValuePair<K, V>> other) => throw new NotImplementedException();
    bool ISet<KeyValuePair<K, V>>.Overlaps(IEnumerable<KeyValuePair<K, V>> other) => throw new NotImplementedException();
    bool ISet<KeyValuePair<K, V>>.SetEquals(IEnumerable<KeyValuePair<K, V>> other) => throw new NotImplementedException();
    void ISet<KeyValuePair<K, V>>.SymmetricExceptWith(IEnumerable<KeyValuePair<K, V>> other) => throw new NotImplementedException();
    void ISet<KeyValuePair<K, V>>.UnionWith(IEnumerable<KeyValuePair<K, V>> other) => throw new NotImplementedException();
    
    void ICollection<KeyValuePair<K, V>>.Add(KeyValuePair<K, V> item) => throw new NotImplementedException();
    void ICollection<KeyValuePair<K, V>>.Clear() => throw new NotImplementedException();
    bool ICollection<KeyValuePair<K, V>>.Contains(KeyValuePair<K, V> item) => throw new NotImplementedException();
    void ICollection<KeyValuePair<K, V>>.CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) => throw new NotImplementedException();
    bool ICollection<KeyValuePair<K, V>>.Remove(KeyValuePair<K, V> item) => throw new NotImplementedException();
    
    IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator() => throw new NotImplementedException();
    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

    public ICollection<K> Keys => throw new NotImplementedException();
    public ICollection<V> Values => throw new NotImplementedException();
    public V this[K key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public void Add(K key, V value) => throw new NotImplementedException();
    public bool ContainsKey(K key) => throw new NotImplementedException();
    public bool Remove(K key) => throw new NotImplementedException();
    public bool TryGetValue(K key, [MaybeNullWhen(false)] out V value) => throw new NotImplementedException();
}
