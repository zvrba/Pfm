# Podaga.PersistentCollections

This library implements some persistent data structures:

- Balanced binary trees (AVL and weight-balanced)
- Vector

with adapters to standard `IReadOnlyList<T>`, `ICollection<T>`, `ISet<T>` and `IDictionary<K, V>`.

Features that distinguish the above from standard, immutable and frozen collections in the BCL:

- Performance almost on-par with standard mutable collections.
- Cheap and explicit copy-on-write (COW) semantics.
- Every adapted collection type based on trees can be re-adapted to any other collection type.
- Consequently, it is possible to access dictionary and set elements by _index_ and perform set-operations (e.g., union) on dictionaries.
- Tree iterators supporting forward and backward iteration are provided as well.
- The tree data structure supports _custom augmentation_, which makes it possible to implement other search structures such
  as interval trees. (Size/index is a built-in kind of augmentation.)

See [here](TODO!) for full documentation.

# License

The documentation (all content in docs branch) is licensed under [CC BY-NC-ND 4.0](https://creativecommons.org/licenses/by-nc-nd/4.0/) license.

Pfm-Docs is a submodule pointing to a private repository where I maintain the documentation.
You do not need to fetch it to build the code.

# References

The joinable tree algorithms implemented by this library are based on the following paper:

Guy Blelloch, Daniel Ferizovic, and Yihan Sun. 2022. Joinable Parallel Balanced Binary Trees. ACM Trans.
Parallel Comput. 9, 2, Article 7 (April 2022), 41 pages. https://doi.org/10.1145/3512769

# Library design

Experiments with abstract statics in interfaces to implement traits-like design as commonly seen in C++.
The technique is applied to model binary search trees with reduced overheads (see benchmarks below).

The solution consists of three assemblies:

- `Pfm.Collections` contains various data structure implementations
- `Pfm.Test` has (somewhat messy) correctness tests
- `Pfm.Benchmark` has benchmarks.

This project has been inspired by "Persistence for the Masses" paper, hence also the name.

# Collections

Collection namespaces are divided by data structure and implementation technique:

- `Trie.DenseTrie` implements persistent vector supporting direct access and one-sided push/pop operations (akin to Clojure vectors)
- `TreeSet` implements persistent joinable balanced trees, in AVL and weight-balanced variants.  In addition
  to usual sorted set operations, it also provides iteratos for forward and backward navigation, fast (logarithmic)
  access to n'th element in sorted order, and user-defined monoidal "augmentations" that can be used to implement,
  for example, an interval tree.

Tests include cases that cover correctness of copy on write semantics.  Benchmarks attempt to cover the cost of
COW semantics.

# Future work

I have for a long time been annoyed by the fact that the standard `Dictionary<K,V>` does not support set
operations (intersection, difference, union), even though it is an `ISet<K>`.  `TreeSet` is a building block
that can be used to implement a `MergeableDictionary<K,V>` that _would_ support set operations with
configurable merging of equivalent values.  Efficient bulk-build from a sorted sequence would also be possible
to implement.

Sparse trie - Bagwell's HAMT.

# Benchmarks

Method:

- Build the solution in Release mode
- Run `Pfm.Benchmark.exe` from a console elevated to admin (needed to collect HW perf counters)

Environment:

BenchmarkDotNet=v0.13.2, OS=Windows 11 (10.0.22621.963)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK=7.0.100
  [Host]     : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2

`SortedSet` is used as a reference implementation.  The underlying implementation is a RB-tree which does less
work for insertions and deletions, but results in deeper trees.

## SequencePatternBenchmark

This benchmark tries to find the "worst" sequence of insertions and deletions for a given tree implementation.
Results (not shown) indicate that random/random is among the top 5 of 49 combinations, so it is used throughout
the other benchmarks.

The following benchmarks use a random sequence of 8197 elements.

## TreeModifyBenchmark
This benchmark inserts a random sequence into the tree, then removes the elements in reverse order of insertion.

|       Method |     Mean |     Error |    StdDev | BranchInstructions/Op | InstructionRetired/Op | CacheMisses/Op |     Gen0 |     Gen1 |  Allocated |
|------------- |---------:|----------:|----------:|----------------------:|----------------------:|---------------:|---------:|---------:|-----------:|
|   AvlTreeSet | 2.806 ms | 0.0203 ms | 0.0190 ms |             5,132,036 |            26,197,461 |          5,660 |  27.3438 |   7.8125 |  384.29 KB |
|    WBTreeSet | 2.389 ms | 0.0068 ms | 0.0060 ms |             4,860,134 |            27,351,823 |          5,278 |  27.3438 |   7.8125 |  384.29 KB |
|    AvlCOWSet | 2.949 ms | 0.0085 ms | 0.0079 ms |             5,216,734 |            26,441,406 |          8,294 |  62.5000 |  39.0625 |  833.37 KB |
|     WBCOWSet | 2.584 ms | 0.0139 ms | 0.0123 ms |             4,965,466 |            27,795,312 |          8,067 |  62.5000 |  31.2500 |  827.98 KB |
|    SortedSet | 2.206 ms | 0.0084 ms | 0.0078 ms |             3,708,473 |            11,000,260 |          4,551 |  23.4375 |   7.8125 |  320.24 KB |
| ImmutableSet | 6.636 ms | 0.0581 ms | 0.0485 ms |            12,854,511 |            54,083,333 |         43,002 | 757.8125 | 460.9375 | 9713.33 KB |

"COW" benchmarks attempt to show the cost of COW semantics where the whole tree is rebuilt twice.  These benchmarks
proceed as follows:

- First, only even numbers from the sequence are inserted into the tree.
- Then, a COW copy of the tree is made and all odd numbers are inserted into the copy.
- Then, another COW copy is made and all elements are removed in reverse order of insertion.

## TreeFindBenchmark

This benchmark inserts a random sequence into the tree, then searches for each inserted element in increasing order (0, 1, ..., max).

|       Method |     Mean |   Error |  StdDev | CacheMisses/Op | BranchInstructions/Op | InstructionRetired/Op |
|------------- |---------:|--------:|--------:|---------------:|----------------------:|----------------------:|
|   AvlTreeSet | 298.3 us | 0.87 us | 0.81 us |             87 |               375,178 |             1,208,659 |
|    WBTreeSet | 300.1 us | 0.75 us | 0.66 us |             79 |               383,311 |             1,229,199 |
|    SortedSet | 474.3 us | 2.20 us | 2.06 us |            128 |             1,050,059 |             2,901,164 |
| ImmutableSet | 465.4 us | 1.92 us | 1.70 us |            103 |             1,169,875 |             2,992,643 |

Join tree has even better lookup performance than standard `SortedSet` and `ImmutableSet`.

## VectorModifyBenchmark

This benchmark creates a vector of 16384 elements and adds 1 to each element.

|        Method |        Mean |     Error |    StdDev | BranchInstructions/Op | InstructionRetired/Op | CacheMisses/Op |
|-------------- |------------:|----------:|----------:|----------------------:|----------------------:|---------------:|
|          List |    13.12 us |  0.028 us |  0.027 us |                49,407 |               280,485 |              2 |
| ImmutableList | 5,138.95 us | 20.923 us | 19.571 us |             9,529,344 |            43,057,812 |         57,847 |
|     DenseTrie |   392.71 us |  0.823 us |  0.729 us |             1,332,262 |             6,028,385 |             54 |

As expected, the built-in mutable list has the best performance.  Still, COW `DenseTrie` has significantly better performance
than the built-in immutable list.

## Remarks

I have attempted to convert recursive tree algorithms to iterative algorithms, using `TreeIterator` as a manually
maintained stack.  Surprisingly, the result was _slower_ due to frequent calls to `CORINFO_HELP_ASSIGN_REF`, which
doesn't happen when the reference is pushed onto the stack during recursion.
See https://github.com/dotnet/runtime/issues/59031  This is also the reason for using `ulong` for the node's transient
tag instead of `object` (as is suggested in other papers).
