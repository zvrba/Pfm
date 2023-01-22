using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace Pfm.Benchmark;

[HardwareCounters(HardwareCounter.InstructionRetired, HardwareCounter.CacheMisses, HardwareCounter.BranchInstructions)]
public class FindBenchmark
{
    public const int Size = 8197;

    private readonly int[] data = new int[Size];
    private readonly System.Collections.Generic.SortedSet<int> sortedSet;
    private System.Collections.Immutable.ImmutableSortedSet<int> immTree;

    private Pfm.Collections.TreeSet.JoinableTreeSet<ImplementationBenchmark.MutableAvlTraits, int> joinMutAvlTree = new();
    private Pfm.Collections.TreeSet.JoinableTreeSet<ImplementationBenchmark.ImmutableAvlTraits, int> joinImmAvlTree = new();

#if false
    private Pfm.Collections.JoinTree.JoinTree<int, ImplementationBenchmark.MutableTraits,
        Pfm.Collections.JoinTree.WBTree<int, ImplementationBenchmark.MutableTraits>> joinMutWBTree;
    
    private Pfm.Collections.JoinTree.JoinTree<int, ImplementationBenchmark.ImmutableTraits,
        Pfm.Collections.JoinTree.WBTree<int, ImplementationBenchmark.ImmutableTraits>> joinImmWBTree;
#endif

    public FindBenchmark() {
        for (int i = 0; i < data.Length; ++i)
            data[i] = i;
        Pfm.Collections.PermutationGenerators.Random(data);

        sortedSet = new();
        immTree = System.Collections.Immutable.ImmutableSortedSet<int>.Empty;
        //intrTree = new();

        for (int i = 0; i < data.Length; ++i) {
            sortedSet.Add(data[i]);
            immTree = immTree.Add(data[i]);
            joinMutAvlTree.Add(data[i]);
            joinImmAvlTree.Add(data[i]);
#if false
            joinMutWBTree.Insert(data[i], out var _);
            joinImmWBTree.Insert(data[i], out var _);
#endif
        }
    }

    // Try to make sure code doesn't get optimized out.
    public int C = 0;

    [Benchmark]
    public void MutableJoinAvlTree() {
        for (int i = 0; i < data.Length; ++i)
            C += joinMutAvlTree.Contains(i) ? 1 : 0;
    }

    [Benchmark]
    public void ImmutableJoinAvlTree() {
        for (int i = 0; i < data.Length; ++i)
            C += joinImmAvlTree.Contains(i) ? 1 : 0;
    }

#if false
    [Benchmark]
    public void MutableJoinWBTree() {
        for (int i = 0; i < data.Length; ++i)
            C += joinMutWBTree.Find(i, out var _) ? 1 : 0;
    }

    [Benchmark]
    public void ImmutableJoinWBTree() {
        for (int i = 0; i < data.Length; ++i)
            C += joinImmWBTree.Find(i, out var _) ? 1 : 0;
    }
#endif

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

#if false
    private readonly Pfm.Collections.IntrusiveTree.AvlTree<
        Pfm.Collections.IntrusiveTree.DefaultMutableNode<int, Pfm.Collections.IntrusiveTree.AvlTreeTag>,
        int,
        Pfm.Collections.IntrusiveTree.AvlTreeTag> intrTree;

    [Benchmark]
    public void IntrusiveTree() {
        for (int i = 0; i < data.Length; ++i)
            C += intrTree.Find(i, out var _);
    }

    [Benchmark]
    public void DelegateIntrusiveTree() {
        var d = new Pfm.Collections.IntrusiveTree.DelegateFinder<
            Pfm.Collections.IntrusiveTree.DefaultMutableNode<int, Pfm.Collections.IntrusiveTree.AvlTreeTag>,
            int,
            Pfm.Collections.IntrusiveTree.AvlTreeTag>(
            Pfm.Collections.IntrusiveTree.DefaultMutableNode<int, Pfm.Collections.IntrusiveTree.AvlTreeTag>.GetTraits(),
            intrTree.Root);
        for (int i = 0; i < data.Length; ++i)
            C += d.Find(i, out var _);
    }
#endif
}
