using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Pfm.Collections.Trie;

internal partial class DenseTrie<T>
{
    // A node is valid transient only when node.Transient == root.Transient == reference to the trie.
    internal struct Node
    {
        internal object Data;
        internal object Transient;

        internal Node[] Link => (Node[])Data;
        internal T[] Value => (T[])Data;
        internal bool IsNull => Data == null;

        internal Node(object data, object transient) {
            Data = data;
            Transient = transient;
        }

        internal Node Clone(object transient) => transient != null && Transient == transient  ?
            this : new(((ICloneable)Data).Clone(), null);
    }

    private DenseTrie<T> Clone() => Transient == this ? this : new DenseTrie<T>(this, false);
    private Node Clone(Node node) => node.Clone(Root.Transient);

    private Node CreateLink() => new(new Node[Parameters.ISize], Transient);
    private Node CreateLeaf() => new(new T[Parameters.ESize], Transient);
}
