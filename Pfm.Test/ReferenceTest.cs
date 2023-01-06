using System;
using System.Collections.Generic;
using System.Linq;

using Pfm.Collections.ReferenceTree;

namespace Pfm.Test;

/// <summary>
/// Base test for trees.
/// </summary>
internal class ReferenceTest
{
    public static void Run(Func<AbstractTree<int>> treeFactory, IList<int[]> sequences) {
#if false
        var s1 = sequences[2];
        var s2 = sequences[0];
        var tree = treeFactory();
        var test = new ReferenceTest(tree, s1, s2);
        test.Run();
#else
        for (int i = 0; i < sequences.Count(); ++i) {
            for (int j = 0; j < sequences.Count(); ++j) {
                var tree = treeFactory(); //new AvlTree<int>(new Comparison<int>((x, y) => x - y));
                var test = new ReferenceTest(tree, sequences[i], sequences[j]);
                test.Run();
            }
        }
#endif
    }

    private readonly AbstractTree<int> tree;
    private readonly int[] insert;
    private readonly int[] remove;
    private readonly SortedSet<int> contents;
    private AbstractTree<int>.Iterator iterator;

    private ReferenceTest(AbstractTree<int> tree, int[] insert, int[] remove)
    {
        this.tree = tree;
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
        b = tree.Insert(insert.Length / 2, out var existing);
        Assert.True(!b && existing.V == insert.Length / 2);
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
        if (tree.Insert(v, out var node)) {
            Assert.True(node.V == v);
            contents.Add(v);
            return true;
        }
        return false;
    }

    private bool Remove(int v) {
        var node = tree.Delete(v);
        if (node == null)
            return false;
        Assert.True(node.V == v);
        contents.Remove(v);
        return true;
    }

    private void Verify() {
        Assert.True(tree.Count == contents.Count);

        tree.ValidateStructure();

        VerifyOrder(tree.Root, out var traverseCount, contents.Min, contents.Max);
        Assert.True(traverseCount == tree.Count);

        foreach (var i in contents) {
            var f = tree.Find(i, out var p);
            Assert.True(f);
            Assert.True(p.V == i);
        }

        iterator.First();
        VerifyIteration(iterator, false, contents);

        //iterator.Last();
        //VerifyIteration(ref iterator, iterator.Pred, contents.Reverse());
    }

    private void VerifyOrder(AbstractTree<int>.Node node, out int count, int min, int max) {
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

    //private delegate TNode Advance(ref Iterator<int, TNode> iterator);

    private static void VerifyIteration(
        AbstractTree<int>.Iterator iterator,
        bool backwards,
        IEnumerable<int> reference)
    {
        var r = reference.GetEnumerator();
        while (!iterator.IsEmpty) {
            var b = r.MoveNext();
            Assert.True(b);
            Assert.True(iterator.Top.V == r.Current);

            var n = !backwards ? iterator.Succ() : iterator.Pred();
            Assert.True(iterator.IsEmpty == (n == null));
            Assert.True(n == null || iterator.Top == n);
        }
        Assert.True(!r.MoveNext());
    }
}
