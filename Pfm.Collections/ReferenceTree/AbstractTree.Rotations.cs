using System;
using System.Runtime.CompilerServices;

namespace Pfm.Collections.ReferenceTree;

public abstract partial class AbstractTree<TValue>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Node RotL(Node n) {
        var y = n.R;
        n.R = y.L;
        y.L = n;
        UpdateTag(n);
        UpdateTag(y);
        return y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Node RotR(Node n) {
        var x = n.L;
        n.L = x.R;
        x.R = n;
        UpdateTag(n);
        UpdateTag(x);
        return x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Node RotLR(Node n) {
        var x = n.L;
        var y = x.R;
        x.R = y.L;
        n.L = y.R;
        y.L = x;
        y.R = n;
        UpdateTag(x);
        UpdateTag(n);
        UpdateTag(y);
        return y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Node RotRL(Node n) {
        var x = n.R;
        var y = x.L;
        n.R = y.L;
        x.L = y.R;
        y.L = n;
        y.R = x;
        UpdateTag(n);
        UpdateTag(x);
        UpdateTag(y);
        return y;
    }
}
