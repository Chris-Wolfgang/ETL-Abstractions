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

## Quick start

```csharp
using System;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit;

// In-memory double + deterministic progress timer
var timer = new ManualProgressTimerCore();
var extractor = new TestExtractor<int>(new[] { 1, 2, 3 }).WithManualProgressTimer(timer);

var progress = new LastReport();
await using var e = extractor.ExtractAsync(progress).GetAsyncEnumerator();
await e.MoveNextAsync();
timer.Tick();                 // fires the progress callback exactly once, on this thread
// progress.Value is now the stage's Report

// Keeps the last report it was given. (Progress<T> would post it to the thread pool instead.)
sealed class LastReport : IProgress<Report>
{
    public Report? Value { get; private set; }

    void IProgress<Report>.Report(Report value) => Value = value;
}
```

For xUnit tests, `Wolfgang.Etl.TestKit.Xunit` adds `SynchronousProgress<T>` and `ProgressCapture<T>` for
the progress side, and `EtlScenario` for one-expression end-to-end scenarios:

```csharp
using Wolfgang.Etl.TestKit.Xunit;

// Extract 1..4, fail on the item at index 2, skip it, and assert what was loaded and counted
await EtlScenario
    .From(1, 2, 3, 4)
    .WithExtractorFault(index: 2, new FormatException("bad row"))
    .RunAndAssertAsync(expectedLoaded: new[] { 1, 2, 4 }, expectedErrors: 1);
```

See the doubles' XML docs for the full surface.

## Supported frameworks

Multi-targets `net462`, `net481`, `netstandard2.0`, `net5.0`, `net6.0`, `net7.0`, `net8.0` and `net10.0`.
Other .NET Framework 4.6.2+ and .NET 9 consumers resolve to the nearest of these.

---

Part of the [ETL-Abstractions](https://github.com/Chris-Wolfgang/ETL-Abstractions) family — see the
[repository README](https://github.com/Chris-Wolfgang/ETL-Abstractions) for the full package set and
contributor/build docs.
