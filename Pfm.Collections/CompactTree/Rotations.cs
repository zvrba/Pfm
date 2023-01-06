using System;
using System.Runtime.CompilerServices;

namespace Pfm.Collections.CompactTree;

public readonly struct Rotations<TValue, TTag> where TTag : struct
{
    public readonly IAllocator<TValue, TTag> Al;
    public readonly NodeTraits<TValue, TTag> Tr;

    public Rotations(IAllocator<TValue, TTag> a, NodeTraits<TValue, TTag> tr) {
        Al = a; Tr = tr;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Pointer L(Pointer pn) {
        ref var n = ref Al[pn];

        var py = n.R;
        ref var y = ref Al[py];
        
        n.R = y.L;
        y.L = pn;

        Tr.UpdateTag(Al, ref n);
        Tr.UpdateTag(Al, ref y);

        return py;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Pointer R(Pointer pn) {
        ref var n = ref Al[pn];

        var px = n.L;
        ref var x = ref Al[px];
        
        n.L = x.R;
        x.R = pn;
        
        Tr.UpdateTag(Al, ref n);
        Tr.UpdateTag(Al, ref x);
        
        return px;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Pointer LR(Pointer pn) {
        ref var n = ref Al[pn];

        var px = n.L;
        ref var x = ref Al[px];

        var py = x.R;
        ref var y = ref Al[py];

        x.R = y.L;
        n.L = y.R;
        y.L = px;
        y.R = pn;

        Tr.UpdateTag(Al, ref x);
        Tr.UpdateTag(Al, ref n);
        Tr.UpdateTag(Al, ref y);
        
        return py;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Pointer RL(Pointer pn) {
        ref var n = ref Al[pn];

        var px = n.R;
        ref var x = ref Al[px];

        var py = x.L;
        ref var y = ref Al[py];

        n.R = y.L;
        x.L = y.R;
        y.L = pn;
        y.R = px;

        Tr.UpdateTag(Al, ref n);
        Tr.UpdateTag(Al, ref x);
        Tr.UpdateTag(Al, ref y);

        return py;
    }
}
