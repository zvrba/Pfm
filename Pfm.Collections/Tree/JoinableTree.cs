#nullable enable

using System;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// This abstract class presents the fundamental interface of joinable trees.
/// All other algorithms are defined as extension methods.
/// </summary>
/// <typeparam name="TTag">Tag type used for balancing.</typeparam>
/// <typeparam name="TValue">Value type.</typeparam>
/// <typeparam name="TValueTraits">Value traits.</typeparam>
public abstract class JoinableTree<TTag, TValue, TValueTraits>
    where TTag : struct, ITagTraits<TTag>
    where TValueTraits : struct, IValueTraits<TValue>
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="transient">
    /// Transient tag.  Use <see cref="TransientTag.New"/> to generate a fresh unique value.
    /// </param>
    public JoinableTree(ulong transient) => Transient = transient;

    /// <summary>
    /// Transiens tag used for lazy cloning during modifications.
    /// </summary>
    public readonly ulong Transient;

    /// <summary>
    /// Root node of the tree; <c>null</c> for an empty tree.  Modified in-place by various algorithms.
    /// </summary>
    public JoinableTreeNode<TTag, TValue>? Root;

    /// <summary>
    /// 3-way join is the core tree algorithm on which all other operations are based.
    /// When the tree is not persistent, this operation is destructive to all inputs.
    /// </summary>
    /// <param name="left">Left tree to join.</param>
    /// <param name="middle">The join pivot to insert in the result.</param>
    /// <param name="right">Right tree to join.</param>
    /// <returns>
    /// Tree that has same entries and inorder traversal as the node <c>(left, middle, right)</c>.
    /// </returns>
    public abstract JoinableTreeNode<TTag, TValue> Join(
        JoinableTreeNode<TTag, TValue> left,
        JoinableTreeNode<TTag, TValue> middle,
        JoinableTreeNode<TTag, TValue> right);

    /// <summary>
    /// This method must validate the tree's structure invariant.
    /// Mainly for use in stress-tests.
    /// </summary>
    /// <exception cref="NotImplementedException">Thrown when a violation of the structure invariant is detected.</exception>
    public abstract void ValidateStructure();

    /// <summary>
    /// Describes the result of <see cref="Split(JoinableTreeNode{TTag, TValue}, TValue)"/>.
    /// </summary>
    /// <param name="L">Left part of the split.</param>
    /// <param name="M">Not null if the node with the splitting value was found in the tree.</param>
    /// <param name="R">Right part of the split.</param>
    public record struct SplitResult(
        JoinableTreeNode<TTag, TValue> L,
        JoinableTreeNode<TTag, TValue> M,
        JoinableTreeNode<TTag, TValue> R);

    /// <summary>
    /// Splits tree rooted at <paramref name="root"/> into left and right subtrees holding respectively
    /// values less than and greater than <paramref name="value"/>.
    /// </summary>
    /// <returns>
    /// A structure containing the left and right subtrees and a flag indicating whether <paramref name="value"/> was
    /// found in the tree.
    /// </returns>
    public SplitResult Split(JoinableTreeNode<TTag, TValue> root, TValue value) {
        if (root == null)
            return default;
        var c = TValueTraits.Compare(value, root.V);
        if (c == 0)
            return new(root.L, root, root.R);
        if (c < 0) {
            var s = Split(root.L, value);
            var j = Join(s.R, root, root.R);
            return new(s.L, s.M, j);
        } else {
            var s = Split(root.R, value);
            var j = Join(root.L, root, s.L);
            return new(j, s.M, s.R);
        }
    }
}
