using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace Pfm.Benchmark;

[HardwareCounters(HardwareCounter.InstructionRetired, HardwareCounter.CacheMisses)]
public class FindBenchmark
{
    public const int Size = 8197;

    private readonly int[] data = new int[Size];
    private readonly System.Collections.Generic.SortedSet<int> sortedSet;
    private System.Collections.Immutable.ImmutableSortedSet<int> immTree;
    private readonly Pfm.Collections.ReferenceTree.AvlTree<int> refTree;
    
    private Pfm.Collections.JoinTree.JoinTree<int, ImplementationBenchmark.MutableTraits,
        Pfm.Collections.JoinTree.AvlTree<int, ImplementationBenchmark.MutableTraits>> joinMutAvlTree;

    private Pfm.Collections.JoinTree.JoinTree<int, ImplementationBenchmark.ImmutableTraits,
        Pfm.Collections.JoinTree.AvlTree<int, ImplementationBenchmark.ImmutableTraits>> joinImmAvlTree;

    private Pfm.Collections.JoinTree.JoinTree<int, ImplementationBenchmark.MutableTraits,
        Pfm.Collections.JoinTree.WBTree<int, ImplementationBenchmark.MutableTraits>> joinMutWBTree;
    
    private Pfm.Collections.JoinTree.JoinTree<int, ImplementationBenchmark.ImmutableTraits,
        Pfm.Collections.JoinTree.WBTree<int, ImplementationBenchmark.ImmutableTraits>> joinImmWBTree;

    public FindBenchmark() {
        for (int i = 0; i < data.Length; ++i)
            data[i] = i;
        Pfm.Collections.PermutationGenerators.Random(data);

        sortedSet = new();
        immTree = System.Collections.Immutable.ImmutableSortedSet<int>.Empty;
        refTree = new(new Comparison<int>((x, y) => x - y));
        //intrTree = new();

        for (int i = 0; i < data.Length; ++i) {
            sortedSet.Add(data[i]);
            refTree.Insert(data[i], out var _);
            immTree = immTree.Add(data[i]);
            joinMutAvlTree.Insert(data[i], out var _);
            joinImmAvlTree.Insert(data[i], out var _);
            joinMutWBTree.Insert(data[i], out var _);
            joinImmWBTree.Insert(data[i], out var _);
            //intrTree.Add(data[i], default, out var _);
        }
    }

    // Try to make sure code doesn't get optimized out.
    public int C = 0;

    [Benchmark]
    public void SortedSet() {
        for (int i = 0; i < data.Length; ++i)
            C += sortedSet.Contains(i) ? 1 : 0;
    }

    [Benchmark]
    public void ReferenceTree() {
        for (int i = 0; i < data.Length; ++i)
            C += refTree.Find(i, out var _) ? 1 : 0;
    }

    [Benchmark]
    public void MutableJoinAvlTree() {
        for (int i = 0; i < data.Length; ++i)
            C += joinMutAvlTree.Find(i, out var _) ? 1 : 0;
    }

    [Benchmark]
    public void ImmutableJoinAvlTree() {
        for (int i = 0; i < data.Length; ++i)
            C += joinImmAvlTree.Find(i, out var _) ? 1 : 0;
    }

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
