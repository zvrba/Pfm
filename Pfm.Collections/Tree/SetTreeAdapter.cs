#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Adapts a joinable tree to <see cref="ISet{T}"/> and <see cref="IReadOnlySet{T}"/>.
/// </summary>
/// <remarks>
/// Implementations of set operations check whether the <see cref="IEnumerable{T}"/> argument is actually an instance of
/// <see cref="JoinableTreeNode{TValue}"/> or <see cref="SetTreeAdapter{TValue, TJoin}"/>.  If so, a more efficient
/// join-based recursive strategy is used.  Otherwise, the operation is performed element-wise.  In particular, <see cref="IntersectWith(IEnumerable{TValue})"/>
/// allocates temporary storage in size proportional with the result.
/// </remarks>
/// <typeparam name="TValue">Tree element type.</typeparam>
/// <typeparam name="TJoin">Tree join strategy.</typeparam>
public class SetTreeAdapter<TValue, TJoin> : CollectionTreeAdapter<TValue, TJoin>,
    IReadOnlySet<TValue>,
    ISet<TValue>
    where TJoin : struct, ITreeTraits<TValue>
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="root">Tree root from an existing tree, or <c>null</c> to initialize an empty set.</param>
    /// <param name="transient">Transient tag to reuse, or 0 to create a new one.</param>
    public SetTreeAdapter(JoinableTreeNode<TValue>? root = null, ulong transient = 0) : base(root, transient) { }

    /// <inheritdoc/>
    public bool SetEquals(IEnumerable<TValue> other) => other switch {
        JoinableTreeNode<TValue> n => SetEqual(Root, n),
        IAdaptedTree<TValue, TJoin> t => SetEqual(Root, t.Root),
        _ => other.Count() == Count && other.Count(Contains) == Count,
    };

    /// <summary>
    /// Set equality on trees rooted at <paramref name="a"/> and <paramref name="b"/>.
    /// The implementation uses <see cref="TreeIterator{TValue}"/> to walk both trees.
    /// </summary>
    /// <param name="a">Root of the first tree.</param>
    /// <param name="b">Root of the second tree.</param>
    /// <returns>
    /// True if <paramref name="a"/> and <paramref name="b"/> contain the same elements as determined by <typeparamref name="TJoin"/>.
    /// </returns>
    protected static bool SetEqual(JoinableTreeNode<TValue>? a, JoinableTreeNode<TValue>? b) {
        if (a is null || b is null)
            return (a is null) == (b is null);
        if (a.Size != b.Size)
            return false;

        var ai = a.GetIterator();
        ai.First(null);

        var bi = b.GetIterator();
        bi.First(null);

        // At this point, sizes are equal and at least 1.
        do {
            if (TJoin.Compare(ai.Top.Value, bi.Top.Value) != 0)
                return false;
        } while (ai.Succ() && bi.Succ());
        return true;
    }

    /// <inheritdoc/>
    public bool IsSubsetOf(IEnumerable<TValue> other) => Count == 0 || other.Count(Contains) == Count;

    /// <inheritdoc/>
    public bool IsProperSubsetOf(IEnumerable<TValue> other) => IsSubsetOf(other) && other.Count() > Count;

    /// <inheritdoc/>
    public bool IsSupersetOf(IEnumerable<TValue> other) => other.Count() == 0 || other.All(Contains);

    /// <inheritdoc/>
    public bool IsProperSupersetOf(IEnumerable<TValue> other) => IsSupersetOf(other) && Count > other.Count();

    /// <inheritdoc/>
    public bool Overlaps(IEnumerable<TValue> other) => other.Any(Contains);

    /// <inheritdoc/>
    public void UnionWith(IEnumerable<TValue> other) {
        switch (other) {
            case JoinableTreeNode<TValue> n:
                Root = SetUnion(Root, n);
                break;
            case IAdaptedTree<TValue,TJoin> t:
                Root = SetUnion(Root, t.Root);
                break;
            default:
                foreach (var v in other)
                    Add(v);
                break;
        }
    }

    /// <summary>
    /// Join-based union algorithm.
    /// </summary>
    /// <param name="t1">Root of the first tree.</param>
    /// <param name="t2">Root of the second tree.</param>
    /// <returns>Root of the tree that is the union of <paramref name="t1"/> and <paramref name="t2"/>.</returns>
    protected JoinableTreeNode<TValue>? SetUnion
        (
        JoinableTreeNode<TValue>? t1,
        JoinableTreeNode<TValue>? t2
        )
    {
        if (t1 == null)
            return t2;
        if (t2 == null)
            return t1;

        var s = new TreeSection<TValue> { Transient = Transient }.Split<TJoin>(t1, t2.Value);
        var l = SetUnion(s.Left, t2.Left);
        var r = SetUnion(s.Right, t2.Right);
        if (s.Middle != null) {
            t1 = s.Middle.Clone<TJoin>(s.Transient);
        } else {
            t1 = t2;
        }
        return TJoin.Join(new() { Transient = Transient, Left = l, Middle = t1, Right = r });
    }

    /// <inheritdoc/>
    public void IntersectWith(IEnumerable<TValue> other) {
        switch (other) {
            case JoinableTreeNode<TValue> n:
                Root = SetIntersection(Root, n);
                break;
            case IAdaptedTree<TValue, TJoin> t:
                Root = SetIntersection(Root, t.Root);
                break;
            default:
                var isect = other.Where(Contains).ToList();
                Clear();
                foreach (var x in isect)
                    Add(x);
                break;
        }
    }

    /// <summary>
    /// Join-based intersection algorithm.
    /// </summary>
    /// <param name="t1">Root of the first tree.</param>
    /// <param name="t2">Root of the second tree.</param>
    /// <returns>Root of the tree that is the intersection of <paramref name="t1"/> and <paramref name="t2"/>.</returns>
    protected JoinableTreeNode<TValue>? SetIntersection
        (
        JoinableTreeNode<TValue>? t1,
        JoinableTreeNode<TValue>? t2
        )
    {
        if (t1 == null || t2 == null)
            return null;

        var s = new TreeSection<TValue> { Transient = Transient }.Split<TJoin>(t1, t2.Value);
        var l = SetIntersection(s.Left, t2.Left);
        var r = SetIntersection(s.Right, t2.Right);
        if (s.Middle != null) {
            t1 = s.Middle.Clone<TJoin>(Transient);
            return TJoin.Join(new() { Transient = Transient, Left = l, Middle = t1, Right = r });
        }
        s.Left = l;
        s.Right = r;
        return s.Join2<TJoin>();
    }

    /// <inheritdoc/>
    public void ExceptWith(IEnumerable<TValue> other) {
        switch (other) {
            case JoinableTreeNode<TValue> n:
                Root = SetDifference(Root, n);
                break;
            case IAdaptedTree<TValue, TJoin> t:
                Root = SetDifference(Root, t.Root);
                break;
            default:
                foreach (var x in other)
                    Remove(x);
                break;
        }
    }

    /// <summary>
    /// Join-based difference algorithm.
    /// </summary>
    /// <param name="t1">Root of the first tree.</param>
    /// <param name="t2">Root of the second tree.</param>
    /// <returns>Root of the tree that is the difference of <paramref name="t1"/> and <paramref name="t2"/>.</returns>
    protected JoinableTreeNode<TValue>? SetDifference
        (
        JoinableTreeNode<TValue>? t1,
        JoinableTreeNode<TValue>? t2
        )
    {
        if (t1 == null)
            return null;
        if (t2 == null)
            return t1;

        var s = new TreeSection<TValue> { Transient = Transient }.Split<TJoin>(t1, t2.Value);
        var l = SetDifference(s.Left, t2.Left);
        var r = SetDifference(s.Right, t2.Right);
        s.Left = l;
        s.Right = r;
        return s.Join2<TJoin>();
    }

    /// <inheritdoc/>
    public void SymmetricExceptWith(IEnumerable<TValue> other) {
        foreach (var x in other) {
            if (Contains(x)) Remove(x);
            else Add(x);
        }
    }
}
