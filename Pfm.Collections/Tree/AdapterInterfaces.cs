#nullable enable

using System.Collections.Generic;
using System.Threading;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Common interface implemented by all adapters to <c>System.Collection.Generic</c> interfaces.
/// </summary>
/// <typeparam name="TValue">Value type held by the tree.</typeparam>
/// <typeparam name="TTree">The concrete tree implementation.</typeparam>
public interface IAdaptedTree<TValue, TTree>
    where TTree : struct, ITreeTraits<TValue>
{
    /// <summary>
    /// Transient tag to use by the collection.
    /// </summary>
    ulong Transient { get; }

    /// <summary>
    /// Root of the collection's underlying tree.  <c>null</c> for empty collection.
    /// </summary>
    JoinableTreeNode<TValue>? Root { get; }
}

/// <summary>
/// Contains extension methods for creating different "System" collection views from a tree.
/// </summary>
public static class TreeAdapterExtensions
{
    public static CollectionTreeAdapter<TValue, TJoin> AsCollection<TValue, TJoin>
        (
        this JoinableTreeNode<TValue>? @this,
        ulong transient = 0
        )
        where TJoin : struct, ITreeTraits<TValue>
        => new(@this, transient);

    public static CollectionTreeAdapter<TValue, TJoin> AsCollection<TValue, TJoin>
        (
        this IAdaptedTree<TValue, TJoin> @this
        )
        where TJoin : struct, ITreeTraits<TValue>
        => new(@this.Root, @this.Transient);

    public static ReadOnlyListTreeAdapter<TValue, TJoin> AsReadOnlyList<TValue, TJoin>
        (
        this JoinableTreeNode<TValue>? @this,
        ulong transient = 0
        )
        where TJoin : struct, ITreeTraits<TValue>
        => new(@this, transient);

    public static ReadOnlyListTreeAdapter<TValue, TJoin> AsReadOnlyList<TValue, TJoin>
        (
        this IAdaptedTree<TValue, TJoin> @this
        )
        where TJoin : struct, ITreeTraits<TValue>
        => new(@this.Root, @this.Transient);

    public static SetTreeAdapter<TValue, TJoin> AsSet<TValue, TJoin>
        (
        this JoinableTreeNode<TValue>? @this,
        ulong transient = 0
        )
        where TJoin : struct, ITreeTraits<TValue>
        => new(@this, transient);

    public static SetTreeAdapter<TValue, TJoin> AsSet<TValue, TJoin>
        (
        this IAdaptedTree<TValue, TJoin> @this
        )
        where TJoin : struct, ITreeTraits<TValue>
        => new(@this.Root, @this.Transient);

    public static SetTreeAdapter<TValue, TJoin> SetUnion<TValue, TJoin> 
        (
        this IAdaptedTree<TValue, TJoin> @this,
        IEnumerable<TValue> other
        )
        where TJoin : struct, ITreeTraits<TValue>
    {
        var ret = new SetTreeAdapter<TValue, TJoin>(@this.Root, TransientSource.NewTransient());
        ret.UnionWith(other);
        return ret;
    }

    public static SetTreeAdapter<TValue, TJoin> SetIntersection<TValue, TJoin>
        (
        this IAdaptedTree<TValue, TJoin> @this,
        IEnumerable<TValue> other
        )
        where TJoin : struct, ITreeTraits<TValue>
    {
        var ret = new SetTreeAdapter<TValue, TJoin>(@this.Root, TransientSource.NewTransient());
        ret.IntersectWith(other);
        return ret;
    }

    public static SetTreeAdapter<TValue, TJoin> SetDifference<TValue, TJoin>
        (
        this IAdaptedTree<TValue, TJoin> @this,
        IEnumerable<TValue> other
        )
        where TJoin : struct, ITreeTraits<TValue>
    {
        var ret = new SetTreeAdapter<TValue, TJoin>(@this.Root, TransientSource.NewTransient());
        ret.ExceptWith(other);
        return ret;
    }
}
