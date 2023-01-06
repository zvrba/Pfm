using System;
using System.Collections.Generic;

using Pfm.Collections.JoinTree;

namespace Pfm.Test;

internal class JoinTest<TNodeTraits, TTreeTraits>
    where TNodeTraits : struct, INodeTraits<int>
    where TTreeTraits : struct, ITreeTraits<int>
{
    public static void Run(IList<int[]> sequences) {
        for (int i = 0; i < sequences.Count; ++i) {
            for (int j = 0; j < sequences.Count; ++j) {
                var test = new JoinTest<TNodeTraits, TTreeTraits>(sequences[i], sequences[j]);
                test.Run();
            }
        }
    }

    private readonly int[] insert;
    private readonly int[] remove;
    private readonly SortedSet<int> contents;

    private JoinTree<int, TNodeTraits, TTreeTraits> tree;

    private JoinTest(int[] insert, int[] remove) {
        this.tree = default;
        this.insert = insert;
        this.remove = remove;
        this.contents = new();
    }

    public void Run() {
        var iterator = tree.GetIterator();
        Assert.True(tree.Count == 0);
        Assert.True(iterator.First() == null && iterator.Last() == null);

        // TODO: Traversal during modifications.  Also Fork().
        CheckInsert();
        CheckIndexAccess();
        CheckDelete();

        iterator = tree.GetIterator();
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
        tree.Insert(insert.Length / 2, out var existing);
        Assert.True(existing != null && existing.V == insert.Length / 2);
    }

    private void CheckIndexAccess() {
        for (int i = 0; i < insert.Length; ++i) {
            var j = tree.Nth(i);
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
            var b = Remove(tree.Root.V);
            Assert.True(b);
            Verify();
        }
        Assert.True(tree.Count == 0);
    }

    private bool Insert(int v) {
        if (tree.Insert(v, out var _)) {
            contents.Add(v);
            return true;
        }
        return false;
    }

    private bool Remove(int v) {
        if (!tree.Delete(v, out var node))
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

        var iterator = tree.GetIterator();

        iterator.First();
        VerifyIteration(iterator, false, contents);

        iterator.Last();
        VerifyIteration(iterator, true, contents.Reverse());
    }

    private void VerifyOrder(Node<int> node, out int count, int min, int max) {
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
        Iterator<int> iterator,
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
