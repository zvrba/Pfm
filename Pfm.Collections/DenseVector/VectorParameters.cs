using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Podaga.PersistentCollections.DenseVector;

/// <summary>
/// Used upon vector creation to set the sizes of internal and external nodes.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct VectorParameters
{
    /// <summary>
    /// Constructor.  Both shift parameters must be between 2 and 7 inclusive and <paramref name="eshift"/>
    /// must be less than or equal to <paramref name="ishift"/>.
    /// </summary>
    /// <param name="ishift">Number of bits determining the size of internal nodes.</param>
    /// <param name="eshift">Number of bits determining the size of external nodes.</param>
    public VectorParameters(int ishift, int eshift) {
        if (ishift < 2 || eshift < 2 || ishift > 7 || eshift > ishift)
            throw new ArgumentException("Invalid shift values.");

        Set(ishift, out IShift, out ISize, out IMask);
        Set(eshift, out EShift, out ESize, out EMask);

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

    /// <summary>
    /// "Shift" for internal nodes; this is the value passed to constructor.
    /// </summary>
    public readonly byte IShift;

    /// <summary>
    /// Size of (cont of elements in) internal nodes (precomputed).
    /// </summary>
    public readonly byte ISize;

    /// <summary>
    /// Index mask for internal node elements (precomputed).
    /// </summary>
    public readonly byte IMask;
    private readonly byte _pad0;

    /// <summary>
    /// "Shift" for external nodes; this is the value passed to constructor.
    /// </summary>
    public readonly byte EShift;

    /// <summary>
    /// Size of (count of elements in) external nodes (precomputed).
    /// </summary>
    public readonly byte ESize;

    /// <summary>
    /// Index mask for external node elements (precomputed).
    /// </summary>
    public readonly byte EMask;
    private readonly byte _pad1;
}
