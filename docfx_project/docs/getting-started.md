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
