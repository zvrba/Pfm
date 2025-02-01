using System;
using System.Collections.Generic;
using Podaga.PersistentCollections.TreeSet;

namespace Podaga.PersistentCollections.Test;

public static class Program
{
    const int SequenceSize = 384;

    public static void Main() {
        // NB! Running time grows at least quadratically with element count.
        var sequences = GetSequences(SequenceSize);

        Vector_BasicTest.Run(3, 2);
        Vector_MutationTest.Run(3, 2);
        //Vector_BasicTest.Run(5, 5); // NB! Slow-ish.

        TreeSet_BasicTest<IntAvlTree>.Run(sequences);
        TreeSet_SetTest<IntAvlTree>.Run(SequenceSize);
        
        TreeSet_BasicTest<IntWBTree>.Run(sequences);
        TreeSet_SetTest<IntWBTree>.Run(SequenceSize);
    }

    private static List<int[]> GetSequences(int max) {
        var ret = new List<int[]>();
        foreach (var g in PermutationGenerators.Generators) {
            var a = new int[max];
            g(a);
            ret.Add(a);
        }
        return ret;
    }

    internal interface IIntValueTraits : IValueTraits<int>
    {
        static void IValueTraits<int>.CombineValues(in int left, ref int middle, in int right) => middle = left;
        static int IValueTraits<int>.CompareKey(in int left, in int right) => left - right;
    }

    internal struct IntAvlTree : IIntValueTraits, IAvlTree<IntAvlTree, int>
    {
    }

    internal struct IntWBTree : IIntValueTraits, IWBTree<IntWBTree, int>
    {
    }
}