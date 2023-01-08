using System;
using System.Runtime.CompilerServices;

namespace Pfm.Collections.JoinTree;

/// <summary>
/// <para>
/// Joinable balanced tree based on the paper by
/// Guy Blelloch, Daniel Ferizovic, and Yihan Sun. 2022. Joinable Parallel Balanced Binary Trees. ACM Trans.
/// Parallel Comput. 9, 2, Article 7 (April 2022), 41 pages.  https://doi.org/10.1145/3512769
/// </para>
/// <para>
/// This struct has no  public constructors: a <c>default</c> instance is a valid empty tree.
/// </para>
/// <para>
/// WARNING: all public <c>static</c> operations are destructive to both input trees if node traits are mutable,
/// i.e., do not perform cloning.
/// </para>
/// <para>
/// WARNING: This is a mutable struct holding a reference to the tree root.  All instance methods simulate mutable
/// in-place changes.  To preserve a copy of an immutable tree, make a copy of the struct.
/// </para>
/// </summary>
/// <typeparam name="TValue">
/// Value stored in the tree.  Note that the comparison does not have to take all fields into account, thus
/// making it possible to implement dictionaries.
/// </typeparam>
public struct JoinTree<TValue, TNodeTraits, TTreeTraits>
    where TNodeTraits : struct, INodeTraits<TValue>
    where TTreeTraits : struct, ITreeTraits<TValue>
{
    private Node<TValue> root;

    private JoinTree(Node<TValue> root) => this.root = root;

    /// <summary>
    /// The tree root.  <c>null</c> if the tree is empty.
    /// </summary>
    public Node<TValue> Root => root;

    /// <summary>
    /// Node count in this tree; zero for an empty tree.
    /// </summary>
    public int Count => Root?.Size ?? 0;

    /// <summary>
    /// Returns an iterator for this tree.
    /// </summary>
    public Iterator<TValue> GetIterator() => new(root);

    /// <summary>
    /// Finds a value in the tree.
    /// </summary>
    /// <param name="value">Value to find.</param>
    /// <param name="position">Node containing the value or <c>null</c> if not found.</param>
    /// <returns>True if the value was found, false otherwise.</returns>
    /// <remarks>
    /// For simple searches, this method is more efficient than <see cref="Find(TValue, out Iterator{TValue})"/>
    /// as it does not allocate an iterator or store the path to the node.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public bool Find(TValue value, out Node<TValue> position) {
        Node<TValue> node = root, parent = null;    // Avoid writing to position on hot path; perf penalty.
        int c = -1;
        while (node != null) {
            parent = node;
            if ((c = TNodeTraits.Compare(value, node.V)) == 0)
                break;
            node = c < 0 ? node.L : node.R;
        }

        position = parent;
        return c == 0;
    }

    /// <summary>
    /// Searches for <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to find.</param>
    /// <param name="position">
    /// On entry, must be set to an allocated iterator, belonging to any tree.
    /// On exit, the iterator is set to point to this tree and the top points to the last visited element.
    /// If the tree is empty, the iterator will be empty.
    /// </param>
    /// <returns>
    /// The result of the last comparison that lead to <paramref name="value"/>.
    /// Zero means that an element equal to <paramref name="value"/> has been found.
    /// Non-zero will be returned for an empty tree.
    /// </returns>
    /// <seealso cref="Iterator{TValue}.IsAllocated"/>
    public int Find(TValue value, ref Iterator<TValue> position) {
        if (!position.IsAllocated)
            throw new ArgumentException("Iterator is not allocated.", nameof(position));

        position.Clear();
        if (root == null)
            return -1;
        
        var n = root;
        int c = -1;
        while (n != null) {
            position.Push(n);
            if ((c = TNodeTraits.Compare(value, n.V)) == 0)
                break;
            n = c < 0 ? n.L : n.R;
        }
        return c;
    }

    /// <summary>
    /// Copies all nodes of <c>this</c> if <typeparamref name="TNodeTraits"/> are not persistent - OR -
    /// <paramref name="force"/> is true.
    /// </summary>
    public JoinTree<TValue, TNodeTraits, TTreeTraits> Copy(bool force = false) {
        return (TNodeTraits.IsPersistent && !force) || root == null ? this : new(Copy(root));

        static Node<TValue> Copy(Node<TValue> node) {
            node = new(node);
            if (node.L != null)
                node.L = Copy(node.L);
            if (node.R != null)
                node.R = Copy(node.R);
            return node;
        }
    }

    /// <summary>
    /// Returns the n-th element in collection.
    /// </summary>
    /// <param name="index">Order of the element to retrieve (0 being the smallest).</param>
    /// <returns>The found value.</returns>
    /// <exception cref="IndexOutOfRangeException">
    /// If <paramref name="index"/> is outside of range <c>[0, Count)</c>.
    /// </exception>
    public TValue Nth(int index) {
        if (index < 0 || index >= Count)
            throw new IndexOutOfRangeException("Invalid tree element index.");
        return Nth(root, index + 1);

        static TValue Nth(Node<TValue> n, int i) {
        loop:
            var l = n.L?.Size ?? 0;
            if (i == l + 1)
                return n.V;
            if (i <= l) {
                n = n.L;
            }
            else {
                n = n.R;
                i -= l + 1;
            }
            goto loop;
        }
    }


    /// <summary>
    /// Attempts to insert a value into the tree.
    /// </summary>
    /// <param name="value">Value to insert.</param>
    /// <param name="node">Set to existing node if value was found, null otherwise.</param>
    /// <returns>
    /// True if the value was inserted (and <paramref name="node"/> is set to null), false otherwise.
    /// </returns>
    public bool Insert(TValue value, out Node<TValue> node) {
        root = Insert(root, value, out node);
        return node == null;
    }

    private static Node<TValue> Insert(Node<TValue> root, TValue value, out Node<TValue> existing) {
        if (root == null) {
            existing = null;
            return Rotations<TValue, TNodeTraits, TTreeTraits>.Node(null, value, null);
        }

        int c = TNodeTraits.Compare(value, root.V);
        if (c == 0) {
            existing = root;
            return root;
        }
        if (c < 0) {
            var i = Insert(root.L, value, out existing);
            return TTreeTraits.Join(i, root, root.R);
        } else {
            var i = Insert(root.R, value, out existing);
            return TTreeTraits.Join(root.L, root, i);
        }
    }

    /// <summary>
    /// Attempts to delete a value from the tree.
    /// </summary>
    /// <param name="value">Value to delete.</param>
    /// <param name="node">Set to the removed node if the value was found, null otherwise.</param>
    /// <returns>
    /// True if the value was found and deleted, false otherwise (and <paramref name="node"/> set to null).
    /// </returns>
    public bool Delete(TValue value, out Node<TValue> node) {
        root = Delete(root, value, out node);
        return node != null;
    }

    private static Node<TValue> Delete(Node<TValue> root, TValue value, out Node<TValue> node) {
        if (root == null) {
            node = null;
            return null;
        }

        var c = TNodeTraits.Compare(value, root.V);
        if (c == 0) {
            node = root;
            return Join2(root.L, root.R);
        } else if (c < 0) {
            var d = Delete(root.L, value, out node);
            return TTreeTraits.Join(d, root, root.R);
        } else {
            var d = Delete(root.R, value, out node);
            return TTreeTraits.Join(root.L, root, d);
        }
    }

    /// <summary>
    /// Splits a tree rooted at <paramref name="node"/> into left and right subtrees 
    /// holding respectively values less than and greater than <paramref name="value"/>.
    /// </summary>
    /// <returns>
    /// A structure containing the left and right subtrees and a flag indicating whether <paramref name="value"/> was
    /// found in the tree under <paramref name="node"/>.
    /// </returns>
    public static Node<TValue>.Split Split(Node<TValue> node, TValue value) {
        if (node == null)
            return default;
        var c = TNodeTraits.Compare(value, node.V);
        if (c == 0)
            return new(node.L, node, node.R);
        if (c < 0) {
            var s = Split(node.L, value);
            var j = TTreeTraits.Join(s.R, node, node.R);
            return new(s.L, s.M, j);
        } else {
            var s = Split(node.R, value);
            var j = TTreeTraits.Join(node.L, node, s.L);
            return new(j, s.M, s.R);
        }
    }

    /// <summary>
    /// Returns a tree for which the in-order values are concatenation of in-order values of <paramref name="left"/>
    /// and <paramref name="right"/>. (Thus, like <see cref="Join(Node{TValue}, TValue, Node{TValue})"/>, but without
    /// the middle value.)
    /// </summary>
    public static Node<TValue> Join2(Node<TValue> left, Node<TValue> right) {
        if (left == null)
            return right;
        var n = SplitLast(left, out var leftlast);
        return TTreeTraits.Join(n, leftlast, right);

        static Node<TValue> SplitLast(Node<TValue> node, out Node<TValue> rightmost) {
            if (node.R == null) {
                rightmost = node;
                return node.L;
            }
            var n = SplitLast(node.R, out rightmost);
            var j = TTreeTraits.Join(node.L, node, n);
            return j;
        }
    }

    /// <summary>
    /// Set equality: both sets must contain "same" elements, where element equality is defined by the traits' comparer.
    /// </summary>
    /// <returns>
    /// True if both trees contain the same elements, false otherwise.
    /// </returns>
    public static bool SetEquals(
        JoinTree<TValue, TNodeTraits, TTreeTraits> a,
        JoinTree<TValue, TNodeTraits, TTreeTraits> b)
    {
        if (a.Count != b.Count)
            return false;

        var ai = a.GetIterator();
        var an = ai.First();

        var bi = b.GetIterator();
        var bn = bi.First();

    loop:
        if (an == null || bn == null)
            return an == null && bn == null;
        if (TNodeTraits.Compare(an.V, bn.V) != 0)
            return false;
        an = ai.Succ();
        bn = bi.Succ();
        goto loop;
    }

    /// <summary>
    /// Set-union of <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    /// <returns>Tree containing the union of elements of the two inputs.</returns>
    public static JoinTree<TValue, TNodeTraits, TTreeTraits> SetUnion(
        JoinTree<TValue, TNodeTraits, TTreeTraits> a,
        JoinTree<TValue, TNodeTraits, TTreeTraits> b) => new(Union(a.root, b.root));

    private static Node<TValue> Union(Node<TValue> t1, Node<TValue> t2) {
        if (t1 == null)
            return t2;
        if (t2 == null)
            return t1;

        var s = Split(t1, t2.V);
        var l = Union(s.L, t2.L);
        var r = Union(s.R, t2.R);
        if (s.M != null) {
            t2 = TNodeTraits.Clone(t2);
            t2.V = TNodeTraits.Merge(s.M.V, t2.V);
        }
        return TTreeTraits.Join(l, t2, r);
    }

    /// <summary>
    /// Set-intersection of <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    /// <returns>Tree containing the union of elements of the two inputs.</returns>
    public static JoinTree<TValue, TNodeTraits, TTreeTraits> SetIntersection(
        JoinTree<TValue, TNodeTraits, TTreeTraits> a,
        JoinTree<TValue, TNodeTraits, TTreeTraits> b) => new(Intersection(a.root, b.root));

    private static Node<TValue> Intersection(Node<TValue> t1, Node<TValue> t2) {
        if (t1 == null || t2 == null)
            return null;

        var s = Split(t1, t2.V);
        var l = Intersection(s.L, t2.L);
        var r = Intersection(s.R, t2.R);
        if (s.M != null) {
            t2 = TNodeTraits.Clone(t2);
            t2.V = TNodeTraits.Merge(s.M.V, t2.V);
            return TTreeTraits.Join(l, t2, r);
        }
        return Join2(l, r);
    }

    /// <summary>
    /// Set-difference of <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    /// <returns>Tree containing the union of elements of the two inputs.</returns>
    public static JoinTree<TValue, TNodeTraits, TTreeTraits> SetDifference(
        JoinTree<TValue, TNodeTraits, TTreeTraits> a,
        JoinTree<TValue, TNodeTraits, TTreeTraits> b) => new(Difference(a.root, b.root));

    private static Node<TValue> Difference(Node<TValue> t1, Node<TValue> t2) {
        if (t1 == null)
            return null;
        if (t2 == null)
            return t1;

        var s = Split(t1, t2.V);
        var l = Difference(s.L, t2.L);
        var r = Difference(s.R, t2.R);
        return Join2(l, r);
    }

    internal void ValidateStructure() => TTreeTraits.ValidateStructure(root);
}
