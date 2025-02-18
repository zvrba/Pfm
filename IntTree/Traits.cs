using System;
using System.Runtime.CompilerServices;
using Podaga.PersistentCollections.Tree;

namespace IntTree;

public struct IntValue : ITaggedValue<int>
{
    public static int Nil => default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Combine(int left, ref int result, int right) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Compare(int left, int right) => left.CompareTo(right);
}

public struct IntValueHolder : ITaggedValueHolder<IntValueHolder, int>
{
    public static IntValueHolder Nil => default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Combine(IntValueHolder left, ref IntValueHolder result, IntValueHolder right) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Compare(IntValueHolder left, IntValueHolder right) => left.Value.CompareTo(right.Value);

    public static IntValueHolder Create(int value) => new() { Value = value };

    public int Value { get; set; }
}
