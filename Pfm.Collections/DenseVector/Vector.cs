using System;

namespace Podaga.PersistentCollections.DenseVector;

/// <summary>
/// Immutable/transient vector that can also act as a stack.
/// </summary>
/// <typeparam name="T">Element type held stored by the vector.</typeparam>
public partial class Vector<T>
{
    /// <summary>
    /// Parameters for this vector.
    /// </summary>
    public readonly VectorParameters Parameters;

    /// <summary>
    /// The vector's transient tag.
    /// </summary>
    public ulong Transient => _Transient;
    private ulong _Transient;

    internal Node _Root;
    internal Node _Tail;
    internal int _Shift; // Levels below the root so that (index >> Shift) & IMask is the correct root slot.


    /// <summary>
    /// Number of elements in the vector.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Provides index-based access to elements.
    /// </summary>
    /// <param name="index">Index to access.</param>
    /// <returns>Value stored at index.</returns>
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
        _Transient = TransientSource.NewTransient();
        _Root = CreateLink();
        _Tail = CreateLeaf();
        _Shift = Parameters.EShift;
    }

    private Vector(Vector<T> other) {
        Parameters = other.Parameters;
        _Transient = TransientSource.NewTransient();
        _Root = other._Root;
        _Tail = other._Tail;
        _Shift = other._Shift;
        Count = other.Count;
    }

    /// <summary>
    /// Forks the vector.  Ensures that modifications to <c>this</c> and the forked version are invisible to each other.
    /// </summary>
    /// <returns>
    /// A forked instance that contains the same elements as <c>this</c>.
    /// </returns>
    public Vector<T> Fork() {
        // Original's transient change so that changes in the original do not affect the fork.
        _Transient = TransientSource.NewTransient();
        return new(this);
    }

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

    /// <summary>
    /// Appends an element to <c>this</c>.
    /// </summary>
    /// <param name="element">Element to append.</param>
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

    /// <summary>
    /// Removes the last element of <c>this</c>.
    /// </summary>
    /// <param name="element">Receives the removed element.</param>
    /// <returns>
    /// True if <see cref="Count"/> was positive, in which case <paramref name="element"/> is set to the removed element.
    /// Otherwise false, and <paramref name="element"/> is set to <c>default</c>.
    /// </returns>
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
