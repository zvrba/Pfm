using System;

namespace Podaga.PersistentCollections.Trie;

public partial class DenseTrie<T>
{
    /// <summary>
    /// Trie node.  Two nodes are equal iff their <see cref="Data"/> members are equal (and, consequently, their
    /// <see cref="Transient"/> fields, because of cloning rules).
    /// </summary>
    public readonly struct Node : IEquatable<Node>
    {
        public readonly ICloneable Data;
        public readonly ulong Transient;

        // TODO: Could use Unsafe.As for casting.
        public Node[] Link => (Node[])Data;
        public T[] Value => (T[])Data;
        public bool IsNull => Data == null;

        public Node(object data, ulong transient) {
            Data = (ICloneable)data;
            Transient = transient;
        }

        public Node Clone(ulong transient) => transient == Transient ? this : new(Data.Clone(), transient);

        public bool Equals(Node other) => Data == other.Data;
        public static bool operator ==(Node n1, Node n2) => n1.Equals(n2);
        public static bool operator !=(Node n1, Node n2) => !n1.Equals(n2);

        public override bool Equals(object obj) => obj is Node other && Equals(other);
        public override int GetHashCode() => Data.GetHashCode();    // Transient is not usable as it's sequential.
    }

    private Node CreateLink() => new(new Node[Parameters.ISize], transient);
    private Node CreateLeaf() => new(new T[Parameters.ESize], transient);
}
