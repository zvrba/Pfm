using System;
using System.Runtime.CompilerServices;

namespace Pfm.Collections.TreeSet;

/// <summary>
/// Node of a binary search tree.
/// </summary>
/// <typeparam name="TValue">
/// Value stored in the tree.  This must be a value type that contains the tree key, balance tag for the tree
/// mechanics and optional user-defined monoidal augmentation data.
/// </typeparam>
public sealed class TreeNode<TValue>
{
    /// <summary>
    /// Left child, with key less than <see cref="V"/>.
    /// </summary>
    public TreeNode<TValue> L;

    /// <summary>
    /// Right child, with key larger than <see cref="V"/>.
    /// </summary>
    public TreeNode<TValue> R;

    /// <summary>
    /// Value stored in the tree.
    /// </summary>
    public TValue V;

    /// <summary>
    /// Balance metadata is maintained by tree mechanics and must not be touched by user code.
    /// </summary>
    public int Rank;

    /// <summary>
    /// Size (number of nodes) of the subtree rooted at <c>this</c>, including <c>this</c>.
    /// Maintained by tree mechanics and must not be touched by user code.
    /// </summary>
    public int Size;
    
    /// <summary>
    /// Data constructor.  Does NOT set rank or size.
    /// </summary>
    public TreeNode(TreeNode<TValue> left, TValue value, TreeNode<TValue> right) {
        L = left; R = right; V = value;
    }

    /// <summary>
    /// Copy-constructor.
    /// </summary>
    public TreeNode(TreeNode<TValue> other) {
        L = other.L; R = other.R; V = other.V;
    }

    /// <summary>
    /// Updates <c>this</c> node's balance and monoidal tags.
    /// WARNING: The update is in-place, so the node must have been cloned beforehand.
    /// </summary>
    /// <typeparam name="TValueTraits">Value traits to use.</typeparam>
    /// <typeparam name="TTreeTraits">Tree traits to use.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Update<TValueTraits, TTreeTraits>()
        where TValueTraits : struct, IValueTraits<TValue>
        where TTreeTraits : struct, ITreeTraits<TValue>
    {
        if (L != null && R != null) {
            Rank = TTreeTraits.CombineBalanceTags(L.Rank, R.Rank);
            Size = 1 + L.Size + R.Size;
            TValueTraits.CombineTags(L.V, ref V, R.V);
        }
        else if (L != null) {
            Rank = TTreeTraits.CombineBalanceTags(L.Rank, TTreeTraits.NilBalance);
            Size = 1 + L.Size;
            TValueTraits.CombineTagsLeft(L.V, ref V);
        }
        else if (R != null) {
            Rank = TTreeTraits.CombineBalanceTags(TTreeTraits.NilBalance, R.Rank);
            Size = 1 + R.Size;
            TValueTraits.CombineTagsRight(ref V, R.V);
        }
    }
}

/// <summary>
/// <para>
/// Value traits determine how values are handled by the tree.  This interface is separate from
/// <see cref="ITreeTraits{TValue, TValueTraits}"/> so that it can be reused in various algorithms
/// that do not depend on the tree mechanics.
/// </para>
/// <para>
/// Default tag operations are no-op and <see cref="HasMonoidalTag"/> is false.
/// </para>
/// </summary>
/// <typeparam name="TValue">Value type of the tree.</typeparam>
public interface IValueTraits<TValue>
{
    /// <summary>
    /// Comparison function over key fields of <typeparamref name="TValue"/>.
    /// </summary>
    /// <param name="left">Left value to compare.</param>
    /// <param name="right">Right value to compare.</param>
    /// <returns>
    /// Negative, zero or positive integer when <paramref name="left"/> is, respectively smaller than, equal
    /// to, or greater than <paramref name="right"/>.
    /// </returns>
    abstract static int CompareKey(in TValue left, in TValue right);

