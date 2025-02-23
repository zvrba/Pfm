using System;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Any value stored in the tree must implement this interface.
/// </summary>
/// <typeparam name="TValue">
/// Value stored in the tree; consists of a mutable "tag" (augmentation) part and an immutable value part used for comparisons.
/// </typeparam>
public interface IValueTraits<TValue>
{
    /// <summary>
    /// Value corresponding to <c>null</c> node.  Used as left or right argument to <see cref="CombineTags(TValue, ref TValue, TValue)"/>.
    /// </summary>
    abstract static TValue NilTag { get; }

    /// <summary>
    /// <para>
    /// Computes <c>result = left + result + right</c> on the TAG part of <typeparamref name="TValue"/>.
    /// </para>
    /// <para>
    /// A correct implementation obeys monoidal laws with <see cref="NilTag"/> as the neutral element.
    /// </para>
    /// </summary>
    /// <param name="left">Value corresponding to the left branch.  <see cref="NilTag"/> if there is no left branch.</param>
    /// <param name="result">Value corresponding to the current node.</param>
    /// <param name="right">Value corresponding to the right branch.  <see cref="NilTag"/> if there is no right branch.</param>
    abstract static void CombineTags(TValue left, ref TValue result, TValue right);

    /// <summary>
    /// Compares two values.  Must implement a total order.
    /// </summary>
    /// <param name="left">Left value to compare.</param>
    /// <param name="right">Right value to compare.</param>
    /// <returns>
    /// Negative, zero or positive integer when <paramref name="left"/> is, respectively smaller than, equal
    /// to, or greater than <paramref name="right"/>.
    /// </returns>
    abstract static int Compare(TValue left, TValue right);

    /// <summary>
    /// Clones <paramref name="value"/> such that the tag part is safe to mutate independently of the original value.
    /// Default implementation returns <paramref name="value"/> which is a sufficient implementation for value types.
    /// </summary>
    /// <param name="value">Value to clone.</param>
    /// <returns>
    /// The cloned value.
    /// </returns>
    virtual static TValue Clone(TValue value) => value;
}
