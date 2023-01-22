using System;

namespace Pfm.Collections.TreeSet;

public partial class JoinTree<TValue, TValueTraits, TTreeTraits, TPersistenceTraits>
    where TValueTraits : struct, IValueTraits<TValue>
    where TTreeTraits : struct, ITreeTraits<TValue>
    where TPersistenceTraits : struct, IPersistenceTraits<TValue>
{

    /// <summary>
    /// Finds a value equivalent to <paramref name="value"/>.
    /// </summary>
    /// <param name="node">Node to start the search from.</param>
    /// <param name="value">
    /// Value to look for; only the key fields must be initialized.
    /// On return, it will be overwritten with the found value, if any.
    /// </param>
    /// <returns>True if an equivalent value was found, fale otherwise.</returns>
    public static bool Find(TreeNode<TValue> node, ref TValue value) {
        var _value = value; // Local copy for optimization.
        int c;
        while (node != null) {
            if ((c = TValueTraits.CompareKey(_value, node.V)) == 0) {
                value = node.V;
                return true;
            }
            node = c < 0 ? node.L : node.R;
        }
        return false;
    }

    /// <summary>
    /// Finds a value equivalent to <paramref name="value"/>.  <see cref="Find(TreeNode{TValue}, ref TValue)"/> is
    /// a more efficient method for use cases not needing an iterator.
    /// </summary>
    /// <param name="node">
    /// Node to start the search from.  If <c>null</c>, the top node of <paramref name="iterator"/> is used.
    /// Otherwise, <paramref name="iterator"/> is cleared and search restarted with the given node.
    /// </param>
    /// <param name="value">Value to look for.  Only the key fields must be initialized.</param>
    /// <param name="iterator">An allocated iterator holding the path to the last visited node during traversal.</param>
    /// <returns>
    /// The result of the last comparison leading to the top node in <paramref name="iterator"/>.
    /// Zero means that an equivalent value was found and is on top of the stack.
    /// If <paramref name="node"/> was <c>null</c>, -1 is returned (arbitrarily).
    /// </returns>
    public static int Find(TreeNode<TValue> node, TValue value, ref TreeIterator<TValue> iterator) {
        if (node != null) iterator.Clear();
        else if (!iterator.IsEmpty) node = iterator.Top;

        var _iterator = iterator;
        int c = -1;
        while (node != null) {
            _iterator.Push(node);
            if ((c = TValueTraits.CompareKey(value, node.V)) == 0)
                break;
            node = c < 0 ? node.L : node.R;
        }
        iterator = _iterator;
        return c;
    }

    /// <summary>
    /// Moves <paramref name="iterator"/> to the smallest value in the subtree.
    /// </summary>
    /// <param name="node">
    /// Node to start the search from.  If <c>null</c>, the top node of <paramref name="iterator"/> is used.
    /// Otherwise, <paramref name="iterator"/> is cleared and search restarted with the given node.
    /// </param>
    /// <param name="iterator">An allocated iterator holding the path to the last visited node during traversal.</param>
    /// <returns>
    /// True if a node was found.  False is returned only when both <paramref name="node"/> is <c>null</c> and
    /// <paramref name="iterator"/> is empty.
    /// </returns>
    public static bool First(TreeNode<TValue> node, ref TreeIterator<TValue> iterator) {
        if (node != null) iterator.Clear();
        else if (!iterator.IsEmpty) node = iterator.Top;

        var _iterator = iterator;   // Local copy for optimization
        for (; node != null; node = node.L)
            _iterator.Push(node);
        iterator = _iterator;
        return !iterator.IsEmpty;
    }

    /// <summary>
    /// Moves <paramref name="iterator"/> to the largest value in the subtree.
    /// </summary>
    /// <param name="node">
    /// Node to start the search from.  If <c>null</c>, the top node of <paramref name="iterator"/> is used.
    /// Otherwise, <paramref name="iterator"/> is cleared and search restarted with the given node.
    /// </param>
    /// <param name="iterator">An allocated iterator holding the path to the last visited node during traversal.</param>
    /// <returns>
    /// True if a node was found.  False is returned only when both <paramref name="node"/> is <c>null</c> and
    /// <paramref name="iterator"/> is empty.
    /// </returns>
    public static bool Last(TreeNode<TValue> node, ref TreeIterator<TValue> iterator) {
        if (node != null) iterator.Clear();
        else if (!iterator.IsEmpty) node = iterator.Top;

        var _iterator = iterator;   // Local copy for optimization
        for (; node != null; node = node.R)
            _iterator.Push(node);
        iterator = _iterator;
        return !iterator.IsEmpty;
    }

    /// <summary>
    /// Moves <paramref name="iterator"/> to the next element in sort order.
    /// </summary>
    /// <param name="iterator">
    /// Iterator pointing to an existing node in the tree.
    /// </param>
    /// <returns>True if the next element exists, false otherwise.</returns>
    public static bool Succ(ref TreeIterator<TValue> iterator) {
        var _iterator = iterator;   // Local copy for optimization
        var found = true;

        var current = _iterator.TryPop();
        if (current == null) {
            found = false;
            goto done;
        }

        if (current.R != null) {
            _iterator.Push(current);
            for (current = current.R; current != null; current = current.L)
                _iterator.Push(current);
        } else {
            TreeNode<TValue> y;
            do {
                y = current;
                if ((current = _iterator.TryPop()) == null) {
                    found = false;
                    goto done;
                }
            } while (y == current.R);
            _iterator.Push(current);
        }

    done:
        iterator = _iterator;
        return found;
    }

    /// <summary>
    /// Moves <paramref name="iterator"/> to the previous element in sort order.
    /// </summary>
    /// <param name="iterator">
    /// Iterator pointing to an existing node in the tree.
    /// </param>
    /// <returns>True if the next element exists, false otherwise.</returns>
    public static bool Pred(ref TreeIterator<TValue> iterator) {
        var _iterator = iterator;   // Local copy for optimization
        var found = true;

        var current = _iterator.TryPop();
        if (current == null) {
            found = false;
            goto done;
        }

        if (current.L != null) {
            _iterator.Push(current);
            for (current = current.L; current != null; current = current.R)
                _iterator.Push(current);
        } else {
            TreeNode<TValue> y;
            do {
                y = current;
                if ((current = _iterator.TryPop()) == null) {
                    found = false;
                    goto done;
                }
            } while (y == current.L);
            _iterator.Push(current);
        }

    done:
        iterator = _iterator;
        return found;
    }

    /// <summary>
    /// Returns the n'th element in sorted order in the tree.  Using this method is more efficient than
    /// <c>First</c>, <c>Last</c>, <c>Succ</c>, <c>Pred</c> in succession.
    /// </summary>
    /// <param name="node">Node from which to start the search.</param>
    /// <param name="index">Order of the element to retrieve.</param>
    /// <returns>The found element.</returns>
    /// <exception cref="IndexOutOfRangeException">
    /// Index is outside of range <c>[0, Size-1)</c>, size being the size of the subtree.
    /// </exception>
    public static TValue Nth(TreeNode<TValue> node, int index) {
        if (index < 0 || index >= node.Size)
            throw new IndexOutOfRangeException("Invalid tree element index.");
        ++index;    // Makes calculations easier.
    loop:
        var l = node.L?.Size ?? 0;
        if (index == l + 1)
            return node.V;
        if (index <= l) {
            node = node.L;
        } else {
            node = node.R;
            index -= l + 1;
        }
        goto loop;
    }

    /// <summary>
    /// Sets <paramref name="iterator"/> the n'th element in sorted order in the tree.
    /// <see cref="Nth(TreeNode{TValue}, int)"/> is a more efficient method for use-cases not needing an iterator.
    /// </summary>
    /// <param name="node">
    /// Node from which to start the search.  If <c>null</c>, the top node of <paramref name="iterator"/> is used.
    /// Otherwise, <paramref name="iterator"/> is cleared and search restarted with the given node.
    /// </param>
    /// <param name="index">Order of the element to retrieve.</param>
    /// <param name="iterator">Position to set.  Must be allocated.</param>
    /// <exception cref="IndexOutOfRangeException">
    /// Index is outside of range <c>[0, Size-1)</c>, size being the size of the subtree.
    /// </exception>
    public static void Nth(TreeNode<TValue> node, int index, ref TreeIterator<TValue> iterator) {
        if (node != null) iterator.Clear();
        else if (!iterator.IsEmpty) node = iterator.Top;

        if (node == null || index < 0 || index >= node.Size)
            throw new IndexOutOfRangeException("Invalid tree element index.");
        ++index;    // Makes calculations easier.


        var _position = iterator;   // Local copy for optimization
    loop:
        _position.Push(node);
        var l = node.L?.Size ?? 0;
        if (index == l + 1) {
            iterator = _position;
            return;
        }
        if (index <= l) {
            node = node.L;
        } else {
            node = node.R;
            index -= l + 1;
        }
        goto loop;
    }
}
