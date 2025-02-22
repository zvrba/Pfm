#nullable enable

using System.Collections.Generic;
using System.Threading;

namespace Podaga.PersistentCollections.Tree;

public static class TransientSource
{
    private static ulong NextTransient = 0;
    public static ulong NewTransient() => Interlocked.Increment(ref NextTransient);
}

/// <summary>
/// Bridge between "naked" values of type <typeparamref name="TValue"/> and the underlying joinable tree.
/// The necessity of this interface is dictated by the fact that types we cannot implement <see cref="ITaggedValue{TValue}"/>
/// on types we don't control.
/// </summary>
/// <typeparam name="TSelf">The concrete implementing type.</typeparam>
/// <typeparam name="TValue">"Naked" value type held in the container.</typeparam>
public interface ITaggedValueHolder<TSelf, TValue> : ITaggedValue<TSelf>
    where TSelf : struct, ITaggedValueHolder<TSelf, TValue>, ITreeJoin<TSelf>
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
/// <typeparam name="TValue">Value type held by the tree.</typeparam>
/// <typeparam name="THolder">Join strategy and value traits.</typeparam>
public interface IAdaptedTree<THolder, TValue>
    where THolder : struct, ITaggedValueHolder<THolder, TValue>, ITreeJoin<THolder>
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
    public static object AsCollection<TValue, THolder>(this JoinableTreeNode<THolder> @this, ulong transient = 0)
        where THolder : struct, ITaggedValueHolder<THolder, TValue>, ITreeJoin<THolder>
        => new CollectionTreeAdapter<TValue, THolder>(@this, transient);
}
