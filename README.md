# BPlusTreeCollection

[![CI](https://github.com/psangrama/BPlusTreeCollection/actions/workflows/ci.yml/badge.svg)](https://github.com/psangrama/BPlusTreeCollection/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

`BPlusTreeCollection` is a sorted, in-memory `IDictionary<TKey, TValue>` implementation
for .NET backed by a **B+ tree**. It is built for workloads that load a large number of
keys at runtime and then query them repeatedly — including **range queries**, where the
ordered leaf level lets you walk a contiguous slice of keys instead of doing one lookup
per hit.

Against .NET's built-in sorted collections at 450,000 keys, it loads **104× faster than
`SortedList`** with the same memory, and beats `SortedDictionary` on load, memory, exact
lookup and range search alike. [See the numbers](#why-this-exists).

## Why this exists

Each of the built-in sorted collections is excellent at something and unusable at
something else. `SortedList` gives you the fastest queries but takes almost a minute
to load 450,000 keys. `SortedDictionary` loads quickly but collapses on range
queries and costs the most memory.

`BPlusTreeDictionary` is the only one of the three that is at or near the top on
every axis at once — measured, not asserted:

| 450,000 `double` keys | `SortedList` | `SortedDictionary` | **`BPlusTreeDictionary`** |
| --- | --- | --- | --- |
| Load | 52.56 s | 0.79 s | **0.51 s** |
| Memory | **38.50 MB** | 54.21 MB | 38.94 MB |
| Exact find × 100,000 | **0.030 s** | 0.106 s | 0.040 s |
| Range search × 500 | **0.072 s** | 57.52 s | 0.078 s |
| Slowest result | 52.56 s | 57.52 s | **0.51 s** |

Read the last row first. `SortedList` and `SortedDictionary` each have a case that
is roughly a thousand times worse than their best; `BPlusTreeDictionary` does not.
It loads **104× faster than `SortedList`** while matching it on memory and staying
within ~10% of it on both query paths, and it beats `SortedDictionary` on every
single measure — including a range search that is **735× faster**.

If your workload is *only* repeated lookups against a table you can afford to build
once and slowly, `SortedList` is still marginally faster and you should use it. For
anything that has to be built at runtime and then queried, this is the better trade.

Full methodology and how to reproduce these numbers is in [Benchmarks](#benchmarks).

### Why a B+ tree gets this shape

A B+ tree keeps every value at the leaf level and keeps the leaves in key order, so:

- **Inserts stay local.** A key lands inside one leaf's array, and only a full leaf
  splits — no shifting of a half-million-element backing array, as `SortedList` does
  on every insert.
- **Range queries become a walk.** Find the starting leaf, then read forward through
  a sorted, indexable list. `SortedDictionary` has no indexed access at all, so the
  only option is a full scan per query — which is exactly what the 57-second result
  above measures.
- **Nodes are wide.** With 1024 slots per node, 450,000 keys fit in a tree two levels
  deep, so a lookup is a couple of binary searches rather than ~19 pointer hops
  through a red-black tree.

# What is B+ Tree?
A B+ tree is an advanced form of a self-balancing tree in which all the values are present in the leaf level.

# Properties of a B+ Tree
1. All data will be in the leaf nodes and will have duplicated data in internal nodes. Internal nodes can only store the key values.
2. All nodes will have node size, it depends on configuration (here I have taken, 1024 values in a node)
3. This also called as max degree. Node will have a max of node size - 1 values underneath it.
4. The leaf nodes of a B+ tree are linked together in the form of a singly linked lists to make the search queries more efficient.

# How it works?

![image](https://github.com/psangrama/BPlusTreeCollection/assets/113549457/f0ad8ec8-b117-4f94-8f88-0421d0ae81c7)

Better visualization for understanding, refer https://www.cs.usfca.edu/~galles/visualization/BPlusTree.html

## Installation

```bash
dotnet add package SP.BPlusTreeCollection
```

Target framework: **.NET 10.0**.

## Quick start

```csharp
using SP.BPlusTreeCollection.BPlusTree;

var tree = new BPlusTreeDictionary<double, string>();

tree.Add(39.2373600900, "alpha");
tree.Add(12.5000000000, "beta");
tree[72.0000000000] = "gamma";     // indexer set is an Add

if (tree.TryGetValue(12.5, out var value))
    Console.WriteLine(value);      // beta

Console.WriteLine(tree.First());   // 12.5   — smallest key
Console.WriteLine(tree.Last());    // 72.0   — largest key
Console.WriteLine(tree.Count);     // 3
```

> **Comparers:** the constructor accepts an `IComparer<TKey>`, but the current
> implementation ignores it and always uses `Comparer<TKey>.Default`
> ([`BPlusTreeDictionary.cs:15`](src/SP.BPlusTreeCollection/BPlusTree/BPlusTreeDictionary.cs#L15)).
> Until that is fixed, only key types with a sensible default ordering are supported.

### Storing duplicate keys

The dictionary holds one value per key. To model duplicates, make the value a list — this is also the shape the range-search helper expects:

```csharp
var tree = new BPlusTreeDictionary<double, List<int>>();

void Insert(double key, int id)
{
    if (tree.TryGetValue(key, out var bucket) && bucket != null)
        bucket.Add(id);
    else
        tree.Add(key, new List<int> { id });
}
```

## Range search

The leaf level is exposed as an ordered, indexable list, which is what makes range queries cheap. Binary-search the leaves for the boundary keys, then read straight through:

```csharp
using SP.BPlusTreeCollection.Nodes;

IList<INode<double, List<int>>> leaves = tree.LeafNodes;

// leaves[i].Keys is sorted; leaves[i].Keys[0] is the leaf's first key,
// and leaf i's keys all sort before leaf i+1's.
var leaf = (Leaf<double, List<int>>)leaves[0];
var keys   = leaf.Keys;     // List<double>
var values = leaf.Values;   // List<List<int>>, index-aligned with Keys
```

A complete, working implementation of this pattern — locating the start and end leaf, then the start and end index within each — lives in [`BPlusTreeIndexedRangeSearch.cs`](src/SP.BPlusTreeCollection.Test/Common/BPlusTreeIndexedRangeSearch.cs) in the test project. It is a good starting point to copy into your own code.

## API surface

| Member | Notes |
| --- | --- |
| `Add(TKey, TValue)` / `Add(KeyValuePair<,>)` | Inserts into the correct leaf, splitting when a node fills. |
| `this[TKey]` | Getter returns `default` when the key is absent; setter is an `Add`. |
| `TryGetValue(TKey, out TValue)` | Preferred lookup — distinguishes "missing" from "default value". |
| `ContainsKey(TKey)` | Implemented via `TryGetValue`. |
| `Remove(TKey)` | Removes and rebalances. Note the public overload returns `void`; the `IDictionary` explicit implementation returns `bool`. |
| `Remove(KeyValuePair<,>)` | Removes by the pair's key. |
| `Clear()` | Drops the tree and the leaf index. |
| `Count` | Sums the leaf counts on each call — O(number of leaves), not O(1). Cache it in hot loops. |
| `First()` / `Last()` | Smallest / largest key. |
| `LeafNodes` | The ordered leaf list — the entry point for range queries. |
| `Nodes` | Enumerates the nodes of the tree. |
| `Verify()` | Structural self-check, intended for tests and debugging. |
| `GetEnumerator()` | Walks the whole tree recursively. **Slow** — prefer `LeafNodes` for bulk reads. |

### Known limitations

These are deliberate and worth knowing before you adopt it:

- **`Keys` and `Values` throw `NotImplementedException`.** Materialising either would copy the whole collection; iterate `LeafNodes` instead.
- **Enumeration is slow.** `foreach (var kv in tree)` recurses the tree per item. Use `LeafNodes` for scans.
- **`Count` is recomputed** on every access (see table above).
- **Not thread-safe.** Guard concurrent access yourself; the intended pattern is bulk load, then many concurrent reads with no writers.
- **Custom comparers are ignored** (see the note under Quick start).
- **Node size is fixed at compile time** — `Constants.NodeSize` is `1024`. Changing it requires rebuilding the library.

## Benchmarks

All numbers on this page come from the suite in
[`BPlusTreePerformanceComparisonTests.cs`](src/SP.BPlusTreeCollection.Test/BPlusTree/BPlusTreePerformanceComparisonTests.cs),
run in `Release` on .NET 10. **They are from one developer machine and are not a
substitute for measuring your own workload** — reproduce them with:

```bash
cd src
dotnet test -c Release --filter "FullyQualifiedName~BPlusTreePerformanceComparisonTests"
```

### Method

450,000 `double` keys — including 4,000 duplicates and 2,000 nulls, one key repeated
877 times — are loaded into all three collections, each keyed to a `List<int>` bucket
so duplicates are retained. Then:

| Measurement | What it does |
| --- | --- |
| Load | Inserts all 450,000 records into a freshly created collection. |
| Memory | `GC.GetTotalMemory(true)` before and after the load, with the collection held alive across the sample. |
| Exact find | 100,000 `TryGetValue` calls for keys drawn from a shuffled copy of the input. |
| Exact find (repeated) | 100,000 `TryGetValue` calls for one fixed key, measuring warm-cache lookup. |
| Range search | 500 queries for a ±0.05 window around a randomly chosen key. |

### Results

| Measurement | `SortedList` | `SortedDictionary` | **`BPlusTreeDictionary`** |
| --- | --- | --- | --- |
| Load (450k records) | 52.56 s | 0.79 s | **0.51 s** |
| Memory after load | **38.50 MB** | 54.21 MB | 38.94 MB |
| Exact find × 100,000 (shuffled) | **0.030 s** | 0.106 s | 0.040 s |
| Exact find × 100,000 (same key) | **0.012 s** | 0.012 s | 0.015 s |
| Range search × 500 | **0.072 s** | 57.52 s | 0.078 s |

### Reading these honestly

- **Load is the decisive win.** 52.56 s → 0.51 s against `SortedList` is the reason
  this library exists. `SortedList` inserts into a sorted array, so each of the
  450,000 inserts shifts elements — quadratic behaviour that a B+ tree avoids.
- **Range search is a tie with `SortedList`, not a win.** 0.078 s vs 0.072 s is
  within noise of each other. The honest claim is that `BPlusTreeDictionary` gives
  you `SortedList`-class range performance *without* `SortedList`'s load cost — not
  that it is faster at range search in isolation.
- **Exact find: `SortedList` wins narrowly, `SortedDictionary` loses badly.** A
  binary search over one flat array beats two levels of node search, but only by
  ~10 ms across 100,000 lookups.
- **The `SortedDictionary` range number is not a rigged comparison.**
  `SortedDictionary` exposes no indexed access to its ordered contents, so a LINQ
  scan is the realistic option a caller has. The `SortedList` and `BPlusTree` range
  searches both use hand-written binary-search indexing, which is the realistic
  option *those* types offer.
- **The range search is caller-side code, not library code.** The B+ tree makes it
  cheap by exposing ordered leaves, but you supply the search — see
  [Range search](#range-search) above.

### Not yet measured

Removal is not benchmarked. The three `RemovePerformance_*` methods exist but are
disabled, because they mutate the shared fixture collections and would make the
suite order-dependent. Treat removal performance as unmeasured.

## Building from source

```bash
git clone https://github.com/psangrama/BPlusTreeCollection.git
cd BPlusTreeCollection/src
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

Requires the [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later. To build the NuGet package locally:

```bash
dotnet pack SP.BPlusTreeCollection/SP.BPlusTreeCollection.csproj -c Release -o ../artifacts
```

### Tests and coverage

The suite is MSTest, and covers both correctness and the benchmarks above:

```bash
cd src
dotnet test -c Release --collect:"XPlat Code Coverage"
```

34 tests, currently **93.5% line / 86.0% branch** coverage of the library.

Because a node holds 1024 children, a tree only grows past two levels at roughly 1.5M
keys — so the internal-node split, merge and borrow paths are only reachable at that
scale. `MultiLevelTree_LoadRemoveAndVerify_KeepsTreeConsistent` and
`MultiLevelTree_LeafNodesRemainInKeyOrder` build trees that large deliberately, and the
benchmark suite loads 450,000 records per collection. A full run therefore takes around
16 minutes.

Those tests carry `[TestCategory("Long")]`, so day-to-day you can skip them:

```bash
dotnet test -c Release --filter "TestCategory!=Long"   # a few seconds
```

CI does the same: [`ci.yml`](.github/workflows/ci.yml) runs the fast tests on every push
and pull request, and runs the full suite nightly and on demand. Note that the fast
subset alone covers 78.7% line / 70.6% branch — the 93.5% figure requires the long tests,
so the nightly run is the one to trust for coverage.

## Repository layout

```
src/
  SP.BPlusTreeCollection/          # the library
    BPlusTree/                     # BPlusTreeDictionary, split across partial classes
    Nodes/                         # INode, Leaf, Internal, Constants
  SP.BPlusTreeCollection.Test/     # MSTest functional + performance suites
```

## Contributing

Issues and pull requests are welcome. Please make sure `dotnet test -c Release` passes and add a test alongside any behaviour change.

## License

[MIT](LICENSE) © Sangrama Pattanayak
