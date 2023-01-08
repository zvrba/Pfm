using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows;

namespace Pfm.Benchmark;

[HardwareCounters(HardwareCounter.InstructionRetired, HardwareCounter.CacheMisses)]
[MemoryDiagnoser]
public class ImplementationBenchmark
{
    public const int Size = 8197;

    private readonly int[] data = new int[Size];

    public ImplementationBenchmark() {
        for (int i = 0; i < data.Length; ++i)
            data[i] = i;
        Pfm.Collections.PermutationGenerators.Random(data);
    }

    [Benchmark]
    public void ReferenceAvlTree() {
        var tree = new Pfm.Collections.ReferenceTree.AvlTree<int>(new Comparison<int>((x, y) => x - y));
        for (int i = 0; i < data.Length; ++i)
            tree.Insert(data[i], out var _);
        for (int i = data.Length - 1; i >= 0; --i)
            tree.Delete(data[i]);
    }

    [Benchmark]
    public void MutableJoinAvlTree() {
        Pfm.Collections.JoinTree.JoinTree<int, MutableTraits, Pfm.Collections.JoinTree.AvlTree<int, MutableTraits>> tree = default;
        for (int i = 0; i < data.Length; ++i)
            tree.Insert(data[i], out var _);
        for (int i = data.Length - 1; i >= 0; --i)
            tree.Delete(data[i], out var _);
    }

    [Benchmark]
    public void ImmutableJoinAvlTree() {
        Pfm.Collections.JoinTree.JoinTree<int, ImmutableTraits, Pfm.Collections.JoinTree.AvlTree<int, ImmutableTraits>> tree = default;
        for (int i = 0; i < data.Length; ++i)
            tree.Insert(data[i], out var _);
        for (int i = data.Length - 1; i >= 0; --i)
            tree.Delete(data[i], out var _);
    }

    [Benchmark]
    public void MutableJoinWBTree() {
        Pfm.Collections.JoinTree.JoinTree<int, MutableTraits, Pfm.Collections.JoinTree.WBTree<int, MutableTraits>> tree = default;
        for (int i = 0; i < data.Length; ++i)
            tree.Insert(data[i], out var _);
        for (int i = data.Length - 1; i >= 0; --i)
            tree.Delete(data[i], out var _);
    }

    [Benchmark]
    public void ImmutableJoinWBTree() {
        Pfm.Collections.JoinTree.JoinTree<int, ImmutableTraits, Pfm.Collections.JoinTree.WBTree<int, ImmutableTraits>> tree = default;
        for (int i = 0; i < data.Length; ++i)
            tree.Insert(data[i], out var _);
        for (int i = data.Length - 1; i >= 0; --i)
            tree.Delete(data[i], out var _);
    }

    [Benchmark]
    public void SortedSet() {
        var s = new System.Collections.Generic.SortedSet<int>();
        for (int i = 0; i < data.Length; ++i)
            s.Add(data[i]);
        for (int i = data.Length - 1; i >= 0; --i)
            s.Remove(data[i]);
    }

    [Benchmark]
    public void ImmutableSet() {
        var s = System.Collections.Immutable.ImmutableSortedSet<int>.Empty;
        for (int i = 0; i < data.Length; ++i)
            s = s.Add(data[i]);
        for (int i = data.Length - 1; i >= 0; --i)
            s = s.Remove(data[i]);
    }

    internal struct MutableTraits : Pfm.Collections.JoinTree.INodeTraits<int>
    {
        public static int Compare(int left, int right) => left - right;
        public static int Merge(int left, int right) => left;
        public static bool IsPersistent => false;
        public static Pfm.Collections.JoinTree.Node<int> Clone(Pfm.Collections.JoinTree.Node<int> node) => node;
    }

    internal struct ImmutableTraits : Pfm.Collections.JoinTree.INodeTraits<int>
    {
        public static int Compare(int left, int right) => left - right;
        public static int Merge(int left, int right) => left;
        public static bool IsPersistent => true;
        public static Pfm.Collections.JoinTree.Node<int> Clone(Pfm.Collections.JoinTree.Node<int> node) => new(node);
    }
}
