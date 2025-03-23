using System;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Describes a "section" of a tree.
/// </summary>
/// <typeparam name="TValue">Tree element type.</typeparam>
public struct TreeSection<TValue>
{
    /// <summary>
    /// Transient value for modifications.  Must be initialized before calling any other method.
    /// </summary>
    public ulong Transient;

    /// <summary>
    /// Left subtree.
    /// </summary>
    public JoinableTreeNode<TValue> Left;

    /// <summary>
    /// The middle node.
    /// </summary>
    public JoinableTreeNode<TValue> Middle;

    /// <summary>
    /// Right subtree.
    /// </summary>
    public JoinableTreeNode<TValue> Right;

    /// <summary>
    /// Attaches <see cref="Left" /> and <see cref="Right" /> as left and right children of <see cref="Middle" />.
    /// This method assumes that the result will be properly balanced.
    /// </summary>
    /// <returns>
    /// The updated (possibly cloned) node that was <see cref="Middle"/>.
    /// </returns>
    /// <typeparam name="TJoin">The tree's join algorithm.</typeparam>
    public readonly JoinableTreeNode<TValue> JoinBalanced<TJoin>() where TJoin : ITreeTraits<TValue>
    {
        var m = Middle.Clone<TJoin>(Transient);
        m.Left = Left;
        m.Right = Right;
        m.Update<TJoin>();
        return m;
    }

    /// <summary>
    /// Joins <see cref="Left"/> and <see cref="Right"/> balanced subtrees into a single balanced tree.
    /// </summary>
    /// <typeparam name="TJoin">Tree join strategy.</typeparam>
    /// <returns>A root of the joined tree.</returns>
    public JoinableTreeNode<TValue> Join2<TJoin>() where TJoin : ITreeTraits<TValue> {
        if (Left == null)
            return Right;
        Left = SplitLast<TJoin>(Left);
        return TJoin.Join(this);
    }

    // Sets Middle to the rightmost value.
    private JoinableTreeNode<TValue> SplitLast<TJoin>(JoinableTreeNode<TValue> node) 
        where TJoin : ITreeTraits<TValue>
    {
        if (node.Right == null) {
            Middle = node;
            return node.Left;
        }
        var n = SplitLast<TJoin>(node.Right);
        var jd1 = new TreeSection<TValue> { Transient = Transient, Left = node.Left, Middle = node, Right = n };
        return TJoin.Join(jd1);
    }

    /// <summary>
    /// Splits a tree rooted at <paramref name="this"/> into left and right subtrees 
    /// holding respectively values less than and greater than <paramref name="value"/>.
    /// </summary>
    /// <param name="this">Tree root.</param>
    /// <param name="value">Value used for splitting.</param>
    /// <typeparam name="TJoin">Tree join strategy.</typeparam>
    /// <returns>
    /// A structure containing the left and right subtrees and a flag indicating whether <paramref name="value"/> was
    /// found in the tree under <paramref name="this"/>.
    /// </returns>
    public TreeSection<TValue> Split<TJoin>
        (
        JoinableTreeNode<TValue> @this,
        TValue value
        )
        where TJoin : struct, ITreeTraits<TValue>
    {
        if (@this == null)
            return default;
        var c = TJoin.Compare(value, @this.Value);
        if (c == 0)
            return new() { Left = @this.Left, Middle = @this, Right = @this.Right };

        if (c < 0) {
            var s = Split<TJoin>(@this.Left, value);
            var jd = new TreeSection<TValue> { Transient = Transient, Left = s.Right, Middle = @this, Right = @this.Right };
            var j = TJoin.Join(jd);
            return new() { Left = s.Left, Middle = s.Middle, Right = j };
        } else {
            var s = Split<TJoin>(@this.Right, value);
            var jd = new TreeSection<TValue> { Transient = Transient, Left = @this.Left, Middle = @this, Right = s.Left };
            var j = TJoin.Join(jd);
            return new() { Left = j, Middle = s.Middle, Right = s.Right };
        }
    }
}