    /// <summary>
    /// Combines equivalent values, i.e., those for which <see cref="CompareKey(TValue, TValue)"/> on
    /// value fields of <paramref name="left"/> and <paramref name="right"/> returns zero.  A valid
    /// implementation for simple scenarios is to just return <paramref name="left"/> or <paramref name="right"/>.
    /// </summary>
    /// <param name="middle">
    /// The node into which to store the merge result.  The key part of <typeparamref name="TValue"/> must
    /// remain unchanged.  WARNING: With  mutable nodes, this parameter may alias either of the other arguments!
    /// </param>
    /// <param name="left">Left value to merge.</param>
    /// <param name="right">Right value to merge.</param>
    abstract static void CombineValues(in TValue left, ref TValue middle, in TValue right);

    /// <summary>
    /// Must return <c>true</c> if <typeparamref name="TValue"/> includes a monoidal tag, in which case
    /// the implementing class must also override <see cref="CombineMonoidalTags(TreeNode{TValue}, TreeNode{TValue}, TreeNode{TValue})"/>.
    /// Defaul implementation returns false.
    /// </summary>
    virtual static bool HasMonoidalTag => false;

    /// <summary>
    /// Performs monoidal addition of three tags.
    /// The operation performed must be <c>middle = left + middle + right</c>.
    /// </summary>
    /// <param name="left">Left argument to monoidal addition.  May be null.</param>
    /// <param name="middle">Node holding the result.  The key part must remain unchanged.  Never null.</param>
    /// <param name="right">Right argument to monoidal addition.  May be null.</param>
    virtual static void CombineTags(in TValue left, ref TValue middle, in TValue right) { }

    /// <summary>
    /// Like <see cref="CombineTags(in TValue, ref TValue, in TValue)"/>, but computing <c>middle = left + middle</c>.
    /// </summary>
    virtual static void CombineTagsLeft(in TValue left, ref TValue middle) { }

    /// <summary>
    /// Like <see cref="CombineTags(in TValue, ref TValue, in TValue)"/>, but computing <c>middle = middle + right</c>.
    /// </summary>
    virtual static void CombineTagsRight(ref TValue middle, in TValue right) { }
}

/// <summary>
/// Tree traits are core operations related to tree mechanics.
/// Operations that modify the tree depend on these.
/// </summary>
/// <typeparam name="TValue">Value type of the tree.</typeparam>
public interface ITreeTraits<TValue>
{
    /// <summary>
    /// Value provided to <see cref="CombineBalanceTags(int, int)"/> when the node is missing a child.
    /// </summary>
    abstract static int NilBalance { get; }

    /// <summary>
    /// Combines balance tags from children.
    /// </summary>
    /// <param name="left">Balance tag of the left child.</param>
    /// <param name="right">Balance tag of the right child.</param>
    abstract static int CombineBalanceTags(int left, int right);

    /// <summary>
    /// 3-way join; all other operations are based on this method.
    /// </summary>
    /// <param name="wa">Work area for the algorithm; must be allocated and have sufficient remaining capacity. 
    /// The contents, up to the depth on entry, will be preserved.
    /// </param>
    /// <param name="left">Left tree to join.</param>
    /// <param name="middle">The join pivot to insert in the result.</param>
    /// <param name="right">Right tree to join.</param>
    /// <returns>
    /// Tree that has same entries and inorder traversal as the node <c>(left, middle, right)</c>.
    /// </returns>
    /// <remarks>
    /// When the tree is not persistent, this operation is destructive to all inputs.
    /// </remarks>
    abstract static TreeNode<TValue> Join(
        TreeIterator<TValue> wa,
        TreeNode<TValue> left,
        TreeNode<TValue> middle,
        TreeNode<TValue> right);

    /// <summary>
    /// For exhaustive tests: validates the tree's structure invariant.
    /// </summary>
    /// <exception cref="NotImplementedException">Thrown when a violation of the structure invariant is detected.</exception>
    internal abstract static void ValidateStructure(TreeNode<TValue> root);
}
