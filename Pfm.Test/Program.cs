using System;
using System.Collections.Generic;

namespace Pfm.Test;

public static class Program
{
    public static void Main() {
        // NB! Running time grows at least quadratically with element count.
        var sequences = GetSequences(211);

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

        JoinTest<MutableTraits, Pfm.Collections.JoinTree.AvlTree<int, MutableTraits>>.Run(sequences);
        JoinTest<ImmutableTraits, Pfm.Collections.JoinTree.AvlTree<int, ImmutableTraits>>.Run(sequences);

        JoinTest<MutableTraits, Pfm.Collections.JoinTree.WBTree<int, MutableTraits>>.Run(sequences);
        JoinTest<ImmutableTraits, Pfm.Collections.JoinTree.WBTree<int, ImmutableTraits>>.Run(sequences);

        JoinSetTest<MutableTraits, Pfm.Collections.JoinTree.AvlTree<int, MutableTraits>>.Run(518);
        JoinSetTest<ImmutableTraits, Pfm.Collections.JoinTree.WBTree<int, ImmutableTraits>>.Run(518);

        IntrusiveTreeTest.Run(sequences);

        CompactTest.Run(sequences);
    }

    struct MutableTraits : Pfm.Collections.JoinTree.INodeTraits<int>
    {
        public static int Compare(int left, int right) => left - right;
        public static int Merge(int left, int right) => left;
        public static bool IsPersistent => false;
        public static Pfm.Collections.JoinTree.Node<int> Clone(Pfm.Collections.JoinTree.Node<int> x) => x;
    }

    struct ImmutableTraits : Pfm.Collections.JoinTree.INodeTraits<int>
    {
        public static int Compare(int left, int right) => left - right;
        public static int Merge(int left, int right) => left;
        public static bool IsPersistent => true;
        public static Pfm.Collections.JoinTree.Node<int> Clone(Pfm.Collections.JoinTree.Node<int> x) => new(x);
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