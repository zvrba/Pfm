using System;
using System.Runtime.CompilerServices;
using Podaga.PersistentCollections.Tree;

namespace IntTree;

public interface IIntValueHolder<TSelf> : ITaggedValueHolder<TSelf, int>
    where TSelf : struct, ITaggedValueHolder<TSelf, int>, ITreeJoin<TSelf>
{
    static TSelf ITaggedValue<TSelf>.Nil {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void ITaggedValue<TSelf>.Combine(TSelf left, ref TSelf result, TSelf right) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int ITaggedValue<TSelf>.Compare(TSelf left, TSelf right) =>
        left.Value < right.Value ? -1 : left.Value > right.Value ? 1 : 0;
}

public struct AvlIntValueHolder :
    IIntValueHolder<AvlIntValueHolder>,
    IAvlJoin<AvlIntValueHolder, AvlIntValueHolder>
{
    public static AvlIntValueHolder Create(int value) => new() { Value = value };
    public int Value { get; set; }
}

public struct WBIntValueHolder :
    IIntValueHolder<WBIntValueHolder>,
    IWBJoin<WBIntValueHolder, WBIntValueHolder>
{
    public static WBIntValueHolder Create(int value) => new() { Value = value };
    public int Value { get; set; }
}
