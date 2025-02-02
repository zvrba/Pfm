using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Node of a binary search tree.  
/// </summary>
/// <typeparam name="TTag">Type of tags stored in the tree.  Use <see cref="TreeBalanceTag"/> when no custom tag is needed.</typeparam>
/// <typeparam name="TValue">Type of values stored in the tree.</typeparam>
[DebuggerDisplay("V={V} (S={Size}, R={Rank})")]
public sealed class JoinableTreeNode<TTag, TValue> where TTag : struct, ITagTraits<TTag>
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="transient">Transient value for this node.</param>
    public JoinableTreeNode(ulong transient) {
        Transient = transient;
    }

    /// <summary>
    /// Transient tag.
    /// </summary>
    public readonly ulong Transient;

    /// <summary>
    /// Value contained in the node.
    /// </summary>
    public TValue V;

    /// <summary>
    /// Tag contained in the node.
    /// </summary>
    public TTag T;

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
        : new(transient) { V = TValueTraits.Clone(V), T = T, L = L, R = R, };

    /// <summary>
    /// Updates <c>this</c> node's balance and monoidal tags.
    /// WARNING: The update is in-place, so the node must have been cloned beforehand.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining /*| MethodImplOptions.AggressiveOptimization*/)]
    public void Update<TTagTraits>() where TTagTraits : struct, ITagTraits<TTag>
    {
        var ltag = L?.T ?? TTag.Nil;
        var rtag = R?.T ?? TTag.Nil;
        TTagTraits.Combine(ltag, ref T, rtag);
    }
}

