# Getting Started

This guide will help you quickly get up and running with Wolfgang.Etl.Abstractions.

## Prerequisites

- A project targeting any framework the package supports — .NET Framework 4.6.2+,
  .NET Standard 2.0+, or .NET 5.0 through .NET 10.0.
- Any editor: Visual Studio 2022, Visual Studio Code, or JetBrains Rider.

## Installation

### Via NuGet Package Manager

```bash
dotnet add package Wolfgang.Etl.Abstractions
```

### Via Package Manager Console

```powershell
Install-Package Wolfgang.Etl.Abstractions
```

## Quick Start

An ETL flow has three kinds of stage — an **extractor** (produces items), zero or
more **transformers** (reshape items), and a **loader** (consumes items). Implement
the matching async interface for each:

```csharp
using Wolfgang.Etl.Abstractions;

// Extract: produce a stream of items.
internal sealed class NumberExtractor : IExtractAsync<int>
{
    public async IAsyncEnumerable<int> ExtractAsync()
    {
        for (var i = 1; i <= 5; i++)
        {
            yield return i;
            await Task.Yield();
        }
    }
}

// Transform: map each item to a new shape or type.
internal sealed class ToStringTransformer : ITransformAsync<int, string>
{
    public async IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<int> items)
    {
        await foreach (var item in items)
        {
            yield return $"Item {item}";
        }
    }
}

// Load: consume the final stream.
internal sealed class ConsoleLoader : ILoadAsync<string>
{
    public async Task LoadAsync(IAsyncEnumerable<string> items)
    {
        await foreach (var item in items)
        {
            Console.WriteLine(item);
        }
    }
}
```

Then compose them into a single, strongly-typed pipeline with the fluent
`Pipeline` API and run it:

```csharp
await Pipeline
    .Extract(new NumberExtractor())
    .Transform(new ToStringTransformer())
    .Load(new ConsoleLoader())
    .RunAsync();
```

The compiler enforces that each stage's output type matches the next stage's
input, so a mismatch is a build error rather than a runtime surprise.

### Optional capabilities

The chain grows to fit the run without changing the stages:

- **Cancellation** — pass a token to `RunAsync(token)`; it is forwarded to every
  stage that supports cancellation.
- **Progress** — for stages that implement a progress-capable interface, call
  `.WithProgress(progress)` to receive `IProgress<T>` reports. Calling it on a
  stage that doesn't support progress is a compile error.
- **Naming** — `.WithName("nightly-import")` labels the pipeline for diagnostics.
- **Stage disposal** — `.DisposeStagesOnCompletion()` disposes each stage (in
  reverse construction order) once the run finishes, so short-lived stages the
  call site owns don't each need their own `using`.

```csharp
await Pipeline
    .Extract(extractor).WithProgress(extractProgress)
    .Transform(transformer)
    .Load(loader).WithProgress(loadProgress)
    .WithName("nightly-import")
    .DisposeStagesOnCompletion()
    .RunAsync(cancellationToken);
```

