using System;
using System.Collections.Generic;
using System.Threading;

namespace Podaga.PersistentCollections.Trie;

/// <summary>
/// Immutable/transient vector that can also act as a stack.
/// </summary>
public partial class Vector<T>
{
    /// <summary>
    /// Parameters for this vector.
    /// </summary>
    public readonly VectorParameters Parameters;
    
    /// <summary>
    /// The vector's transient tag.
    /// </summary>
    public readonly ulong Transient;

    internal Node _Root;
    internal Node _Tail;
    internal int _Shift; // Levels below the root so that (index >> Shift) & IMask is the correct root slot.


    public int Count { get; private set; }
    public T this[int index] {
        get => Get(index);
        set => Set(index, value);
    }

    // INVARIANT: The tail is never empty, except when the trie is empty.

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="parameters">Parameters that determine the branching factors.</param>
    public Vector(VectorParameters parameters) {
        Parameters = parameters;
        Transient = TransientSource.NewTransient();
        _Root = CreateLink();
        _Tail = CreateLeaf();
        _Shift = Parameters.EShift;
    }

    private Vector(Vector<T> other) {
        Parameters = other.Parameters;
        Transient = TransientSource.NewTransient();
        _Root = other._Root;
        _Tail = other._Tail;
        _Shift = other._Shift;
        Count = other.Count;
    }

    public Vector<T> Fork() => new(this);

    private void CheckIndex(int index) {
        if (index < 0 || index >= Count)
            throw new IndexOutOfRangeException($"Index {index} is out of range.  Vector size is {Count}.");
    }

    private T Get(int index) {
        CheckIndex(index);

        ref Node node = ref _Tail;
        if (index < ((Count - 1) & ~Parameters.EMask)) {
            node = ref _Root;
            for (var shift = this._Shift; shift >= Parameters.EShift; shift -= Parameters.IShift)
                node = ref node.Link[(index >> shift) & Parameters.IMask];
        }
        
        return node.Value[index & Parameters.EMask];
    }

    private void Set(int index, T element) {
        CheckIndex(index);

        ref Node node = ref _Tail;
        if (index < ((Count - 1) & ~Parameters.EMask)) {
            //ret.Root = ret.Clone(Root);
            node = ref _Root;
            for (var shift = _Shift; shift >= Parameters.EShift; shift -= Parameters.IShift) {
                node = node.Clone(Transient);
                node = ref node.Link[(index >> shift) & Parameters.IMask];
            }
        }

        node = node.Clone(Transient);
        node.Value[index & Parameters.EMask] = element;
    }

    public void Push(T element) {
        if ((Count & Parameters.EMask) == 0) {
            if (Count > 0)
                PushTail();
        }
        else {
            _Tail = _Tail.Clone(Transient);
        }

        _Tail.Value[Count & Parameters.EMask] = element;
        ++Count;
    }

    private void PushTail() {
        if (Count > 1 << (_Shift + Parameters.IShift)) {
            var newroot = CreateLink();
            newroot.Link[0] = _Root;
            _Root = newroot;
            _Shift += Parameters.IShift;
        }
        DoPush(ref _Root, _Shift);
        _Tail = CreateLeaf();

        void DoPush(ref Node node, int shift) {
            if (node.IsNull) node = CreateLink();
            else node = node.Clone(Transient);

            var islot = ((Count - 1) >> shift) & Parameters.IMask;
            if (shift <= Parameters.IShift) node.Link[islot] = _Tail;
            else DoPush(ref node.Link[islot], shift - Parameters.IShift);
        }
    }

    public bool TryPop(out T element) {
        if (Count == 0) {
            element = default;
            return false;
        }
        _Tail = _Tail.Clone(Transient);
        --Count;
        element = _Tail.Value[Count & Parameters.EMask];
        _Tail.Value[Count & Parameters.EMask] = default;   // Must have for GC to collect previously referenced data.
        if ((Count & Parameters.EMask) == 0)
            PopTail();
        return true;
    }

    private void PopTail() {
        DoPop(ref _Root, _Shift);
        if (_Shift > Parameters.EShift) {
            if (_Root.Link[1].IsNull) {
                _Root = _Root.Link[0];
                _Shift -= Parameters.IShift;
            }
        }
        else if (_Root.IsNull) {
            _Root = CreateLink();    // TODO: transient.
        }

        void DoPop(ref Node node, int shift) {
            var islot = ((Count - 1) >> shift) & Parameters.IMask;
            node = node.Clone(Transient);

            if (shift == Parameters.EShift) {
                _Tail = node.Link[islot];
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
