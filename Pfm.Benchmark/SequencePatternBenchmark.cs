using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Pfm.Collections.ReferenceTree;

namespace Pfm.Benchmark;

public class SequencePatternBenchmark
{
    private readonly SequenceProvider sequenceProvider = new();

    [Benchmark]
    public void TreeBenchmark() {
        var tree = new AvlTree<int>(new Comparison<int>((x, y) => x - y));
        var iterator = tree.GetIterator();

        for (int i = 0; i < AddPermutation.Data.Length; ++i)
            tree.Insert(AddPermutation.Data[i], out var _);
        for (int i = 0; i < RemovePermutation.Data.Length; ++i)
            tree.Delete(RemovePermutation.Data[i]);
    }

    [ParamsSource(nameof(SequenceDescriptors))]
    public SequenceProvider.Descriptor AddPermutation { get; set; }

    [ParamsSource(nameof(SequenceDescriptors))]
    public SequenceProvider.Descriptor RemovePermutation { get; set; }

    public IEnumerable<SequenceProvider.Descriptor> SequenceDescriptors => sequenceProvider.Sequences;
}
