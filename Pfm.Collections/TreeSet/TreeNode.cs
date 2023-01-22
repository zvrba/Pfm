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

    /// <summary>
    /// Left child, with key less than <see cref="V"/>.
    /// </summary>
    public TreeNode<TValue> L;

    /// <summary>
    /// Right child, with key larger than <see cref="V"/>.
    /// </summary>
    public TreeNode<TValue> R;

    /// <summary>
    /// Value stored in the tree.
    /// </summary>
    public TValue V;

    private uint _RS;   // Rank and size

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
    /// Simultaneously sets <see cref="Rank"/> and <see cref=" Size"/>; no range checking is performed.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetRankAndSizeUnchecked(int rank, int size) => _RS = (((uint)size) << RankBits) | (uint)rank;

    /// <summary>
    /// Default constructor: does nothing.
    /// </summary>
    public TreeNode() { }

    /// <summary>
    /// Copy-constructor.
    /// </summary>
    public TreeNode(TreeNode<TValue> other) {
        L = other.L; R = other.R; V = other.V; _RS = other._RS;
    }

    /// <summary>
    /// Updates <c>this</c> node's balance and monoidal tags.
    /// WARNING: The update is in-place, so the node must have been cloned beforehand.
    /// </summary>
    /// <typeparam name="TTree">A fully instantiated tree implementation type to use.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Update<TTree>()
        where TTree : struct, IJoinTree<TTree, TValue>, IValueTraits<TValue>, IPersistenceTraits<TValue>
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
}

