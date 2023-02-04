using System;

using Pfm.Collections.TreeSet;

namespace Pfm.Test;

// Tests set operations.  Must be used only with immutable nodes!
internal class TreeSet_SetTest<TTree> where TTree : struct, IValueTraits<int>, IBalanceTraits<TTree, int>
{
    public static void Run(int size) {
        if ((size & 1) == 1)  // Make it even to simplify tests.
            ++size;
        var test = new TreeSet_SetTest<TTree>(size);
        test.Run();
    }

    private readonly int size;
    private readonly JoinableTreeSet<int, TTree> empty = new();
    private readonly JoinableTreeSet<int, TTree> numbers = new();
    private readonly JoinableTreeSet<int, TTree> evens = new();
    private readonly JoinableTreeSet<int, TTree> odds = new();

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
        private readonly JoinableTreeSet<int, TTree>[] checkpreserve;
        private readonly JoinableTreeSet<int, TTree>[] preservecopy;

        public AssertArgumentsPreserved(params JoinableTreeSet<int, TTree>[] trees) {
            checkpreserve = trees;
            checkpreserveroots = new object[trees.Length];
            preservecopy = new JoinableTreeSet<int, TTree>[trees.Length];
            for (int i = 0; i < trees.Length; ++i) {
                checkpreserveroots[i] = trees[i]._Root;
                preservecopy[i] = trees[i].Fork(true);
                Assert.True(preservecopy[i]._Tree != trees[i]._Tree);
                Assert.True((trees[i]._Root == null && preservecopy[i]._Root == null) || preservecopy[i]._Root != trees[i]._Root);
            }
        }

        public void Dispose() {
            for (int i = 0; i < checkpreserve.Length; ++i) {
                Assert.True(JoinableTreeSet.SetEquals(checkpreserve[i], preservecopy[i]));
                Assert.True(checkpreserveroots[i] == checkpreserve[i]._Root);
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
        Assert.True(empty.Count == 0 && empty._Root == null);
        Assert.True(numbers.Count == size);
        Assert.True(evens.Count == size / 2 && odds.Count == size / 2);
        Assert.True(evens.Count + odds.Count == size);

        Assert.True(JoinableTreeSet.SetEquals(empty, empty));
        Assert.True(JoinableTreeSet.SetEquals(numbers, numbers));
        
        Assert.True(!JoinableTreeSet.SetEquals(empty, numbers));
        Assert.True(!JoinableTreeSet.SetEquals(numbers, evens));
        Assert.True(!JoinableTreeSet.SetEquals(numbers, odds));
        Assert.True(!JoinableTreeSet.SetEquals(evens, odds));
    }

    private void CheckUnion() {
        using (var a = new AssertArgumentsPreserved(empty, numbers, evens, odds)) {
            var s1 = JoinableTreeSet.SetUnion(numbers, empty);
            Assert.True(JoinableTreeSet.SetEquals(s1, numbers));

            var s2 = JoinableTreeSet.SetUnion(numbers, numbers);
            Assert.True(JoinableTreeSet.SetEquals(numbers, s2));
            Assert.True(numbers._Root != s2._Root);

            var s3 = JoinableTreeSet.SetUnion(evens, odds);
            Assert.True(JoinableTreeSet.SetEquals(s3, numbers));
            Assert.True(JoinableTreeSet.SetEquals(numbers, s3));
            Assert.True(numbers._Root != s3._Root);   // Immutable.
        }

        using (var a = new AssertArgumentsPreserved(odds)) {
            var s1 = evens.Fork(true);
            s1.SetUnion(odds);
            Assert.True(JoinableTreeSet.SetEquals(numbers, s1));
        }
    }

    private void CheckIntersection() {
        using (var a = new AssertArgumentsPreserved(empty, numbers, evens, odds)) {
            var s1 = JoinableTreeSet.SetIntersection(numbers, empty);
            Assert.True(s1.Count == 0 && s1._Root == null);

            var s2 = JoinableTreeSet.SetIntersection(evens, odds);
            Assert.True(s2.Count == 0 && s2._Root == null);

            var s3 = JoinableTreeSet.SetIntersection(evens, numbers);
            Assert.True(JoinableTreeSet.SetEquals(s3, evens));

            var s4 = JoinableTreeSet.SetIntersection(odds, odds);
            Assert.True(JoinableTreeSet.SetEquals(s4, odds));
            Assert.True(s4._Root != odds._Root);
        }

        using (var a = new AssertArgumentsPreserved(numbers)) {
            var s1 = odds.Fork(true);
            s1.SetIntersect(numbers);
            Assert.True(JoinableTreeSet.SetEquals(odds, s1));
        }
    }

    private void CheckDifference() {
        using (var a = new AssertArgumentsPreserved(empty, numbers, evens, odds)) {
            var s1 = JoinableTreeSet.SetDifference(numbers, empty);
            Assert.True(JoinableTreeSet.SetEquals(s1, numbers));

            var s2 = JoinableTreeSet.SetDifference(empty, numbers);
            Assert.True(s2._Root == null);

            var s3 = JoinableTreeSet.SetDifference(odds, evens);
            Assert.True(JoinableTreeSet.SetEquals(s3, odds));

            var s4 = JoinableTreeSet.SetDifference(odds, numbers);
            Assert.True(s4._Root == null);

            var s5 = JoinableTreeSet.SetDifference(numbers, odds);
            Assert.True(JoinableTreeSet.SetEquals(s5, evens));
        }

        using (var a = new AssertArgumentsPreserved(evens)) {
            var s1 = numbers.Fork();
            s1.SetSubtract(evens);
            Assert.True(JoinableTreeSet.SetEquals(odds, s1));
        }
    }
}
