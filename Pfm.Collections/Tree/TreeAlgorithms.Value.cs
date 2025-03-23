#nullable enable
using System;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// This struct is used as input to
/// <see cref="TreeAlgorithms.Insert{TValue, TJoin}(Podaga.PersistentCollections.Tree.JoinableTreeNode{TValue}?, ref Podaga.PersistentCollections.Tree.ModifyState{TValue})"/>
/// and
/// <see cref="TreeAlgorithms.Delete{TValue, TJoin}(Podaga.PersistentCollections.Tree.JoinableTreeNode{TValue}?, ref Podaga.PersistentCollections.Tree.ModifyState{TValue})"/>
/// methods.
/// </summary>
/// <typeparam name="TValue">Tree element type.</typeparam>
public struct ModifyState<TValue>
{
    /// <summary>
    /// The value to insert or delete.
    /// </summary>
    public TValue Value;

    /// <summary>
    /// Transient tag used to determine the need for cloning during tree modifications.
    /// </summary>
    public ulong Transient;

    /// <summary>
    /// A node with <see cref="Value"/> that was found in the tree during insert or delete.
    /// </summary>
    /// <remarks>
    /// This field reflects the status of the operation.  For insert, the operation was successful if this value IS <c>null</c>.
    /// For delete, the operation was successful if this value IS NOT <c>null</c>.
    /// </remarks>
    public JoinableTreeNode<TValue>? Found;
}

public static partial class TreeAlgorithms
{
    /// <summary>
    /// Finds a value in the tree.
    /// </summary>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <typeparam name="TValueTraits">Value traits.</typeparam>
    /// <param name="this">Tree node from which to start the search.</param>
    /// <param name="value">Value to look for.</param>
    /// <param name="found">
    /// Set to the result of comparison with the last visited node.  When 0, the value was found.
    /// </param>
    /// <returns>
    /// The last visited node in the tree.  If <paramref name="found"/> is 0, the node contains a value that
    /// compares equal to <paramref name="value"/>.
    /// </returns>
    public static JoinableTreeNode<TValue>? Find<TValue, TValueTraits>
        (
        this JoinableTreeNode<TValue>? @this,
        TValue value,
        out int found
        )
        where TValueTraits : IValueTraits<TValue>
    {
        JoinableTreeNode<TValue>? prev = null;
        var c = -1;
        while (@this != null && c != 0) {
            c = TValueTraits.Compare(value, @this.Value);
            prev = @this;
            @this = c < 0 ? @this.Left : @this.Right;
        }
        found = c;
        return prev;
    }

    /// <summary>
    /// Inserts a value into the tree.
    /// </summary>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <typeparam name="TJoin">Tree join strategy.</typeparam>
    /// <param name="this">Root node at which to insert.</param>
    /// <param name="state">
    /// An instance of <see cref="ModifyState{TValue}"/> with <c>Value</c> and <c>Transient</c> initialized.
    /// </param>
    /// <returns>
    /// Root of the modified tree.  Success (i.e., the value is not a duplicate) is indicated by
    /// <see cref="ModifyState{TValue}.Found"/> member of <paramref name="state"/>.
    /// </returns>
    public static JoinableTreeNode<TValue> Insert<TValue, TJoin>
        (
        this JoinableTreeNode<TValue>? @this,
        ref ModifyState<TValue> state
        )
        where TJoin : struct, ITreeTraits<TValue>
    {
        if (@this is null) {
            state.Found = null;
            var n = new JoinableTreeNode<TValue>(state.Transient, state.Value);
            n.Update<TJoin>();
            return n;
        }

        var c = TJoin.Compare(state.Value, @this.Value);
        if (c == 0) {
            state.Found = @this;
            return @this;
        }

        if (c < 0) {
            var n = Insert<TValue, TJoin>(@this.Left, ref state);
            if (state.Found != null)
                return @this;

            var jd = new TreeSection<TValue> { Transient = state.Transient, Left = n, Middle = @this, Right = @this.Right };
            return TJoin.Join(jd);
        } else {
            var n = Insert<TValue, TJoin>(@this.Right, ref state);
            if (state.Found != null)
                return @this;


            var jd = new TreeSection<TValue> { Transient = state.Transient, Left = @this.Left, Middle = @this, Right = n };
            return TJoin.Join(jd);
        }
    }

    /// <summary>
    /// Removes a value from the tree.
    /// </summary>
    /// <typeparam name="TValue">Tree element type.</typeparam>
    /// <typeparam name="TJoin">Tree join strategy.</typeparam>
    /// <param name="this">Root node at which to insert.</param>
    /// <param name="state">
    /// An instance of <see cref="ModifyState{TValue}"/> with <c>Value</c> and <c>Transient</c> initialized.
    /// </param>
    /// <returns>
    /// Root of the modified tree.  Success (i.e., the value was found and removed) is indicated by
    /// <see cref="ModifyState{TValue}.Found"/> member of <paramref name="state"/>.
    /// </returns>
    public static JoinableTreeNode<TValue>? Delete<TValue, TJoin>
        (
        this JoinableTreeNode<TValue>? @this,
        ref ModifyState<TValue> state
        )
        where TJoin : struct, ITreeTraits<TValue>
    {
        if (@this is null) {
            state.Found = null;
            return null;
        }

        var c = TJoin.Compare(state.Value, @this.Value);
        if (c == 0) {
            state.Found = @this;
            var jd = new TreeSection<TValue> { Transient = state.Transient, Left = @this.Left, Right = @this.Right };
            return jd.Join2<TJoin>();
        }

        if (c < 0) {
            var n = Delete<TValue, TJoin>(@this.Left, ref state);
            if (state.Found == null)
                return @this;

            var jd = new TreeSection<TValue> { Transient = state.Transient, Left = n, Middle = @this, Right = @this.Right };
            return TJoin.Join(jd);
        }else {
            var n = Delete<TValue, TJoin>(@this.Right, ref state);
            if (state.Found == null)
                return @this;

            var jd = new TreeSection<TValue> { Transient = state.Transient, Left = @this.Left, Middle = @this, Right = n };
            return TJoin.Join(jd);
        }
    }
}
