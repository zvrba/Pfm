#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace Podaga.PersistentCollections.Tree;

/// <summary>
/// Adapts a joinable tree to <see cref="ISet{T}"/> and <see cref="IReadOnlySet{T}"/>.
/// </summary>
public class SetTreeAdapter<TValue, TJoin> : CollectionTreeAdapter<TValue, TJoin>,
    IAdaptedTree<TValue, TJoin>,
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
