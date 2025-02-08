using System.Runtime.CompilerServices;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Node of a binary search tree.  
/// </summary>
/// <typeparam name="TTag">Type of tags stored in the tree.</typeparam>
/// <typeparam name="TValue">Type of values stored in the tree.</typeparam>
public sealed class JoinableTreeNode<TTag, TValue> where TTag : struct, ITagTraits<TTag>
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="transient">Transient tag.</param>
    /// <param name="value">Value stored in the node.</param>
    public JoinableTreeNode(ulong transient, TTag tag, TValue value) {
        Transient = transient;
        T = tag;
        V = value;
    }

    /// <summary>
    /// The transient tag.
    /// </summary>
    public readonly ulong Transient;

    /// <summary>
    /// Tag contained in the node.
    /// </summary>
    public TTag T;

    /// <summary>
    /// Value contained in the node.  Immutable because it determines the node's position in the tree.
    /// </summary>
    public readonly TValue V;

    /// <summary>
    /// Left child, with key less than <see cref="V"/>.
    /// </summary>
    public JoinableTreeNode<TTag, TValue> L;

    /// <summary>
    /// Right child, with key larger than <see cref="V"/>.
    /// </summary>
    public JoinableTreeNode<TTag, TValue> R;

    /// <summary>
    /// Clones <c>this</c> if this' transient tag is different from <paramref name="transient"/>.
    /// <returns>
    /// New instance or <c>this</c>, depending on the transient tag.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining /*| MethodImplOptions.AggressiveOptimization*/)]
    public JoinableTreeNode<TTag, TValue> Clone<TValueTraits>(ulong transient) where TValueTraits : IValueTraits<TValue>
        => transient == Transient 
        ? this 
        : new(transient, T, TValueTraits.Clone(V)) { L = L, R = R, };

    /// <summary>
    /// Updates <c>this</c> node's balance, size and monoidal tags by invoking <see cref="ITagTraits{TTag}.Combine(TTag, ref TTag, TTag)"/>
    /// with appropriate arguments.
    /// WARNING: The update is in-place, so the node must have been cloned beforehand.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining /*| MethodImplOptions.AggressiveOptimization*/)]
    public void Update()
    {
        var ltag = L?.T ?? TTag.Nil;
        var rtag = R?.T ?? TTag.Nil;
        TTag.Combine(ltag, ref T, rtag);
    }
}

