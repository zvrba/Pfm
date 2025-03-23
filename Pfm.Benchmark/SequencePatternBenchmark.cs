using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Podaga.PersistentCollections.Tree;
using IntTree;

namespace Podaga.PersistentCollections.Benchmark;

public class SequencePatternBenchmark
{
    private readonly SequenceProvider sequenceProvider = new();

    [Benchmark]
    public void TreeBenchmark() {
        var coll = new CollectionTreeAdapter<int, AvlIntTree>();
        for (int i = 0; i < AddPermutation.Data.Length; ++i)
            coll.Add(AddPermutation.Data[i]);
        for (int i = 0; i < RemovePermutation.Data.Length; ++i)
            coll.Remove(RemovePermutation.Data[i]);
    }

    [ParamsSource(nameof(SequenceDescriptors))]
    public SequenceProvider.Descriptor AddPermutation { get; set; }

    [ParamsSource(nameof(SequenceDescriptors))]
    public SequenceProvider.Descriptor RemovePermutation { get; set; }

    public IEnumerable<SequenceProvider.Descriptor> SequenceDescriptors => sequenceProvider.Sequences;
}
