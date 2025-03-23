using System;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// A joinable tree must provide an implementation of this interface.
/// </summary>
/// <typeparam name="TValue">Tagged value stored in the tree.</typeparam>
public interface ITreeTraits<TValue> : IValueTraits<TValue>
{
    /// <summary>
    /// Rank corresponding to <c>null</c> node.
    /// </summary>
    abstract static int NilRank { get; }

    /// <summary>
    /// Combines node ranks.
    /// </summary>
    /// <param name="left">Rank of the left subtree.</param>
    /// <param name="middle">Rank of the node being updated.</param>
    /// <param name="right">Rank of the right subtree.</param>
    /// <returns>Combined rank value.</returns>
    abstract static int CombineRanks(int left, int middle, int right);

    /// <summary>
    /// 3-way join is the core tree algorithm on which all other operations are based.
    /// When the tree is not persistent, this operation is destructive to all inputs.
    /// </summary>
    /// <param name="jd">Join parameters.  All fields must be initialized.</param>
    /// <returns>
    /// Tree that has same entries and inorder traversal as the node <c>(left, middle, right)</c>.
    /// </returns>
    abstract static JoinableTreeNode<TValue> Join(TreeSection<TValue> jd);

    /// <summary>
    /// This method must validate the tree's structure invariant starting from <paramref name="node"/>.
    /// Mainly for use in stress-tests.
    /// </summary>
    /// <param name="node">Node from which to begin validation.</param>
    /// <exception cref="NotImplementedException">Thrown when a violation of the structure invariant is detected.</exception>
    abstract static void ValidateStructure(JoinableTreeNode<TValue> node);
}


