using System;

namespace Podaga.PersistentCollections.TreeSet;

public partial class JoinTree<TValue, TTreeTraits>
    where TTreeTraits : struct, IValueTraits<TValue>, IBalanceTraits<TTreeTraits, TValue>
{
    /// <summary>
    /// State threaded by reference through recursive calls to insert and delete.
    /// </summary>
    public struct ModificationState {
        /// <summary>
        /// If true, insertion or deletion succeeded.
        /// </summary>
        public bool Success;

        /// <summary>
        /// Value to insert or delete.
        /// </summary>
        public TValue Input;

        /// <summary>
        /// For insertion: if <see cref="Success"/> is false, this is the equivalent value found, otherwise left unchanged.
        /// For deletion: if <see cref="Success"/> is true, this is the equivalent value found.
        /// </summary>
        public TValue Output;
    }

    /// <summary>
    /// Tries to insert a value into the tree.
    /// </summary>
    /// <param name="root">Root of the tree into which to insert the value.</param>
    /// <param name="ma">
    /// On entry, the <c>Input</c> field must be initialized to the value to insert.
    /// On return, <c>Success</c> and <c>Output</c> are updated to reflect the result.
    /// </param>
    /// <returns>
    /// Tree root after the update.
    /// </returns>
    public TreeNode<TValue> Insert(TreeNode<TValue> root, ref ModificationState ma) {
        if (root == null) {
            var node = new TreeNode<TValue>(transient) { V = ma.Input };
            node.Update<TTreeTraits>();
            ma.Success = true;
            return node;
        }

        int c = TTreeTraits.CompareKey(ma.Input, root.V);
        if (c == 0) {
            ma.Output = root.V;
            ma.Success = false;
            return root;
        }
        
        if (c < 0) {
            var i = Insert(root.L, ref ma);
            return !ma.Success ? root : TTreeTraits.Join(this, i, root, root.R);
        } else {
            var i = Insert(root.R, ref ma);
            return !ma.Success ? root : TTreeTraits.Join(this, root.L, root, i);
        }
    }

    /// <summary>
    /// Tries to delete a value from the tree.
    /// </summary>
    /// <param name="root">Root of the tree into which to insert the value.</param>
    /// <param name="ma">
    /// On entry, the <c>Input</c> field must be initialized to the value to delete.
    /// On return, <c>Success</c> and <c>Output</c> are updated to reflect the result.
    /// </param>
    /// <returns>
    /// Tree root after the update.
    /// </returns>
    public TreeNode<TValue> Delete(TreeNode<TValue> root, ref ModificationState ma) {
        if (root == null) {
            ma.Success = false;
            return null;
        }

        var c = TTreeTraits.CompareKey(ma.Input, root.V);
        if (c == 0) {
            ma.Output = root.V;
            ma.Success = true;
            var j = Join2(root.L, root.R);
            return j;
        }
        
        if (c < 0) {
            var d = Delete(root.L, ref ma);
            return !ma.Success ? root : TTreeTraits.Join(this, d, root, root.R);
        } else {
            var d = Delete(root.R, ref ma);
            return !ma.Success ? root : TTreeTraits.Join(this, root.L, root, d);
        }
    }
}
