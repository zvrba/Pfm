using System;
using Podaga.PersistentCollections.Tree;

namespace Podaga.PersistentCollections.Test;

// Tests set operations.  Must be used only with immutable nodes!
internal class TreeSet_SetTest<TTree> where TTree : struct, ITreeTraits<int>
{
    public static void Run(int size) {
        if ((size & 1) == 1)  // Make it even to simplify tests.
            ++size;
        var test = new TreeSet_SetTest<TTree>(size);
        test.Run();
    }

    private readonly int size;
    private readonly SetTreeAdapter<int, TTree> empty = new();
    private readonly SetTreeAdapter<int, TTree> numbers = new();
    private readonly SetTreeAdapter<int, TTree> evens = new();
    private readonly SetTreeAdapter<int, TTree> odds = new();

    private TreeSet_SetTest(int size) {
        this.size = size;
        for (int i = 0; i < size; ++i) {
            numbers.Add(i);
            if (i % 2 == 0) evens.Add(i);
            else odds.Add(i);
        }
    }

    private class AssertArgumentsPreserved : IDisposable
    {
        private readonly object[] checkpreserveroots;
        private readonly SetTreeAdapter<int, TTree>[] checkpreserve;
        private readonly SetTreeAdapter<int, TTree>[] preservecopy;

        public AssertArgumentsPreserved(params SetTreeAdapter<int, TTree>[] trees) {
            checkpreserve = trees;
            checkpreserveroots = new object[trees.Length];
            preservecopy = new SetTreeAdapter<int, TTree>[trees.Length];
            for (int i = 0; i < trees.Length; ++i) {
                checkpreserveroots[i] = trees[i].Root;
                preservecopy[i] = trees[i].Fork(true).AsSet();
                Assert.True((trees[i].Root == null && preservecopy[i].Root == null) || preservecopy[i].Root != trees[i].Root);
            }
        }

        public void Dispose() {
            for (int i = 0; i < checkpreserve.Length; ++i) {
                Assert.True(checkpreserve[i].SetEquals(preservecopy[i]));
                Assert.True(checkpreserveroots[i] == checkpreserve[i].Root);
            }
        }
    }

    private void Run() {
        CheckEquality();
        CheckUnion();
        CheckIntersection();
        CheckDifference();
        Assert.True(empty.Count == 0);
        Assert.True(numbers.Count == size);
        Assert.True(evens.Count == size / 2);
        Assert.True(odds.Count == size / 2);
    }

    private void CheckEquality() {
        Assert.True(empty.Count == 0 && empty.Root == null);
        Assert.True(numbers.Count == size);
        Assert.True(evens.Count == size / 2 && odds.Count == size / 2);
        Assert.True(evens.Count + odds.Count == size);

        Assert.True(empty.SetEquals(empty));
        Assert.True(numbers.SetEquals(numbers));
        
        Assert.True(!empty.SetEquals(numbers));
        Assert.True(!numbers.SetEquals(evens));
        Assert.True(!numbers.SetEquals(odds));
        Assert.True(!evens.SetEquals(odds));
    }

    private void CheckUnion() {
        using (var a = new AssertArgumentsPreserved(empty, numbers, evens, odds)) {
            var s1 = numbers.SetUnion(empty);
            Assert.True(s1.SetEquals(numbers));

            var s2 = numbers.SetUnion(numbers);
            Assert.True(numbers.SetEquals(s2));
            Assert.True(numbers.Root != s2.Root);

            var s3 = evens.SetUnion(odds);
            Assert.True(s3.SetEquals(numbers));
            Assert.True(numbers.SetEquals(s3));
            Assert.True(numbers.Root != s3.Root);   // Immutable.
        }

        using (var a = new AssertArgumentsPreserved(odds)) {
            var s1 = evens.Fork(false).AsSet();
            s1.UnionWith(odds);
            Assert.True(numbers.SetEquals(s1));
        }
    }

    private void CheckIntersection() {
        using (var a = new AssertArgumentsPreserved(empty, numbers, evens, odds)) {
            var s1 = numbers.SetIntersection(empty);
            Assert.True(s1.Count == 0 && s1.Root == null);

            var s2 = evens.SetIntersection(odds);
            Assert.True(s2.Count == 0 && s2.Root == null);

            var s3 = evens.SetIntersection(numbers);
            Assert.True(s3.SetEquals(evens));

            var s4 = odds.SetIntersection(odds);
            Assert.True(s4.SetEquals(odds));
            Assert.True(s4.Root != odds.Root);
        }

        using (var a = new AssertArgumentsPreserved(numbers)) {
            var s1 = odds.Fork(false).AsSet();
            s1.IntersectWith(numbers);
            Assert.True(odds.SetEquals(s1));
        }
    }

    private void CheckDifference() {
        using (var a = new AssertArgumentsPreserved(empty, numbers, evens, odds)) {
            var s1 = numbers.SetDifference(empty);
            Assert.True(s1.SetEquals(numbers));

            var s2 = empty.SetDifference(numbers);
            Assert.True(s2.Root == null);

            var s3 = odds.SetDifference(evens);
            Assert.True(s3.SetEquals(odds));

            var s4 = odds.SetDifference(numbers);
            Assert.True(s4.Root == null);

            var s5 = numbers.SetDifference(odds);
            Assert.True(s5.SetEquals(evens));
        }

        using (var a = new AssertArgumentsPreserved(evens)) {
            var s1 = numbers.Fork(false).AsSet();
            s1.ExceptWith(evens);
            Assert.True(odds.SetEquals(s1));
        }
    }
}
