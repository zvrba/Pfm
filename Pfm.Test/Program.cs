using System;
using System.Collections.Generic;

using IntTree;

using Podaga.PersistentCollections.Tree;

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

        TreeSet_BasicTest<AvlIntTree>.Run(sequences);
        TreeSet_SetTest<AvlIntTree>.Run(SequenceSize);
        
        TreeSet_BasicTest<WBIntTree>.Run(sequences);
        TreeSet_SetTest<WBIntTree>.Run(SequenceSize);
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
}