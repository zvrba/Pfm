using System;
using System.Runtime.CompilerServices;

namespace Pfm.Collections.IntrusiveTree;

public class AvlTree<TNode, TValue, TTag> : AbstractIntrusiveTree<TNode, TValue, TTag, AvlTreeTag>
    where TNode : class, INodeTraits<TNode, TValue, TTag>
    where TTag : struct, ITagTraits<TTag>
{
    private Iterator<TNode, TValue, TTag> position;

    public AvlTree() {
        position = new(this);
    }

    public override bool Add(TValue value, TTag tag, out TNode node) {
        var c = position.Find(value);
        if (c == 0) {
            node = position.Top;
            return false;
        }

        node = TNode.Create(null);
        node.L = node.R = null;
        node.V = value;
        node.T = tag;
        BaseTag(ref node.T).Height = 1;

        if (_Root == null) {
            _Root = node;
        } else {
            var parent = position.Top;
            if (c < 0) parent.L = node;
            else parent.R = node;
            _Root = Rebalance();
        }

        ++Count;
        return true;
    }

    public override TNode Remove(TValue value) {
        var c = position.Find(value);
        if (c != 0)
            return null;

        var node = position.Top;
        var inode = position.Count - 1;     // Index on the path to replace the removed node with.
        TNode replacement;                  // Node to replace with on the path

        if (node.R == null) {               // Case 1: no right child
            if ((replacement = node.L) == null)
                position.TryPop();
        } else if (node.R.L == null) {      // Case 2: right child has no left child
            replacement = node.R;
            replacement.L = node.L;
        } else {                            // Case 3: right child has a left child
            position.Push(node.R);
            while (position.Top.L != null)
                position.Push(position.Top.L);
            replacement = position.TryPop();

            position.Top.L = replacement.R;
            replacement.L = node.L;
            replacement.R = node.R;
        }

        position.Path[inode] = replacement;

        if (inode > 0) {
            var parent = position.Path[inode - 1];
            if (node == parent.L) parent.L = replacement;
            else parent.R = replacement;
        }

        if (position.Count > 0) _Root = Rebalance();
        else _Root = null;
        --Count;
        return node;
    }

    private TNode Rebalance() {
        int b;
        for (int i = position.Count - 1; i >= 0; --i) {
            ref var n = ref position.Path[i];
            int link = 0;
            if (i > 0)
                link = position.Path[i - 1].L == n ? -1 : 1;

            UpdateTag(n);
            b = H(n.R) - H(n.L);

            if (b > 1) {
                if (H(n.R.R) >= H(n.R.L)) n = RotL(n);
                else n = RotRL(n);
            } else if (b < -1) {
                if (H(n.L.L) >= H(n.L.R)) n = RotR(n);
                else n = RotLR(n);
            }

            if (link == -1) position.Path[i - 1].L = n;
            else if (link == 1) position.Path[i - 1].R = n;
        }
        return position.Path[0];

        static int H(TNode n) => n == null ? 0 : BaseTag(ref n.T).Height;
    }
}
