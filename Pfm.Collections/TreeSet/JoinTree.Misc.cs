using System;

namespace Podaga.PersistentCollections.TreeSet;

public partial class JoinTree<TValue, TTreeTraits>
    where TTreeTraits : struct, IValueTraits<TValue>, IBalanceTraits<TTreeTraits, TValue>
{
    /// <summary>
    /// Copies all nodes of the tree rooted at <paramref name="root"/>.
    /// Each node is copied if the node's transient tag is different from <c>this</c>.
    /// </summary>
    /// <param name="root">Root of the (sub)tree to copy; must not be null.</param>
    /// </param>
    /// <returns>The root of the copied tree.</returns>
    public TreeNode<TValue> Copy(TreeNode<TValue> root) {
        root = root.Clone<TTreeTraits>(transient);
        if (root.L != null)
            root.L = Copy(root.L);
        if (root.R != null)
            root.R = Copy(root.R);
        return root;
    }

    /// <summary>
    /// Finds a value equivalent to <paramref name="value"/>.  This method is more efficient than using
    /// the equivalent method in <see cref="TreeIterator{TValue}"/>.
    /// </summary>
    /// <param name="root">
    /// Starting node for the search, usually the root of the tree.
    /// </param>
    /// <param name="value">
    /// Value to look for; only the key fields must be initialized.
    /// On return, it will be overwritten with the found value, if any.
    /// </param>
    /// <returns>True if an equivalent value was found, fale otherwise.</returns>
    public static bool Find(TreeNode<TValue> root, ref TValue value) {
        var _value = value; // Local copy for optimization.
        int c;
        while (root != null) {
            if ((c = TTreeTraits.CompareKey(_value, root.V)) == 0) {
                value = root.V;
                return true;
            }
            root = c < 0 ? root.L : root.R;
        }
        return false;
    }

    /// <summary>
    /// Returns the n'th element in sorted order in the tree.  Using this method is more efficient than
    /// the equivalent methods in <see cref="TreeIterator{TValue}"/>.
    /// </summary>
    /// <param name="root">
    /// Starting node for the search, usually the root of the tree.
    /// </param>
    /// <param name="index">Order of the element to retrieve.</param>
    /// <returns>The found element.</returns>
    /// <exception cref="IndexOutOfRangeException">
    /// Index is outside of range <c>[0, Size-1)</c>, size being the size of the subtree.
    /// </exception>
    public static TValue Nth(TreeNode<TValue> root, int index) {
        if (index < 0 || index >= root.Size)
            throw new IndexOutOfRangeException("Invalid tree element index.");
        ++index;    // Makes calculations easier.
    loop:
        var l = root.L?.Size ?? 0;
        if (index == l + 1)
            return root.V;
        if (index <= l) {
            root = root.L;
        } else {
            root = root.R;
            index -= l + 1;
        }
        goto loop;
    }

    /// <summary>
    /// Splits a tree rooted at <paramref name="root"/> into left and right subtrees 
    /// holding respectively values less than and greater than <paramref name="value"/>.
    /// </summary>
    /// <returns>
    /// A structure containing the left and right subtrees and a flag indicating whether <paramref name="value"/> was
    /// found in the tree under <paramref name="root"/>.
    /// </returns>
    public Splitting Split(TreeNode<TValue> root, in TValue value) {
        if (root == null)
            return default;
        var c = TTreeTraits.CompareKey(value, root.V);
        if (c == 0)
            return new(root.L, root, root.R);
        if (c < 0) {
            var s = Split(root.L, value);
            var j = TTreeTraits.Join(this, s.R, root, root.R);
            return new(s.L, s.M, j);
        } else {
            var s = Split(root.R, value);
            var j = TTreeTraits.Join(this, root.L, root, s.L);
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
        return TTreeTraits.Join(this, n, leftlast, right);

        TreeNode<TValue> SplitLast(TreeNode<TValue> node, out TreeNode<TValue> rightmost) {
            if (node.R == null) {
                rightmost = node;
                return node.L;
            }
            var n = SplitLast(node.R, out rightmost);
            var j = TTreeTraits.Join(this, node.L, node, n);
            return j;
        }
    }
}