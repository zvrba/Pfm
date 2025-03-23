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
    /// <summary>
    /// Presents a tree as <see cref="ICollection{T}"/>.
    /// </summary>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <typeparam name="TJoin">Tree join strategy.</typeparam>
    /// <param name="this">Tree root.</param>
    /// <param name="transient">Transient tag to use.  If 0, a new transient is generated.</param>
    /// <returns>
    /// An instance of <see cref="CollectionTreeAdapter{TValue, TJoin}"/>.
    /// </returns>
    public static CollectionTreeAdapter<TValue, TJoin> AsCollection<TValue, TJoin>
        (
        this JoinableTreeNode<TValue>? @this,
        ulong transient = 0
        )
        where TJoin : struct, ITreeTraits<TValue>
        => new(@this, transient);

    /// <summary>
    /// Presents a tree as <see cref="ICollection{T}"/>.
    /// </summary>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <typeparam name="TJoin">Tree join strategy.</typeparam>
    /// <param name="this">Tree adapted as a possibly other collection type.</param>
    /// <returns>
    /// An instance of <see cref="CollectionTreeAdapter{TValue, TJoin}"/>.
    /// </returns>
    public static CollectionTreeAdapter<TValue, TJoin> AsCollection<TValue, TJoin>
        (
        this IAdaptedTree<TValue, TJoin> @this
        )
        where TJoin : struct, ITreeTraits<TValue>
        => new(@this.Root, @this.Transient);

    /// <summary>
    /// Presents a tree as <see cref="IReadOnlyList{T}"/>.
    /// </summary>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <typeparam name="TJoin">Tree join strategy.</typeparam>
    /// <param name="this">Tree root.</param>
    /// <param name="transient">Transient tag to use.  If 0, a new transient is generated.</param>
    /// <returns>
    /// An instance of <see cref="ReadOnlyListTreeAdapter{TValue, TJoin}"/>.
    /// </returns>
    public static ReadOnlyListTreeAdapter<TValue, TJoin> AsReadOnlyList<TValue, TJoin>
        (
        this JoinableTreeNode<TValue>? @this,
        ulong transient = 0
        )
        where TJoin : struct, ITreeTraits<TValue>
        => new(@this, transient);

    /// <summary>
    /// Presents a tree as <see cref="IReadOnlyList{T}"/>.
    /// </summary>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <typeparam name="TJoin">Tree join strategy.</typeparam>
    /// <param name="this">Tree adapted as a possibly other collection type.</param>
    /// <returns>
    /// An instance of <see cref="ReadOnlyListTreeAdapter{TValue, TJoin}"/>.
    /// </returns>
    public static ReadOnlyListTreeAdapter<TValue, TJoin> AsReadOnlyList<TValue, TJoin>
        (
        this IAdaptedTree<TValue, TJoin> @this
        )
        where TJoin : struct, ITreeTraits<TValue>
        => new(@this.Root, @this.Transient);

    /// <summary>
    /// Presents a tree as <see cref="ISet{T}"/> and <see cref="IReadOnlySet{T}"/>.
    /// </summary>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <typeparam name="TJoin">Tree join strategy.</typeparam>
    /// <param name="this">Tree root.</param>
    /// <param name="transient">Transient tag to use.  If 0, a new transient is generated.</param>
    /// <returns>
    /// An instance of <see cref="SetTreeAdapter{TValue, TJoin}"/>.
    /// </returns>
    public static SetTreeAdapter<TValue, TJoin> AsSet<TValue, TJoin>
        (
        this JoinableTreeNode<TValue>? @this,
        ulong transient = 0
        )
        where TJoin : struct, ITreeTraits<TValue>
        => new(@this, transient);

    /// <summary>
    /// Presents a tree as <see cref="ISet{T}"/> and <see cref="IReadOnlySet{T}"/>.
    /// </summary>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <typeparam name="TJoin">Tree join strategy.</typeparam>
    /// <param name="this">Tree adapted as a possibly other collection type.</param>
    /// <returns>
    /// An instance of <see cref="SetTreeAdapter{TValue, TJoin}"/>.
    /// </returns>
    public static SetTreeAdapter<TValue, TJoin> AsSet<TValue, TJoin>
        (
        this IAdaptedTree<TValue, TJoin> @this
        )
        where TJoin : struct, ITreeTraits<TValue>
        => new(@this.Root, @this.Transient);

    /// <summary>
    /// Non-destructive set-union between <paramref name="this"/> and <paramref name="other"/>.
    /// </summary>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <typeparam name="TJoin">Tree join strategy.</typeparam>
    /// <param name="this">Tree adapted as a possibly other collection type.  First argument to union.</param>
    /// <param name="other">Tree adapted as a possibly other collection type.  Second argument to union.</param>
    /// <returns>
    /// A new set that is union of <paramref name="this"/> and <paramref name="other"/>.
    /// </returns>
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

    /// <summary>
    /// Non-destructive set-intersection between <paramref name="this"/> and <paramref name="other"/>.
    /// </summary>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <typeparam name="TJoin">Tree join strategy.</typeparam>
    /// <param name="this">Tree adapted as a possibly other collection type.  First argument to union.</param>
    /// <param name="other">Tree adapted as a possibly other collection type.  Second argument to union.</param>
    /// <returns>
    /// A new set that is intersection of <paramref name="this"/> and <paramref name="other"/>.
    /// </returns>
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

    /// <summary>
    /// Non-destructive set-difference between <paramref name="this"/> and <paramref name="other"/>.
    /// Difference is a non-commutative operation and <paramref name="this"/> is treated as the "left" argument, i.e.,
    /// the resulting set wil contain no element present in <paramref name="other"/>.
    /// </summary>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <typeparam name="TJoin">Tree join strategy.</typeparam>
    /// <param name="this">Tree adapted as a possibly other collection type.  First argument to union.</param>
    /// <param name="other">Tree adapted as a possibly other collection type.  Second argument to union.</param>
    /// <returns>
    /// A new set that is difference of <paramref name="this"/> and <paramref name="other"/>.
    /// </returns>
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
