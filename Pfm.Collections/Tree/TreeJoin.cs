using System;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Describes the input for 3-way join.
/// </summary>
public struct TreeJoin<TValue> where TValue : ITaggedValue<TValue>
{
    public ulong Transient;
    public JoinableTreeNode<TValue> Left;
    public JoinableTreeNode<TValue> Middle;
    public JoinableTreeNode<TValue> Right;

    /// <summary>
    /// Attaches <see cref="Left" /> and <see cref="Right" /> as left and right children of <see cref="Middle" />.
    /// This method assumes that the result will be properly balanced.
    /// </summary>
    /// <returns>
    /// The updated (possibly cloned) node that was <see cref="Middle"/>.
    /// </returns>
    public readonly JoinableTreeNode<TValue> JoinBalanced<TJoin>() where TJoin : ITreeJoin<TValue>
    {
        var m = Middle.Clone(Transient);
        m.Left = Left;
        m.Right = Right;
        m.Update<TJoin>();
        return m;
    }

    public JoinableTreeNode<TValue> Join2<TJoin>() where TJoin : ITreeJoin<TValue> {
        if (Left == null)
            return Right;
        Left = SplitLast<TJoin>(Left);
        return TJoin.Join(this);
    }

    // Sets Middle to the rightmost value.
    private JoinableTreeNode<TValue> SplitLast<TJoin>(JoinableTreeNode<TValue> node) 
        where TJoin : ITreeJoin<TValue>
    {
        if (node.Right == null) {
            Middle = node;
            return node.Left;
        }
        var n = SplitLast<TJoin>(node.Right);
        var jd1 = new TreeJoin<TValue> { Transient = Transient, Left = node.Left, Middle = node, Right = n };
        return TJoin.Join(jd1);
    }
}

