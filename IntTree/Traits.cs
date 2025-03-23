using System;
using System.Runtime.CompilerServices;
using Podaga.PersistentCollections.Tree;

namespace IntTree;

#region Value traits

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

#endregion

#region Define tree types

public struct AvlIntTree : IIntValueTraits, IAvlJoin<AvlIntTree, int>
{
}

public struct WBIntTree : IIntValueTraits, IWBJoin<WBIntTree, int>
{
}

#endregion