using System;
using System.Runtime.CompilerServices;

namespace Pfm.Collections.ReferenceTree;

public abstract partial class AbstractTree<TValue>
{
    public sealed class Node
    {
        public Node L, R;
        public int T;
        public TValue V;
    }

    public struct Iterator
    {
        public readonly AbstractTree<TValue> Tree;
        public readonly Node[] Path;
        public int Count;

        internal Iterator(AbstractTree<TValue> tree) {
            this.Tree = tree ?? throw new ArgumentNullException(nameof(tree));
            this.Path = new Node[MaxDepth];
            this.Count = 0;
        }

        private Iterator(Iterator other) {
            this.Tree = other.Tree;
            this.Path = new Node[MaxDepth];
            this.Count = other.Count;
            Array.Copy(other.Path, Path, Count);
        }

        public Iterator Clone() => new(this);

        /// <summary>
        /// False for <c>default</c> instance.
        /// </summary>
        public bool IsAllocated => Tree != null;

        public bool IsEmpty => Count == 0;
        public ref Node Top => ref Path[Count - 1];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Count = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(Node node) => Path[Count++] = node ?? throw new ArgumentNullException(nameof(node));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Node TryPop() => Count > 0 ? Path[--Count] : null;

        public Node First() {
            Clear();
            for (var n = Tree.Root; n != null; n = n.L)
                Push(n);
            return IsEmpty ? null : Top;
        }

        public Node Last() {
            Clear();
            for (var n = Tree.Root; n != null; n = n.R)
                Push(n);
            return IsEmpty ? null : Top;
        }

        /// <summary>
        /// Searches for <paramref name="value"/> and initializes the iterator to the last visited element, which is
        /// available through <see cref="Top"/> property.
        /// </summary>
        /// <param name="value">Value to search for; only the key fields (as determined by the comparison) must be set.</param>
        /// <returns>
        /// The result of the last comparison that lead to <paramref name="value"/>.
        /// Zero means that an element equal to <paramref name="value"/> has been found.
        /// Non-zero will be returned for an empty tree.
        /// </returns>
        public int Find(TValue value) {
            var n = Tree.Root;
            int c = -1;
            Clear();
            while (n != null) {
                Push(n);
                if ((c = Tree.Comparison(value, n.V)) == 0)
                    break;
                n = c < 0 ? n.L : n.R;
            }
            return c;
        }

        public Node Succ() {
            var current = TryPop();
            if (current == null)
                return null;
            if (current.R != null) {
                Push(current);
                for (current = current.R; current != null; current = current.L)
                    Push(current);
            } else {
                Node y;
                do {
                    y = current;
                    if ((current = TryPop()) == null)
                        return null;
                } while (y == current.R);
                Push(current);
            }
            return Top;
        }

        public Node Pred() {
            var current = TryPop();
            if (current == null)
                return null;
            if (current.L != null) {
                Push(current);
                for (current = current.L; current != null; current = current.R)
                    Push(current);
            } else {
                Node y;
                do {
                    y = current;
                    if ((current = TryPop()) == null)
                        return null;
                } while (y == current.L);
                Push(current);
            }
            return Top;
        }
    }
}
