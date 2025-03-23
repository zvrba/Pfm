using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

using Podaga.PersistentCollections.Tree;
using IntTree;

namespace Podaga.PersistentCollections.Benchmark;

//[HardwareCounters(HardwareCounter.InstructionRetired, HardwareCounter.CacheMisses, HardwareCounter.BranchInstructions)]
public class TreeFindBenchmark
{
    public const int Size = 8197;

    private readonly int[] data = new int[Size];
    private System.Collections.Generic.SortedSet<int> sortedSet;
    private System.Collections.Immutable.ImmutableSortedSet<int> immTree;
    private CollectionTreeAdapter<int, AvlIntTree> avlTreeSet = new();
    private CollectionTreeAdapter<int, WBIntTree> wbTreeSet = new();

#if false
    private Pfm.Collections.JoinTree.JoinTree<int, ImplementationBenchmark.MutableTraits,
        Pfm.Collections.JoinTree.WBTree<int, ImplementationBenchmark.MutableTraits>> joinMutWBTree;
    
    private Pfm.Collections.JoinTree.JoinTree<int, ImplementationBenchmark.ImmutableTraits,
        Pfm.Collections.JoinTree.WBTree<int, ImplementationBenchmark.ImmutableTraits>> joinImmWBTree;
#endif

    public TreeFindBenchmark() {
        for (int i = 0; i < data.Length; ++i)
            data[i] = i;
        PermutationGenerators.Random(data);

        sortedSet = new();
        immTree = System.Collections.Immutable.ImmutableSortedSet<int>.Empty;
        //intrTree = new();

        for (int i = 0; i < data.Length; ++i) {
            sortedSet.Add(data[i]);
            immTree = immTree.Add(data[i]);
            avlTreeSet.Add(data[i]);
            wbTreeSet.Add(data[i]);
        }
    }

    // Try to make sure code doesn't get optimized out.
    public int C = 0;

    [Benchmark]
    public void AvlTree() {
        for (int i = 0; i < data.Length; ++i)
            C += avlTreeSet.Contains(i) ? 1 : 0;
    }

    [Benchmark]
    public void WBTree() {
        for (int i = 0; i < data.Length; ++i)
            C += wbTreeSet.Contains(i) ? 1 : 0;
    }

    [Benchmark]
    public void SortedSet() {
        for (int i = 0; i < data.Length; ++i)
            C += sortedSet.Contains(i) ? 1 : 0;
    }

    [Benchmark]
    public void ImmutableSet() {
        for (int i = 0; i < data.Length; ++i)
            C += immTree.Contains(i) ? 1 : 0;
    }
}
