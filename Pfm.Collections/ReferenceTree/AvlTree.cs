using System;
using System.Runtime.CompilerServices;

namespace Pfm.Collections.ReferenceTree;

public sealed class AvlTree<TValue> : AbstractTree<TValue>
{
    private Iterator position;

    public AvlTree(Comparison<TValue> comparison) : base(comparison) {
        position = new(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void UpdateTag(Node node) {
        var l = node.L?.T ?? 0;
        var r = node.R?.T ?? 0;
        node.T = 1 + (l > r ? l : r);
    }

    public override bool Insert(TValue value, out Node node) {
        var c = position.Find(value);
        if (c == 0) {
            node = position.Top;
            return false;
        }

        node = new() {
            V = value,
            T = 1
        };

        if (Root == null) {
            Root = node;
        }
        else {
            var parent = position.Top;
            if (c < 0) parent.L = node;
            else parent.R = node;
            Root = Rebalance(position);
        }

        position.Push(node);    // Make iterator point to the inserted node.
        ++Count;
        return true;
    }

    /// <inheritdoc/>
    public override Node Delete(TValue value) {
        if (position.Find(value) != 0)
            return null;

        var node = position.Top;
        var inode = position.Count - 1;     // Index on the path to replace the removed node with.
        Node replacement;                   // Node to replace with on the path

        if (node.R == null) {               // Case 1: no right child
            if ((replacement = node.L) == null)
                position.TryPop();
        }
        else if (node.R.L == null) {        // Case 2: right child has no left child
            replacement = node.R;
            replacement.L = node.L;
        }
        else {                              // Case 3: right child has a left child
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

        if (position.Count > 0) Root = Rebalance(position);
        else Root = null;
        --Count;

        return node;
    }

    private Node Rebalance(Iterator position) {
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
            }
            else if (b < -1) {
                if (H(n.L.L) >= H(n.L.R)) n = RotR(n);
                else n = RotLR(n);
            }

            if (link == -1) position.Path[i - 1].L = n;
            else if (link == 1) position.Path[i - 1].R = n;
        }
        return position.Path[0];

        static int H(Node n) => n?.T ?? 0;
    }

    internal override void ValidateStructure() {
        ValidateHeights(Root);

        static int ValidateHeights(Node node) {
            if (node == null)
                return 0;
            var l = ValidateHeights(node.L);
            var r = ValidateHeights(node.R);
            var h = 1 + (l > r ? l : r);
            var b = r - l;

            if (node.T != h)
                throw new NotImplementedException();
            if (b < -1 || b > 1)
                throw new NotImplementedException();

            return h;
        }
    }
}
