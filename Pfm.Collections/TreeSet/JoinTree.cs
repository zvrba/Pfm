using System;
using System.Threading;

namespace Podaga.PersistentCollections.TreeSet;

/// <summary>
/// Tree traits are core operations related to tree mechanics.  This interface also provides a number of
/// virtual static operations that depend on the abstract operations declared in this interface.
/// </summary>
/// <typeparam name="TValue">Value type of the tree.</typeparam>
/// <remarks>
/// Provides basic mutation and iteration algorithms over joinable trees.
/// See the paper by Guy Blelloch, Daniel Ferizovic, and Yihan Sun. 2022. Joinable Parallel Balanced Binary Trees.
/// ACM Trans. Parallel Comput. 9, 2, Article 7 (April 2022), 41 pages.  https://doi.org/10.1145/3512769
/// This implementation is not parallel.
/// </remarks>
public partial class JoinTree<TValue, TTreeTraits>
    where TTreeTraits : struct, IValueTraits<TValue>, IBalanceTraits<TTreeTraits, TValue>
{
    // ulong is used as transient tag for nodes as assigning references is somewhat expensive
    // (reference assignment calls a helper method for gc tracking etc.)
    private static ulong TransientCounter;

    private readonly ulong transient;

    public ulong Transient => transient;

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

    public JoinTree() {
        transient = Interlocked.Increment(ref TransientCounter);
    }
}
