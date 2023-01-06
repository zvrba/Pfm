using System;
using System.Collections.Generic;

namespace Pfm.Test;

public static class Program
{
    public static void Main() {
        // NB! Running time grows at least quadratically with element count.
        var sequences = GetSequences(518);

        Vector_BasicTest.Run(3, 2);
        Vector_MutationTest.Run(3, 2);
        //Vector_BasicTest.Run(5, 5); // NB! Slow-ish.


        ReferenceTest.Run(
            () => new Pfm.Collections.ReferenceTree.AvlTree<int>(new Comparison<int>((x, y) => x - y)),
            sequences);
#if false   // Commented out because the tree gets degenerate, overflowing the iterator.
        ReferenceTest.Run(
            () => new ZipTree<int>(new Comparison<int>((x, y) => x - y)) { Random = new Random(314159) },
            sequences);
#endif

        JoinTest<
            Pfm.Collections.JoinTree.MutableNodeTraits<int>,
            Pfm.Collections.JoinTree.AvlTree<int, Pfm.Collections.JoinTree.MutableNodeTraits<int>>
        >.Run(sequences);

        JoinTest<
            Pfm.Collections.JoinTree.ImmutableNodeTraits<int>,
            Pfm.Collections.JoinTree.AvlTree<int, Pfm.Collections.JoinTree.ImmutableNodeTraits<int>>
        >.Run(sequences);

        JoinTest<
            Pfm.Collections.JoinTree.MutableNodeTraits<int>,
            Pfm.Collections.JoinTree.WBTree<int, Pfm.Collections.JoinTree.MutableNodeTraits<int>>
        >.Run(sequences);

        JoinTest<
            Pfm.Collections.JoinTree.ImmutableNodeTraits<int>,
            Pfm.Collections.JoinTree.WBTree<int, Pfm.Collections.JoinTree.ImmutableNodeTraits<int>>
        >.Run(sequences);

        JoinSetTest<
            Pfm.Collections.JoinTree.MutableNodeTraits<int>,
            Pfm.Collections.JoinTree.AvlTree<int, Pfm.Collections.JoinTree.MutableNodeTraits<int>>
        >.Run(230);

        JoinSetTest<
            Pfm.Collections.JoinTree.ImmutableNodeTraits<int>,
            Pfm.Collections.JoinTree.WBTree<int, Pfm.Collections.JoinTree.ImmutableNodeTraits<int>>
        >.Run(230);

        IntrusiveTreeTest.Run(sequences);

        CompactTest.Run(sequences);
    }

    private static List<int[]> GetSequences(int max) {
        var ret = new List<int[]>();
        foreach (var g in Collections.PermutationGenerators.Generators) {
            var a = new int[max];
            g(a);
            ret.Add(a);
        }
        return ret;
    }
}