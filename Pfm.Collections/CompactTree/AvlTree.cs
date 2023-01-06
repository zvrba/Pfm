using System;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Pfm.Collections.CompactTree;

/// <summary>
/// To augment the tag, create a new struct with sequential layout having <c>AvlTag</c> as the first member.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct AvlTag
{
    // TODO: We could use height sign to store balance.
    public byte Height;
    public sbyte Balance;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UpdateTag<TValue, TTag>(IAllocator<TValue, TTag> al, ref Node<TValue, TTag> node)
        where TTag : struct
    {
        var l = GetHeight(node.L);
        var r = GetHeight(node.R);
        
        ref var t = ref Unsafe.As<TTag, AvlTag>(ref node.T);
        t.Height = (byte)(1 + (l > r ? l : r));
        t.Balance = (sbyte)(r - l);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int GetHeight(Pointer p) =>
            p.IsNull ? 0 :
            Unsafe.As<TTag, AvlTag>(ref al[p].T).Height;
    }
}

public class AvlTree<TValue, TTag> : AbstractTree<TValue, TTag>
    where TTag : struct
{
    public readonly Rotations<TValue, TTag> Rotations;

    public AvlTree(IAllocator<TValue, TTag> allocator, NodeTraits<TValue, TTag> nodeTraits) : base(allocator, nodeTraits)
    {
        Rotations = new Rotations<TValue, TTag>(allocator, nodeTraits);
    }

    public override bool Add(ref Iterator<TValue, TTag> position, TValue value, TTag tag) {
        if (position.Tree != this)
            throw new ArgumentException("Position does not belong to this tree.", nameof(position));

        var c = position.Find(value);
        if (c == 0)
            return false;

        var pnode = Allocator.Allocate();
        ref var node = ref this[pnode];
        node.L = node.R = Pointer.Null;
        node.V = value;
        {
            ref var t = ref NodeTag(ref node);
            t.Height = 1;
            t.Balance = 0;
        }

        if (Root.IsNull) {
            Root = pnode;
        }
        else {
            ref var parent = ref this[position.Top];
            if (c < 0) parent.L = pnode;
            else parent.R = pnode;
            Root = Rebalance(position);
        }

        position.Push(pnode);
        ++Count;
        return true;
    }

    public override void Remove(ref Iterator<TValue, TTag> position) {
        if (position.Tree != this)
            throw new ArgumentException("Position does not belong to this tree.", nameof(position));

        var pnode = position.Top;
        ref readonly var node = ref this[pnode];
        var inode = position.Count - 1;
        Pointer preplacement;

        if (node.R.IsNull) {
            if ((preplacement = node.L).IsNull)
                position.TryPop();
        }
        else if (this[node.R].L.IsNull) {
            preplacement = node.R;
            this[preplacement].L = node.L;
        }
        else {
            position.Push(node.R);
            for (ref var top = ref this[position.Top]; !top.L.IsNull; top = ref this[position.Top])
                position.Push(top.L);
            preplacement = position.TryPop();

            ref var replacement = ref this[preplacement];
            this[position.Top].L = replacement.R;
            replacement.L = node.L;
            replacement.R = node.R;
        }

        position.Path[inode] = preplacement;

        if (inode > 0) {
            ref var parent = ref this[position.Path[inode - 1]];
            if (pnode == parent.L) parent.L = preplacement;
            else parent.R = preplacement;
        }

        if (position.Count > 0) Root = Rebalance(position);
        else Root = Pointer.Null;
        --Count;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private Pointer Rebalance(Iterator<TValue, TTag> position) {
        for (int i = position.Count - 1; i >= 0; --i) {
            ref var pn = ref position.Path[i];
            int link = 0;
            if (i > 0)
                link = this[position.Path[i - 1]].L == pn ? -1 : 1;

            ref var n = ref this[pn];
            NodeTraits.UpdateTag(Allocator, ref n);

            var balance = NodeTag(ref n).Balance;
            if (balance > 1) {
                ref var r = ref this[n.R];
                var rrh = r.R.IsNull ? 0 : NodeTag(ref this[r.R]).Height;
                var rlh = r.L.IsNull ? 0 : NodeTag(ref this[r.L]).Height;
                pn = rrh >= rlh ? Rotations.L(pn) : Rotations.RL(pn);
            } else if (balance < -1) {
                ref var l = ref this[n.L];
                var llh = l.L.IsNull ? 0 : NodeTag(ref this[l.L]).Height;
                var lrh = l.R.IsNull ? 0 : NodeTag(ref this[l.R]).Height;
                pn = llh >= lrh ? Rotations.R(pn) : Rotations.LR(pn);
            }

            if (link == -1) this[position.Path[i - 1]].L = pn;
            else if (link == 1) this[position.Path[i - 1]].R = pn;
        }
        return position.Path[0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref AvlTag NodeTag(ref Node<TValue, TTag> node) => ref Unsafe.As<TTag, AvlTag>(ref node.T);
}