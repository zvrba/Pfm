# Pfm - Persistence for the masses

Immutable collections are "all the rage" these days, for good reasons.  Their story in .NET, however,
is very fragmented:

- `IReadOnlyCollection`, `ReadOnlyCollection` wrappers (not thread-safe)
- `System.Collections.Immutable` (thread-safe, but slow and memory-hungry)
- and, int .NET8, "frozen collections", with clumsy semantics (https://devblogs.microsoft.com/premier-developer/immutable-collections-with-mutable-performance/)

However, there are no mutable collections with _cheap_ "copy on write" semantics.

# Library design

Experiments with abstract statics in interfaces to implement traits-like design as commonly seen in C++.
The technique is applied to model intrusive binary search trees with reduced overheads.

The solution consists of three assemblies:

- `Pfm.Collections` contains various data structure implementations
- `Pfm.Test` has (somewhat messy) correctness tests
- `Pfm.Benchmark` has benchmarks.

This project has been inspired by "Persistence for the Masses" paper, hence also the name.

Converting recursion to iteration, by manually maintaining a stack in an araray, made algorithms
_slower_ due to frequent calls to `CORINFO_HELP_ASSIGN_REF`, which doesn't happen when the reference
is pushed onto the stack during recursion.  See https://github.com/dotnet/runtime/issues/59031
This is also the reason for using `ulong` for the node's transient tag instead of `object`.

# Collections

Collection namespaces are divided by data structure and implementation technique:

- `Trie` implements an immutable vector supporting transience, direct access and one-sided push/pop
  operations (akin to Clojure vectors)
- `ReferenceTree` takes a "traditional" OO approach to modeling tree nodes and balancing rules.
- `JoinTree` uses the new "abstract statics in interfaces" feature to model a traits-based design with
  "static polymorphism" as commonly seen in C++ code.
- `IntrusiveTree` is another attempt at using "abstract statics", but to enable implementation of
  _intrusive_ tree nodes where the tree data and metadata is stored in an arbitrary, existing object.
- `CompactTree` uses a "memory cell" abstraction making it possible to use small integers for node links,
  with nodes being stored in an indexable structure.  Such implementation is favorable for maintaining
  cache locality.  The idea (not followed through) was to also use the above trie to simulate immutable
  memory and thus get an immutable tree implementation "for free".  A fully-fledged implementation would
  also need a garbage collector.

All variants implement at least AVL tree.  A generic, non-recursive forward and backward iteration
is implemented by each variant as well.

All variants, _except_ `JoinTree`, also have disappointing performance and are thus more-or-less abandoned.
`IntrusiveTree` can perhaps be fixed by using structs instead of classes (as `JoinTree` does) to force the
JIT to generate fresh code for every instantiation, thus enabling devirtualization and inlining.  That would
require rewriting the code in a cumbersome manner, e.g., `TNodeTraits.L(node)` instead of `node.L`.

`ReferenceTree` implements in addition Zip tree.  I see no reason to use them: they get much deeper than AVL
trees and some deletion patterns can cause them to get very degenerate (i.e., deep): tests fail because the
iterator's preallocated stack space overflows.  The pattern that causes most trouble is repeated deletion
of the middle (root) element.  My implementation is perhaps buggy, but the tree passes the structural test
after repeated insertions and deletions.

In addition, `PermutationGenerators` class is provided to generated insertion/deletion sequences according
to different patterns.  The patterns are inspired by Ben Pfaff's `libavl`.

# Future work

I have for a long time been annoyed by the fact that the standard `Dictionary<K,V>` does not support set
operations (intersection, difference, union), even though it is an `ISet<K>`.  The end goal of this project
is to implement a `MergeableDictionary<K,V>` with optional immutability that _does_ support set operations.
It will in addition support merging of values through `Func<K, V, V, V>`.  Efficient bulk-build from a
sorted sequence would also be possible to implement.

Another goal was to experiment with user-extensible "augmentations" (called "tags" in the code) on tree
nodes to support applications other than dictionaries.  This idea is supported by `IntrusiveTree` and might
be pursued further.

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

# Old benchmarks (removed from the code)

NB! These use smaller sequences than the above benchmarks.

IntrusiveTree performs search using Pfm.Collections.IntrusiveTree.AbstractTree<>.Find method which uses
INodeTraits interface to acces node links and compare values.  DelegateIntrusiveTree simulates abstract
statics by packing a number of delegates into a readonly struct.  These delegates are in turn used by
Pfm.Collections.IntrusiveTree.DelegateFinder<>.Find for accessing the tree nodes.

As a final comparison, ReferenceTree.AbstractTree<> uses "direct" implementation with no indirections
or customization opportunities (see AbstractTree.* files).

Tables below show benchmark results; the relevant lines to look at are IntrusiveTree and DelegateIntrusiveTree.
Superficial conclusions:
- The JIT seems to fail to inline instance property access through a type/interface combination statically known at compile-time
- The JIT seems to fail to inline invocation of static abstract methods
- The JIT seems to have more success with inlining calls through a delegate than calls through an interface
Compare "BranchInstructions" column across ReferenceTree, IntrusiveTree and DelegateIntrusiveTree.
In addition, more instructions in total are executed (I guess it's attributable to method prologue/epilogue
overhead when the method fails to inline.)

These results are somewhat disappointing (esp. that invocation of delegates is faster than invocation of statically
known inteface methods) given that abstract statics were designed with arithmetic in mind.

WITHOUT TieredPGO, direct call:
|                Method |     Mean |   Error |  StdDev | BranchInstructions/Op | BranchMispredictions/Op | InstructionRetired/Op | CacheMisses/Op |
|---------------------- |---------:|--------:|--------:|----------------------:|------------------------:|----------------------:|---------------:|
|             SortedSet | 508.6 us | 2.73 us | 2.56 us |             1,052,486 |                  24,060 |             2,913,737 |            381 |
|         ReferenceTree | 426.0 us | 2.87 us | 2.68 us |               705,308 |                  22,182 |             2,266,081 |            297 |
|          ImmutableSet | 507.5 us | 3.83 us | 3.58 us |             1,172,629 |                  24,285 |             3,006,250 |            459 |
|         IntrusiveTree | 885.9 us | 4.69 us | 4.38 us |             2,309,719 |                  28,787 |             6,406,782 |            573 |
| DelegateIntrusiveTree | 764.3 us | 2.42 us | 2.02 us |             1,527,185 |                  25,438 |             4,082,812 |            474 |

WITHOUT TieredPGO, ICollection<> interface call: (removed from the code)
|                Method |     Mean |   Error |  StdDev | BranchInstructions/Op | BranchMispredictions/Op | InstructionRetired/Op | CacheMisses/Op |
|---------------------- |---------:|--------:|--------:|----------------------:|------------------------:|----------------------:|---------------:|
|             SortedSet | 511.2 us | 2.92 us | 2.59 us |             1,068,710 |                  23,936 |             2,945,768 |            368 |
|         ReferenceTree | 434.0 us | 1.79 us | 1.59 us |               738,313 |                  23,067 |             2,366,960 |            306 |
|          ImmutableSet | 525.1 us | 1.80 us | 1.60 us |             1,226,891 |                  25,314 |             3,143,880 |            277 |
|         IntrusiveTree | 878.3 us | 5.49 us | 5.14 us |             2,309,679 |                  28,587 |             6,405,729 |            564 |
| DelegateIntrusiveTree | 767.6 us | 3.93 us | 3.67 us |             1,527,059 |                  25,466 |             4,082,775 |            499 |

WITH TieredPGO:
|                Method |     Mean |   Error |  StdDev | BranchInstructions/Op | InstructionRetired/Op | BranchMispredictions/Op | CacheMisses/Op |
|---------------------- |---------:|--------:|--------:|----------------------:|----------------------:|------------------------:|---------------:|
|             SortedSet | 524.1 us | 1.38 us | 1.15 us |             1,012,037 |             2,440,310 |                  24,808 |            192 |
|         ReferenceTree | 340.0 us | 1.21 us | 1.07 us |               462,043 |             1,668,107 |                  25,546 |            134 |
|          ImmutableSet | 408.9 us | 1.47 us | 1.37 us |               954,425 |             3,734,017 |                  21,800 |            154 |
|         IntrusiveTree | 673.0 us | 2.81 us | 2.63 us |             1,466,778 |             4,090,771 |                  24,825 |            209 |
| DelegateIntrusiveTree | 567.5 us | 2.10 us | 1.64 us |             1,230,297 |             3,762,207 |                  24,976 |            275 |

Final comment: ReferenceTree is faster on searches than System.Collections.Generic.SortedSet<> because the former
is an AVL tree, which is on average shallower than RB-tree used by the latter.  Insertion/deletion performance of
ReferenceTree is somewhat slower (benchmarks not included here).


