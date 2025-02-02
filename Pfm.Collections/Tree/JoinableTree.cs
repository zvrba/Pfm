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
}
