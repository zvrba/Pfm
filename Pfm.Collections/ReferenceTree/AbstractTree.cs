using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pfm.Collections.ReferenceTree;

public abstract partial class AbstractTree<TValue> : ICollection<TValue>
{
    public const int MaxDepth = 48;                         // Depth of balanced with up to 2^32 elements tree should not exceed this.

    public Node Root { get; protected set; }
    public int Count { get; protected set; }
    public Comparison<TValue> Comparison { get; }

    protected AbstractTree(Comparison<TValue> comparison) {
        Root = null;
        Count = 0;
        Comparison = comparison;
    }

    public abstract bool Insert(TValue value, out Node node);
    public abstract Node Delete(TValue value);
    protected abstract void UpdateTag(Node node);

    // Used by tests: Should throw NotImplementedException on failure.
    internal abstract void ValidateStructure();

    public bool Find(TValue value, out Node position) {
        Node node = Root, parent = null;    // Avoid writing to position on hot path; perf penalty.
        int c = -1;
        while (node != null) {
            parent = node;
            if ((c = Comparison(value, node.V)) == 0)
                break;
            node = c < 0 ? node.L : node.R;
        }

        position = parent;
        return c == 0;
    }

    public int Depth() {
        return Recurse(Root);

        static int Recurse(Node n) {
            if (n == null)
                return 0;
            var l = Recurse(n.L);
            var r = Recurse(n.R);
            return 1 + (l > r ? l : r);
        }
    }

    public Iterator GetIterator() => new(this);

    public bool IsReadOnly => false;
    public void Add(TValue item) => Insert(item, out var _);
    public void Clear() { Root = null; Count = 0; }
    public bool Contains(TValue item) => Find(item, out var _);
    public bool Remove(TValue item) => Delete(item) != null;
    
    public void CopyTo(TValue[] array, int idx) {
        if (idx + Count > array.Length)
            throw new ArgumentException("Invalid start index.", nameof(idx));    // Exception type given by iface contract.
        foreach (var n in Inorder(Root))
            array[idx++] = n.V;
    }

    public IEnumerator<TValue> GetEnumerator() => Inorder(Root).Select(x => x.V).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    private IEnumerable<Node> Inorder(Node node) {
        if (node.L != null) {
            foreach (var n in Inorder(node.L))
                yield return n;
        }
        
        yield return node;

        if (node.R != null) {
            foreach (var n in Inorder(node.R))
                yield return n;
        }
    }
}
