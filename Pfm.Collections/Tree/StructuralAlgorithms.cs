#if false
using System;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Structural algorithm extension methods on <see cref="JoinableTree{TTag, TValue, TValueTraits}"/>.  These algorithms do not
/// depend on value traits, except for cloning.  The implicit <c>this</c> parameter is used as the transient context for
/// cloning during mutations.
/// </summary>
public static class StructuralAlgorithms
{
    /// <summary>
    /// Copies all nodes from <paramref name="other"/> into <c>this</c>, replacing the root.
    /// Each node is copied if the node's transient tag is different from <c>this</c>.
    /// </summary>
    /// <param name="this">The tree instance to copy into.</param>
    /// <param name="other">Root of the (sub)tree to copy; must not be null.</param>
    public static void CopyFrom<TTag, TValue, TValueTraits>
        (
        this JoinableTree<TTag, TValue, TValueTraits> @this,
        JoinableTree<TTag, TValue, TValueTraits> other
        )
        where TTag : struct, ITagTraits<TTag>
        where TValueTraits : struct, IValueTraits<TValue>
    {
        @this.Root = other.Root is null ? null : Copy<TTag, TValue, TValueTraits>(@this.Transient, other.Root);
    }



    /// <summary>
    /// Returns a tree for which the in-order values are concatenation of in-order values of <paramref name="left"/>
    /// and <paramref name="right"/>.
    /// </summary>
    /// <param name="this">Tree instance; used for transient context.</param>
    /// <param name="left">Left side of the join.</param>
    /// <param name="right">Right side of the join.</param>
    public static JoinableTreeNode<TTag, TValue> Join2<TTag, TValue, TValueTraits>
        (
        this JoinableTree<TTag, TValue, TValueTraits> @this,
        JoinableTreeNode<TTag, TValue> left,
        JoinableTreeNode<TTag, TValue> right
        )
        where TTag : struct, ITagTraits<TTag>
        where TValueTraits : struct, IValueTraits<TValue>
    {
        if (left is null)
            return right;
        var n = SplitLast(left, out var leftlast);
        return @this.Join(n, leftlast, right);

        JoinableTreeNode<TTag, TValue> SplitLast(JoinableTreeNode<TTag, TValue> node, out JoinableTreeNode<TTag, TValue> rightmost) {
            if (node.R == null) {
                rightmost = node;
                return node.L;
            }
            var n = SplitLast(node.R, out rightmost);
            return @this.Join(node.L, node, n);
        }
    }


}
#endif