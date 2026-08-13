# Wolfgang.Etl.TestKit

Test doubles and harnesses for exercising components built on
[`Wolfgang.Etl.Abstractions`](https://www.nuget.org/packages/Wolfgang.Etl.Abstractions/) — ready-made
extractors, loaders, and transformers, fault injection, a deterministic clock and progress timer, and
end-to-end scenario helpers.

[![NuGet](https://img.shields.io/nuget/v/Wolfgang.Etl.TestKit.svg?logo=nuget&label=NuGet)](https://www.nuget.org/packages/Wolfgang.Etl.TestKit/)
[![Downloads](https://img.shields.io/nuget/dt/Wolfgang.Etl.TestKit.svg?logo=nuget&label=downloads)](https://www.nuget.org/packages/Wolfgang.Etl.TestKit/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/Chris-Wolfgang/ETL-Abstractions/blob/main/LICENSE)
[![GitHub](https://img.shields.io/badge/GitHub-Repository-181717?logo=github)](https://github.com/Chris-Wolfgang/ETL-Abstractions)

---

## Install

```
dotnet add package Wolfgang.Etl.TestKit
```

For xUnit **contract-test base classes** that verify your own components against the full behavioural
contract of their base type, add
[`Wolfgang.Etl.TestKit.Xunit`](https://www.nuget.org/packages/Wolfgang.Etl.TestKit.Xunit/) as well.

## What's in the box

| Type | Use |
| --- | --- |
| `TestExtractor<T>` / `TestLoader<T>` / `TestTransformer<T>` | Drive or capture a pipeline with in-memory doubles. |
| `FaultyExtractor<T>` / `FaultyLoader<T>` / `FaultyTransformer<T>` | Inject a fault at a chosen item; route it through the base error hook (`SkipErrors()` / `HandleErrorsWith(policy)` / `CapturedErrors`). |
| `SnapshotTestLoader<T>` | Snapshot / approval testing of loaded output. |
| `RecordingMiddleware<T>` | Assert on items flowing through `WithMiddleware(...)`. |
| `ManualTimeSource` + `WithTimeSource` | Drive `Report` timing (`Elapsed`, throughput, ETA) from a fake clock. |
| `ManualProgressTimerCore` + `WithManualProgressTimer` | Fire a stage's progress callback deterministically with `Tick()` — no per-component timer plumbing. |
| `EtlScenario` | Compose extract → (transform) → load, optionally inject a fault, run through `EtlPipeline`, and assert the final state in one expression. |

## Quick start

```csharp
using Wolfgang.Etl.TestKit;

// In-memory doubles + deterministic progress timer
var timer = new ManualProgressTimerCore();
var extractor = new TestExtractor<int>(new[] { 1, 2, 3 }).WithManualProgressTimer(timer);

Report? captured = null;
await using var e = extractor.ExtractAsync(new SyncProgress<Report>(r => captured = r)).GetAsyncEnumerator();
await e.MoveNextAsync();
timer.Tick();                 // fires the progress callback exactly once
Assert.NotNull(captured);
```

```csharp
// One-liner end-to-end scenario with a fault that is skipped and counted
await EtlScenario
    .From(new FaultyExtractor<int>(source, failAt: 2))
    .To(new TestLoader<int>(collectItems: true))
    .SkipErrors()
    .RunAndAssertAsync(expectedLoaded: new[] { 1, 3 }, expectedErrorItemCount: 1);
```

See the doubles' XML docs for the full surface.

## Supported frameworks

Multi-targets `net462`, `netstandard2.0`, `netstandard2.1`, and `net5.0` through `net10.0`.

---

Part of the [ETL-Abstractions](https://github.com/Chris-Wolfgang/ETL-Abstractions) family — see the
[repository README](https://github.com/Chris-Wolfgang/ETL-Abstractions) for the full package set and
contributor/build docs.
