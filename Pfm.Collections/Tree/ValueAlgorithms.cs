#nullable enable
using System;

namespace Podaga.PersistentCollections.Tree;


/// <summary>
/// Value algorithm extension methods on <see cref="JoinableTree{TTag, TValue, TValueTraits}"/>.  These algorithms
/// depend on value traits.  The implicit <c>this</c> parameter is used as the transient context for
/// cloning during mutations.
/// </summary>
public static class ValueAlgorithms
{
    /// <summary>
    /// Finds a value equivalent to <paramref name="value"/>.  This method is more efficient than using
    /// the equivalent iterator method..
    /// </summary>
    /// <param name="this">
    /// The tree within which to search.
    /// </param>
    /// <param name="value">
    /// Value to look for; only the key fields must be initialized.
    /// On return, it will be overwritten with the found value, if any.
    /// </param>
    /// <returns>True if an equivalent value was found, fale otherwise.</returns>
    public static bool Find<TTag, TValue, TValueTraits>
        (
        this JoinableTree<TTag, TValue, TValueTraits> @this,
        ref TValue value
        )
        where TTag : struct, ITagTraits<TTag>
        where TValueTraits : struct, IValueTraits<TValue>
    {
        var _value = value; // Local copy for optimization.
        var root = @this.Root;
        int c;
        while (root != null) {
            if ((c = TValueTraits.Compare(_value, root.V)) == 0) {
                value = root.V;
                return true;
            }
            root = c < 0 ? root.L : root.R;
        }
        return false;
    }


}
