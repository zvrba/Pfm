using System.Runtime.CompilerServices;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Node of a binary search tree.  Stores a tagged value.
/// </summary>
/// <typeparam name="TValue">Type of values stored in the node.</typeparam>
public sealed class JoinableTreeNode<TValue> where TValue : ITaggedValue<TValue>
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
    /// <returns>
    /// New instance or <c>this</c>, depending on the transient tag.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining /*| MethodImplOptions.AggressiveOptimization*/)]
    public JoinableTreeNode<TValue> Clone(ulong transient)
        => transient == Transient 
        ? this 
        : new(transient, TValue.Clone(Value)) { Left = Left, Right = Right, };

    /// <summary>
    /// Updates <c>this</c> node's tag by invoking <see cref="ITaggedValue{TValue}.Combine(in TValue, ref TValue, in TValue)"/>
    /// with appropriate arguments.
    /// WARNING: The update is in-place, so the node must have been cloned beforehand.
    /// </summary>
    //[MethodImpl(MethodImplOptions.AggressiveInlining /*| MethodImplOptions.AggressiveOptimization*/)]
    public void Update<TJoin>() where TJoin : ITreeJoin<TValue>
    {
        if (Left != null && Right != null) {
            Size = 1 + Left.Size + Right.Size;
            Rank = TJoin.Combine(Left.Rank, Rank, Right.Rank);
            TValue.Combine(Left.Value, ref Value, Right.Value);
        }
        else if (Left != null) {
            Size = 1 + Left.Size;
            Rank = TJoin.Combine(Left.Rank, Rank, TJoin.Nil);
            TValue.Combine(Left.Value, ref Value, TValue.Nil);
        }
        else if (Right != null) {
            Size = 1 + Right.Size;
            Rank = TJoin.Combine(TJoin.Nil, Rank, Right.Rank);
            TValue.Combine(TValue.Nil, ref Value, Right.Value);
        }
        else {
            Size = 1;
            Rank = TJoin.Combine(TJoin.Nil, TJoin.Nil, TJoin.Nil);
            TValue.Combine(TValue.Nil, ref Value, TValue.Nil);
        }
    }
}
