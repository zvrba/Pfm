﻿using System;

namespace Pfm.Collections.TreeSet;

// TODO: Adding optional 'transient' parameter to Clone() would allow implementing optional transience, i.e.,
// cloning when needed at the cost of additional reference in the node.  Transient tag can be made optional
// by placing it into the node's tag value tag.  TValue would also need a getter/setter for the transient tag.

/// <summary>
/// Persistence traits determine how mutation of nodes and values is handled.  The interface provides two
/// implementations with default overrides to be used as mixins for constructing the final tree type.
/// </summary>
/// <typeparam name="TValue">Value type of the tree.</typeparam>
/// <seealso cref="IMutable"/>
/// <seealso cref="IImmutable"/>
public interface IPersistenceTraits<TValue>
{
    /// <summary>
    /// Must return true if the tree is persistent, i.e., <see cref="Clone(TreeNode{TValue})"/> returns a new node instance.
    /// </summary>
    abstract static bool IsPersistent { get; }

    /// <summary>
    /// Clones <paramref name="node"/>.  When <see cref="IsPersistent"/> is true, the method must clone the node
    /// (<see cref="TreeNode{TValue}.TreeNode(TreeNode{TValue})"/>) and, if desired, also the value if it is a
    /// reference type with any mutable fields.  When <see cref="IsPersistent"/> is false,
    /// just <paramref name="node"/> can be returned.
    /// </summary>
    abstract static TreeNode<TValue> Clone(TreeNode<TValue> node);

    /// <summary>
    /// Default implementation of <see cref="IPersistenceTraits{TValue}"/> that provides no persistence: nodes
    /// and values are modified in-place.
    /// </summary>
    public interface IMutable : IPersistenceTraits<TValue>
    {
        static bool IPersistenceTraits<TValue>.IsPersistent => false;
        static TreeNode<TValue> IPersistenceTraits<TValue>.Clone(TreeNode<TValue> node) => node;
    }

    /// <summary>
    /// Default implementation of <see cref="IPersistenceTraits{TValue}"/> that provides shallow node persistence:
    /// nodes are cloned before modification, but NOT the contained values.
    /// </summary>
    public interface IShallowImmutable : IPersistenceTraits<TValue>
    {
        static bool IPersistenceTraits<TValue>.IsPersistent => true;
        static TreeNode<TValue> IPersistenceTraits<TValue>.Clone(TreeNode<TValue> node) => new(node);
    }
}