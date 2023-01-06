using System;

namespace Pfm.Collections.IntrusiveTree;

/// <summary>
/// Used by tree code to access relevant properties of a node.  The same class can implement this interface
/// multiple times for same <typeparamref name="TValue"/> by using explicit implementations with different
/// <typeparamref name="TTag"/> types.
/// </summary>
/// <typeparam name="TNode">Node type holding tree data.</typeparam>
/// <typeparam name="TValue">Value type stored in the tree.</typeparam>
/// <typeparam name="TTag">Tag type used by the tree.</typeparam>
public interface INodeTraits<TNode, TValue, TTag>
    where TNode : class, INodeTraits<TNode, TValue, TTag>
    where TTag : struct, ITagTraits<TTag>
{
    /// <summary>Creates a new holder instance.</summary>
    /// <param name="transient">Initial transient tag, or <c>null</c> for mutable implementation.</param>
    /// <returns>A new, uninitialized instance.</returns>
    /// <exception cref="NotSupportedException">Thrown by a purely intrusive implementation.</exception>
    abstract static TNode Create(object transient);

    /// <summary>
    /// Compares two values.
    /// </summary>
    /// <returns>
    /// Negative, zero or positive if <paramref name="left"/> is respectively less than, equal to, or
    /// greater than <paramref name="right"/>.
    /// </returns>
    abstract static int Compare(TValue left, TValue right);

    /// <summary>
    /// Link to the left node.
    /// </summary>
    ref TNode L { get; }

    /// <summary>
    /// Link to the right node.
    /// </summary>
    ref TNode R { get; }

    /// <summary>
    /// <para>
    /// Writable reference to the value.  The part of the value used by <see cref="Compare(TValue, TValue)"/> must
    /// NOT be changed, otherwise results will be undefined.
    /// </para>
    /// <para>
    /// Providing a writable reference enables implementing key-value dictionaries where the value part can be freely changed.
    /// <see cref="MutableKeyValuePair{K, V}"/> for an example.
    /// </para>
    /// </summary>
    ref TValue V { get; }

    /// <summary>
    /// Writable reference to the tag.  The tree's base tag must not be changed by external code.
    /// </summary>
    ref TTag T { get; }
}

/// <summary>
/// Default implementation of mutable node holder and <see cref="ITreeTraits{THolder, TValue, TTag}"/>.
/// The comparison used is <see cref="System.Collections.Generic.Comparer{T}.Default"/>.
/// Usable for basic sets and dictionaries.
/// </summary>
/// <seealso cref="MutableKeyValuePair{K, V}"/>
public sealed class DefaultMutableNode<TValue, TTag> : INodeTraits<DefaultMutableNode<TValue, TTag>, TValue, TTag>
    where TTag : struct, ITagTraits<TTag>
{
    private static readonly System.Collections.Generic.Comparer<TValue> Comparer =
        System.Collections.Generic.Comparer<TValue>.Default;

    private DefaultMutableNode<TValue, TTag> l, r;
    private TTag tag;
    private TValue value;

    private DefaultMutableNode() { }

    static DefaultMutableNode<TValue, TTag> INodeTraits<DefaultMutableNode<TValue, TTag>, TValue, TTag>.Create(object _)
        => new DefaultMutableNode<TValue, TTag>();
    
    static int INodeTraits<DefaultMutableNode<TValue, TTag>, TValue, TTag>.Compare(TValue left, TValue right)
        => Comparer.Compare(left, right);

    ref DefaultMutableNode<TValue, TTag> INodeTraits<DefaultMutableNode<TValue, TTag>, TValue, TTag>.L => ref l;
    ref DefaultMutableNode<TValue, TTag> INodeTraits<DefaultMutableNode<TValue, TTag>, TValue, TTag>.R => ref r;
    ref TValue INodeTraits<DefaultMutableNode<TValue, TTag>, TValue, TTag>.V => ref value;
    ref TTag INodeTraits<DefaultMutableNode<TValue, TTag>, TValue, TTag>.T => ref tag;

    public static DelegateFinder<DefaultMutableNode<TValue, TTag>, TValue, TTag>.NodeTraits
    GetTraits() =>
        new(
            null,
            Comparer.Compare,
            n => ref n.l,
            n => ref n.r,
            n => ref n.value,
            n => ref n.tag);
}

/// <summary>
/// A key-value pair with mutable value part.  May be used in dictionary implementations based on trees from this
/// namespace to allow for low-overhead updates of values associated with keys.
/// </summary>
public struct MutableKeyValuePair<K, V>
{
    public readonly K Key;
    public V Value;

    public MutableKeyValuePair(K key, V value) {
        Key = key;
        Value = value;
    }
}
