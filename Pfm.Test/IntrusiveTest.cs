using System;
using System.Collections.Generic;

using Pfm.Collections.IntrusiveTree;

namespace Pfm.Test;

internal class IntrusiveTreeTest
{
    public static void Run(IList<int[]> sequences) {
#if false
        var s1 = sequences[1];  // DESC
        var s2 = sequences[0];  // ASC
        var tree = new MutableAvlTree<DefaultMutableNode<int, AvlTreeTag>, int, AvlTreeTag>();
        var test = new MutableKitTest<DefaultMutableNode<int, AvlTreeTag>, AvlTreeTag, AvlTreeTag>(tree, AvlValidator, s1, s2);
        test.Run();
#else
        foreach (var s1 in sequences) {
            foreach (var s2 in sequences) {
                var tree = new AvlTree<DefaultMutableNode<int, AvlTreeTag>, int, AvlTreeTag>();
                var test = new IntrusiveTreeTest<DefaultMutableNode<int, AvlTreeTag>, AvlTreeTag, AvlTreeTag>(tree, AvlValidator, s1, s2);
                test.Run();
            }
        }
#endif
    }

    private static void AvlValidator(object _tree) {
        var tree = (AvlTree<DefaultMutableNode<int, AvlTreeTag>, int, AvlTreeTag>)_tree;
        ValidateHeights(tree.Root);

        static int ValidateHeights(INodeTraits<DefaultMutableNode<int, AvlTreeTag>, int, AvlTreeTag> node) {
            if (node == null)
                return 0;
            var l = ValidateHeights(node.L);
            var r = ValidateHeights(node.R);
            var h = 1 + (l > r ? l : r);
            var b = r - l;
            Assert.True(node.T.Height == h);
            Assert.True(b >= -1 && b <= 1);
            return h;
        }
    }
}

/// <summary>
/// Base test for trees.
/// </summary>
internal class IntrusiveTreeTest<TNode, TTag, TBaseTag>
    where TNode : class, INodeTraits<TNode, int, TTag>
    where TTag : struct, ITagTraits<TTag>
    where TBaseTag : struct, ITagTraits<TBaseTag>

{
    private readonly AbstractIntrusiveTree<TNode, int, TTag, TBaseTag> tree;
    private readonly Action<AbstractIntrusiveTree<TNode, int, TTag, TBaseTag>> validator;
    private readonly int[] insert;
    private readonly int[] remove;
    private readonly SortedSet<int> contents;
    private Iterator<TNode, int, TTag> iterator;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="tree">An empty tree instance.</param>
    /// <param name="validator">Validation delegate.  Should use <see cref="Assert"/> for checks.</param>
    /// <param name="insert">Elements to insert in order; must be a permutation.</param>
    /// <param name="remove">Elements to remove in order; must be a permutation.</param>
    public IntrusiveTreeTest(
        AbstractIntrusiveTree<TNode, int, TTag, TBaseTag> tree,
        Action<object> validator,
        int[] insert,
        int[] remove)
    {
        this.tree = tree;
        this.validator = validator;
        this.insert = insert;
        this.remove = remove;
        this.contents = new();
        this.iterator = tree.GetIterator();
    }

    public void Run() {
        Assert.True(tree.Count == 0);
        Assert.True(iterator.First() == null && iterator.Last() == null);
        
        // TODO: Traversal during modifications.  Also Fork().
        CheckInsert();
        CheckDelete();

        Assert.True(tree.Count == 0);
        Assert.True(iterator.First() == null && iterator.Last() == null);

        CheckDeleteRoot();
    }

    private void CheckInsert() {
        bool b;
        for (int i = 0; i < insert.Length; ++i) {
            b = Insert(insert[i]);
            Assert.True(b);
            Verify();
        }
        b = tree.Add(insert.Length / 2, default, out var node);
        Assert.True(!b && node.V == insert.Length / 2);
    }

    private void CheckDelete() {
        bool b;
        for (int i = 0; i < remove.Length; ++i) {
            b = Remove(remove[i]);
            Assert.True(b);
            Verify();
        }
    }

    // Deleting a root node is a special case.
    private void CheckDeleteRoot() {
        Assert.True(tree.Count == 0);
        for (int i = 0; i < insert.Length; ++i)
            Insert(insert[i]);
        Assert.True(tree.Count == insert.Length);

        while (tree.Count > 0) {
            var b = Remove(tree.Root.V);
            Assert.True(b);
            Verify();
        }
        Assert.True(tree.Count == 0);
    }

    private bool Insert(int v) {
        if (tree.Add(v, default, out var node)) {
            Assert.True(node.V == v);
            contents.Add(v);
            return true;
        }
        return false;
    }

    private bool Remove(int v) {
        TNode node;
        if ((node = tree.Remove(v)) != null) {
            Assert.True(node.V == v);
            contents.Remove(v);
            return true;
        }
        return false;
    }

    private void Verify() {
        Assert.True(tree.Count == contents.Count);

        validator(tree);

        VerifyOrder(tree.Root, out var traverseCount, contents.Min, contents.Max);
        Assert.True(traverseCount == tree.Count);

        foreach (var i in contents) {
            var f = tree.Find(i, out var p);
            Assert.True(f == 0);
            Assert.True(p.V == i);
        }

        iterator.First();
        VerifyIteration(() => iterator.Succ(), contents);

        iterator.Last();
        VerifyIteration(() => iterator.Pred(), contents.Reverse());
    }

    private void VerifyOrder(TNode node, out int count, int min, int max) {
        if (node == null) {
            count = 0;
            return;
        }
        
        Assert.True(min <= max);
        Assert.True(node.V >= min);
        Assert.True(node.V <= max);

        VerifyOrder(node.L, out var lc, min, node.V - 1);
        VerifyOrder(node.R, out var rc, node.V + 1, max);
        count = lc + rc + 1;
    }

    private void VerifyIteration(Action advance, IEnumerable<int> reference)
    {
        var r = reference.GetEnumerator();
        while (iterator.Count > 0) {
            var b = r.MoveNext();
            Assert.True(b);
            Assert.True(iterator.Top.V == r.Current);
            advance();
        }
        Assert.True(!r.MoveNext());
    }
}
