using System;

namespace Pfm.Collections.JoinTree;

public struct Rotations<TValue, TNodeTraits, TTreeTraits>
    where TNodeTraits : struct, INodeTraits<TValue>
    where TTreeTraits : struct, ITreeTraits<TValue>
{
    // Utility node constructor.
    public static Node<TValue> Node(Node<TValue> left, TValue value, Node<TValue> right) {
        var n = new Node<TValue>() { L = left, R = right, V = value };
        TTreeTraits.Update(n);
        return n;
    }

    public static Node<TValue> RotL(Node<TValue> n) {
        n = TNodeTraits.Clone(n);
        var y = TNodeTraits.Clone(n.R);
        n.R = y.L;
        y.L = n;
        TTreeTraits.Update(n);
        TTreeTraits.Update(y);
        return y;
    }

    public static Node<TValue> RotR(Node<TValue> n) {
        n = TNodeTraits.Clone(n);
        var x = TNodeTraits.Clone(n.L);
        n.L = x.R;
        x.R = n;
        TTreeTraits.Update(n);
        TTreeTraits.Update(x);
        return x;
    }

    public static Node<TValue> RotLR(Node<TValue> n) {
        n = TNodeTraits.Clone(n);
        var x = TNodeTraits.Clone(n.L);
        var y = TNodeTraits.Clone(x.R);
        x.R = y.L;
        n.L = y.R;
        y.L = x;
        y.R = n;
        TTreeTraits.Update(x);
        TTreeTraits.Update(n);
        TTreeTraits.Update(y);
        return y;
    }

    public static Node<TValue> RotRL(Node<TValue> n) {
        n = TNodeTraits.Clone(n);
        var x = TNodeTraits.Clone(n.R);
        var y = TNodeTraits.Clone(x.L);
        n.R = y.L;
        x.L = y.R;
        y.L = n;
        y.R = x;
        TTreeTraits.Update(n);
        TTreeTraits.Update(x);
        TTreeTraits.Update(y);
        return y;
    }
}