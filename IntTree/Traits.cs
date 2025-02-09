using System;
using Podaga.PersistentCollections.Tree;

namespace IntTree;

struct IntTraits : IValueTraits<int>
{
    public static int Compare(int left, int right) => left.CompareTo(right);

    public static JoinableAvlTree<AvlTag, int, IntTraits> CreateAvlTree() =>
        new JoinableAvlTree<AvlTag, int, IntTraits>();
    public static JoinableWBTree<WBTag, int, IntTraits> CreateWBTree() =>
        new JoinableWBTree<WBTag, int, IntTraits>();
}

struct AvlTag : IAvlTagTraits<AvlTag>
{
    public int Rank { get; set; }
    public int Size { get; set; }
}

struct WBTag : IWBTagTraits<WBTag>
{
    public int Rank { get; set; }
    public int Size { get; set; }
}
