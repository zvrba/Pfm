using System;

namespace Pfm.Collections.JoinTree;

/// <summary>
/// Node traits determine how values are compared and nodes cloned.
/// </summary>
public interface INodeTraits<TValue>
{
    private static readonly Comparison<TValue> Comparison = System.Collections.Generic.Comparer<TValue>.Default.Compare;
    
    /// <summary>
    /// Comparison method.  Default implementation delegates to
    /// <see cref="System.Collections.Generic.Comparer{T}.Default"/>.
    /// </summary>
    public virtual static int Compare(TValue left, TValue right)
        => Comparison(left, right);

    /// <summary>
    /// True if the underlying node traits are "persistent", i.e., copied on modification.
    /// </summary>
    public abstract static bool IsPersistent { get; }

    /// <summary>
    /// Cloning method.  When <see cref="IsPersistent"/> is true, the method must clone the node
    /// and, if desired, also the value.  When <see cref="IsPersistent"/> is false, it can just
    /// return <c>this</c>.
    /// </summary>
    public abstract static Node<TValue> Clone(Node<TValue> node);
}

/// <summary>
/// Predefined tratis using default from <see cref="INodeTraits{TValue}"/>.
/// </summary>
public struct MutableNodeTraits<TValue> : INodeTraits<TValue>
{
    public static bool IsPersistent => false;
    public static Node<TValue> Clone(Node<TValue> node) => node;
}

/// <summary>
/// Predefined traits that clone the node but NOT the value.  Default comparison is used.
/// </summary>
public struct ImmutableNodeTraits<TValue> : INodeTraits<TValue>
{
    public static bool IsPersistent => true;
    public static Node<TValue> Clone(Node<TValue> node) => new(node);
}

/// <summary>
/// Common node definition for all joinable trees.
/// </summary>
public sealed class Node<TValue>
{
    public Node() { }

    public Node(Node<TValue> other) {
        L = other.L;
        R = other.R;
        V = other.V;
        Rank = other.Rank;
        Size = other.Size;
    }

    public Node<TValue> L;
    public Node<TValue> R;
    public TValue V;
    public int Rank;
    public int Size;

    /// <summary>
    /// Describes the result of splitting a joinable tree at a given value.
    /// </summary>
    public readonly struct Split
    {
        /// <summary>
        /// Left part of the split.
        /// </summary>
        public readonly Node<TValue> L;

        /// <summary>
        /// Not null if the node with the splitting value was found in the tree.
        /// </summary>
        public readonly Node<TValue> M;

        /// <summary>
        /// Right part of the split.
        /// </summary>
        public readonly Node<TValue> R;

        internal Split(Node<TValue> l, Node<TValue> m,  Node<TValue> r) {
            L = l; M = m; R = r;
        }
    }
}

/// <summary>
/// Tree traits determine how node ranks / sizes ("tag") are updated and how subtrees are joined.
/// These are the only tree-specific operations that have to be implemented for each tree type.
/// </summary>
public interface ITreeTraits<TValue>
{
    /// <summary>
    /// Updates the node's "tag" after its children have been updated.
    /// </summary>
    public abstract static void Update(Node<TValue> node);

    /// <summary>
    /// 3-way join; all other operations are based on this method.
    /// </summary>
    /// <param name="left">Left tree to join.</param>
    /// <param name="middle">The join pivot to insert in the result.</param>
    /// <param name="right">Right tree to join.</param>
    /// <returns>
    /// Tree that has same entries and inorder traversal as the node <c>(left, middle, right)</c>.
    /// </returns>
    /// <remarks>
    /// If the nodes are mutable, this operation is destructive to all inputs.
    /// </remarks>
    public abstract static Node<TValue> Join(Node<TValue> left, Node<TValue> middle, Node<TValue> right);

    /// <summary>
    /// Validates the tree's structural invariants.
    /// </summary>
    internal abstract static void ValidateStructure(Node<TValue> root);
}