A complete, runnable version of the basic walkthrough lives in
[`examples/Net8.0/Example7-FluentPipeline`](https://github.com/Chris-Wolfgang/ETL-Abstractions/tree/main/examples/Net8.0/Example7-FluentPipeline)
(the `Net4.8` folder has the same example for .NET Framework).

## Generic pipeline (`EtlPipeline`)

Alongside the fluent `Pipeline`, `EtlPipeline` is a **format-agnostic** pipeline
built for cross-format flows (CSV → JSON, JSON → SQL, …) and for being extended by
other packages. Where `Pipeline` starts from a typed extractor, `EtlPipeline` starts
from *any* source — an `IAsyncEnumerable<T>` or an `ExtractorBase<T, TProgress>` —
appends transformer stages with `Through`, and terminates with a loader:

```csharp
using Wolfgang.Etl.Abstractions;

await EtlPipeline.From(source)               // IAsyncEnumerable<string> or an ExtractorBase
    .Through(new ParseTransformer())         // ITransformAsync<string, Order>
    .Through(new EnrichTransformer(lookup))  // ITransformAsync<Order, EnrichedOrder>
    .To(sqlLoader)                           // LoaderBase<EnrichedOrder, TProgress>
    .RunAsync(progress, cancellationToken);
```

`Through` returns `IEtlPipeline<TOut>`, so you can chain as many stages as you like;
the element type flows from one stage's output to the next and the compiler enforces
the match (a same-type `ITransformAsync<T, T>` stage is fine). Nothing runs until
`To(...).RunAsync()` — records are then pulled through the whole chain one at a time,
with no buffering between stages.

### Supplying a stage: a transformer class or an inline delegate

`Through` has two shapes. Pass an `ITransformAsync<T, TOut>` (or the cancellation-aware
`ITransformWithCancellationAsync<T, TOut>`) when you have a reusable transformer class —
or pass a **stream-to-stream delegate** to define a one-off stage inline, without
declaring a class:

```csharp
await EtlPipeline.From(orders)
    .Through(s => s.Where(o => o.Amount > 0))   // Func<IAsyncEnumerable<Order>, IAsyncEnumerable<Order>>
    .Through(Enrich)                            // a method: IAsyncEnumerable<Order> -> IAsyncEnumerable<EnrichedOrder>
    .To(sqlLoader)
    .RunAsync();
```

The delegate is the same stream-to-stream contract as `ITransformAsync.TransformAsync`
(the lambda body typically composes `System.Linq.Async` operators), so it stays at the
"append a stage" level — it is *not* a per-element projection. (A per-element
`Where`/`Select` over individual records is an operator, provided by
`Wolfgang.Etl.Transformers`; see below.) A cancellation-aware delegate overload,
`Func<IAsyncEnumerable<T>, CancellationToken, IAsyncEnumerable<TOut>>`, receives the
run's token.

And if the source already produces what the loader consumes, skip `Through` entirely —
the compiler requires the loader's input type to match the source's output:

```csharp
await EtlPipeline.From(orders).To(orderLoader).RunAsync();
```

### Progress, cancellation, and the escape hatch

- **Progress** — pass an `IProgress<EtlPipelineProgress>` to `RunAsync`. The snapshot
  reports `RecordsExtracted` (counted at the source) and `RecordsLoaded` (counted at
  the sink) plus `Elapsed`, regardless of how many stages sit in between.
- **Cancellation** — the token passed to `RunAsync` is observed while pulling from the
  source and is forwarded into any cancellation-aware transformer stage.
- **Escape hatch** — `AsAsyncEnumerable(token)` drops to the raw `IAsyncEnumerable<T>`
  so you can apply `System.Linq.Async` operators directly.

```csharp
var progress = new Progress<EtlPipelineProgress>(p =>
    Console.WriteLine($"extracted {p.RecordsExtracted}, loaded {p.RecordsLoaded}"));

await EtlPipeline.From(extractor)
    .Through(enrich)
    .To(loader)
    .RunAsync(progress, cancellationToken);
```

### Operators and source factories

The core deliberately ships only the plumbing — `From`, `Through`, `To`, and
`AsAsyncEnumerable`. Two layers build on it:

- **LINQ-flavored operators** (`Where`, `Select`, `Distinct`, `Take`, `Buffer`, …) are
  provided by the companion `Wolfgang.Etl.Transformers` package as extension methods
  over `Through`, reusing the transformers it already ships. With that package
  referenced the chain reads `EtlPipeline.From(...).Where(...).Select(...).To(...)`.
- **Source factories and sink terminators** (for example `CsvExtractor<T>(...)` or
  `SqlBulkCopyLoader<T>(...)`) are provided by the format packages, hung off the
  `EtlPipeline.Source` sentinel: `EtlPipeline.Source.CsvExtractor<Order>("orders.csv")`.

A complete, runnable version lives in
[`examples/Net8.0/Example8-EtlPipeline`](https://github.com/Chris-Wolfgang/ETL-Abstractions/tree/main/examples/Net8.0/Example8-EtlPipeline)
(the `Net4.8` folder has the same example for .NET Framework).

### `Pipeline` vs `EtlPipeline`

| | `Pipeline` (fluent) | `EtlPipeline` (generic) |
|---|---|---|
| Starts from | a typed extractor | any `IAsyncEnumerable<T>` or `ExtractorBase<T, TProgress>` |
| Appends stages | `.Transform(...)` | `.Through(...)` (plus operators via `Wolfgang.Etl.Transformers`) |
| Extended by packages | no | yes — source factories via the `Source` sentinel |
| Progress | per-stage `IProgress<T>` | pipeline-level `EtlPipelineProgress` |

Reach for `Pipeline` when you already hold discrete extractor/transformer/loader
objects and want per-stage progress; reach for `EtlPipeline` for cross-format flows or
when you want the operator and format-package extensions.

## Next Steps

- Explore the [API Reference](../api/index.md) for detailed documentation
- Read the [Introduction](introduction.md) to learn more about Wolfgang.Etl.Abstractions
- Check out example projects in the [GitHub repository](https://github.com/Chris-Wolfgang/ETL-Abstractions)

## Common Issues

<!-- Add common issues and their solutions here -->

## Additional Resources

- [GitHub Repository](https://github.com/Chris-Wolfgang/ETL-Abstractions)
- [Contributing Guidelines](https://github.com/Chris-Wolfgang/ETL-Abstractions/blob/main/CONTRIBUTING.md)
- [Report an Issue](https://github.com/Chris-Wolfgang/ETL-Abstractions/issues)
