using System;
using System.Runtime.CompilerServices;
using Podaga.PersistentCollections.Tree;

namespace IntTree;

public struct IntValueHolder : ICollectionValueHolder<IntValueHolder, int>
{
    public static IntValueHolder Nil => default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Combine(IntValueHolder left, ref IntValueHolder result, IntValueHolder right) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Compare(IntValueHolder left, IntValueHolder right) => left.Value.CompareTo(right.Value);

    public static IntValueHolder Create(int value) => new() { Value = value };

    public int Value { get; set; }
}
