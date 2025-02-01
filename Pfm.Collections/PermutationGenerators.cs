using System;

namespace Podaga.Collections;

/// <summary>
/// Data generators for tree tests.
/// </summary>
public static class PermutationGenerators
{
    private static readonly Random random = new Random(3141);

    public static readonly Action<int[]>[] Generators = new Action<int[]>[] {
        Ascending, Descending, Balanced, ZigZag, Shifted, Bitonic, Random
    };

    public static void Random(int[] a) {
        for (int i = 0; i < a.Length; ++i)
            a[i] = i;
        for (int i = 0; i < a.Length - 1; ++i) {
            var j = random.Next(i, a.Length);
            (a[i], a[j]) = (a[j], a[i]);
        }
    }

    public static void Ascending(int[] a) {
        for (var i = 0; i < a.Length; ++i)
            a[i] = i;
    }

    public static void Descending(int[] a) {
        for (var i = 0; i < a.Length; ++i)
            a[i] = a.Length - i - 1;
    }

    public static void Balanced(int[] a) {
        int i = 0;
        Recurse(0, a.Length - 1);

        // Left and right are inclusive
        void Recurse(int l, int r) {
            if (l <= r) {
                var k = (l + r + 1) / 2;
                a[i++] = k;
                Recurse(l, k - 1);
                Recurse(k + 1, r);
            }
        }
    }

    public static void ZigZag(int[] a) {
        int zig = 0, zag = a.Length - 1;
        for (int i = 0; i < a.Length; ++i) {
            if ((i % 2) == 0) a[i] = zig++;
            else a[i] = zag--;
        }
    }

    public static void Shifted(int[] a) {
        var half = a.Length / 2;
        int i, k;
        for (i = 0, k = half; i < a.Length - half; ++i)
            a[i] = k++;
        for (k = 0; i < a.Length; ++i)
            a[i] = k++;
    }

    public static void Bitonic(int[] a) {
        int i, k;
        for (i = 0, k = 0; k < a.Length; ++i, k += 2)
            a[i] = k;
        for (k = a.Length - 1 - (a.Length & 1); k > 0; ++i, k -= 2)
            a[i] = k;
    }
}
