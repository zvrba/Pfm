using System;
using System.Collections.Generic;

namespace Pfm.Benchmark;

public class SequenceProvider
{
    private readonly ICollection<Descriptor> sequences;

    public virtual int SequenceSize => 2048;
    public virtual ICollection<Descriptor> Sequences => sequences;

    public SequenceProvider() {
        sequences = new List<Descriptor>() {
            new("Ascending", SequenceSize, Pfm.Collections.PermutationGenerators.Ascending),
            new("Descending", SequenceSize, Pfm.Collections.PermutationGenerators.Descending),
            new("Balanced", SequenceSize, Pfm.Collections.PermutationGenerators.Balanced),
            new("ZigZag", SequenceSize, Pfm.Collections.PermutationGenerators.ZigZag),
            new("Shifted", SequenceSize, Pfm.Collections.PermutationGenerators.Shifted),
            new("Bitonic", SequenceSize, Pfm.Collections.PermutationGenerators.Bitonic),
            new("Random", SequenceSize, Pfm.Collections.PermutationGenerators.Random),
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
