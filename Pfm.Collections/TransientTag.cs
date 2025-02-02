using System.Threading;

namespace Podaga.PersistentCollections;

/// <summary>
/// Utility methods for transient tags used by the data structures.
/// </summary>
public static class TransientTag
{
    private static ulong Tag;

    /// <summary>
    /// <para>
    /// Generates a new, unique transient tag.
    /// </para>
    /// <para>
    /// Implementation note: a global 64-bit counter is used, and no overflow checking is performed as it is highly unlikely.
    /// </para>
    /// </summary>
    /// <returns>
    /// The tag value.
    /// </returns>
    public static ulong New() => Interlocked.Increment(ref Tag);
}
