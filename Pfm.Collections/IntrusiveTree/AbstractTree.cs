using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Pfm.Collections.IntrusiveTree;

/// <summary>
/// Base class used by Iterator.
/// </summary>
public abstract class AbstractTree<TNode, TValue, TTag>
    where TNode : class, INodeTraits<TNode, TValue, TTag>
    where TTag : struct, ITagTraits<TTag>
{
    protected TNode _Root;

    public TNode Root => _Root;
    public int Count { get; protected set; }

    /// <summary>
    /// Finds a node with value <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to find.</param>
    /// <param name="node">Set to the last visited node before returning.</param>
    /// <returns>
    /// Result of the last comparison before returning; if 0 <paramref name="value"/> was found.
    /// Searching in an empty tree will arbitrarily return -1 and set <paramref name="node"/> to <c>null</c>.
    /// </returns>
    //[MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public int Find(TValue value, out TNode node) {
        int c = -1;
        TNode p = null;
        for (var n = Root; n != null; p = n, n = c < 0 ? n.L : n.R) {
            if ((c = TNode.Compare(value, n.V)) == 0) {
                node = n;
                return 0;
            }
        }
        node = p;
        return c;
    }

    public Iterator<TNode, TValue, TTag> GetIterator() => new(this);
}

/// <summary>
/// Mutable tree, i.e., no support for transience.
/// </summary>
public abstract class AbstractIntrusiveTree<TNode, TValue, TTag, TBaseTag> : AbstractTree<TNode, TValue, TTag>
    where TNode : class, INodeTraits<TNode, TValue, TTag>
    where TTag : struct, ITagTraits<TTag>
    where TBaseTag : struct, ITagTraits<TBaseTag>
{
    // Needs to run only once.
    static AbstractIntrusiveTree() {
        var baseTagType = typeof(TBaseTag);
        var tagType = typeof(TTag);

        if (!baseTagType.IsValueType)
            throw new ArgumentException("The tree tag type is not a struct.");

        if (!tagType.IsValueType)
            throw new ArgumentException("TTag generic argument is not a struct.");
        if (tagType == baseTagType) // The same; no need for further checks.
            return;

        var a = tagType.GetCustomAttribute<StructLayoutAttribute>();
        if (a == null || a.Value != LayoutKind.Sequential)
            throw new ArgumentException("Custom tag must be annotated with StructLayout(LayoutKind.Sequential).");

        FieldInfo f0 = null;
        foreach (var f in tagType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
            if (Marshal.OffsetOf(tagType, f.Name) == 0) {
                f0 = f;
                break;
            }
        }
        if (f0 == null)
            throw new ArgumentException("Custom tag has no field at offset 0.");

        if (f0.FieldType != baseTagType)
            throw new ArgumentException($"Field at offset 0 was expected to have type {baseTagType.FullName}, but a field of type {f0.FieldType.FullName} was found instead.");
    }

    protected AbstractIntrusiveTree() {
        _Root = null;
    }

    /// <summary>
    /// Adds a new value to the tree.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <param name="tag">The associated tag.</param>
    /// <param name="node">Set to the newly created or node, or a found node.</param>
    /// <returns>False if <paramref name="value"/> exists in the tree, true otherwise.</returns>
    /// <exception cref="NotSupportedException">
    /// If the implementation is purely intrusive and <paramref name="node"/> is <c>null</c>.
    /// </exception>
    public abstract bool Add(TValue value, TTag tag, out TNode node);

    /// <summary>
    /// Removes a value from the tree.
    /// </summary>
    /// <param name="value">Value to remove/</param>
    /// <returns>
    /// The removed node or <c>null</c> is value was not found.
    /// </returns>
    public abstract TNode Remove(TValue value);

    #region Short-hand accessors

    /// <summary>
    /// This method exists because default interface implementation is not available on concrete base tags.
    /// </summary>
    protected static ref TBaseTag BaseTag(ref TTag tag) => ref TBaseTag.AsSelf(ref tag);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static void UpdateTag(TNode node) {
        var lt = node.L?.T ?? TTag.TZero;
        var rt = node.R?.T ?? TTag.TZero;
        node.T = TTag.TPlus(lt, rt);
    }

    #endregion

    #region Rotations: methods return the rotated-in node.  The parent is NOT fixed.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static TNode RotL(TNode n) {
        var y = n.R;
        n.R = y.L;
        y.L = n;
        UpdateTag(n);
        UpdateTag(y);
        return y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static TNode RotR(TNode n) {
        var x = n.L;
        n.L = x.R;
        x.R = n;
        UpdateTag(n);
        UpdateTag(x);
        return x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static TNode RotLR(TNode n) {
        var x = n.L;
        var y = x.R;
        x.R = y.L;
        n.L = y.R;
        y.L = x;
        y.R = n;
        UpdateTag(x);
        UpdateTag(n);
        UpdateTag(y);
        return y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static TNode RotRL(TNode n) {
        var x = n.R;
        var y = x.L;
        n.R = y.L;
        x.L = y.R;
        y.L = n;
        y.R = x;
        UpdateTag(n);
        UpdateTag(x);
        UpdateTag(y);
        return y;
    }

#endregion
}
