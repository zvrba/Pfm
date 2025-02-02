#nullable enable
using System;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// This struct combines multiple traits into a complete joinable tree data structure.
/// All algorithms are defined as extension methods on this struct, which is used as the context for cloning and keeping
/// track of the root node.
/// </summary>
/// <typeparam name="TTag">Tag type used for balancing.</typeparam>
/// <typeparam name="TValue">Value type.</typeparam>
/// <typeparam name="TSelf">Implementation of <see cref="ITreeTraits{TTag, TValue}"/>.</typeparam>
public struct JoinableTree<TTag, TValue, TSelf>
    where TTag : struct, ITagTraits<TTag>
    where TSelf : ITreeTraits<TTag, TValue>
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
    /// Describes the result of splitting a joinable tree at a given value.
    /// This is a nested struct tied to the outer generic parameters.
    /// </summary>
    public readonly struct Splitting
    {
        /// <summary>
        /// Left part of the split.
        /// </summary>
        public readonly JoinableTreeNode<TTag, TValue> L;

        /// <summary>
        /// Not null if the node with the splitting value was found in the tree.
        /// </summary>
        public readonly JoinableTreeNode<TTag, TValue> M;

        /// <summary>
        /// Right part of the split.
        /// </summary>
        public readonly JoinableTreeNode<TTag, TValue> R;

        internal Splitting(JoinableTreeNode<TTag, TValue> l, JoinableTreeNode<TTag, TValue> m, JoinableTreeNode<TTag, TValue> r) {
            L = l; M = m; R = r;
        }
    }
}
