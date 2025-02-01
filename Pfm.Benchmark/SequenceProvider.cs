using System;
using System.Collections.Generic;

namespace Podaga.PersistentCollections.Benchmark;

public class SequenceProvider
{
    private readonly ICollection<Descriptor> sequences;

    public virtual int SequenceSize => 2048;
    public virtual ICollection<Descriptor> Sequences => sequences;

    public SequenceProvider() {
        sequences = new List<Descriptor>() {
            new("Ascending", SequenceSize, Podaga.Collections.PermutationGenerators.Ascending),
            new("Descending", SequenceSize, Podaga.Collections.PermutationGenerators.Descending),
            new("Balanced", SequenceSize, Podaga.Collections.PermutationGenerators.Balanced),
            new("ZigZag", SequenceSize, Podaga.Collections.PermutationGenerators.ZigZag),
            new("Shifted", SequenceSize, Podaga.Collections.PermutationGenerators.Shifted),
            new("Bitonic", SequenceSize, Podaga.Collections.PermutationGenerators.Bitonic),
            new("Random", SequenceSize, Podaga.Collections.PermutationGenerators.Random),
        };
    }

    public readonly struct Descriptor
    {
        public readonly string Name;
        public readonly int[] Data;
        public Descriptor(string name, int size, Action<int[]> generator) {
            Name = name;
            generator(Data = new int[size]);
        }
        public override string ToString() => string.Format("{0}/{1}", Name, Data.Length);
    }
}
