using System;

namespace Pfm.Collections.TreeSet;

/// <summary>
/// Provides basic mutation and iteration algorithms over joinable trees.
/// See the paper by Guy Blelloch, Daniel Ferizovic, and Yihan Sun. 2022. Joinable Parallel Balanced Binary Trees.
/// ACM Trans. Parallel Comput. 9, 2, Article 7 (April 2022), 41 pages.  https://doi.org/10.1145/3512769
/// This implementation is not parallel.
/// </summary>
/// <typeparam name="TSelf">Self-reference to the implementing type.</typeparam>
/// <typeparam name="TValue">Value type held by the tree.</typeparam>
/// <typeparam name="TValueTraits">Value traits.</typeparam>
/// <typeparam name="TTreeTraits">Tree traits.</typeparam>
/// <typeparam name="TPersistenceTraits">Persistence traits.</typeparam>
/// <remarks>
/// <para>
/// The only state that this class contains an instance of <see cref="TreeIterator{TValue}"/> that is used as work area.
/// When persistence is used in parallel scenarios, each thread must use own instance of the class.
/// (This concerns only non-static methods).
/// </para>
/// <para>
/// WARNING: Iterating over a non-persistent tree that is being modified yields unspecified results.  The modification
/// does not need to be concurrent either, e.g., using <c>Succ</c> or <c>Pred</c> while inserting or deleting elements
/// will also lead to unspecified results.
/// </para>
/// </remarks>
public partial class JoinTree<TValue, TValueTraits, TTreeTraits, TPersistenceTraits>
    where TValueTraits : struct, IValueTraits<TValue>
    where TTreeTraits : struct, ITreeTraits<TValue>
    where TPersistenceTraits : struct, IPersistenceTraits<TValue>
{
    /// <summary>
    /// Work area for the algorithms.
    /// </summary>
    protected TreeIterator<TValue> _WA;

    public JoinTree() {
        _WA = new(TreeIterator<TValue>.DefaultCapacity);
    }

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

    /// <summary>
    /// Updates balance and monoidal tags of <paramref name="node"/>.  WARNING: The update is in-place,
    /// so the node must have been cloned beforehand.
    /// </summary>
    public static void Update(TreeNode<TValue> node) {
        if (node.L != null && node.R != null) {
            node.Rank = TTreeTraits.CombineBalanceTags(node.L.Rank, node.R.Rank);
            node.Size = 1 + node.L.Size + node.R.Size;
            TValueTraits.CombineTags(node.L.V, ref node.V, node.R.V);
        } else if (node.L != null) {
            node.Rank = TTreeTraits.CombineBalanceTags(node.L.Rank, TTreeTraits.NilBalance);
            node.Size = 1 + node.L.Size;
            TValueTraits.CombineTagsLeft(node.L.V, ref node.V);
        } else if (node.R != null) {
            node.Rank = TTreeTraits.CombineBalanceTags(TTreeTraits.NilBalance, node.R.Rank);
            node.Size = 1 + node.R.Size;
            TValueTraits.CombineTagsRight(ref node.V, node.R.V);
        }
    }

    /// <summary>
    /// Copies all nodes of the tree rooted at <paramref name="node"/>.  The copying is performed also when
    /// the tree is persistent, according to persistence traits.
    /// </summary>
    /// <param name="node">Root of the (sub)tree to copy; must not be null.</param>
    /// <returns>The root of the copied tree.</returns>
    public static TreeNode<TValue> Copy(TreeNode<TValue> node) {
        node = TPersistenceTraits.Clone(node);
        if (node.L != null)
            node.L = Copy(node.L);
        if (node.R != null)
            node.R = Copy(node.R);
        return node;
    }

    /// <summary>
    /// Splits a tree rooted at <paramref name="node"/> into left and right subtrees 
    /// holding respectively values less than and greater than <paramref name="value"/>.
    /// </summary>
    /// <returns>
    /// A structure containing the left and right subtrees and a flag indicating whether <paramref name="value"/> was
    /// found in the tree under <paramref name="node"/>.
    /// </returns>
    public Splitting Split(TreeNode<TValue> node, TValue value) {
        if (node == null)
            return default;
        var c = TValueTraits.CompareKey(value, node.V);
        if (c == 0)
            return new(node.L, node, node.R);
        if (c < 0) {
            var s = Split(node.L, value);
            var j = TTreeTraits.Join(_WA, s.R, node, node.R);
            return new(s.L, s.M, j);
        } else {
            var s = Split(node.R, value);
            var j = TTreeTraits.Join(_WA, node.L, node, s.L);
            return new(j, s.M, s.R);
        }
    }

    /// <summary>
    /// Returns a tree for which the in-order values are concatenation of in-order values of <paramref name="left"/>
    /// and <paramref name="right"/>. (Thus, like <see cref="Join(TreeNode{TValue}, TreeNode{TValue}, TreeNode{TValue})"/>, but without
    /// the middle value.)
    /// </summary>
    public TreeNode<TValue> Join2(TreeNode<TValue> left, TreeNode<TValue> right) {
        if (left == null)
            return right;
        var n = SplitLast(left, out var leftlast);
        return TTreeTraits.Join(_WA, n, leftlast, right);

        TreeNode<TValue> SplitLast(TreeNode<TValue> node, out TreeNode<TValue> rightmost) {
            var originalDepth = _WA.Depth;
            while (node.R != null) {
                _WA.Push(node);
                node = node.R;
            }
            
            rightmost = node;
            var n = node.L;
        
            while (_WA.Depth > originalDepth) {
                node = _WA.TryPop();
                n = TTreeTraits.Join(_WA, node.L, n, n);
            }

            return n;
        }
    }
}

