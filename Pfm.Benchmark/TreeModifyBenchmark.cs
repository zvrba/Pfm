﻿using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows;

using Pfm.Collections.TreeSet;

namespace Pfm.Benchmark;

[HardwareCounters(HardwareCounter.InstructionRetired, HardwareCounter.CacheMisses, HardwareCounter.BranchInstructions)]
[MemoryDiagnoser]
public class TreeModifyBenchmark
{
    public const int Size = 8197;

    private readonly int[] data = new int[Size];

    public TreeModifyBenchmark() {
        for (int i = 0; i < data.Length; ++i)
            data[i] = i;
        Pfm.Collections.PermutationGenerators.Random(data);
    }

    [Benchmark]
    public void AvlTreeSet() {
        JoinableTreeSet<int, Program.IntAvlTree> tree = new();
        for (int i = 0; i < data.Length; ++i)
            tree.Add(data[i]);
        for (int i = data.Length - 1; i >= 0; --i)
            tree.Remove(data[i]);
    }

    [Benchmark]
    public void WBTreeSet() {
        JoinableTreeSet<int, Program.IntWBTree> tree = new();
        for (int i = 0; i < data.Length; ++i)
            tree.Add(data[i]);
        for (int i = data.Length - 1; i >= 0; --i)
            tree.Remove(data[i]);
    }

    [Benchmark]
    public void AvlCOWSet() {
        JoinableTreeSet<int, Program.IntAvlTree> tree = new();
        for (int i = 0; i < data.Length; ++i)
            if ((data[i] & 1) == 1)
                tree.Add(data[i]);

        tree = tree.Fork();
        for (int i = 0; i < data.Length; ++i)
            if ((data[i] & 1) == 0)
                tree.Add(data[i]);

        tree = tree.Fork();
        for (int i = data.Length - 1; i >= 0; --i)
            tree.Remove(data[i]);
    }

    [Benchmark]
    public void WBCOWSet() {
        JoinableTreeSet<int, Program.IntWBTree> tree = new();
        for (int i = 0; i < data.Length; ++i)
            if ((data[i] & 1) == 1)
                tree.Add(data[i]);

        tree = tree.Fork();
        for (int i = 0; i < data.Length; ++i)
            if ((data[i] & 1) == 0)
                tree.Add(data[i]);

        tree = tree.Fork();
        for (int i = data.Length - 1; i >= 0; --i)
            tree.Remove(data[i]);
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
}
