using System;
using System.Runtime.CompilerServices;

namespace Pfm.Collections.IntrusiveTree;

/// <summary>
/// <para>
/// Monoidal operations for a tree node tag.
/// </para>
/// <para>
/// Custom node tags must have the tree's base tag as the first member. Struct layout should be set to sequential
/// so that the base tag is at offset 0 (the traditional C trick of simulating inheritance) so that <c>Unsafe.As</c>
/// returns a reference to the correct object type.
/// </para>
/// <para>
/// Implementations for custom tags must also take into account the tree's base tag.
/// </para>
/// </summary>
/// <typeparam name="TTag">Type of tag implementing this.</typeparam>
public interface ITagTraits<TTag> where TTag : struct, ITagTraits<TTag>
{
    /// <summary>
    /// The monoid's zero value is assigned to <c>null</c> nodes.
    /// </summary>
    abstract static TTag TZero { get; }

    /// <summary>
    /// The monoidal operation that combines <paramref name="left"/> and <paramref name="right"/> and
    /// stores the result in <paramref name="result"/>.
    /// The operation must be associative.
    /// </summary>
    abstract static TTag TPlus(TTag left, TTag right);

    /// <summary>
    /// Default conversion from <typeparamref name="TFrom"/> to <typeparamref name="TTag"/> uses
    /// <see cref="Unsafe.As{TFrom, TTo}(ref TFrom)"/>.
    /// </summary>
    /// <typeparam name="TFrom">Tag type being converted to <typeparamref name="TTag"/>.</typeparam>
    /// <param name="tag">Value to convert.</param>
    /// <returns>Writable reference to a value of type <typeparamref name="TTag"/>.</returns>
    virtual static ref TTag AsSelf<TFrom>(ref TFrom tag) => ref Unsafe.As<TFrom, TTag>(ref tag);
}

// BASE TAG IMPLEMENTATIONS

public struct AvlTreeTag : ITagTraits<AvlTreeTag>
{
    public byte Height;

    public static AvlTreeTag TZero => default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AvlTreeTag TPlus(AvlTreeTag left, AvlTreeTag right) =>
        new() { Height = (byte)(1 + (left.Height > right.Height ? left.Height : right.Height)) };
}
