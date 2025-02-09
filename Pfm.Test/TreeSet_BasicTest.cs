using System;
using System.Collections.Generic;

using Podaga.PersistentCollections.Tree;

namespace Podaga.PersistentCollections.Test;

internal class TreeSet_BasicTest<TTag, TValueTraits>
    where TTag : struct, ITagTraits<TTag>
    where TValueTraits : struct, IValueTraits<int>
{
    public static void Run(Func<JoinableTree<TTag, int, TValueTraits>> factory, IList<int[]> sequences) {
        for (int i = 0; i < sequences.Count; ++i) {
            for (int j = 0; j < sequences.Count; ++j) {
                var test = new TreeSet_BasicTest<TTag, TValueTraits>(factory, sequences[i], sequences[j]);
                test.Run();
            }
        }
    }

    private readonly int[] insert;
    private readonly int[] remove;
    private readonly SortedSet<int> contents;
    private readonly Func<JoinableTree<TTag, int, TValueTraits>> factory;
    private readonly JoinableTree<TTag, int, TValueTraits> tree;

    private TreeSet_BasicTest(Func<JoinableTree<TTag, int, TValueTraits>> factory, int[] insert, int[] remove) {
        this.factory = factory;
        this.tree = factory();
        this.insert = insert;
        this.remove = remove;
        this.contents = new();
    }

    public void Run() {
        var iterator = tree.GetIterator();
        Assert.True(tree.Root is null);
        Assert.True(!iterator.First(null) && !iterator.Last(null));

        // TODO: Traversal during modifications.  Also Fork().
        CheckInsert();
        CheckIndexAccess();
        CheckDelete();

        iterator = tree.GetIterator();
        Assert.True(tree.Root is null);
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
        b = TryAdd(ref v);
        Assert.True(!b && v == insert.Length / 2);
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

    private void CheckPersistence() {
        var original = new CollectionTreeAdapter<TTag, int, TValueTraits>(factory());
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

    private bool TryAdd(ref int v) {
        var state = new ValueAlgorithms.SearchState<TTag, int, TValueTraits> { Value = v };
        var success = tree.Insert(ref state);
        v = state.Result.V;
        return success;
    }

    private bool Insert(int v) {
        var original = v;
        if (TryAdd(ref v)) {
            Assert.True(v == original);
            contents.Add(v);
            return true;
        }
        return false;
    }

    private bool TryRemove(ref int v) {
        var state = new ValueAlgorithms.SearchState<TTag, int, TValueTraits> { Value = v };
        if (!tree.Delete(ref state))
            return false;
        v = state.Result.V;
        return true;
    }

    private bool Remove(int v) {
        var removed = v;
        if (!TryRemove(ref removed))
            return false;
        Assert.True(removed == v);
        contents.Remove(v);
        return true;
    }

    private void Verify() {
        Assert.True(tree.Count == contents.Count);

        tree.ValidateStructure();

        VerifyOrder(tree.Root, out var traverseCount, contents.Min, contents.Max);
        Assert.True(traverseCount == tree.Count);

        foreach (var i in contents) {
            var state = new ValueAlgorithms.SearchState<TTag, int, TValueTraits> { Value = i };
            var b = tree.Find(ref state);
            Assert.True(b && state.Result.V == i);
        }

        var iterator = tree.GetIterator();

        iterator.First(null);
        VerifyIteration(iterator, false, contents);

        iterator.Last(tree.Root);
        VerifyIteration(iterator, true, contents.Reverse());
    }

    private void VerifyOrder(JoinableTreeNode<TTag, int> node, out int count, int min, int max) {
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
        TreeIterator<TTag, int> iterator,
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
