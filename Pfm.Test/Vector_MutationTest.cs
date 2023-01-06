using Pfm.Collections.Trie;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Pfm.Test;

using VI = ImmutableVector<int>;
using VNode = Collections.Trie.DenseTrie<int>.Node;

internal class Vector_MutationTest
{
    private readonly VI emptyv;
    private readonly int maxsize;
    private readonly List<VI> persistents;
    private readonly List<VI> transients;

    private Vector_MutationTest(int ishift, int eshift) {
        emptyv = VI.Create(ishift, eshift);
        maxsize = 2 << (2 * emptyv.InternalBits + emptyv.ExternalBits);
        persistents = new List<VI>(maxsize);
        transients = new List<VI>(maxsize);
    }

    public static void Run(int ishift, int eshift) {
        var instance = new Vector_MutationTest(ishift, eshift);
        instance.Run();
    }

    private void Run() {
        A_CreateInitial();
        B_Update();
    }

    void A_CreateInitial() {
        var p = emptyv;
        var t = p.MakeTransient();
        persistents.Add(p);
        transients.Add(t);

        for (int i = 1; i < maxsize; ++i) {
            t = p.MakeTransient();
            p.Push(i - 1);
            t.Push(i - 1);
            persistents.Add(p);
            transients.Add(t);
        }

        for (int i = 0; i < maxsize; ++i) {
            Assert.True(persistents[i] != transients[i]);
            
            Assert.True(persistents[i].Count == i);
            CheckValues(persistents[i], x => x);

            Assert.True(transients[i].Count == i);
            CheckValues(transients[i], x => x);
        }

        // Storage is shared.
        for (int i = 0; i < persistents.Count - 1; ++i) {
            var pn1 = GetExternalNodes(persistents[i].Trie.Root);
            var pn2 = GetExternalNodes(persistents[i + 1].Trie.Root);
            Assert.True(pn1.Zip(pn2).All(x => x.First.Data == x.Second.Data));

            // Transients also share storage as they've been created from the same persistents.
            var tn1 = GetExternalNodes(transients[i].Trie.Root);
            var tn2 = GetExternalNodes(transients[i + 1].Trie.Root);
            Assert.True(tn1.Zip(tn2).All(x => x.First.Data == x.Second.Data));
        }
    }

    void B_Update() {
        for (int i = 0; i < persistents.Count; ++i) {
            var p = persistents[i]; // A struct can't be updated in-place in the list.
            for (int j = 0; j < p.Count; ++j)
                p[j] = p[j] * 2;
            persistents[i] = p;

            var t = transients[i];
            var ot = t;
            Assert.True(t.IsTransient);
            for (int j = 0; j < t.Count; ++j)
                t[j] = t[j] * 3;
            Assert.True(ot == t);
            Assert.True(transients[i] == t);
        }

        for (int i = 0; i < persistents.Count; ++i) {
            Assert.True(persistents[i].Count == i);
            CheckValues(persistents[i], x => x * 2);

            Assert.True(transients[i].Count == i);
            CheckValues(transients[i], x => x * 3);
        }
    }

    static void CheckValues(VI v, Func<int, int> f) {
        for (int i = 0; i < v.Count; ++i) {
            var actual = v[i];
            var expected = f(i);
            Assert.True(actual == expected);
        }
    }

    static IEnumerable<VNode> GetExternalNodes(VNode node) {
        if (node.Data is int[])
            return new VNode[] { node };
        return node.Link.Where(x => !x.IsNull).SelectMany(x => GetExternalNodes(x));
    }
}
