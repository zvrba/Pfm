using System;
using System.Runtime.CompilerServices;
using Podaga.PersistentCollections.Tree;

namespace IntTree;

public struct AvlIntValueHolder :
    IAvlJoin<AvlIntValueHolder, AvlIntValueHolder>,
    ITaggedValueHolder<AvlIntValueHolder, int>
{
    public static AvlIntValueHolder Nil => default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Combine(AvlIntValueHolder left, ref AvlIntValueHolder result, AvlIntValueHolder right) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Compare(AvlIntValueHolder left, AvlIntValueHolder right) => left.Value.CompareTo(right.Value);

    public static AvlIntValueHolder Create(int value) => new() { Value = value };
    public int Value { get; set; }
}

public struct WBIntValueHolder :
    IWBJoin<WBIntValueHolder, WBIntValueHolder>,
    ITaggedValueHolder<WBIntValueHolder, int>
{
    public static WBIntValueHolder Nil => default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Combine(WBIntValueHolder left, ref WBIntValueHolder result, WBIntValueHolder right) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Compare(WBIntValueHolder left, WBIntValueHolder right) => left.Value.CompareTo(right.Value);

    public static WBIntValueHolder Create(int value) => new() { Value = value };
    public int Value { get; set; }
}
