using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Pfm.Collections.Trie;

/// <summary>
/// Used upon vector creation to set the sizes of internal and external nodes.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct TrieParameters
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="ishift">Number of bits determining the size of internal nodes.</param>
    /// <param name="eshift">Number of bits determining the size of external nodes.</param>
    public TrieParameters(int ishift, int eshift) {
        if (ishift < 2 || eshift < 2 || ishift > 7 || eshift > ishift)
            throw new ArgumentException("Invalid shift values.");

        checked {
            Set(ishift, out IShift, out ISize, out IMask);
            Set(eshift, out EShift, out ESize, out EMask);
        }

        Unsafe.SkipInit(out _pad0);
        Unsafe.SkipInit(out _pad1);

        static void Set(int bits, out byte shift, out byte size, out byte mask) {
            checked {
                shift = (byte)bits;
                size = (byte)(1 << bits);
                mask = (byte)(size - 1);
            }
        }
    }



    // Make it take exactly 8 bytes.

    public readonly byte IShift;
    public readonly byte ISize;
    public readonly byte IMask;
    private readonly byte _pad0;
    public readonly byte EShift;
    public readonly byte ESize;
    public readonly byte EMask;
    private readonly byte _pad1;
}
