using System;
using System.Collections.Generic;
using Pfm.Collections.TreeSet;

namespace Pfm.Test;

public static class Program
{
    const int SequenceSize = 211;

    public static void Main() {
        // NB! Running time grows at least quadratically with element count.
        var sequences = GetSequences(SequenceSize);

        Vector_BasicTest.Run(3, 2);
        Vector_MutationTest.Run(3, 2);
        //Vector_BasicTest.Run(5, 5); // NB! Slow-ish.

        JoinableTreeSet1<MutableAvlTree>.Run(sequences);
        JoinableTreeSet1<ImmutableAvlTree>.Run(sequences);
    }

    internal interface IIntValueTraits : IValueTraits<int>
    {
        static void IValueTraits<int>.CombineValues(in int left, ref int middle, in int right) => middle = left;
        static int IValueTraits<int>.CompareKey(in int left, in int right) => left - right;
    }

    internal struct MutableAvlTree : IIntValueTraits, IAvlTree<MutableAvlTree, int>, IPersistenceTraits<int>.IMutable
    {
    }

    internal struct ImmutableAvlTree : IIntValueTraits, IAvlTree<ImmutableAvlTree, int>, IPersistenceTraits<int>.IShallowImmutable
    {
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