using System;
using System.Collections.Generic;

namespace Pfm.Collections.TreeSet;

/// <summary>
/// Basic collection adapter for joinable trees.  This class behaves as a mutable set; however
/// cheap copying/versioning is possible with <see cref="Fork(bool)"/> method.
/// </summary>
/// <remarks>
/// Indexed access to set elements through <c>IReadOnlyList</c> interface takes O(log n) time.
/// <see cref="ISet{T}"/> is a somewhat useless abstraction: all methods take an <see cref="IEnumerable{T}"/>
/// parameter which may be any collection of elements, even containing duplicates. This would force all
/// operations to be implemented in a stupid inefficient looping manner with no room for optimizations
/// (e.g., counting to implement subset checking).  Many <see cref="ISet{T}"/> methods on this class throws
/// <see cref="NotSupportedException"/> if the incoming enumerable is not an <see cref="ISet{T}"/> itself.
/// The code contains special optimizations if the incoming enumerable is also a joinable tree set.
/// NB: <see cref="ISet{T}.SymmetricExceptWith(IEnumerable{T})"/> is not implemented.
/// </remarks>
public partial class JoinableTreeSet<TValue, TTreeTraits> :
    ICollection<TValue>,
    IReadOnlyList<TValue>,
    ISet<TValue>
    where TTreeTraits : struct, IValueTraits<TValue>, IBalanceTraits<TTreeTraits, TValue>
{
    /// <summary>
    /// Tree root.
    /// </summary>
    internal protected TreeNode<TValue> _Root;

    /// <summary>
    /// Instance of tree algorithms and transient tag.
    /// </summary>
    internal protected JoinTree<TValue, TTreeTraits> _Tree;

    /// <summary>
    /// Default constructor: makes an empty tree.
    /// </summary>
    public JoinableTreeSet() {
        _Tree = new();
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
    /// Forks <c>this</c> into separate instances such that modifications to either instance will
    /// not affect the other instance.
    /// </summary>
    /// <param name="immediate">
    /// If true, all nodes are copied immediately.  If false (default), nodes are copied on-demand, i.e.,
    /// only if a modification to one of the instances is attempted.
    /// </param>
    /// <returns>
    /// A new instance with the same contents.
    /// </returns>
    public JoinableTreeSet<TValue, TTreeTraits> Fork(bool immediate = false) {
        var ret = new JoinableTreeSet<TValue, TTreeTraits>();
        if (_Root != null)
            ret._Root = !immediate ? _Root : ret._Tree.Copy(_Root);

        // Change the transient tag of this.  This ensures that modifications to this are invisible to the forked version.
        _Tree = new();
        return ret;
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
    public bool Find(ref TValue value) => JoinTree<TValue, TTreeTraits>.Find(_Root, ref value);

    /// <summary>
    /// Utility overload that does not modify its argument.
    /// </summary>
    /// <seealso cref=" Find(ref TValue)"/>
    public bool Find(TValue value) => JoinTree<TValue, TTreeTraits>.Find(_Root, ref value);

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
        var ma = new JoinTree<TValue, TTreeTraits>.ModificationState() { Input = value };
        _Root = _Tree.Insert(_Root, ref ma);
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
        var ma = new JoinTree<TValue, TTreeTraits>.ModificationState() { Input = value };
        _Root = _Tree.Delete(_Root, ref ma);
        if (ma.Success)
            value = ma.Output;
        return ma.Success;
    }
}

