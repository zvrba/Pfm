using System;
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

using Pfm.Collections.Trie;

namespace Pfm.Benchmark;

[HardwareCounters(HardwareCounter.InstructionRetired, HardwareCounter.CacheMisses, HardwareCounter.BranchInstructions)]
public class VectorModifyBenchmark
{
    public const int Size = 16384;

    private ImmutableList<int> list;
    private DenseTrie<int> trie;

    public VectorModifyBenchmark() {
        var b = ImmutableList<int>.Empty.ToBuilder();
        for (int i = 0; i < Size; ++i)
            b.Add(i);
        list = b.ToImmutable();

        trie = new(new(5, 5));
        for (int i = 0; i < Size; ++i)
            trie.Push(i);
    }

    [Benchmark]
    public void ImmutableList() {
        for (int i = 0; i < list.Count; ++i)
            list = list.SetItem(i, list[i] + 1);
    }

    [Benchmark]
    public void DenseTrie() {
        for (int i = 0; i < trie.Count; ++i)
            trie[i] += 1;
    }
}
