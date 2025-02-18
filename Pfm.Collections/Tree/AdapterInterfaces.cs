#nullable enable

using System.Collections.Generic;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Bridge between "naked" values of type <typeparamref name="TValue"/> and the underlying joinable tree.
/// The necessity of this interface is dictated by the fact that <see cref="ITaggedValue{TValue}"/> can't
/// be implemented on existing types that can't be changed (e.g., <c>int</c>).
/// </summary>
/// <typeparam name="TSelf">The concrete implementing type.</typeparam>
/// <typeparam name="TValue">"Naked" value type held in the container.</typeparam>
public interface ITaggedValueHolder<TSelf, TValue> : ITaggedValue<TSelf>
    where TSelf : struct, ITaggedValueHolder<TSelf, TValue>
{
    /// <summary>
    /// Creates an instance of <typeparamref name="TSelf"/> from <paramref name="value"/>.
    /// </summary>
    /// <param name="value">"Naked" value.</param>
    /// <returns>An instance of <typeparamref name="TSelf"/> with <see cref="Value"/> being the same as <paramref name="value"/>.</returns>
    abstract static TSelf Create(TValue value);

    /// <summary>
    /// "Naked" value.
    /// </summary>
    TValue Value { get; set; }
}

/// <summary>
/// Common interface implemented by all adapters to <c>System.Collection.Generic</c> interfaces.
/// </summary>
/// <typeparam name="TValue">Type presented by the "System" interface.</typeparam>
/// <typeparam name="TJoin">Tree join strategy.</typeparam>
/// <typeparam name="THolder">Value holder type.</typeparam>
public interface IAdaptedTree<TValue, TJoin, THolder>
    where TJoin : struct, ITreeJoin<THolder>
    where THolder : struct, ITaggedValueHolder<THolder, TValue>
{
    /// <summary>
    /// Transient tag to use by the collection.
    /// </summary>
    ulong Transient { get; }

    /// <summary>
    /// Root of the collection's underlying tree.  <c>null</c> for empty collection.
    /// </summary>
    JoinableTreeNode<THolder>? Root { get; }
}

/// <summary>
/// Contains extension methods for creating different "System" collection views from a tree.
/// </summary>
public static class CollectionAdapters
{
    public static object AsCollection<TValue, TJoin, THolder>(this JoinableTreeNode<THolder> @this, ulong transient = 0)
        where THolder : struct, ITaggedValueHolder<THolder, TValue>
        where TJoin : struct, ITreeJoin<THolder>
        => new CollectionTreeAdapter<TValue, TJoin, THolder>(@this, transient);
}