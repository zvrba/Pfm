using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Pfm.Collections.CompactTree;

[DebuggerDisplay("Bits = {Bits}")]
public readonly struct Pointer : IEquatable<Pointer>
{
    public static readonly Pointer Null = new(0);

    public readonly ushort Bits;
    public bool IsNull => Bits == 0;
 
    public Pointer(int bits) => Bits = checked((ushort)bits);

    public bool Equals(Pointer other) => Bits == other.Bits;
    public override bool Equals(object other) => other is Pointer p && Equals(p);
    public override int GetHashCode() => Bits;

    public static bool operator ==(Pointer left, Pointer right) => left.Equals(right);
    public static bool operator !=(Pointer left, Pointer right) => !(left == right);
}

[StructLayout(LayoutKind.Sequential)]
public struct Node<TValue, TTag> where TTag : struct
{
    public Pointer L;
    public Pointer R;
    public TValue V;
    public TTag T;
}

public interface IAllocator<TValue, TTag> where TTag : struct
{
    int Capacity => ushort.MaxValue - 1;
    ref Node<TValue, TTag> this[Pointer pointer] { get; }
    Pointer Allocate();
    void Free(Pointer pointer);
    void Compact(float threshold);
}

public delegate void TagUpdate<TValue, TTag>(IAllocator<TValue, TTag> al, ref Node<TValue, TTag> node)
    where TTag : struct;

public readonly struct NodeTraits<TValue, TTag> where TTag : struct
{
    public NodeTraits(Comparison<TValue> comparison, TagUpdate<TValue, TTag> tagUpdate) {
        Compare = comparison;
        UpdateTag = tagUpdate;
    }
    public readonly Comparison<TValue> Compare;
    public readonly TagUpdate<TValue, TTag> UpdateTag;
}

public abstract class AbstractTree<TValue, TTag> where TTag : struct
{
    public readonly IAllocator<TValue, TTag> Allocator;
    public readonly NodeTraits<TValue, TTag> NodeTraits;
    public Pointer Root { get; protected set; }
    public int Count { get; protected set; }

    public ref Node<TValue, TTag> this[Pointer pointer] => ref Allocator[pointer];

    protected AbstractTree(IAllocator<TValue, TTag> allocator, NodeTraits<TValue, TTag> nodeTraits) {
        Allocator = allocator;
        NodeTraits = nodeTraits;
        Root = Pointer.Null;
    }

    public abstract bool Add(ref Iterator<TValue, TTag> position, TValue value, TTag tag);
    public abstract void Remove(ref Iterator<TValue, TTag> position);

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public bool Find(TValue value, out Node<TValue, TTag> position) {
        Pointer node = Root, parent = Pointer.Null;
        var c = -1;
        while (!node.IsNull) {
            parent = node;
            ref readonly var n = ref Allocator[node];
            if ((c = NodeTraits.Compare(value, n.V)) == 0)
                break;
            node = c < 0 ? n.L : n.R;
        }

        position = parent.IsNull ? default : Allocator[parent];
        return c == 0;
    }

    public Iterator<TValue, TTag> GetIterator() => new(this);

    public Pointer First(out Iterator<TValue, TTag> iterator) {
        iterator = new(this);
        return iterator.First();
    }

    public Pointer Last(out Iterator<TValue, TTag> iterator) {
        iterator = new(this);
        return iterator.Last();
    }

    public int Seek(TValue value, out Iterator<TValue, TTag> iterator) {
        iterator = new(this);
        return iterator.Find(value);
    }
}
