using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows;

namespace Pfm.Benchmark;

[HardwareCounters(HardwareCounter.InstructionRetired, HardwareCounter.CacheMisses, HardwareCounter.BranchInstructions)]
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
    public void MutableJoinAvlTree() {
        Pfm.Collections.TreeSet.JoinableTreeSet<MutableAvlTraits, int> tree = new();
        for (int i = 0; i < data.Length; ++i)
            tree.Add(data[i]);
        for (int i = data.Length - 1; i >= 0; --i)
            tree.Remove(data[i]);
    }

    [Benchmark]
    public void ImmutableJoinAvlTree() {
        Pfm.Collections.TreeSet.JoinableTreeSet<ImmutableAvlTraits, int> tree = new();
        for (int i = 0; i < data.Length; ++i)
            tree.Add(data[i]);
        for (int i = data.Length - 1; i >= 0; --i)
            tree.Remove(data[i]);
    }

#if false
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
#endif

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

    internal interface IIntValueTraits : Pfm.Collections.TreeSet.IValueTraits<int>
    {
        static void Pfm.Collections.TreeSet.IValueTraits<int>.CombineValues(in int left, ref int middle, in int right) => middle = left;
        static int Pfm.Collections.TreeSet.IValueTraits<int>.CompareKey(in int left, in int right) => left - right;
    }

    internal struct MutableAvlTraits :
        IIntValueTraits,
        Pfm.Collections.TreeSet.IPersistenceTraits<int>.IMutable,
        Pfm.Collections.TreeSet.IAvlTree<MutableAvlTraits, int>
    {
    }

    internal struct ImmutableAvlTraits :
        IIntValueTraits,
        Pfm.Collections.TreeSet.IPersistenceTraits<int>.IShallowImmutable,
        Pfm.Collections.TreeSet.IAvlTree<ImmutableAvlTraits, int>
    {
    }
}
