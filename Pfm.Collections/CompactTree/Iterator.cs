using System;
using System.Runtime.CompilerServices;


namespace Pfm.Collections.CompactTree;

public struct Iterator<TValue, TTag> : ICloneable where TTag : struct
{
    public const int MaxDepth = 48;

    internal Iterator(AbstractTree<TValue, TTag> tree) {
        this.Tree = tree ?? throw new ArgumentNullException(nameof(tree));
        this.Path = new Pointer[MaxDepth];
        this.Count = 0;
    }

    private Iterator(Iterator<TValue, TTag> other)  {
        this.Tree = other.Tree;
        this.Path = new Pointer[MaxDepth];
        this.Count = other.Count;
        Array.Copy(other.Path, Path, Count);
    }

    public readonly AbstractTree<TValue, TTag> Tree;
    public readonly Pointer[] Path;
    public int Count;

    public Iterator<TValue, TTag> Clone() => new (this);
    object ICloneable.Clone() => Clone();

    public bool IsAllocated => Tree != null;
    
    public bool IsEmpty => Count == 0;
    public ref Pointer Top => ref Path[Count - 1];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => Count = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(Pointer node) {
        if (node.IsNull)
            throw new ArgumentNullException(nameof(node));
        Path[Count++] = node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Pointer TryPop() => Count > 0 ? Path[--Count] : Pointer.Null;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public Pointer First() {
        Clear();
        for (var n = Tree.Root; !n.IsNull; n = Tree[n].L)
            Push(n);
        return IsEmpty ? Pointer.Null : Top;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public Pointer Last() {
        Clear();
        for (var n = Tree.Root; !n.IsNull; n = Tree[n].R)
            Push(n);
        return IsEmpty ? Pointer.Null : Top;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public int Find(TValue value) {
        var pn = Tree.Root;
        int c = -1;
        Clear();
        while (!pn.IsNull) {
            Push(pn);
            ref var n = ref Tree[pn];
            if ((c = Tree.NodeTraits.Compare(value, n.V)) == 0)
                break;
            pn = c < 0 ? n.L : n.R;
        }
        return c;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public Pointer Succ() {
        var pcurrent = TryPop();
        if (pcurrent.IsNull)
            return Pointer.Null;
        ref var current = ref Tree[pcurrent];
        if (!current.R.IsNull) {
            Push(pcurrent);
            for (pcurrent = current.R; !pcurrent.IsNull; pcurrent = Tree[pcurrent].L)
                Push(pcurrent);
        } else {
            Pointer y;
            do {
                y = pcurrent;
                if ((pcurrent = TryPop()).IsNull)
                    return Pointer.Null;
            } while (y == Tree[pcurrent].R);
            Push(pcurrent);
        }
        return Top;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public Pointer Pred() {
        var pcurrent = TryPop();
        if (pcurrent.IsNull)
            return Pointer.Null;
        ref var current = ref Tree[pcurrent];
        if (!current.L.IsNull) {
            Push(pcurrent);
            for (pcurrent = current.L; !pcurrent.IsNull; pcurrent = Tree[pcurrent].R)
                Push(pcurrent);
        } else {
            Pointer y;
            do {
                y = pcurrent;
                if ((pcurrent = TryPop()).IsNull)
                    return Pointer.Null;
            } while (y == Tree[pcurrent].L);
            Push(pcurrent);
        }
        return Top;
    }
}
