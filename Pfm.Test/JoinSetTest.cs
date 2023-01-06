using System;
using Pfm.Collections.JoinTree;

namespace Pfm.Test;

// Tests set operations.  Must be used only with immutable nodes!
internal class JoinSetTest<TNodeTraits, TTreeTraits>
    where TNodeTraits : struct, INodeTraits<int>
    where TTreeTraits : struct, ITreeTraits<int>
{
    public static void Run(int size) {
        if ((size & 1) == 1)  // Make it even to simplify tests.
            ++size;
        var test = new JoinSetTest<TNodeTraits, TTreeTraits>(size);
        test.Run();
    }

    private readonly int size;
    private JoinTree<int, TNodeTraits, TTreeTraits> empty;
    private JoinTree<int, TNodeTraits, TTreeTraits> numbers;
    private JoinTree<int, TNodeTraits, TTreeTraits> evens;
    private JoinTree<int, TNodeTraits, TTreeTraits> odds;

    private JoinSetTest(int size) {
        this.size = size;
        empty = default;
        for (int i = 0; i < size; ++i) {
            numbers.Insert(i, out var _);
            if (i % 2 == 0) evens.Insert(i, out var _);
            else odds.Insert(i, out var _);
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

        Assert.True(JoinTree<int, TNodeTraits, TTreeTraits>.SetEquals(empty, empty));
        Assert.True(JoinTree<int, TNodeTraits, TTreeTraits>.SetEquals(numbers, numbers));
        
        Assert.True(!JoinTree<int, TNodeTraits, TTreeTraits>.SetEquals(empty, numbers));
        Assert.True(!JoinTree<int, TNodeTraits, TTreeTraits>.SetEquals(numbers, evens));
        Assert.True(!JoinTree<int, TNodeTraits, TTreeTraits>.SetEquals(numbers, odds));
        Assert.True(!JoinTree<int, TNodeTraits, TTreeTraits>.SetEquals(evens, odds));
    }

    private void CheckUnion() {
        var s1 = JoinTree<int, TNodeTraits, TTreeTraits>.SetUnion(numbers.Copy(), empty.Copy());
        Assert.True(JoinTree<int, TNodeTraits, TTreeTraits>.SetEquals(s1, numbers));
        Assert.True(empty.Root == null);

        var s2 = JoinTree<int, TNodeTraits, TTreeTraits>.SetUnion(evens.Copy(), odds.Copy());
        Assert.True(JoinTree<int, TNodeTraits, TTreeTraits>.SetEquals(s2, numbers));
        Assert.True(JoinTree<int, TNodeTraits, TTreeTraits>.SetEquals(numbers, s2));
        Assert.True(numbers.Root != s2.Root);   // Immutable.

        var s3 = JoinTree<int, TNodeTraits, TTreeTraits>.SetUnion(numbers.Copy(), numbers.Copy());
        Assert.True(JoinTree<int, TNodeTraits, TTreeTraits>.SetEquals(numbers, s3));
        Assert.True(numbers.Root != s3.Root);
    }

    private void CheckIntersection() {
        var s1 = JoinTree<int, TNodeTraits, TTreeTraits>.SetIntersection(numbers.Copy(), empty.Copy());
        Assert.True(s1.Count == 0 && s1.Root == null);

        var s2 = JoinTree<int, TNodeTraits, TTreeTraits>.SetIntersection(evens.Copy(), odds.Copy());
        Assert.True(s2.Count == 0 && s2.Root == null);

        var s3 = JoinTree<int, TNodeTraits, TTreeTraits>.SetIntersection(evens.Copy(), numbers.Copy());
        Assert.True(JoinTree<int, TNodeTraits, TTreeTraits>.SetEquals(s3, evens));

        var s4 = JoinTree<int, TNodeTraits, TTreeTraits>.SetIntersection(odds.Copy(), odds.Copy());
        Assert.True(JoinTree<int, TNodeTraits, TTreeTraits>.SetEquals(s4, odds));
        Assert.True(s4.Root != odds.Root);
    }

    private void CheckDifference() {
        var s1 = JoinTree<int, TNodeTraits, TTreeTraits>.SetDifference(numbers.Copy(), empty.Copy());
        Assert.True(JoinTree<int, TNodeTraits, TTreeTraits>.SetEquals(s1, numbers));

        var s2 = JoinTree<int, TNodeTraits, TTreeTraits>.SetDifference(empty.Copy(), numbers.Copy());
        Assert.True(s2.Root == null);

        var s3 = JoinTree<int, TNodeTraits, TTreeTraits>.SetDifference(odds.Copy(), evens.Copy());
        Assert.True(JoinTree<int, TNodeTraits, TTreeTraits>.SetEquals(s3, odds));

        var s4 = JoinTree<int, TNodeTraits, TTreeTraits>.SetDifference(odds.Copy(), numbers.Copy());
        Assert.True(s4.Root == null);

        var s5 = JoinTree<int, TNodeTraits, TTreeTraits>.SetDifference(numbers.Copy(), odds.Copy());
        Assert.True(JoinTree<int, TNodeTraits, TTreeTraits>.SetEquals(s5, evens));
    }
}
