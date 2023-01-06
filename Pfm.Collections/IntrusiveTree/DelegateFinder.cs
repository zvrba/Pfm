using System;

namespace Pfm.Collections.IntrusiveTree;

// Proof of concept: binary tree search using delegates instead of static abstract methods in interfaces.
public class DelegateFinder<TNode, TValue, TTag>
    where TNode : class
    where TTag : struct
{
    public readonly NodeTraits NT;
    public readonly TNode Root;

    public DelegateFinder(NodeTraits nt, TNode root) {
        NT = nt;
        Root = root;
    }

    public int Find(TValue value, out TNode node) {
        int c = -1;
        TNode p = null;
        for (var n = Root; n != null; p = n, n = c < 0 ? NT.L(n) : NT.R(n)) {
            if ((c = NT.Compare(value, NT.V(n))) == 0) {
                node = n;
                return 0;
            }
        }
        node = p;
        return c;
    }

    public delegate ref T RefGetter<T>(TNode node);

    public readonly struct NodeTraits
    {
        public readonly Func<TNode> Create;
        public readonly Comparison<TValue> Compare;
        public readonly RefGetter<TNode> L;
        public readonly RefGetter<TNode> R;
        public readonly RefGetter<TValue> V;
        public readonly RefGetter<TTag> T;

        public NodeTraits(
            Func<TNode> create,
            Comparison<TValue> compare,
            RefGetter<TNode> l,
            RefGetter<TNode> r,
            RefGetter<TValue> v,
            RefGetter<TTag> t)
        {
            Create = create;
            Compare = compare;
            L = l;
            R = r;
            V = v;
            T = t;
        }
    }
}
