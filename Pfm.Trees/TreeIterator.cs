using System;
using System.Diagnostics;

namespace Pfm.Collections.TreeSet;

/// <summary>
/// <para>
/// A light-weight stack of <see cref="TreeNode{TValue}"/> used by iterative tree traversal algorithms.
/// A <c>default</c> instance is unusable (<see cref="IsAllocated"/> is false) and must be allocated with
/// <see cref="Allocate(int)"/>.
/// </para>
/// <para>
/// WARNINGS. This is a mutable struct.  For performance reasons, the provided methods and properties perform
/// no error checking on their own and rely on the runtime throwing <see cref="NullReferenceException"/> or
/// <see cref="IndexOutOfRangeException"/>.
/// </para>
/// </summary>
/// <typeparam name="TValue">Value type held by the tree.</typeparam>
public struct TreeIterator<TValue>
{
    /// <summary>
    /// Default capacity allocated by the stack.  This should be sufficient for any balanced tree of up to 4G elements.
    /// </summary>
    public const int DefaultCapacity = 48;

    /// <summary>
    /// Allocating constructor.
    /// </summary>
    /// <param name="capacity">Maximum capacity supported by this stack.</param>
    public TreeIterator(int capacity) => Allocate(capacity);

    /// <summary>
    /// Copy constructor: copies all fields and the stack array, but not the nodes.  The new
    /// stack has the same capacity as <paramref name="other"/>.
    /// </summary>
    /// <param name="other">
    /// An existing, allocated stack to copy.
    /// </param>
    public TreeIterator(TreeIterator<TValue> other) {
        Depth = other.Depth;
        Path = new TreeNode<TValue>[other.Path.Length];
        Array.Copy(other.Path, Path, Depth);
    }

    /// <summary>
    /// Tree traversal stack; root is always at index 0 (bottom) and the current node at <c>Count-1</c>.
    /// </summary>
    /// <seealso cref="Top"/>
    /// <seealso cref="Depth"/>
    public TreeNode<TValue>[] Path;

    /// <summary>
    /// Number of elements on the stack.
    /// </summary>
    public int Depth;

    /// <summary>
    /// Allocates <see cref="Path"/> and sets <see cref="Depth"/> to 0.
    /// </summary>
    /// <param name="capacity">Maximum capacity supported by this stack.</param>
    public void Allocate(int capacity = DefaultCapacity) {
        Path = new TreeNode<TValue>[capacity];
        Depth = 0;
    }

    /// <summary>
    /// False if <c>this</c> is not usable.
    /// </summary>
    public bool IsAllocated => Path != null;

    /// <summary>
    /// True when there are no elements on the stack.
    /// </summary>
    public bool IsEmpty => Depth == 0;

    /// <summary>
    /// A reference to the top node on the stack which is at index <c>Count - 1</c>.
    /// </summary>
    public ref TreeNode<TValue> Top => ref Path[Depth - 1];

    /// <summary>
    /// Removes all elements from the stack.
    /// (NB! Only resets the stack pointer, the contents of <see cref="Path"/> array is unchanged.)
    /// </summary>
    public void Clear() => Depth = 0;

    /// <summary>
    /// Pushes a node onto the stack.  <paramref name="node"/> must not be null (checked only in debug builds).
    /// </summary>
    /// <param name="node"></param>
    public void Push(TreeNode<TValue> node) {
        Debug.Assert(node != null);
        Path[Depth++] = node;
    }

    /// <summary>
    /// Attempts to pop the top node from the stack.
    /// </summary>
    /// <returns>
    /// The popped node or <c>null</c> if the stack was empty.
    /// </returns>
    public TreeNode<TValue> TryPop() => Depth > 0 ? Path[--Depth] : null;
}
