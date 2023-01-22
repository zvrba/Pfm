using System;
using System.Runtime.CompilerServices;

namespace Pfm.Collections.TreeSet;

/// <summary>
/// Tree traits are core operations related to tree mechanics.  This interface also provides a number of
/// virtual static operations that depend on the abstract operations declared in this interface.
/// </summary>
/// <typeparam name="TSelf">The type implementing both abstract join tree methods and value traits.</typeparam>
/// <typeparam name="TValue">Value type of the tree.</typeparam>
/// <remarks>
/// Provides basic mutation and iteration algorithms over joinable trees.
/// See the paper by Guy Blelloch, Daniel Ferizovic, and Yihan Sun. 2022. Joinable Parallel Balanced Binary Trees.
/// ACM Trans. Parallel Comput. 9, 2, Article 7 (April 2022), 41 pages.  https://doi.org/10.1145/3512769
/// This implementation is not parallel.
/// </remarks>
public partial interface IJoinTree<TSelf, TValue>
    where TSelf : struct, IJoinTree<TSelf, TValue>, IValueTraits<TValue>, IPersistenceTraits<TValue>
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
        TreeNode<TValue> left,
        TreeNode<TValue> middle,
        TreeNode<TValue> right);

    /// <summary>
    /// For exhaustive tests: validates the tree's structure invariant.
    /// </summary>
    /// <exception cref="NotImplementedException">Thrown when a violation of the structure invariant is detected.</exception>
    internal abstract static void ValidateStructure(TreeNode<TValue> root);

    /// <summary>
    /// Describes the result of splitting a joinable tree at a given value.
    /// </summary>
    public readonly struct Splitting
    {
        /// <summary>
        /// Left part of the split.
        /// </summary>
        public readonly TreeNode<TValue> L;

        /// <summary>
        /// Not null if the node with the splitting value was found in the tree.
        /// </summary>
        public readonly TreeNode<TValue> M;

        /// <summary>
        /// Right part of the split.
        /// </summary>
        public readonly TreeNode<TValue> R;

        internal Splitting(TreeNode<TValue> l, TreeNode<TValue> m, TreeNode<TValue> r) {
            L = l; M = m; R = r;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static TreeNode<TValue> JoinBalanced(TreeNode<TValue> l, TreeNode<TValue> m, TreeNode<TValue> r) {
        m = TSelf.Clone(m);
        m.L = l; m.R = r;
        m.Update<TSelf>();
        return m;
    }

}
