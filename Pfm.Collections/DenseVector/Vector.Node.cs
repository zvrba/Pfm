using System;

namespace Podaga.PersistentCollections.DenseVector;

public partial class Vector<T>
{
    /// <summary>
    /// Trie node.  Two nodes are equal iff their <see cref="Data"/> members are equal (and, consequently, their
    /// <see cref="Transient"/> fields, because of cloning rules).
    /// </summary>
    public readonly struct Node
    {
        /// <summary>
        /// Node data.  This is more conveniently accessed through <see cref="Link"/> or <see cref="Value"/>.
        /// </summary>
        public readonly ICloneable Data;

        /// <summary>
        /// The node's transient tag.
        /// </summary>
        public readonly ulong Transient;

        // TODO: Could use Unsafe.As for casting.

        /// <summary>
        /// Interprets <see cref="Data"/> as an array of nodes.
        /// This property can be used only on internal nodes, otherwise <see cref="InvalidCastException"/> is thrown.
        /// </summary>
        public Node[] Link => (Node[])Data;

        /// <summary>
        /// Interprets <see cref="Data"/> as an array of <typeparamref name="T"/> values.
        /// This property can be used only on external nodes, otherwise <see cref="InvalidCastException"/> is thrown.
        /// </summary>
        public T[] Value => (T[])Data;
        
        /// <summary>
        /// This property is true when <see cref="Data"/> is null (i.e., a <c>default</c> instance).
        /// </summary>
        public bool IsNull => Data == null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="data">
        /// Array of data values.  The type must be either <c>T[]</c> or <c>Node[]</c>.  This is not checked.
        /// </param>
        /// <param name="transient">Transient tag for cloning.</param>
        public Node(object data, ulong transient) {
            Data = (ICloneable)data;
            Transient = transient;
        }

        /// <summary>
        /// Shallowly clones <c>this</c>.
        /// </summary>
        /// <param name="transient">Transient tag for cloning.</param>
        /// <returns>New instance or <c>this</c>, depending on whether <paramref name="transient"/> is equal to <c>this.Transient</c>.</returns>
        public Node Clone(ulong transient) => transient == Transient ? this : new(Data.Clone(), transient);
    }

    private Node CreateLink() => new(new Node[Parameters.ISize], Transient);
    private Node CreateLeaf() => new(new T[Parameters.ESize], Transient);
}
