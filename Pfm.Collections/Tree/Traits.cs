using System;

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

    /// <summary>
    /// Rank used for internal balancing operations.  Can be modified only by the tree implementation.
    /// </summary>
    int Rank { get; set; }

    /// <summary>
    /// Count of nodes under and including this node.  Can be modified only by the tree implementation.
    /// </summary>
    int Size { get; set; }
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
