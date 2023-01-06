using Pfm.Collections.Trie;

using System;
using System.Linq;

namespace Pfm.Test;

/// <summary>
/// Uses small tree widths for manual testing in debugger.
/// </summary>
internal class Vector_BasicTest
{
    private ImmutableVector<int> v;
    private readonly int l1Size;
    private readonly int l2Size;

    private Vector_BasicTest(int ishift, int eshift) {
        v = ImmutableVector<int>.Create(ishift, eshift);
        l1Size = 1 << (v.InternalBits + v.ExternalBits);
        l2Size = 1 << (2 * v.InternalBits + v.ExternalBits);
    }

    public static void Run(int ishift, int eshift) {
        var instance = new Vector_BasicTest(ishift, eshift);
        instance.Run();
    }

    void Run() {
        A_FillLevel1();
        B_GrowRoot();
        C_Increment();
        D_ShrinkRoot();
    }

    void A_FillLevel1() {
        Assert.True(v.Trie.Root.Link.All(x => x.IsNull));

        for (int i = 0; i < l1Size; ++i) {
            v.Push(i);
            Assert.True(v.Count == i + 1);
            Assert.True(v[i] == i);
        }
        Assert.True(v.Count == l1Size);
        Assert.True(v.Trie.Shift == v.ExternalBits);
    }

    void B_GrowRoot() {
        for (int i = l1Size; i < l2Size; ++i) {
            v.Push(i);
            Assert.True(v.Count == i + 1);
            Assert.True(v[i] == i);
        }
        Assert.True(v.Count == l2Size);
        Assert.True(v.Trie.Shift == v.InternalBits + v.ExternalBits);
    }

    void C_Increment() {
        for (int i = 0; i < v.Count; ++i)
            v[i] = v[i] + 1;
    }

    // Assumes that Increment() has run.
    void D_ShrinkRoot() {
        for (int i = v.Count; i > 0; --i) {
            for (int j = 0; j < v.Count; ++j)
                Assert.True(v[j] == j + 1);
            var e = v.Pop();
            Assert.True(e == v.Count + 1);
        }

        Assert.True(v.Count == 0);
        Assert.True(v.Trie.Shift == v.ExternalBits);
        Assert.True(v.Trie.Root.Link.All(x => x.IsNull));
        Assert.Throws<InvalidOperationException>(() => v.Pop());
    }
}
