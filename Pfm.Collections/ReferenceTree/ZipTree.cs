using System;
using System.Diagnostics;

namespace Pfm.Collections.ReferenceTree;

/// <summary>
/// Zip tree, based on description and algorithms given in
/// Robert E.Tarjan, Caleb Levy, and Stephen Timmel. 2021. Zip Trees. ACM Trans. Algorithms 17, 4, Article 34
/// (October 2021), 12 pages. https://doi.org/10.1145/3476830
/// See also https://stackoverflow.com/questions/61944198/what-is-a-zip-tree-and-how-does-it-work
/// </summary>
public class ZipTree<TValue> : AbstractTree<TValue>
{
    // Less-than and greater-than nodes encountered during traversal.
    private Iterator ltn;
    private Iterator gtn;

    public ZipTree(Comparison<TValue> comparison) : base(comparison) {
        Random = Random.Shared;
        ltn = new Iterator(this);
        gtn = new Iterator(this);
    }

    /// <summary>
    /// Random source used for ranks.  Normally, <see cref="Random.Shared"/> is used, but internal setter
    /// is available for deterministic tests.
    /// </summary>
    public Random Random { get; internal set; }

    public override bool Insert(TValue value, out Node node) {
        // The following remember what and where to insert at do_add label.
        Node prev = null, leftPath = null, rightPath = null;
        int cprev = 0;

        // Loop rewritten to invoke comparison only once.
        // Original: while (cur != null && (rank < cur.T || (rank == cur.T && Comparison(value, cur.V) > 0)))
        var rank = GetRank();
        //Debug.Print($"{value} (@{rank})\n");

        Node cur = Root;
        while (cur != null) {
            var c = Comparison(value, cur.V);
            if (c == 0) {
                node = cur;
                return false;
            }
            if (rank < cur.T || (rank == cur.T && c > 0)) {
                prev = cur;
                cprev = c;
                cur = cprev < 0 ? cur.L : cur.R;
            } else {
                break;
            }
        }

        // We're done if the above search ends up at null; nothing to unzip.
        if (cur == null)
            goto do_add;

        // Insertion point is between prev and cur.  Continue searching to find existing value, if any.
        ltn.Clear();
        gtn.Clear();
        node = cur;
        do {
            var c = Comparison(value, node.V);
            if (c == 0)
                return false;
            if (c < 0) {
                gtn.Push(node);
                node = node.L;
            } else {
                ltn.Push(node);
                node = node.R;
            }
        } while (node != null);

        // Value not found, finalize unzipping and link the node.
        if (!ltn.IsEmpty) {
            for (int i = 0; i < ltn.Count - 1; ++i)
                ltn.Path[i].R = ltn.Path[i + 1];
            ltn.Top.R = null;
            leftPath = ltn.Path[0];
        }

        if (!gtn.IsEmpty) {
            for (int i = 0; i < gtn.Count - 1; ++i)
                gtn.Path[i].L = gtn.Path[i + 1];
            gtn.Top.L = null;
            rightPath = gtn.Path[0];
        }

    do_add:
        node = new() { V = value, T = rank, L = leftPath, R = rightPath };
        if (prev != null) {
            if (cprev < 0) prev.L = node;
            else prev.R = node;
        }
        else {
            Root = node;
        }
        ++Count;
        return true;
    }

    // Geometric distribution with mean = 1.  LZC always returns > 0 since Next() returns a positive 32-bit
    // number,  i.e., the sign bit is always 0.
    private int GetRank() =>
        (int)System.Runtime.Intrinsics.X86.Lzcnt.LeadingZeroCount((uint)Random.Next()) - 1;

    public override Node Delete(TValue value) {
        if (ltn.Find(value) != 0)
            return null;

        // Find the node to return and the parent to fix.
        int cprev = ltn.Find(value);
        if (cprev != 0)
            return null;
        var node = ltn.TryPop();
        var prev = ltn.TryPop();
        if (prev != null)
            cprev = prev.L == node ? -1 : 1;

        // Zip the paths.
        ltn.Clear();
        gtn.Clear();
        for (var n = node.L; n != null; n = n.R)
            ltn.Push(n);
        for (var n = node.R; n != null; n = n.L)
            gtn.Push(n);

        // Temporarily use the removed node's L link as head to add nodes of the zipped list.
        ref var tail = ref node.L;          
        int iln = 0, ign = 0;
        while (true) {
            Node ln = iln < ltn.Count ? ltn.Path[iln] : null;
            Node rn = ign < gtn.Count ? gtn.Path[ign] : null;
            if (ln == null && rn == null)
                break;

            if (ln == null) goto output_right;
            else if (rn == null) goto output_left;
            else if (ln.T > rn.T) goto output_left;
            else if (ln.T < rn.T) goto output_right;
            else if (Comparison(ln.V, rn.V) < 0) goto output_left;
            else goto output_right;

        output_left:
            tail = ln;
            tail = ref ln.R;
            ++iln;
            continue;

        output_right:
            tail = rn;
            tail = ref rn.L;
            ++ign;
            continue;
        }


        if (prev != null) {
            if (cprev < 0) prev.L = node.L;
            else prev.R = node.L;
        }
        else {
            Root = node.L;
        }
        node.L = node.R = null;
        --Count;
        return node;
    }
    
    // Not needed by zip trees: the node's rank is constant.
    protected override void UpdateTag(Node node) => throw new NotImplementedException();

    internal override void ValidateStructure() {
        ValidateRanks(Root);

        static int ValidateRanks(Node node) {
            if (node == null)   // Lowest rank is 0.
                return -1;
            var l = ValidateRanks(node.L);
            var r = ValidateRanks(node.R);
            if (!(node.T > l && node.T >= r))
                throw new NotImplementedException();
            return node.T;
        }
    }

#if false   // This method can be used to validate the distribution.
    private static void ValidateDistribution() {
        var c = new int[33];
        var M = 1 << 20;
        for (int i = 0; i < M; ++i) {
            var r = Random.Shared.Next();
            var z = System.Runtime.Intrinsics.X86.Lzcnt.LeadingZeroCount((uint)r) - 1;
            ++c[z];
        }
        for (int i = 0; i < c.Length; ++i)
            Console.WriteLine(string.Format("{0:D2} {1}", i, c[i] / (float)M));
    }
#endif
}
