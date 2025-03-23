using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Node of a binary search tree; stores a tagged value.
/// Enumerating a node will return its children in in-order traversal.
/// </summary>
/// <typeparam name="TValue">Type of values stored in the node.</typeparam>
public sealed class JoinableTreeNode<TValue> : IEnumerable<TValue>
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="transient">Transient tag.</param>
    /// <param name="value">Value stored in the node.</param>
    public JoinableTreeNode(ulong transient, TValue value) {
        Transient = transient;
        Value = value;
    }

    /// <summary>
    /// The transient tag.
    /// </summary>
    public readonly ulong Transient;

    /// <summary>
    /// Tagged value contained in the node.
    /// </summary>
    public TValue Value;

    /// <summary>
    /// Left child, with key less than <see cref="Value"/>.
    /// </summary>
    public JoinableTreeNode<TValue> Left;

    /// <summary>
    /// Right child, with key larger than <see cref="Value"/>.
    /// </summary>
    public JoinableTreeNode<TValue> Right;

    /// <summary>
    /// Count of the nodes under, and including, this node.
    /// </summary>
    public int Size;

    /// <summary>
    /// Rank; needed by some tree implementations.
    /// </summary>
    public int Rank;

    /// <summary>
    /// Clones <c>this</c> if this' transient tag is different from <paramref name="transient"/>.
    /// </summary>
    /// <param name="transient">Transient tag for the cloned node.</param>
    /// <typeparam name="TValueTraits">Value traits.</typeparam>
    /// <returns>
    /// New instance or <c>this</c>, depending on the transient tag.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining /*| MethodImplOptions.AggressiveOptimization*/)]
    public JoinableTreeNode<TValue> Clone<TValueTraits>(ulong transient)
        where TValueTraits : IValueTraits<TValue>
        => transient == Transient 
        ? this 
        : new(transient, TValueTraits.Clone(Value)) { Left = Left, Right = Right, Size = Size, Rank = Rank };

    /// <summary>
    /// Updates <c>this</c> node's tag by invoking <see cref="IValueTraits{TValue}.CombineTags(TValue, ref TValue, TValue)"/>
    /// with appropriate arguments.
    /// WARNING: The update is in-place, so the node must have been cloned beforehand.
    /// </summary>
    /// <typeparam name="TJoin">Tree join strategy.</typeparam>
    //[MethodImpl(MethodImplOptions.AggressiveInlining /*| MethodImplOptions.AggressiveOptimization*/)]
    public void Update<TJoin>() where TJoin : ITreeTraits<TValue>
    {
        if (Left != null && Right != null) {
            Size = 1 + Left.Size + Right.Size;
            Rank = TJoin.CombineRanks(Left.Rank, Rank, Right.Rank);
            TJoin.CombineTags(Left.Value, ref Value, Right.Value);
        }
        else if (Left != null) {
            Size = 1 + Left.Size;
            Rank = TJoin.CombineRanks(Left.Rank, Rank, TJoin.NilRank);
            TJoin.CombineTags(Left.Value, ref Value, TJoin.NilTag);
        }
        else if (Right != null) {
            Size = 1 + Right.Size;
            Rank = TJoin.CombineRanks(TJoin.NilRank, Rank, Right.Rank);
            TJoin.CombineTags(TJoin.NilTag, ref Value, Right.Value);
        }
        else {
            Size = 1;
            Rank = TJoin.CombineRanks(TJoin.NilRank, TJoin.NilRank, TJoin.NilRank);
            TJoin.CombineTags(TJoin.NilTag, ref Value, TJoin.NilTag);
        }
    }

    /// <inheritdoc/>
    public IEnumerator<TValue> GetEnumerator() {
        var it = TreeIterator<TValue>.New();
        it.First(this);
        do {
            yield return it.Top.Value;
        } while (it.Succ());
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
