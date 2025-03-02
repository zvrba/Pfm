using System;
using System.Threading;

namespace Podaga.PersistentCollections;

/// <summary>
/// This class is used as a default source of unique transient values throughout this assembly.
/// </summary>
/// <remarks>
/// The generator uses a single, atomic 64-bit counter.  No overflow checking is performed, so the counter can
/// theoretically overflow to 0 and cause transient values to be reused.  This, however is highly unlikely as the
/// program would need to generate 10^9 values per second for almost 585 years.
/// </remarks>
public static class TransientSource
{
    private static ulong NextTransient = 0;

    /// <summary>
    /// Generates a new unique transient value.
    /// </summary>
    /// <returns>
    /// A new transient value.
    /// </returns>
    public static ulong NewTransient() => Interlocked.Increment(ref NextTransient);

    /// <summary>
    /// Conditionally generates a new unique transient value.
    /// </summary>
    /// <param name="transient">Existing transient value to check.</param>
    /// <returns>
    /// <paramref name="transient"/> if it was non-zero, a new transient otherwise.
    /// </returns>
    public static ulong NewTransient(ulong transient) => transient > 0 ? transient : NewTransient();
}

