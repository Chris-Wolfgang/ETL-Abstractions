# Wolfgang.Etl.TestKit

An Extractor, Transformer and Loader designed to be used in testing libraries built with Wolfgang.Etl.Abstractions

[![NuGet](https://img.shields.io/nuget/v/Wolfgang.Etl.TestKit.svg?logo=nuget&label=NuGet)](https://www.nuget.org/packages/Wolfgang.Etl.TestKit/)
[![Downloads](https://img.shields.io/nuget/dt/Wolfgang.Etl.TestKit.svg?logo=nuget&label=downloads)](https://www.nuget.org/packages/Wolfgang.Etl.TestKit/)
[![PR build](https://img.shields.io/github/actions/workflow/status/Chris-Wolfgang/ETL-Abstractions/pr.yaml?event=pull_request_target&label=PR%20build&logo=github)](https://github.com/Chris-Wolfgang/ETL-Abstractions/actions/workflows/pr.yaml)
[![release](https://img.shields.io/github/actions/workflow/status/Chris-Wolfgang/ETL-Abstractions/release.yaml?event=release&label=release&logo=github)](https://github.com/Chris-Wolfgang/ETL-Abstractions/actions/workflows/release.yaml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Multi--Targeted-purple.svg)](https://dotnet.microsoft.com/)
[![GitHub](https://img.shields.io/badge/GitHub-Repository-181717?logo=github)](https://github.com/Chris-Wolfgang/ETL-Abstractions)
[![OpenSSF Scorecard](https://api.scorecard.dev/projects/github.com/Chris-Wolfgang/ETL-Abstractions/badge)](https://scorecard.dev/viewer/?uri=github.com/Chris-Wolfgang/ETL-Abstractions)

---

## 📦 Installation

This repo ships two NuGet packages.

**Core** — test doubles for building ETL test fixtures:

```bash
dotnet add package Wolfgang.Etl.TestKit
```

**xUnit add-on** — abstract xUnit contract-test base classes for verifying custom extractors, transformers, and loaders built on `Wolfgang.Etl.Abstractions`:

```bash
dotnet add package Wolfgang.Etl.TestKit.Xunit
```

**NuGet packages:**

- [Wolfgang.Etl.TestKit](https://www.nuget.org/packages/Wolfgang.Etl.TestKit) — test doubles
- [Wolfgang.Etl.TestKit.Xunit](https://www.nuget.org/packages/Wolfgang.Etl.TestKit.Xunit) — xUnit contract tests

Install `Wolfgang.Etl.TestKit.Xunit` whenever you author a custom `ExtractorBase` / `LoaderBase` / `TransformerBase` and want your test project to inherit the canonical xUnit contract coverage with zero boilerplate:

```csharp
public sealed class MyExtractorTests : ExtractorBaseContractTests<MyExtractor, MyItem, MyProgress>
{
    protected override MyExtractor CreateSut(int itemCount) => new MyExtractor(itemCount);
    protected override IReadOnlyList<MyItem> CreateExpectedItems() => ...;
    protected override MyExtractor CreateSutWithTimer(IProgressTimer timer) => ...;
}
```
---

## 📄 License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

---

## 📚 Documentation

- **GitHub Repository:** [https://github.com/Chris-Wolfgang/ETL-Abstractions](https://github.com/Chris-Wolfgang/ETL-Abstractions)
- **API Documentation:** https://Chris-Wolfgang.github.io/ETL-Abstractions/
- **Formatting Guide:** [docs/README-FORMATTING.md](docs/README-FORMATTING.md)
- **Contributing Guide:** [CONTRIBUTING.md](CONTRIBUTING.md)

---

## 🚀 Quick Start

The kit ships two packages that solve different problems. Use the **core** test doubles to drive a pipeline in a test or benchmark; use the **xUnit add-on** to verify your own `ExtractorBase` / `LoaderBase` / `TransformerBase` implementations against the full behavioural contract.

### Core — driving a pipeline with the test doubles

`TestExtractor<T>`, `TestTransformer<T>`, and `TestLoader<T>` let you wire up an extract → transform → load pipeline entirely in memory. All three are generic over the item type, which must be `notnull`.

```csharp
using Wolfgang.Etl.TestKit;

var source = new[] { "alpha", "bravo", "charlie" };

var extractor   = new TestExtractor<string>(source);
var transformer = new TestTransformer<string>();          // pass-through, returns each item unchanged
var loader      = new TestLoader<string>(collectItems: true);

await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync()));

IReadOnlyList<string>? loaded = loader.GetCollectedItems();
// loaded => [ "alpha", "bravo", "charlie" ]
```

`TestExtractor<T>` also accepts an `IEnumerator<T>` so you can stream large, generated sequences without materializing them:

```csharp
static IEnumerator<int> Generate(int count)
{
    for (var i = 0; i < count; i++)
        yield return i;
}

var extractor = new TestExtractor<int>(Generate(1_000_000));
```

Construct `TestLoader<T>` with `collectItems: false` to enumerate the full pipeline (for realistic benchmark throughput) without storing items — `GetCollectedItems()` returns `null` in that mode.

### Core — generating data with `TestExtractor<T>` factory constructors

Instead of materializing a collection, build a `TestExtractor<T>` from a factory delegate. Pass a `Func<T>` (or a `Func<int, T>` that receives the zero-based index) plus an optional item count:

```csharp
using Wolfgang.Etl.TestKit;

// Func<int, T> — receives the index; here capped at 1,000 items
var indexed = new TestExtractor<string>(i => $"row-{i}", count: 1_000);

// Func<T> — same value each call; capped at 5 items
var constant = new TestExtractor<string>(() => "ping", count: 5);
```

### Core — injecting failures with the `Faulty*` doubles

`FaultyExtractor<T>`, `FaultyLoader<T>`, and `FaultyTransformer<T>` let you exercise error and retry paths. The `ThrowAt`, `ThrowAfterCompletion`, and `DuplicateAt` methods are fluent and chainable:

```csharp
using System;
using Wolfgang.Etl.TestKit;

var source = new[] { "alpha", "bravo", "charlie", "delta" };

var extractor = new FaultyExtractor<string>(source)
    .ThrowAt(2, new InvalidOperationException("boom on the third item"));

// Or fail only after all items are produced:
var afterAll = new FaultyExtractor<string>(source)
    .ThrowAfterCompletion(new TimeoutException());

// Or emit a duplicate of the item at a given index (to test de-duplication):
var loader = new FaultyLoader<string>(collectItems: true)
    .DuplicateAt(1);
```

### Core — skipping bad items with the error hook

Built on the `Wolfgang.Etl.Abstractions` 0.18 per-item error hook, the `Faulty*` doubles can route an injected fault through the base `HandleItemError` policy instead of only failing fast. Call `SkipErrors()` (or `HandleErrorsWith(policy)` for a per-item decision) so the bad item is discarded and counted as an error (`CurrentErrorItemCount`) while the run continues; `CapturedErrors` records each `ItemErrorContext`:

```csharp
using System;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit;

var source = new[] { "alpha", "bravo", "charlie", "delta" };

// Skip every failed item and keep going:
var extractor = new FaultyExtractor<string>(source)
    .ThrowAt(1, new FormatException("bad row"))
    .SkipErrors();

// Or decide per item — skip parse errors, abort on anything else:
var picky = new FaultyExtractor<string>(source)
    .ThrowAt(1, new FormatException("bad row"))
    .HandleErrorsWith(ctx => ctx.Exception is FormatException
        ? ItemErrorAction.Skip
        : ItemErrorAction.Abort);

// After the run: extractor.CurrentErrorItemCount == 1, and extractor.CapturedErrors
// holds the ItemErrorContext for the discarded item.
```

### Core — snapshot / approval testing with `SnapshotTestLoader<T>`

`SnapshotTestLoader<T>` captures every item your pipeline loads and renders them as a single, deterministic, diff-friendly `Snapshot` string — ready to hand to an approval / snapshot framework such as [Verify](https://github.com/VerifyTests/Verify). Instead of writing per-field assertions for every record, you lock in the whole output and let the framework flag any drift.

It is deliberately **capture-only**: no file I/O, and **no dependency on any snapshot framework**, so referencing `Wolfgang.Etl.TestKit` never pulls one in. The framework (in your own snapshot test project) owns the golden `.verified.txt` file, the diff, and the approval workflow; the loader only produces the content to lock in.

```csharp
using Wolfgang.Etl.TestKit;
using VerifyXunit;      // in your snapshot test project only

// Project only the fields under test and scrub non-deterministic values
// (timestamps, GUIDs, auto-increment IDs) so the snapshot stays stable:
var loader = new SnapshotTestLoader<OrderRecord>(o => $"<id>|{o.Customer}|{o.Total:0.00}");

await loader.LoadAsync(pipeline.ExtractAsync());

await Verify(loader.Snapshot);   // Verify owns the .verified.txt golden file + diff
```

`Snapshot` is one formatted line per item joined by `\n` (a fixed line feed, not `Environment.NewLine`, so snapshots are stable across operating systems). The default constructor formats each item with its `ToString()` — diff-friendly for `record` types — and `LoadedItems` exposes the raw captured items. `SkipItemCount` / `MaximumItemCount` bound what is captured; each `LoadAsync` clears the buffer first.

**The fleet convention** (see ETL-FixedWidth, ETL-DbClient, ETL-Json): put snapshot tests in a **dedicated `*.Tests.Snapshot` project targeting a single modern TFM** (e.g. `net10.0` — Verify needs net6+ and the output is TFM-agnostic, which keeps snapshot filenames stable), reference `Verify.Xunit`, commit the `.verified.txt` golden files under `Snapshots/`, and gitignore the `.received.txt` files written during local iteration.

### Core — deterministic `Report` timing with `ManualTimeSource`

A stage's `Report` timing metrics — `Elapsed`, `ItemsPerSecond`, `PercentComplete`, `EstimatedRemaining` (Abstractions 0.14) — are normally driven by wall-clock time, so they can't be asserted on exactly. `ManualTimeSource` freezes time until you `Advance` it, making them deterministic. Attach it with `WithTimeSource(...)` **before** the run (the stage captures its start timestamp when the run begins), then advance by a known amount:

```csharp
using System;
using System.Linq;
using Wolfgang.Etl.TestKit;

var clock = new ManualTimeSource();
var extractor = new TestExtractor<int>(Enumerable.Range(0, 50).ToArray()).WithTimeSource(clock);

await extractor.ExtractAsync().ToListAsync();   // start timestamp captured from the frozen clock
clock.Advance(TimeSpan.FromSeconds(10));

// A report built now has Elapsed == 10s exactly and ItemsPerSecond == 5.
```

`WithTimeSource` has extractor, loader, and transformer overloads. It works because `Wolfgang.Etl.TestKit` is an internals-visible friend of `Wolfgang.Etl.Abstractions`, so it can supply the internal clock seam the base classes read — no change to your production code.

### Core — asserting on middleware with `RecordingMiddleware<T>`

`RecordingMiddleware<T>` is a test `IItemMiddleware<T>` (Abstractions 0.20) that records every item it is handed and, by default, keeps each one flowing — so you can assert exactly what reached a stage of the pipeline. Supply a policy to transform or drop items:

```csharp
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit;

// Record and pass through:
var recorder = new RecordingMiddleware<int>();
var kept = await source.WithMiddleware(recorder).ToListAsync();
// recorder.Observed lists every item seen; kept == the ones it let through.

// Drop odds, double evens:
var shaping = new RecordingMiddleware<int>(i =>
    i % 2 == 0 ? MiddlewareResult.Continue(i * 2) : MiddlewareResult.Drop<int>());
```

`Observed` reflects every item the middleware received, *including* ones a policy later drops, and composes across a middleware chain (each middleware sees the previous one's output).

### xUnit — capturing and asserting on progress

`ProgressCapture<T>` is an `IProgress<T>` that records every report; pass it straight to any progress-aware overload, then assert with `ProgressAssert`:

```csharp
using System.Threading;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit;
using Wolfgang.Etl.TestKit.Xunit;

var capture = new ProgressCapture<Report>();
var extractor = new TestExtractor<string>(new[] { "a", "b", "c" });

await foreach (var _ in extractor.ExtractAsync(capture, CancellationToken.None)) { }

ProgressAssert.HasReports(capture);
ProgressAssert.IsMonotonicallyIncreasing(capture, r => r.CurrentItemCount);

Report? last = capture.FinalReport;   // the final report, or null if none captured
```

### xUnit — verifying idempotency

When a component must produce identical results across repeated runs, derive from the matching opt-in idempotency base and implement its factory methods:

```csharp
using System.Collections.Generic;
using Wolfgang.Etl.TestKit.Xunit;

public sealed class MyExtractorIdempotencyTests
    : IdempotentExtractorContractTests<MyExtractor, MyRecord, MyProgress>
{
    protected override MyExtractor CreateSut(int itemCount) => new MyExtractor(itemCount);

    protected override IReadOnlyList<MyRecord> CreateExpectedItems() =>
        new List<MyRecord> { new("a"), new("b"), new("c"), new("d"), new("e") };
}
```

`IdempotentLoaderContractTests<TSut, TItem>` adds a `TryGetLoadedItems(TSut sut)` factory (return `null` if the loader does not expose its loaded items), and `IdempotentTransformerContractTests<TSut, TItem>` follows the extractor shape with `CreateExpectedItems()`.

### xUnit — verifying error handling

If your stage opts into the 0.18 error hook, derive from `ErrorHandlingContractTests<TSut>` and implement one harness method that runs a scenario with a single failing item under a given policy. The base verifies that `Skip` completes the run and counts the failure as an error kept *distinct* from the intentional-skip count, while `Abort` re-throws and counts no error:

```csharp
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit.Xunit;

public sealed class MyExtractorErrorHandlingTests
    : ErrorHandlingContractTests<MyExtractor>
{
    protected override async Task<ErrorHandlingOutcome> RunSingleFaultScenarioAsync(ItemErrorAction policy)
    {
        var sut = new MyExtractor(SourceWithOneBadRow()) { ErrorPolicy = policy };
        var aborted = false;
        try { await foreach (var _ in sut.ExtractAsync()) { } }
        catch { aborted = true; }
        return new ErrorHandlingOutcome(aborted, sut.CurrentItemCount, sut.CurrentErrorItemCount, sut.CurrentSkippedItemCount);
    }
}
```

### xUnit — verifying disposal

Derive from `DisposableStageContractTests<TSut>` to verify the 0.14/0.17 dispose guarantees — that a public operation throws `ObjectDisposedException` after `Dispose()`/`DisposeAsync()`, and that disposing twice is a harmless no-op:

```csharp
using System.Threading.Tasks;
using Wolfgang.Etl.TestKit.Xunit;

public sealed class MyExtractorDisposableTests
    : DisposableStageContractTests<MyExtractor>
{
    protected override MyExtractor CreateSut() => new MyExtractor(source);

    protected override async Task<bool> InvokeReportsObjectDisposedAsync(bool disposeFirst, bool useAsyncDispose)
    {
        var sut = CreateSut();
        if (disposeFirst)
        {
            if (useAsyncDispose) await sut.DisposeAsync(); else sut.Dispose();
        }
        try { await foreach (var _ in sut.ExtractAsync()) { } return false; }
        catch (System.ObjectDisposedException) { return true; }
    }
}
```

### xUnit — guarding an allocation budget

Derive from `AllocationBudgetContractTests<TSut>` to lock in that your stage's hot path stays allocation-free (or within a declared per-item budget). The harness measures the *marginal* allocation per item — `(alloc(10N) − alloc(N)) / 9N` — so one-time setup cancels out. Because it reads the process-wide GC counter, put derived tests in a serialized collection:

```csharp
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.TestKit.Xunit;
using Xunit;

[Collection("Allocation")]   // serialize — the counter is process-wide
public sealed class MyExtractorAllocationTests
    : AllocationBudgetContractTests<MyExtractor>
{
    protected override MyExtractor CreateSut(int itemCount) => new MyExtractor(itemCount);

    protected override async Task ExerciseHotPathAsync(CancellationToken ct)
    {
        await foreach (var _ in Sut.ExtractAsync(ct)) { }
    }

    // A record-materializing extractor declares its budget instead:
    // protected override double MaxBytesPerItem => 48;
}
```

`CreateSut(itemCount)` runs outside the measurement window (its cost is excluded); exercise the harness-supplied `Sut`. The test skips on frameworks without `GC.GetTotalAllocatedBytes` (net462 / netstandard2.0).

### xUnit — verifying prompt cancellation

Derive from `CancellationContractTests<TSut>` to verify a stage honours cancellation **promptly** — it stops shortly after the token is cancelled (rather than draining its source), throws `OperationCanceledException`, and processes nothing when handed an already-cancelled token. Pair it with a latent source such as the core-package `DelayingExtractor<T>` so a cancel interrupts an in-flight wait:

```csharp
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.TestKit;
using Wolfgang.Etl.TestKit.Xunit;

public sealed class MyExtractorCancellationTests
    : CancellationContractTests<DelayingExtractor<int>>
{
    protected override async Task<CancellationOutcome> RunAndCancelMidStreamAsync(int itemCount, int cancelAfter)
    {
        var sut = new DelayingExtractor<int>(Enumerable.Range(0, itemCount).ToArray(), TimeSpan.FromMilliseconds(5));
        using var cts = new CancellationTokenSource();
        var processed = 0; var canceled = false;
        try
        {
            await foreach (var _ in sut.ExtractAsync(cts.Token))
            {
                if (++processed == cancelAfter) cts.Cancel();
            }
        }
        catch (OperationCanceledException) { canceled = true; }
        return new CancellationOutcome(canceled, processed);
    }

    protected override async Task<CancellationOutcome> RunWithPreCancelledTokenAsync(int itemCount)
    {
        var sut = new DelayingExtractor<int>(Enumerable.Range(0, itemCount).ToArray(), TimeSpan.FromMilliseconds(5));
        var processed = 0; var canceled = false;
        try
        {
            await foreach (var _ in sut.ExtractAsync(new CancellationToken(canceled: true))) processed++;
        }
        catch (OperationCanceledException) { canceled = true; }
        return new CancellationOutcome(canceled, processed);
    }
}
```

The derived class owns and drives its stage (the base never receives the SUT, so no null-argument boilerplate); override `ItemCount` / `CancelAfter` / `PromptStopSlack` to tune. `DelayingExtractor<T>` waits a fixed `TimeSpan` — or a per-index `Func<int, TimeSpan>` — before each item, and honours `SkipItemCount` / `MaximumItemCount`.

### xUnit — verifying pipeline composition

Derive from `EtlPipelineContractTests<TItem, TProgress>` to verify that your loader composes into the `Wolfgang.Etl.Abstractions` 0.16 `EtlPipeline` and runs end-to-end — every source item is delivered, and the `EtlPipelineProgress` counts each record as extracted and loaded. The harness composes and runs the pipeline; you supply the source data, the sink, and a read-back:

```csharp
using System.Collections.Generic;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit.Xunit;

public sealed class MyLoaderPipelineTests
    : EtlPipelineContractTests<MyRecord, MyProgress>
{
    protected override IReadOnlyList<MyRecord> CreateSourceItems() =>
        new List<MyRecord> { new("a"), new("b"), new("c") };

    protected override LoaderBase<MyRecord, MyProgress> CreateSink() => new MyLoader();

    // Read back what the harness-composed `Sink` received, or return null to skip the delivery check.
    protected override IReadOnlyList<MyRecord>? GetLoadedItems() => ((MyLoader)Sink).Written;
}
```

### xUnit — verifying retry / resilience

Derive from `CancellationContractTests<TSut>`'s sibling `RetryContractTests<TSut>` to verify a stage whose `Wolfgang.Etl.Abstractions` 0.20 `WrapWorkerExecution` override adds a retry strategy: a transient fault that clears within the retry budget completes the run, and a fault that never clears fails after the maximum number of attempts (no infinite loop). The core package ships `RetryingExtractor<T>` — a ready-made component that fails its first `failFirstAttempts` worker invocations then succeeds, retrying up to `maxAttempts` — which your override can drive:

```csharp
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.TestKit;
using Wolfgang.Etl.TestKit.Xunit;

public sealed class MyRetryTests : RetryContractTests<RetryingExtractor<int>>
{
    protected override Task<RetryOutcome> RunWithTransientFaultAsync(int failFirstAttempts, int maxAttempts) =>
        Drive(new RetryingExtractor<int>(Enumerable.Range(0, 5).ToArray(), failFirstAttempts, maxAttempts));

    protected override Task<RetryOutcome> RunWithPermanentFaultAsync(int maxAttempts) =>
        Drive(new RetryingExtractor<int>(Enumerable.Range(0, 5).ToArray(), failFirstAttempts: maxAttempts, maxAttempts: maxAttempts));

    static async Task<RetryOutcome> Drive(RetryingExtractor<int> sut)
    {
        var items = 0; var ok = false;
        try { await foreach (var _ in sut.ExtractAsync(CancellationToken.None)) items++; ok = true; }
        catch (System.InvalidOperationException) { ok = false; }
        return new RetryOutcome(ok, sut.AttemptCount, items);
    }
}
```

`RetryingExtractor<T>` also serves as a worked example of building stream-level retry on the `WrapWorkerExecution` seam (each retry re-invokes the worker for a fresh stream).

### xUnit — one-liner scenarios with `EtlScenario`

`EtlScenario` composes an extract → (transform) → load pipeline from the doubles — optionally injecting a fault into the extractor or loader — runs it, and asserts the final state (loaded items, aggregate error count, or a terminal exception) in a single fluent expression:

```csharp
using System;
using System.Threading.Tasks;
using Wolfgang.Etl.TestKit;
using Wolfgang.Etl.TestKit.Xunit;

// A skipped extractor fault drops the item and counts one aggregate error:
await EtlScenario
    .From(1, 2, 3, 4)
    .WithExtractorFault(index: 2, new FormatException("bad row"))
    .RunAndAssertAsync(expectedLoaded: new[] { 1, 2, 4 }, expectedErrors: 1);

// Insert a transform stage:
await EtlScenario.From(1, 2, 3).Through(new TestTransformer<int>()).RunAndAssertAsync(new[] { 1, 2, 3 });

// A non-skipped fault propagates:
await EtlScenario
    .From(1, 2, 3)
    .WithExtractorFault(index: 1, new InvalidOperationException("boom"), skip: false)
    .RunAndAssertThrowsAsync<InvalidOperationException>();
```

Faults default to being *skipped* (routed through the base error hook and counted in `EtlPipelineProgress.ErrorItemCount`); pass `skip: false` to let one propagate and assert it with `RunAndAssertThrowsAsync<TException>()`.

### xUnit add-on — contract-testing your own ETL types

Derive your test class from the matching contract base and implement the abstract factory methods. You inherit the complete suite of `ExtractAsync` / `TransformAsync` / `LoadAsync` contract tests — all overloads, cancellation, progress, `SkipItemCount`, and `MaximumItemCount` — with zero boilerplate.

```csharp
using System.Collections.Generic;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit.Xunit;

public sealed class MyExtractorContractTests
    : ExtractorBaseContractTests<MyExtractor, MyRecord, MyProgress>
{
    protected override MyExtractor CreateSut(int itemCount) =>
        new MyExtractor("path/to/test-data.csv", itemCount);

    protected override IReadOnlyList<MyRecord> CreateExpectedItems() =>
        new List<MyRecord> { new("a"), new("b"), new("c"), new("d"), new("e") };

    protected override MyExtractor CreateSutWithTimer(IProgressTimer timer) =>
        new MyExtractor("path/to/test-data.csv", timer);
}
```

`LoaderBaseContractTests<TSut, TItem, TProgress>` follows the same shape, except its source factory is named `CreateSourceItems()`:

```csharp
public sealed class MyLoaderContractTests
    : LoaderBaseContractTests<MyLoader, MyRecord, MyProgress>
{
    protected override MyLoader CreateSut(int itemCount) => new MyLoader(connectionString);

    protected override IReadOnlyList<MyRecord> CreateSourceItems() =>
        new List<MyRecord> { new("a"), new("b"), new("c"), new("d"), new("e") };

    protected override MyLoader CreateSutWithTimer(IProgressTimer timer) =>
        new MyLoader(connectionString, timer);
}
```

`TransformerBaseContractTests<TSut, TItem, TProgress>` uses `CreateExpectedItems()` (like the extractor) and constrains `TSut` to `TransformerBase<TItem, TItem, TProgress>`.

> **Note:** Expose a `protected` constructor that accepts an `IProgressTimer` on your extractor/loader/transformer so `CreateSutWithTimer` can inject a `ManualProgressTimer` to fire progress callbacks on demand. `CreateExpectedItems()` / `CreateSourceItems()` must return at least 5 items.

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| **`TestExtractor<T>`** | In-memory extractor that yields items from an `IEnumerable<T>` (reusable across runs) or an `IEnumerator<T>` (for large, on-the-fly generated sequences) |
| **`TestTransformer<T>`** | Pass-through transformer that returns each item unchanged — useful when a pipeline needs a transformer stage but the test focuses on the extractor or loader |
| **`TestLoader<T>`** | In-memory loader that always enumerates the full stream; with `collectItems: true` it buffers items for assertion via `GetCollectedItems()`, with `false` it measures throughput without storing |
| **Pagination** | `SkipItemCount` and `MaximumItemCount` on every test double for skipping and limiting items |
| **Progress reporting** | Timer-based `IProgress<Report>` callbacks, with a `protected` `IProgressTimer` constructor for deterministic, on-demand firing in tests |
| **Contract test bases** | `ExtractorBaseContractTests<,,>`, `LoaderBaseContractTests<,,>`, and `TransformerBaseContractTests<,,>` — comprehensive xUnit suites covering every `Wolfgang.Etl.Abstractions` base-class behaviour |
| **`ManualProgressTimer`** | An `IProgressTimer` whose `Fire()` method triggers progress callbacks synchronously, so progress tests are deterministic |
| **`SynchronousProgress<T>`** | An `IProgress<T>` that invokes its callback synchronously for predictable progress assertions |
| **`TestExtractor<T>` factory ctors** | Build a `TestExtractor<T>` from a `Func<T>` or `Func<int, T>` factory (with an optional item count) instead of materializing a collection up front |
| **`FaultyExtractor<T>` / `FaultyLoader<T>` / `FaultyTransformer<T>`** | Fault-injection doubles with fluent `ThrowAt`, `ThrowAfterCompletion`, and `DuplicateAt` knobs, plus `SkipErrors()` / `HandleErrorsWith(policy)` / `CapturedErrors` to drive the Abstractions 0.18 per-item error hook |
| **`ProgressCapture<T>` + `ProgressAssert`** | `ProgressCapture<T>` is an `IProgress<T>` that records every report; `ProgressAssert` provides xUnit assertions (`HasReports`, `HasExactly`, `FinalReportSatisfies`, `IsMonotonicallyIncreasing`, …) over a capture |
| **`Idempotent*ContractTests`** | Opt-in `IdempotentExtractorContractTests<,,>`, `IdempotentLoaderContractTests<,>`, and `IdempotentTransformerContractTests<,>` bases that verify a component produces identical results across repeated runs |
| **`ErrorHandlingContractTests<TSut>`** | Opt-in base verifying a stage's 0.18 error hook — `Skip` continues and counts the failure as an error (distinct from the intentional-skip count); `Abort` re-throws |
| **`DisposableStageContractTests<TSut>`** | Opt-in base verifying the 0.14/0.17 dispose guarantees — use-after-dispose throws `ObjectDisposedException`, and double-dispose is a no-op |
| **Multi-TFM support** | net462, net481, netstandard2.0, net8.0, net10.0 |

---

## 🎯 Supported Frameworks

This library targets:

- **.NET Framework:** 4.6.2, 4.8.1
- **.NET Standard:** 2.0
- **.NET:** 8.0, 10.0

See the [NuGet package page](https://www.nuget.org/packages/Wolfgang.Etl.TestKit/) for the authoritative per-TFM compatibility matrix.

## 🔍 Code Quality & Static Analysis

This project enforces **strict code quality standards** through **7 specialized analyzers** and custom async-first rules:

### Analyzers in Use

1. **Microsoft.CodeAnalysis.NetAnalyzers** - Built-in .NET analyzers for correctness and performance
2. **Roslynator.Analyzers** - Advanced refactoring and code quality rules
3. **AsyncFixer** - Async/await best practices and anti-pattern detection
4. **Microsoft.VisualStudio.Threading.Analyzers** - Thread safety and async patterns
5. **Microsoft.CodeAnalysis.BannedApiAnalyzers** - Prevents usage of banned synchronous APIs
6. **Meziantou.Analyzer** - Comprehensive code quality rules
7. **SonarAnalyzer.CSharp** - Industry-standard code analysis

### Async-First Enforcement

This library uses **`BannedSymbols.txt`** to prohibit synchronous APIs and enforce async-first patterns:

**Blocked APIs Include:**
- ❌ `Task.Wait()`, `Task.Result` - Use `await` instead
- ❌ `Thread.Sleep()` - Use `await Task.Delay()` instead
- ❌ Synchronous file I/O (`File.ReadAllText`) - Use async versions
- ❌ Synchronous stream operations - Use `ReadAsync()`, `WriteAsync()`
- ❌ `Parallel.For/ForEach` - Use `Task.WhenAll()` or `Parallel.ForEachAsync()`
- ❌ Obsolete APIs (`WebClient`, `BinaryFormatter`)

**Why?** To ensure all code is **truly async** and **non-blocking** for optimal performance in async contexts.

---

## 🛠️ Building from Source

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- Optional: [PowerShell Core](https://github.com/PowerShell/PowerShell) for formatting scripts

### Build Steps

```bash
# Clone the repository
git clone https://github.com/Chris-Wolfgang/ETL-Abstractions.git
cd ETL-Abstractions

# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release

# Run code formatting (PowerShell Core)
pwsh ./scripts/format.ps1
```

### Code Formatting

This project uses `.editorconfig` and `dotnet format`:

```bash
# Format code
dotnet format

# Verify formatting (as CI does)
dotnet format --verify-no-changes
```

See [docs/README-FORMATTING.md](docs/README-FORMATTING.md) for detailed formatting guidelines.

### Building Documentation

This project uses [DocFX](https://dotnet.github.io/docfx/) to generate API documentation:

```bash
# Install DocFX (one-time setup)
dotnet tool install -g docfx

# Generate API metadata and build documentation
cd docfx_project
docfx metadata  # Extract API metadata from source code
docfx build     # Build HTML documentation

# Documentation is generated in the docs/ folder at the repository root
```

The documentation is automatically built and deployed to GitHub Pages when changes are pushed to the `main` branch.

**Local Preview:**
```bash
# Serve documentation locally (with live reload)
cd docfx_project
docfx build --serve

# Open http://localhost:8080 in your browser
```

**Documentation Structure:**
- `docfx_project/` - DocFX configuration and source files
- `docs/` - Generated HTML documentation (published to GitHub Pages)
- `docfx_project/index.md` - Main landing page content
- `docfx_project/docs/` - Additional documentation articles
- `docfx_project/api/` - Auto-generated API reference YAML files

---

## 🔐 Verify the build

Every release is built deterministically, and each GitHub Release attaches a
`reproducible-build-manifest.json` with the SHA-256 of every shipped assembly.
You can independently rebuild from the tag and confirm the hashes match — see
[docs/REPRODUCIBLE-BUILD.md](docs/REPRODUCIBLE-BUILD.md) for the step-by-step
procedure and how to publish a third-party attestation.

---

## 🤝 Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for:
- Code quality standards
- Build and test instructions
- Pull request guidelines
- Analyzer configuration details

---


## 🙏 Acknowledgments

- **[Wolfgang.Etl.Abstractions](https://github.com/Chris-Wolfgang/ETL-Abstractions)** — provides the `ExtractorBase`, `LoaderBase`, and `TransformerBase` base classes, progress-reporting infrastructure, and the `IProgressTimer` contract that this test kit builds on and verifies.
- **[xUnit](https://xunit.net/)** — the test framework the `Wolfgang.Etl.TestKit.Xunit` contract base classes are built on.

