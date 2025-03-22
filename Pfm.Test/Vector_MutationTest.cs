using System;
using System.Collections.Generic;
using Podaga.PersistentCollections.DenseVector;

namespace Podaga.PersistentCollections.Test;

using VI = Vector<int>;

internal class Vector_MutationTest
{
    private readonly VI emptyv;
    private readonly int maxsize;
    private readonly List<VI> vectors;

    private Vector_MutationTest(int ishift, int eshift) {
        emptyv = new(new(ishift, eshift));
        maxsize = 2 << (2 * emptyv.Parameters.IShift + emptyv.Parameters.EShift);
        vectors = new List<VI>(maxsize);
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
        vectors.Add(p);

        for (int i = 1; i < maxsize; ++i) {
            p = vectors[i - 1].Fork();
            p.Push(i - 1);
            vectors.Add(p);
        }

        for (int i = 0; i < maxsize; ++i) {
            Assert.True(vectors[i].Count == i);
            CheckValues(vectors[i], x => x);
        }
    }

    void B_Update() {
        for (int i = 0; i < vectors.Count; ++i) {
            vectors[i] = vectors[i].Fork();
            for (int j = 0; j < vectors[i].Count; ++j)
                vectors[i][j] *= 2;
        }

        for (int i = 0; i < vectors.Count; ++i) {
            Assert.True(vectors[i].Count == i);
            CheckValues(vectors[i], x => x * 2);
        }
    }

    static void CheckValues(VI v, Func<int, int> f) {
        for (int i = 0; i < v.Count; ++i) {
            var actual = v[i];
            var expected = f(i);
            Assert.True(actual == expected);
        }
    }
}
