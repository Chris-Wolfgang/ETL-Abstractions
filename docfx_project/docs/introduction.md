# Introduction

Welcome to Wolfgang.Etl.Abstractions!

## Overview

Wolfgang.Etl.Abstractions provides the interfaces and base classes for building
Extract–Transform–Load flows on top of `IAsyncEnumerable<T>`. You implement an
extractor, zero or more transformers, and a loader, then compose them into a
single strongly-typed, streaming pipeline.

## Key Features

- **Fluent, type-safe pipeline** — `Pipeline.Extract(...).Transform(...).Load(...).RunAsync()`
  composes stages into one runnable flow; the compiler enforces that each stage's
  output type matches the next stage's input.
- **Generic, format-agnostic pipeline** — `EtlPipeline.Create().From(...).Through(...).To(...).RunAsync()`
  starts from any `IAsyncEnumerable<T>` or extractor, chains transformer stages, and is
  the extension point that operator (`Wolfgang.Etl.Transformers`) and format packages
  build on.
- **Async streaming** — built on `IAsyncEnumerable<T>`, so items flow through the
  pipeline without buffering the whole set in memory.
- **Opt-in progress reporting** — progress-capable stages surface `IProgress<T>`
  reports with throughput and ETA, on a configurable interval.
- **Cooperative cancellation** — a `CancellationToken` passed to `RunAsync` is
  forwarded to every stage that supports it.
- **Optional stage disposal** — `.DisposeStagesOnCompletion()` tears stages down
  in reverse order after the run, for call sites that own short-lived stages.
- **Broad target coverage** — .NET Framework 4.6.2+, .NET Standard 2.0+, and
  .NET 5.0 through .NET 10.0.

See the [Getting Started](getting-started.md) guide for a full walkthrough.

## Getting Help

If you need help with Wolfgang.Etl.Abstractions, please:

- Check the [Getting Started](getting-started.md) guide
- Review the [API Reference](../api/index.md)
- Visit the [GitHub repository](https://github.com/Chris-Wolfgang/ETL-Abstractions)
- Open an issue on [GitHub Issues](https://github.com/Chris-Wolfgang/ETL-Abstractions/issues)
