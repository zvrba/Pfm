﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

using Pfm.Collections.Trie;

namespace Pfm.Benchmark;

[HardwareCounters(HardwareCounter.InstructionRetired, HardwareCounter.CacheMisses, HardwareCounter.BranchInstructions)]
public class VectorModifyBenchmark
{
    public const int Size = 16384;

    private List<int> mlist;
    private ImmutableList<int> ilist;
    private DenseTrie<int> trie;

    public VectorModifyBenchmark() {
        var b = ImmutableList<int>.Empty.ToBuilder();
        for (int i = 0; i < Size; ++i)
            b.Add(i);
        ilist = b.ToImmutable();

        mlist = new();
        trie = new(new(5, 5));
        for (int i = 0; i < Size; ++i) {
            trie.Push(i);
            mlist.Add(i);
        }
    }

    [Benchmark]
    public void List() {
        for (int i = 0; i < mlist.Count; ++i)
            mlist[i] += 1;
    }

    [Benchmark]
    public void ImmutableList() {
        for (int i = 0; i < ilist.Count; ++i)
            ilist = ilist.SetItem(i, ilist[i] + 1);
    }

    [Benchmark]
    public void DenseTrie() {
        for (int i = 0; i < trie.Count; ++i)
            trie[i] += 1;
    }
}
