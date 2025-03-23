using System;

namespace Podaga.PersistentCollections;

/// <summary>
/// Data generators for tree tests.
/// </summary>
public static class PermutationGenerators
{
    private static readonly Random random = new Random(3141);

    /// <summary>
    /// This array contains delegates for all predefined patterns.
    /// </summary>
    public static readonly Action<int[]>[] Generators = new Action<int[]>[] {
        Ascending, Descending, Balanced, ZigZag, Shifted, Bitonic, Random
    };

    /// <summary>
    /// Fills <paramref name="a"/> with a random permutation of elements from 0 up to <c>a.Length - 1</c>.
    /// </summary>
    /// <param name="a">Array to fill.</param>
    public static void Random(int[] a) {
        for (int i = 0; i < a.Length; ++i)
            a[i] = i;
        for (int i = 0; i < a.Length - 1; ++i) {
            var j = random.Next(i, a.Length);
            (a[i], a[j]) = (a[j], a[i]);
        }
    }

    /// <summary>
    /// Fills <paramref name="a"/> with elements in order 0, 1, 2, ..., <c>a.Length-1</c>.
    /// </summary>
    /// <param name="a">Array to fill.</param>
    public static void Ascending(int[] a) {
        for (var i = 0; i < a.Length; ++i)
            a[i] = i;
    }

    /// <summary>
    /// Fills <paramref name="a"/> with elements in order <c>a.Length-1</c>, ..., 2, 1, 0.
    /// </summary>
    /// <param name="a">Array to fill.</param>
    public static void Descending(int[] a) {
        for (var i = 0; i < a.Length; ++i)
            a[i] = a.Length - i - 1;
    }

    /// <summary>
    /// Fills <paramref name="a"/> with a "balanced" permutation of elements from 0 to <c>a.Length-1</c>.
    /// </summary>
    /// <param name="a">Array to fill.</param>
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

    /// <summary>
    /// Fills <paramref name="a"/> with a "zig-zag" permutation of elements from 0 to <c>a.Length-1</c>.
    /// </summary>
    /// <param name="a">Array to fill.</param>
    public static void ZigZag(int[] a) {
        int zig = 0, zag = a.Length - 1;
        for (int i = 0; i < a.Length; ++i) {
            if ((i % 2) == 0) a[i] = zig++;
            else a[i] = zag--;
        }
    }

    /// <summary>
    /// Fills <paramref name="a"/> with a "shifted" (by half length) permutation of elements from 0 to <c>a.Length-1</c>.
    /// </summary>
    /// <param name="a">Array to fill.</param>
    public static void Shifted(int[] a) {
        var half = a.Length / 2;
        int i, k;
        for (i = 0, k = half; i < a.Length - half; ++i)
            a[i] = k++;
        for (k = 0; i < a.Length; ++i)
            a[i] = k++;
    }

    /// <summary>
    /// Fills <paramref name="a"/> with a "bitonic" permutation of elements from 0 to <c>a.Length-1</c>.
    /// </summary>
    /// <param name="a">Array to fill.</param>
    public static void Bitonic(int[] a) {
        int i, k;
        for (i = 0, k = 0; k < a.Length; ++i, k += 2)
            a[i] = k;
        for (k = a.Length - 1 - (a.Length & 1); k > 0; ++i, k -= 2)
            a[i] = k;
    }
}
