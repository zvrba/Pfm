using System;

namespace Pfm.Collections.Trie;

/// <summary>
/// Holder of a persistent vector reference.  NB!  This struct is mutable, so as to provide an illusion of a mutable
/// interface over a persistent data structure.  The equality is "shallow" equality, i.e., vectors are equal iff they
/// point to the physically same tree.
/// </summary>
/// <remarks>
/// Having multiple copies of the same transient vector should be avoided as all copies share the same underlying storage.
/// The language does not allow overloading of assignment operator, so it's impossible to implement any compile-time or
/// run-time checks for this scenario.
/// </remarks>
/// <typeparam name="T">Type of elements held by the vector.</typeparam>
public struct ImmutableVector<T> : IEquatable<ImmutableVector<T>>
{
    internal DenseTrie<T> Trie;  // Internal for testing.

    /// <summary>
    /// Creates a new instance with the given internal and external node sizes.
    /// Both shifts must be between 2 and 7 (inclusive).
    /// </summary>
    /// <param name="ishift">Number of bits used to address an internal node.  Default is 5 (32 elements).</param>
    /// <param name="eshift">Number of bits used to address an external node.  Default is 5 (32 elements).</param>
    /// <returns></returns>
    public static ImmutableVector<T> Create(int ishift = 5, int eshift = 5) =>
        new(new DenseTrie<T>(new TrieParameters(ishift, eshift)));

    internal ImmutableVector(DenseTrie<T> trie) {
        Trie = trie;
    }

    public bool IsTransient => Trie.Transient == Trie;
    public int InternalBits => Trie.Parameters.IShift;
    public int ExternalBits => Trie.Parameters.EShift;
    public int Count => Trie.Count;

    public T this[int index] {
        get => Trie.Get(index);
        set => Trie = Trie.Set(index, value);
    }

    public void Clear() => Trie = new DenseTrie<T>(Trie.Parameters);

    public void Push(T element) => Trie = Trie.Push(element);

    public T Pop() {
        Trie = Trie.Pop(out var ret);
        return ret;
    }

    public ImmutableVector<T> MakeTransient() => new(new DenseTrie<T>(Trie, true));
    public ImmutableVector<T> MakePersistent() => new(new DenseTrie<T>(Trie, false));

    public bool Equals(ImmutableVector<T> other) => ReferenceEquals(Trie, other.Trie);
    public override bool Equals(object obj) => obj is ImmutableVector<T> v && Equals(v);
    public override int GetHashCode() => Trie.GetHashCode();
    public static bool operator ==(ImmutableVector<T> v1, ImmutableVector<T> v2) => v1.Equals(v2);
    public static bool operator !=(ImmutableVector<T> v1, ImmutableVector<T> v2) => !v1.Equals(v2);
}
