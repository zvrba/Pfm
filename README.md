# Pfm - Persistence for the masses

Immutable collections are "all the rage" these days, for good reasons.  Their story in .NET, however,
is very fragmented:

- `IReadOnlyCollection`, `ReadOnlyCollection` wrappers (not thread-safe)
- `System.Collections.Immutable` (thread-safe, but slow and memory-hungry)
- and, int .NET8, "frozen collections", with clumsy semantics (https://devblogs.microsoft.com/premier-developer/immutable-collections-with-mutable-performance/)

However, there are no mutable collections with _cheap_ "copy on write" semantics.

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

  Tests include cases that cover correctness of copy on write semantics.  However, I was not able to design a
  benchmark that could meaningfully compare the cost of a COW mutable collection with "always copy" semantics
  of an immutable collection.

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

## ImplementationBenchmark
This benchmark inserts a random sequence into the tree, then removes the elements in reverse order of insertion.

|               Method |     Mean |     Error |    StdDev | InstructionRetired/Op | CacheMisses/Op |     Gen0 |     Gen1 |  Allocated |
|--------------------- |---------:|----------:|----------:|----------------------:|---------------:|---------:|---------:|-----------:|
|     ReferenceAvlTree | 3.326 ms | 0.0234 ms | 0.0219 ms |            28,540,104 |          6,189 |  23.4375 |   7.8125 |  320.66 KB |
|   MutableJoinAvlTree | 2.567 ms | 0.0060 ms | 0.0056 ms |            17,372,917 |          6,849 |  27.3438 |   7.8125 |  384.24 KB |
| ImmutableJoinAvlTree | 4.204 ms | 0.0661 ms | 0.0552 ms |            32,066,667 |         45,107 | 757.8125 | 460.9375 | 9712.08 KB |
|    MutableJoinWBTree | 2.183 ms | 0.0131 ms | 0.0123 ms |            19,393,490 |          6,487 |  27.3438 |   7.8125 |  384.24 KB |
|  ImmutableJoinWBTree | 3.703 ms | 0.0422 ms | 0.0374 ms |            33,895,573 |         49,957 | 746.0938 | 484.3750 |  9550.6 KB |
|            SortedSet | 2.008 ms | 0.0143 ms | 0.0133 ms |            10,904,167 |          5,609 |  23.4375 |   7.8125 |  320.24 KB |
|         ImmutableSet | 5.963 ms | 0.0757 ms | 0.0632 ms |            53,734,375 |         77,965 | 757.8125 | 460.9375 | 9713.33 KB |

## FindBenchmark

This benchmark inserts a random sequence into the tree, then searches for each inserted element in increasing order (0, 1, ..., max).

|               Method |     Mean |   Error |  StdDev | InstructionRetired/Op | CacheMisses/Op |
|--------------------- |---------:|--------:|--------:|----------------------:|---------------:|
|            SortedSet | 430.0 us | 2.73 us | 2.55 us |             2,872,241 |            445 |
|        ReferenceTree | 359.8 us | 1.47 us | 1.30 us |             2,236,198 |            301 |
|   MutableJoinAvlTree | 286.5 us | 2.75 us | 2.57 us |             1,195,705 |            338 |
| ImmutableJoinAvlTree | 263.9 us | 1.56 us | 1.38 us |             1,195,768 |            238 |
|    MutableJoinWBTree | 292.2 us | 5.58 us | 5.48 us |             1,215,208 |            437 |
|  ImmutableJoinWBTree | 281.9 us | 1.69 us | 1.41 us |             1,214,160 |            534 |
|         ImmutableSet | 414.1 us | 2.91 us | 2.72 us |             2,962,923 |            335 |

Join tree has even better lookup performance than standard `SortedSet` and `ImmutableSet`.

# Random implementation remarks

I have attempted to convert recursive tree algorithms to iterative algorithms, using `TreeIterator` as a manually
maintained stack.  Surprisingly, the result was _slower_ due to frequent calls to `CORINFO_HELP_ASSIGN_REF`, which
doesn't happen when the reference is pushed onto the stack during recursion.
See https://github.com/dotnet/runtime/issues/59031  This is also the reason for using `ulong` for the node's transient
tag instead of `object` (as is suggested in the papers).
