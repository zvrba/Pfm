using System;

namespace Podaga.PersistentCollections.TreeSet;

public interface IBalanceTraits<TSelf, TValue>
    where TSelf : struct, IBalanceTraits<TSelf, TValue>, IValueTraits<TValue>
{
    /// <summary>
    /// Value provided to <see cref="CombineBalanceTags(int, int)"/> when the node is missing a child.
    /// </summary>
    abstract static int NilBalance { get; }

    /// <summary>
    /// Combines balance tags from children.
    /// </summary>
    /// <param name="left">Balance tag of the left child.</param>
    /// <param name="right">Balance tag of the right child.</param>
    abstract static int CombineBalanceTags(int left, int right);

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
    /// When the tree is not persistent, this operation is destructive to all inputs.
    /// </remarks>
    abstract static TreeNode<TValue> Join(
        JoinTree<TValue, TSelf> transient,
        TreeNode<TValue> left,
        TreeNode<TValue> middle,
        TreeNode<TValue> right);

    /// <summary>
    /// For exhaustive tests: validates the tree's structure invariant.
    /// </summary>
    /// <exception cref="NotImplementedException">Thrown when a violation of the structure invariant is detected.</exception>
    internal abstract static void ValidateStructure(TreeNode<TValue> root);
}
