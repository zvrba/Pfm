using System;
using System.Diagnostics;

namespace Pfm.Collections.TreeSet;

public partial class JoinTree<TValue, TValueTraits, TTreeTraits, TPersistenceTraits>
    where TValueTraits : struct, IValueTraits<TValue>
    where TTreeTraits : struct, ITreeTraits<TValue>
    where TPersistenceTraits : struct, IPersistenceTraits<TValue>
{
    /// <summary>
    /// Grafts a node into the tree.  This method is protected because it is somewhat "dangerous": all preconditions
    /// for the parameters must be met, otherwise the behavior is unspecified, i.e., the tree may get corrupt.
    /// </summary>
    /// <param name="node">
    /// Node to graft: the value must not already exist in the tree.
    /// </param>
    /// <param name="iterator">
    /// Top node must be the correct parent node for grafting, i.e., the iterator must not be empty.
    /// (Grafting into an empty tree is not supported.)  On return, <paramref name="node"/> will be the top node.
    /// </param>
    /// <param name="c">
    /// Indicates whether the node is grafted as a left (< 0) or right  (> 0) child of the parent.
    /// </param>
    protected void Graft(TreeNode<TValue> node, ref TreeIterator<TValue> iterator, int c) {
        iterator.Push(node);

        var path = iterator.Path;
        var depth = iterator.Depth - 2;
    ascend:
        Debug.Assert(c != 0);
        if (c < 0) node = TTreeTraits.Join(_WA, path[depth + 1], path[depth], path[depth].R);
        else node = TTreeTraits.Join(_WA, path[depth].L, path[depth], path[depth + 1]);

        c = 0;
        if (depth > 0) {
            if (path[depth - 1].L == path[depth]) c = -1;
            else if (path[depth - 1].R == path[depth]) c = 1;
            path[depth--] = node;
            goto ascend;
        }

        path[0] = node;
    }

    /// <summary>
    /// Attempts to insert <paramref name="value"/> into the subtree rooted at <paramref name="root"/>.
    /// </summary>
    /// <param name="root">
    /// On entry: root of the tree into which to insert the value.
    /// On return, set to the new root if the value was inserted.
    /// Allowed to be <c>null</c>, in which case the node is always created and <c>root == node</c> on return.
    /// </param>
    /// <param name="value">Value to insert.</param>
    /// <param name="node">On return, set to the equivalent found or newly inserted value.</param>
    /// <returns>
    /// True if the value did not exist in the tree (and <paramref name="node"/> is set to a new node).
    /// False otherwise (an equivalent value was found) and <paramref name="node"/> is set to the node
    /// containing it.
    /// </returns>
    public bool Insert(ref TreeNode<TValue> root, TValue value, out TreeNode<TValue> node) {
        var c = Find(root, value, ref _WA);
        if (c == 0) {
            node = _WA.Top;
            return false;
        }
        
        node = new(null, value, null) { Size = 1 };
        node.Update<TValueTraits, TTreeTraits>();
        if (root == null) {
            root = node;
        }
        else {
            Graft(node, ref _WA, c);
            root = _WA.Path[0];
        }

        return true;
    }

    /// <summary>
    /// Attempts to insert a node <paramref name="value"/> into the subtree rooted at <paramref name="root"/>.
    /// This method exists to reduce allocations when moving many values between trees.
    /// </summary>
    /// <param name="root">
    /// On entry: root of the tree into which to insert the value.
    /// On return, set to the new root if the value was inserted.
    /// Allowed to be <c>null</c>, in which case the node is always created and <c>root == node</c> on return.
    /// </param>
    /// <param name="value">
    /// On entry, a reference to the node containing the value to insert.  On return, either set to the node
    /// containing an equivalent value, or unchanged.
    /// </param>
    /// <returns>
    /// True if the value did not exist (and <paramref name="value"/> is unchanged), false otherwise (and
    /// <paramref name="value"/> is set to the node with the equivalent value).
    /// </returns>
    public bool Insert(ref TreeNode<TValue> root, ref TreeNode<TValue> value) {
        var c = Find(root, value.V, ref _WA);
        if (c == 0) {
            value = _WA.Top;
            return false;
        }

        value.L = value.R = null;
        value.Size = 1;
        value.Update<TValueTraits, TTreeTraits>();
        if (root == null) {
            root = value;
        } else {
            Graft(value, ref _WA, c);
            root = _WA.Path[0];
        }

        return true;
    }

    /// <summary>
    /// Deletes a value from the tree.
    /// </summary>
    /// <param name="root">
    /// On entry: root of the subtree from which to delete the value.
    /// On return, set to the root of the modified tree, or <c>null</c> if the tree is empty.
    /// </param>
    /// <param name="value">Value to delete.</param>
    /// <param name="node">Set to the node with the found value or null.</param>
    /// <returns>
    /// True if an equivalent value was found (in which case <paramref name="node"/> is set to the node containing it),
    /// false otherwise (in which case <paramref name="node"/> is set to null).
    /// </returns>
    public bool Delete(ref TreeNode<TValue> root, TValue value, out TreeNode<TValue> node) {
        var c = Find(root, value, ref _WA);
        if (c != 0) {
            node = null;
            return false;
        }

        node = Delete(ref _WA);
        root = _WA.IsEmpty ? null : _WA.Path[0];
        return true;
    }

    /// <summary>
    /// Deletes the top value of <paramref name="iterator"/> from the tree.
    /// </summary>
    /// <param name="iterator">
    /// Iterator containing the complete path from the tree root to the node being deleted.
    /// After deletion, the new tree root will be at the bottom (index 0) of the iterator's path.
    /// </param>
    /// <returns>
    /// The deleted node, i.e., node that the iterator's top node upon entry.
    /// </returns>
    public TreeNode<TValue> Delete(ref TreeIterator<TValue> iterator) {
        var node = iterator.TryPop();
        if (!iterator.IsEmpty) {
            Debug.Assert(iterator.Top.L == node || iterator.Top.R == node);
            var c = iterator.Top.L == node ? -1 : 1;
            var n = Join2(node.L, node.R);
            Graft(n, ref iterator, c);
        }
        return node;
    }
}
