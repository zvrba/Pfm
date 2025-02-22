using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Podaga.PersistentCollections.Tree;

public static partial class TreeAlgorithms
{
    public static bool SetEquals<TValue>
        (
        JoinableTreeNode<TValue> a,
        JoinableTreeNode<TValue> b
        )
        where TValue : ITaggedValue<TValue>
    {
        if (a is null || b is null)
            return (a is null) == (b is null);
        if (a.Size != b.Size)
            return false;

        var ai = a.GetIterator();
        ai.First(null);

        var bi = b.GetIterator();
        bi.First(null);

        // At this point, sizes are equal and at least 1.
        do {
            if (TValue.Compare(ai.Top.Value, bi.Top.Value) != 0)
                return false;
        } while (ai.Succ() && bi.Succ());
        return true;
    }

    // TODO: XXX: Transient here?

    public static JoinableTreeNode<TValue> SetUnion<TValue, TJoin>
        (
        JoinableTreeNode<TValue> t1,
        JoinableTreeNode<TValue> t2
        )
        where TValue : ITaggedValue<TValue>
        where TJoin : struct, ITreeJoin<TValue>
    {
        if (t1 == null)
            return t2;
        if (t2 == null)
            return t1;

        var s = new TreeSection<TValue> { Transient = 0 }.Split<TJoin>(t1, t2.Value);
        var l = SetUnion<TValue, TJoin>(s.Left, t2.Left);
        var r = SetUnion<TValue, TJoin>(s.Right, t2.Right);
        if (s.Middle != null) {
            t1 = s.Middle.Clone(s.Transient);
            TValue.Combine(t1.Value, ref t1.Value, t2.Value);
        } else {
            t1 = t2;
        }
        return TJoin.Join(new() { Transient = 0, Left = l, Middle = t1, Right = r });
    }
}
