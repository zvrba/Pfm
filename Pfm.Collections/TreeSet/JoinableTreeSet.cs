using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pfm.Collections.TreeSet;

/// <summary>
/// Basic collection adapter for joinable trees.  Persistent sets emulate mutable sets; to preserve a copy of
/// an existing version, use <see cref="Copy(bool)"/> method.
/// </summary>
/// <remarks>
/// Indexed access to set elements through <c>IReadOnlyList</c> interface takes O(log n) time.
/// <see cref="ISet{T}"/> is not implemented because it is a useless abstraction: all methods
/// take an <see cref="IEnumerable{T}"/> parameter which may be any collection of elements,
/// even containing duplicates. This would force all operations to be implemented in a stupid
/// inefficient looping manner with no room for optimizations (e.g., counting to implement
/// subset checking).
/// </remarks>
public partial class JoinableTreeSet<TTree, TValue> :
    ICollection<TValue>,
    IReadOnlyList<TValue>
    where TTree : struct, IJoinTree<TTree, TValue>, IValueTraits<TValue>, IPersistenceTraits<TValue>
{
    /// <summary>
    /// Preallocated iterator that serves as work area for the algorithms.
    /// </summary>
    internal protected TreeIterator<TValue> _Iterator;

    /// <summary>
    /// Tree root.
    /// </summary>
    internal protected TreeNode<TValue> _Root;

    /// <summary>
    /// Default constructor: makes an empty tree.
    /// </summary>
    public JoinableTreeSet() {
        _Iterator.Allocate();
    }

    /// <summary>
    /// Constructor.  May be used to adopt an arbitrary tree node as the root of the new tree.
    /// </summary>
    /// <param name="root">Root node to adopt.</param>
    protected JoinableTreeSet(TreeNode<TValue> root = null) {
        _Root = root;
        _Iterator = new(TreeIterator<TValue>.DefaultCapacity);
    }

    public int Count => _Root?.Size ?? 0;

    /// <summary>
    /// Creates an allocated iterator with only the root node on top (if the tree is not empty).
    /// </summary>
    public TreeIterator<TValue> GetIterator() {
        var ret = TreeIterator<TValue>.New();
        if (_Root != null)
            ret.Push(_Root);
        return ret;
    }

    /// <summary>
    /// Copies all nodes of <c>this</c> if <typeparamref name="TTree"/> is not persistent OR <paramref name="force"/> is true.
    /// </summary>
    /// <param name="force">
    /// If true, also a persistent tree will be copied according using the persistence implementation.
    /// </param>
    /// <returns>
    /// A new instance referring either to the same tree or a copy of the tree, depending on persistence
    /// and <paramref name="force"/>.
    /// </returns>
    public JoinableTreeSet<TTree, TValue> Copy(bool force = false) {
        var root = _Root;
        if (root != null && (!TTree.IsPersistent || force))
            root = IJoinTree<TTree, TValue>.Copy(root);
        return new(root);
    }

    /// <summary>
    /// Finds a value equivalent to <paramref name="value"/>.
    /// </summary>
    /// <param name="value">
    /// On entry: value to search for with all key fields initialized.
    /// On return: if the value is found, the argument is updated with the actual found value.
    /// </param>
    /// <returns>
    /// True if found, in which case <paramref name="value"/> is updated.
    /// Otherwise false, and the argument is unchanged.
    /// </returns>
    public bool Find(ref TValue value) => IJoinTree<TTree, TValue>.Find(_Root, ref value);

    /// <summary>
    /// Utility overload that does not modify its argument.
    /// </summary>
    /// <seealso cref=" Find(ref TValue)"/>
    public bool Find(TValue value) => IJoinTree<TTree, TValue>.Find(_Root, ref value);

    /// <summary>
    /// Attempts to add a value to the set.
    /// </summary>
    /// <param name="value">
    /// On entry: value to add with all key fields initialized.
    /// On return: if the value already exists, the argument is updated with the actual found value.
    /// </param>
    /// <returns>
    /// True if found, in which case <paramref name="value"/> added to the set.
    /// Otherwise false, and <paramref name="value"/> is updated to reflect the found equivalent value.
    /// </returns>
    public bool TryAdd(ref TValue value) {
        var ma = new IJoinTree<TTree, TValue>.ModificationState() { Input = value };
        _Root = IJoinTree<TTree, TValue>.Insert(_Root, ref ma);
        if (!ma.Success)
            value = ma.Output;
        return ma.Success;
    }

    /// <summary>
    /// Attempts to remove a value from the set.
    /// </summary>
    /// <param name="value">
    /// On entry: value to add with all key fields initialized.
    /// On return: if the value exists, the argument is updated with the actual found (and removed) value.
    /// </param>
    /// <returns>
    /// True if removed, in which case <paramref name="value"/> is updated.
    /// Otherwise false, and <paramref name="value"/> is unchanged.
    /// </returns>
    public bool TryRemove(ref TValue value) {
        var ma = new IJoinTree<TTree, TValue>.ModificationState() { Input = value };
        _Root = IJoinTree<TTree, TValue>.Delete(_Root, ref ma);
        if (ma.Success)
            value = ma.Output;
        return ma.Success;
    }
}

