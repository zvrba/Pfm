#nullable enable

using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Podaga.PersistentCollections.Tree;


/// <summary>
/// Value algorithm extension methods on <see cref="JoinableTree{TTag, TValue, TValueTraits}"/>.  These algorithms
/// depend on value traits.  The implicit <c>this</c> parameter is used as the transient context for
/// cloning during mutations.
/// </summary>
public static class ValueAlgorithms
{
    /// <summary>
    /// Used as input and output for find/insert/delete.
    /// </summary>
    public struct SearchState<TTag, TValue, TValueTraits>
        where TTag : struct, ITagTraits<TTag>
        where TValueTraits : struct, IValueTraits<TValue>
    {
        /// <summary>
        /// The extension method automatically sets this member to the tree instance.  This exists only as an optimization.
        /// </summary>
        public JoinableTree<TTag, TValue, TValueTraits> Tree;

        /// <summary>
        /// True if the algorithm succeeded.
        /// </summary>
        public bool Success;

        /// <summary>
        /// The "result" of the operation.  The value is meaningful also when <see cref="Success"/> is false.
        /// </summary>
        public JoinableTreeNode<TTag, TValue>? Result;

        /// <summary>
        /// Value to find / insert / delete.
        /// </summary>
        public TValue Value;

        /// <summary>
        /// Tag to use during insertion of a new node.
        /// </summary>
        public TTag Tag;
    }

    /// <summary>
    /// Finds a value equivalent to the one given in <paramref name="state"/>.  This method is more efficient than using
    /// the equivalent iterator method.
    /// </summary>
    /// <param name="this">
    /// The tree within which to search.
    /// </param>
    /// <param name="state">
    /// <para>
    /// On input, <see cref="SearchState{TTag, TValue, TValueTraits}.Value"/> must be provided.  This is the value to look for.
    /// </para>
    /// On output, <see cref="SearchState{TTag, TValue, TValueTraits}.Result"/> is set to the last visited node during the search.
    /// If <see cref="SearchState{TTag, TValue, TValueTraits}.Success"/> is true, the equivalent value has been found.
    /// </param>
    /// <returns>
    /// True if an equivalent node has been found.
    /// </returns>
    public static bool Find<TTag, TValue, TValueTraits>
        (
        this JoinableTree<TTag, TValue, TValueTraits> @this,
        ref SearchState<TTag, TValue, TValueTraits> state
        )
        where TTag : struct, ITagTraits<TTag>
        where TValueTraits : struct, IValueTraits<TValue>
    {
        var root = @this.Root;
        JoinableTreeNode<TTag, TValue>? prev = null;

        state.Tree = @this;
        
        var value = state.Value;    // Optimization
        int c;
        while (root != null) {
            if ((c = TValueTraits.Compare(value, root.V)) == 0) {
                state.Result = root;
                state.Success = true;
                return true;
            }
            prev = root;
            root = c < 0 ? root.L : root.R;
        }

        state.Result = prev;
        state.Success = false;
        return false;
    }


    /// <summary>
    /// Tries to insert a value into the tree.
    /// </summary>
    /// <param name="this">The tree within which to search.</param>
    /// <param name="state">
    /// <para>
    /// On input, <see cref="SearchState{TTag, TValue, TValueTraits}.Tag"/> and <see cref="SearchState{TTag, TValue, TValueTraits}.Value"/>
    /// must be provided.
    /// </para>
    /// <para>
    /// On output, <see cref="SearchState{TTag, TValue, TValueTraits}.Success"/> and <see cref="SearchState{TTag, TValue, TValueTraits}.Result"/>
    /// are set according to whether the value was inserted or not.
    /// </para>
    /// </param>
    /// <returns>
    /// True on success, i.e., an equivalent was not found.  In this case, the result is the newly inserted node.
    /// On failure the result is the existing node.
    /// </returns>
    public static bool Insert<TTag, TValue, TValueTraits>
        (
        this JoinableTree<TTag, TValue, TValueTraits> @this,
        ref SearchState<TTag, TValue, TValueTraits> state
        )
        where TTag : struct, ITagTraits<TTag>
        where TValueTraits : struct, IValueTraits<TValue>
    {
        state.Tree = @this;
        @this.Root = Insert(@this.Root, ref state);
        return state.Success;
    }


    /// <summary>
    /// Tries to remove a value from the tree.
    /// </summary>
    /// <param name="this">The tree within which to search.</param>
    /// <param name="state">
    /// <para>
    /// On input, <see cref="SearchState{TTag, TValue, TValueTraits}.Value"/> must be provided.
    /// </para>
    /// <para>
    /// On output, <see cref="SearchState{TTag, TValue, TValueTraits}.Success"/> and <see cref="SearchState{TTag, TValue, TValueTraits}.Result"/>
    /// are set according to whether the value was deleted or not.
    /// </para>
    /// </param>
    /// <returns>
    /// True on success, i.e., an equivalent value was found.  In this case, the result is the deleted node.
    /// On failure the result is <c>null</c>.
    /// </returns>
    public static bool Delete<TTag, TValue, TValueTraits>
        (
        this JoinableTree<TTag, TValue, TValueTraits> @this,
        ref SearchState<TTag, TValue, TValueTraits> state
        )
        where TTag : struct, ITagTraits<TTag>
        where TValueTraits : struct, IValueTraits<TValue>
    {
        state.Tree = @this;
        @this.Root = Delete(@this.Root, ref state);
        return state.Success;
    }

    private static JoinableTreeNode<TTag, TValue> Insert<TTag, TValue, TValueTraits>
        (
        JoinableTreeNode<TTag, TValue>? root,
        ref SearchState<TTag, TValue, TValueTraits> state
        )
        where TTag : struct, ITagTraits<TTag>
        where TValueTraits : struct, IValueTraits<TValue>
    {
        if (root is null) {
            var node = new JoinableTreeNode<TTag, TValue>(state.Tree.Transient, state.Tag, state.Value);
            //node.Update(); Not needed since no children.
            state.Result = node;
            state.Success = true;
            return node;
        }

        var c = TValueTraits.Compare(state.Value, root.V);
        if (c == 0) {
            state.Result = root;
            state.Success = false;
            return root;
        }

        if (c < 0) {
            var i = Insert(root.L, ref state);
            return !state.Success ? root : state.Tree.Join(i, root, root.R);
        }
        else {
            var i = Insert(root.R, ref state);
            return !state.Success ? root : state.Tree.Join(root.L, root, i);
        }
    }

    private static JoinableTreeNode<TTag, TValue>? Delete<TTag, TValue, TValueTraits>
        (
        JoinableTreeNode<TTag, TValue>? root,
        ref SearchState<TTag, TValue, TValueTraits> state
        )
        where TTag : struct, ITagTraits<TTag>
        where TValueTraits : struct, IValueTraits<TValue>
    {
        if (root is null) {
            state.Success = false;
            return null;
        }

        var c = TValueTraits.Compare(state.Value, root.V);
        if (c == 0) {
            state.Result = root;
            state.Success = true;
            var j = state.Tree.Join2(root.L, root.R);
            return j;
        }

        if (c < 0) {
            var d = Delete(root.L, ref state);
            return !state.Success ? root : state.Tree.Join(d, root, root.R);
        }
        else {
            var d = Delete(root.R, ref state);
            return !state.Success ? root : state.Tree.Join(root.L, root, d);
        }
    }
}
