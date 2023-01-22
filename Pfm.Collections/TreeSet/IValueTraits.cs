using System;
using System.Runtime.CompilerServices;

namespace Pfm.Collections.TreeSet;

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

