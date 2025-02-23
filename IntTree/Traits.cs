using System;
using System.Runtime.CompilerServices;
using Podaga.PersistentCollections.Tree;

namespace IntTree;

public interface IIntValueTraits : IValueTraits<int>
{
    static int IValueTraits<int>.NilTag {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void IValueTraits<int>.CombineTags(int left, ref int result, int right) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int IValueTraits<int>.Compare(int left, int right) =>
        left < right ? -1 : left > right ? 1 : 0;
}

public struct AvlIntValueHolder : IIntValueTraits, IAvlJoin<AvlIntValueHolder, int>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AvlIntValueHolder Create(int value) => new() { Value = value };
    
    public int Value {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _Value = value;
    }
    private int _Value;
}

public struct WBIntValueHolder : IIntValueTraits, IWBJoin<WBIntValueHolder, int>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static WBIntValueHolder Create(int value) => new() { Value = value };

    public int Value {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _Value = value;
    }
    private int _Value;
}
