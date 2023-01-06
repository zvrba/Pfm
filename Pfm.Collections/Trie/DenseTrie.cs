using System;

namespace Pfm.Collections.Trie;

/// <summary>
/// The structure and algorithms underlying vectors.
/// </summary>
internal partial class DenseTrie<T>
{
    public readonly TrieParameters Parameters;
    internal Node Root;
    internal Node Tail;
    internal int Shift; // Levels below the root so that (index >> Shift) & IMask is the correct root slot.
    internal object Transient;

    // INVARIANT: The tail is never empty, except when the trie is empty.

    public DenseTrie(TrieParameters parameters) {
        Parameters = parameters;
        Root = CreateLink();
        Tail = CreateLeaf();
        Shift = Parameters.EShift;
    }

    internal DenseTrie(DenseTrie<T> other, bool isTransient) {
        this.Parameters = other.Parameters;
        this.Root = other.Root;
        this.Tail = other.Tail;
        this.Shift = other.Shift;
        this.Count = other.Count;
        this.Transient = isTransient ? this : null;
    }

    private void CheckIndex(int index) {
        if (index < 0 || index >= Count)
            throw new IndexOutOfRangeException($"Index {index} is out of range.  Vector size is {Count}.");
    }

    public int Count { get; private set; }

    public T Get(int index) {
        CheckIndex(index);

        ref Node node = ref Tail;
        if (index < ((Count - 1) & ~Parameters.EMask)) {
            node = ref Root;
            for (var shift = this.Shift; shift >= Parameters.EShift; shift -= Parameters.IShift)
                node = ref node.Link[(index >> shift) & Parameters.IMask];
        }
        
        return node.Value[index & Parameters.EMask];
    }

    public DenseTrie<T> Set(int index, T element) {
        CheckIndex(index);

        var ret = Clone();
        ref Node node = ref ret.Tail;
        if (index < ((Count - 1) & ~Parameters.EMask)) {
            //ret.Root = ret.Clone(Root);
            node = ref ret.Root;
            for (var shift = Shift; shift >= Parameters.EShift; shift -= Parameters.IShift) {
                node = Clone(node);
                node = ref node.Link[(index >> shift) & Parameters.IMask];
            }
        }

        node = Clone(node);
        node.Value[index & Parameters.EMask] = element;
        return ret;
    }

    public DenseTrie<T> Push(T element) {
        var ret = Clone();
        if ((Count & Parameters.EMask) == 0) {
            if (Count > 0)
                ret.PushTail();
        }
        else {
            ret.Tail = Clone(this.Tail);
        }

        ret.Tail.Value[ret.Count & Parameters.EMask] = element;
        ++ret.Count;
        return ret;
    }

    private void PushTail() {
        if (Count > 1 << (Shift + Parameters.IShift)) {
            var newroot = CreateLink();
            newroot.Link[0] = Root;
            Root = newroot;
            Shift += Parameters.IShift;
        }
        DoPush(ref Root, Shift);
        Tail = CreateLeaf();

        void DoPush(ref Node node, int shift) {
            if (node.IsNull) node = CreateLink();
            else node = Clone(node);

            var islot = ((Count - 1) >> shift) & Parameters.IMask;
            if (shift <= Parameters.IShift) node.Link[islot] = Tail;
            else DoPush(ref node.Link[islot], shift - Parameters.IShift);
        }
    }

    public DenseTrie<T> Pop(out T element) {
        if (Count == 0)
            throw new InvalidOperationException("The trie is empty.");

        var ret = Clone();
        ret.Tail = Clone(ret.Tail);
        --ret.Count;
        element = ret.Tail.Value[ret.Count & Parameters.EMask];
        ret.Tail.Value[ret.Count & Parameters.EMask] = default;   // Must have for GC to collect previously referenced data.
        if ((ret.Count & Parameters.EMask) == 0)
            ret.PopTail();
        return ret;
    }

    private void PopTail() {
        DoPop(ref Root, Shift);
        if (Shift > Parameters.EShift) {
            if (Root.Link[1].IsNull) {
                Root = Root.Link[0];
                Shift -= Parameters.IShift;
            }
        }
        else if (Root.IsNull) {
            Root = CreateLink();    // TODO: transient.
        }

        void DoPop(ref Node node, int shift) {
            var islot = ((Count - 1) >> shift) & Parameters.IMask;
            node = Clone(node);

            if (shift == Parameters.EShift) {
                Tail = node.Link[islot];
                node.Link[islot] = default;
            }
            else {
                DoPop(ref node.Link[islot], shift - Parameters.IShift);
            }
            
            if (node.Link[0].IsNull)
                node = default;
        }
    }
}
