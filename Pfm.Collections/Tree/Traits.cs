using System;
using System.Runtime.CompilerServices;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Allows for implementation of custom monoidal tags attached to nodes.
/// </summary>
/// <typeparam name="TTag">
/// Tag type; must be a mutable struct.  Struct requirement simplifies cloning of nodes.  Mutability is required because the tag
/// must incorporate mutable properties.
/// </typeparam>
public interface ITagTraits<TTag> where TTag : struct
{
    /// <summary>
    /// Tag value corresponding to <c>null</c> node.
    /// </summary>
    abstract static TTag Nil { get; }

    /// <summary>
    /// Computes <c>result = left + result + right</c>.  The computation of <see cref="Rank"/> and <see cref="Size"/>
    /// must be delegated to the tree's concrete balance tag.
    /// </summary>
    /// <param name="left">Tag value corresponding to the left branch.  <see cref="Nil"/> if there is no left branch.</param>
    /// <param name="result">Tag value corresponding to the current node.</param>
    /// <param name="right">Tag value corresponding to the right branch.  <see cref="Nil"/> if there is no right branch.</param>
    abstract static void Combine(TTag left, ref TTag result, TTag right);
}

/// <summary>
/// Value traits determine how values are interpreted by the tree.  These are independent of tree mechanics and
/// may be reused with different tree implementations.
/// </summary>
/// <typeparam name="TValue">Value type of the tree.</typeparam>
public interface IValueTraits<TValue>
{
    /// <summary>
    /// Compares two values.  Must implement a total order.
    /// </summary>
    /// <param name="left">Left value to compare.</param>
    /// <param name="right">Right value to compare.</param>
    /// <returns>
    /// Negative, zero or positive integer when <paramref name="left"/> is, respectively smaller than, equal
    /// to, or greater than <paramref name="right"/>.
    /// </returns>
    abstract static int Compare(TValue left, TValue right);

    /// <summary>
    /// Clones the value whenever a node is cloned due to modification.
    /// Default implementation is a no-op, i.e., just returns <paramref name="value"/>.
    /// </summary>
    /// <returns>The cloned value.</returns>
    virtual static TValue Clone(TValue value) => value;
}

/// <summary>
/// Every joinable tree must implement this interface.  The join algorithm itself is independent of value traits.
/// </summary>
/// <typeparam name="TTag">Tag type.</typeparam>
/// <typeparam name="TValue">Type of values stored in the tree.</typeparam>
public interface ITreeTraits<TTag, TValue> : ITagTraits<TTag>, IValueTraits<TValue>
    where TTag : struct, ITagTraits<TTag>
{
    /// <summary>
    /// 3-way join is the core tree algorithm on which all other operations are based.
    /// </summary>
    /// <param name="transient">Transient tag used for lazy cloning during tree modifications.</param>
    /// <param name="left">Left tree to join.</param>
    /// <param name="middle">The join pivot to insert in the result.</param>
    /// <param name="right">Right tree to join.</param>
    /// <returns>
    /// Tree that has same entries and inorder traversal as the node <c>(left, middle, right)</c>.
    /// </returns>
    /// <remarks>
    /// When the tree is not persistent, this operation is destructive to all inputs.
    /// </remarks>
    abstract static JoinableTreeNode<TTag, TValue> Join(
        ulong transient,
        JoinableTreeNode<TTag, TValue> left,
        JoinableTreeNode<TTag, TValue> middle,
        JoinableTreeNode<TTag, TValue> right);

    /// <summary>
    /// This method must validate the tree's structure invariant.
    /// Mainly for use in stress-tests.
    /// </summary>
    /// <exception cref="NotImplementedException">Thrown when a violation of the structure invariant is detected.</exception>
    abstract static void ValidateStructure(JoinableTreeNode<TTag, TValue> root);
}
