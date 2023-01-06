using System;
using System.Collections.Generic;
using Pfm.Collections.CompactTree;

namespace Pfm.Test;

internal class CompactTest
{
    private static readonly NodeTraits<int, AvlTag> NodeTratis = new(
        (x, y) => x - y,
        AvlTag.UpdateTag);

    public static void Run(IEnumerable<int[]> sequences) {
        foreach (var s1 in sequences) {
            foreach (var s2 in sequences) {
                var al = new ChunkedArrayAllocator<int, AvlTag>(4);
                var tree = new AvlTree<int, AvlTag>(al, NodeTratis);
                var test = new CompactTest<AvlTree<int, AvlTag>, AvlTag>(tree, AvlValidator, s1, s2);
                test.Run();
            }
        }
    }

    private static void AvlValidator(AvlTree<int, AvlTag> tree) {
        ValidateHeights(tree.Root);

        int ValidateHeights(Pointer pnode) {
            if (pnode.IsNull)
                return 0;

            ref readonly var node = ref tree[pnode];
            var l = ValidateHeights(node.L);
            var r = ValidateHeights(node.R);
            var h = 1 + (l > r ? l : r);
            Assert.True(node.T.Height == h);
            Assert.True(node.T.Balance == r - l);
            Assert.True(node.T.Balance >= -1 && node.T.Balance <= 1);
            return h;
        }
    }
}

/// <summary>
/// This class mirrors <see cref="ReferenceTest{TNode, TTree}"/>.
/// </summary>
internal class CompactTest<TTree, TTag>
    where TTree : AbstractTree<int, TTag>
    where TTag : struct
{
    private readonly TTree tree;
    private readonly Action<TTree> validator;
    private readonly int[] insert;
    private readonly int[] remove;
    private readonly SortedSet<int> contents;
    private Iterator<int, TTag> iterator;

    public CompactTest(TTree tree, Action<TTree> validator, int[] insert, int[] remove) {
        this.tree = tree;
        this.validator = validator;
        this.insert = insert;
        this.remove = remove;
        this.contents = new();
        this.iterator = new(tree);
    }

    public void Run() {
        Assert.True(tree.Count == 0);
        Assert.True(iterator.First().IsNull && iterator.Last().IsNull);

        // TODO: Traversal during modifications.  Also Fork().
        CheckInsert();
        CheckDelete();

        Assert.True(tree.Count == 0);
        Assert.True(iterator.First().IsNull && iterator.Last().IsNull);

        CheckDeleteRoot();
    }

    private void CheckInsert() {
        bool b;
        for (int i = 0; i < insert.Length; ++i) {
            b = Insert(insert[i]);
            Assert.True(b);
            Verify();
        }
        b = tree.Add(ref iterator, insert.Length / 2, default);
        Assert.True(!b && tree[iterator.Top].V == insert.Length / 2);
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
            var b = Remove(tree[tree.Root].V);
            Assert.True(b);
            Verify();
        }
        Assert.True(tree.Count == 0);
    }

    private bool Insert(int v) {
        if (tree.Add(ref iterator, v, default)) {
            Assert.True(tree[iterator.Top].V == v);
            contents.Add(v);
            return true;
        }
        return false;
    }

    private bool Remove(int v) {
        if (iterator.Find(v) == 0) {
            Assert.True(tree[iterator.Top].V == v);
            tree.Remove(ref iterator);
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
            Assert.True(f);
            Assert.True(p.V == i);
        }

        iterator.First();
        VerifyIteration(iterator, false, contents);

        //iterator.Last();
        //VerifyIteration(ref iterator, iterator.Pred, contents.Reverse());
    }

    private void VerifyOrder(Pointer pnode, out int count, int min, int max) {
        if (pnode.IsNull) {
            count = 0;
            return;
        }

        ref readonly var node = ref tree[pnode];

        Assert.True(min <= max);
        Assert.True(node.V >= min);
        Assert.True(node.V <= max);

        VerifyOrder(node.L, out var lc, min, node.V - 1);
        VerifyOrder(node.R, out var rc, node.V + 1, max);
        count = lc + rc + 1;
    }

    private delegate Pointer Advance(ref Iterator<int, TTag> iterator);

    private void VerifyIteration(
        Iterator<int, TTag> iterator,
        bool backwards,
        IEnumerable<int> reference)
    {
        var r = reference.GetEnumerator();
        while (!iterator.IsEmpty) {
            var b = r.MoveNext();
            Assert.True(b);
            Assert.True(tree[iterator.Top].V == r.Current);

            var n = !backwards ? iterator.Succ() : iterator.Pred();
            Assert.True(iterator.IsEmpty == n.IsNull);
            Assert.True(n.IsNull || iterator.Top == n);
        }
        Assert.True(!r.MoveNext());
    }

}
