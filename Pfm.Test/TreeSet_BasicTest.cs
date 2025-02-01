using System;
using System.Collections.Generic;
using Podaga.PersistentCollections.TreeSet;

namespace Podaga.PersistentCollections.Test;

internal class TreeSet_BasicTest<TTree> where TTree : struct, IValueTraits<int>, IBalanceTraits<TTree, int>
{
    public static void Run(IList<int[]> sequences) {
        for (int i = 0; i < sequences.Count; ++i) {
            for (int j = 0; j < sequences.Count; ++j) {
                var test = new TreeSet_BasicTest<TTree>(sequences[i], sequences[j]);
                test.Run();
            }
        }
    }

    private readonly int[] insert;
    private readonly int[] remove;
    private readonly SortedSet<int> contents;
    private readonly JoinableTreeSet<int, TTree> tree;

    private TreeSet_BasicTest(int[] insert, int[] remove) {
        this.tree = new();
        this.insert = insert;
        this.remove = remove;
        this.contents = new();
    }

    public void Run() {
        var iterator = tree.GetIterator();
        Assert.True(tree.Count == 0);
        Assert.True(!iterator.First(null) && !iterator.Last(null));

        // TODO: Traversal during modifications.  Also Fork().
        CheckInsert();
        CheckIndexAccess();
        CheckDelete();

        iterator = tree.GetIterator();
        Assert.True(tree.Count == 0);
        Assert.True(!iterator.First(null) && !iterator.Last(null));

        CheckDeleteRoot();
        CheckPersistence();
    }

    private void CheckInsert() {
        bool b;
        for (int i = 0; i < insert.Length; ++i) {
            b = Insert(insert[i]);
            Assert.True(b);
            Verify();
        }
        int v = insert.Length / 2;
        b = tree.TryAdd(ref v);
        Assert.True(!b && v == insert.Length / 2);
    }

    private void CheckIndexAccess() {
        for (int i = 0; i < insert.Length; ++i) {
            var j = tree[i];
            Assert.True(i == j);
        }
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
            var b = Remove(tree._Root.V);
            Assert.True(b);
            Verify();
        }
        Assert.True(tree.Count == 0);
    }

    private void CheckPersistence() {
        var original = new JoinableTreeSet<int, TTree>();
        for (int i = 0; i < insert.Length; ++i)
            original.Add(insert[i]);
        Assert.True(original.Count == insert.Length);

        var copy = original.Fork();
        for (int i = 0; i < remove.Length; ++i) {
            original.Remove(remove[i]);
            Assert.True(copy.Count == insert.Length);
            for (int j = 0; j < insert.Length; ++j)
                Assert.True(copy.Contains(insert[j]));
        }
        Assert.True(original.Count == 0);
    }

    private bool Insert(int v) {
        var original = v;
        if (tree.TryAdd(ref v)) {
            Assert.True(v == original);
            contents.Add(v);
            return true;
        }
        return false;
    }

    private bool Remove(int v) {
        var removed = v;
        if (!tree.TryRemove(ref removed))
            return false;
        Assert.True(removed == v);
        contents.Remove(v);
        return true;
    }

    private void Verify() {
        Assert.True(tree.Count == contents.Count);

        TTree.ValidateStructure(tree._Root);

        VerifyOrder(tree._Root, out var traverseCount, contents.Min, contents.Max);
        Assert.True(traverseCount == tree.Count);

        foreach (var i in contents) {
            int found = i;
            var b = tree.Find(ref found);
            Assert.True(b && found == i);
        }

        var iterator = tree.GetIterator();

        iterator.First(null);
        VerifyIteration(iterator, false, contents);

        iterator.Last(tree._Root);
        VerifyIteration(iterator, true, contents.Reverse());
    }

    private void VerifyOrder(TreeNode<int> node, out int count, int min, int max) {
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
        TreeIterator<int> iterator,
        bool backwards,
        IEnumerable<int> reference)
    {
        var r = reference.GetEnumerator();
        while (!iterator.IsEmpty) {
            var b1 = r.MoveNext();
            Assert.True(b1);
            Assert.True(iterator.Top.V == r.Current);

            var b2 = !backwards ? iterator.Succ() : iterator.Pred();
            Assert.True(iterator.IsEmpty == !b2);
        }
        Assert.True(!r.MoveNext());
    }
}
