#nullable enable
using System;

namespace Podaga.PersistentCollections.Tree;

public struct ModifyState<TValue> where TValue : ITaggedValue<TValue>
{
    public TValue Value;
    public ulong Transient;
    public JoinableTreeNode<TValue>? Found;
}

public static partial class TreeAlgorithms
{
    public static JoinableTreeNode<TValue>? Find<TValue>
        (
        this JoinableTreeNode<TValue>? @this,
        TValue value,
        out int found
        )
        where TValue : ITaggedValue<TValue>
    {
        JoinableTreeNode<TValue>? prev = null;
        var c = -1;
        while (@this != null && c != 0) {
            c = TValue.Compare(value, @this.Value);
            prev = @this;
            @this = c < 0 ? @this.Left : @this.Right;
        }
        found = c;
        return prev;
    }

    public static JoinableTreeNode<TValue> Insert<TValue, TJoin>
        (
        this JoinableTreeNode<TValue>? @this,
        ref ModifyState<TValue> state
        )
        where TValue : ITaggedValue<TValue>
        where TJoin : struct, ITreeJoin<TValue>
    {
        if (@this is null) {
            state.Found = null;
            var n = new JoinableTreeNode<TValue>(state.Transient, state.Value);
            n.Update<TJoin>();
            return n;
        }

        var c = TValue.Compare(state.Value, @this.Value);
        if (c == 0) {
            state.Found = @this;
            return @this;
        }

        if (c < 0) {
            var n = Insert<TValue, TJoin>(@this.Left, ref state);
            if (state.Found != null)
                return @this;

            var jd = new TreeJoin<TValue> { Transient = state.Transient, Left = n, Middle = @this, Right = @this.Right };
            return TJoin.Join(jd);
        } else {
            var n = Insert<TValue, TJoin>(@this.Right, ref state);
            if (state.Found != null)
                return @this;


            var jd = new TreeJoin<TValue> { Transient = state.Transient, Left = @this.Left, Middle = @this, Right = n };
            return TJoin.Join(jd);
        }
    }

    public static JoinableTreeNode<TValue>? Delete<TValue, TJoin>
        (
        this JoinableTreeNode<TValue>? @this,
        ref ModifyState<TValue> state
        )
        where TValue : ITaggedValue<TValue>
        where TJoin : struct, ITreeJoin<TValue>
    {
        if (@this is null) {
            state.Found = null;
            return null;
        }

        var c = TValue.Compare(state.Value, @this.Value);
        if (c == 0) {
            state.Found = @this;
            var jd = new TreeJoin<TValue> { Transient = state.Transient, Left = @this.Left, Right = @this.Right };
            return jd.Join2<TJoin>();
        }

        if (c < 0) {
            var n = Delete<TValue, TJoin>(@this.Left, ref state);
            if (state.Found == null)
                return @this;

            var jd = new TreeJoin<TValue> { Transient = state.Transient, Left = n, Middle = @this, Right = @this.Right };
            return TJoin.Join(jd);
        }else {
            var n = Delete<TValue, TJoin>(@this.Right, ref state);
            if (state.Found == null)
                return @this;

            var jd = new TreeJoin<TValue> { Transient = state.Transient, Left = @this.Left, Middle = @this, Right = n };
            return TJoin.Join(jd);
        }
    }
}
