using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Pfm.Collections.TreeSet;

namespace Pfm.Benchmark;

public class SequencePatternBenchmark
{
    private readonly SequenceProvider sequenceProvider = new();

    [Benchmark]
    public void TreeBenchmark() {
        var tree = new JoinableTreeSet<int, Program.IntAvlTree>();
        var iterator = tree.GetIterator();

        for (int i = 0; i < AddPermutation.Data.Length; ++i)
            tree.Add(AddPermutation.Data[i]);
        for (int i = 0; i < RemovePermutation.Data.Length; ++i)
            tree.Remove(RemovePermutation.Data[i]);
    }

    [ParamsSource(nameof(SequenceDescriptors))]
    public SequenceProvider.Descriptor AddPermutation { get; set; }

    [ParamsSource(nameof(SequenceDescriptors))]
    public SequenceProvider.Descriptor RemovePermutation { get; set; }

    public IEnumerable<SequenceProvider.Descriptor> SequenceDescriptors => sequenceProvider.Sequences;
}
