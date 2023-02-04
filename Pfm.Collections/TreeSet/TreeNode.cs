using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Pfm.Collections.TreeSet;

/// <summary>
/// Node of a binary search tree.  The implementation uses single int for rank and size: 6 bits for rank and 26
/// bits for size.  This limits the tree size to 2^26 (~67M) elements.
/// </summary>
/// <typeparam name="TValue">
/// Value stored in the tree.  This must be a value type that contains the tree key, balance tag for the tree
/// mechanics and optional user-defined monoidal augmentation data.
/// </typeparam>
[DebuggerDisplay("V={V} (S={Size}, R={Rank})")]
public sealed class TreeNode<TValue>
{
    public const int RankBits = 6;
    public const uint RankMask = (1U << RankBits) - 1;
    public const uint SizeMask = ~RankMask;

    public TreeNode(ulong transient) {
        _T = transient;
    }

    /// <summary>
    /// Value stored in the tree.
    /// </summary>
    public TValue V;

    /// <summary>
    /// Contains packed rank (6 bits) and size (26 bits).
    /// Must not be touched by user code.
    /// </summary>
    public uint _RS;

    /// <summary>
    /// Transient tag used to eschew node cloning.  Must not be touched by user code.
    /// </summary>
    public readonly ulong _T;

    /// <summary>
    /// Left child, with key less than <see cref="V"/>.
    /// </summary>
    public TreeNode<TValue> L;

    /// <summary>
    /// Right child, with key larger than <see cref="V"/>.
    /// </summary>
    public TreeNode<TValue> R;

    /// <summary>
    /// Balance metadata is maintained by tree mechanics and must not be touched by user code.
    /// </summary>
    public int Rank => (int)(_RS & RankMask);

    /// <summary>
    /// Size (number of nodes) of the subtree rooted at <c>this</c>, including <c>this</c>.
    /// Maintained by tree mechanics and must not be touched by user code.
    /// </summary>
    public int Size => (int)(_RS >> RankBits);

    /// <summary>
    /// Updates <c>this</c> node's balance and monoidal tags.
    /// WARNING: The update is in-place, so the node must have been cloned beforehand.
    /// </summary>
    //[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Update<TTree>()
        where TTree : struct, IValueTraits<TValue>, IBalanceTraits<TTree, TValue>
    {
        int rank, size;
        if (L != null && R != null) {
            rank = TTree.CombineBalanceTags(L.Rank, R.Rank);
            size = 1 + L.Size + R.Size;
            TTree.CombineTags(L.V, ref V, R.V);
        } else if (L != null) {
            rank = TTree.CombineBalanceTags(L.Rank, TTree.NilBalance);
            size = 1 + L.Size;
            TTree.CombineTagsLeft(L.V, ref V);
        } else if (R != null) {
            rank = TTree.CombineBalanceTags(TTree.NilBalance, R.Rank);
            size = 1 + R.Size;
            TTree.CombineTagsRight(ref V, R.V);
        }
        else {
            rank = TTree.CombineBalanceTags(TTree.NilBalance, TTree.NilBalance);
            size = 1;
        }
        SetRankAndSizeUnchecked(rank, size);
    }

    /// <summary>
    /// Clones <c>this</c> if this' transient tag is different from <paramref name="transient"/>.
    /// <returns>
    /// New instance or <c>this</c>, depending on the transience tag.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public TreeNode<TValue> Clone<TValueTraits>(ulong transient) where TValueTraits : struct, IValueTraits<TValue> {
        return transient == _T ? this : new(transient) { V = TValueTraits.CloneValue(V), L = L, R = R, _RS = _RS };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private void SetRankAndSizeUnchecked(int rank, int size) => _RS = (((uint)size) << RankBits) | (uint)rank;
}

