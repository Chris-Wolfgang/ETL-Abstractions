# Migrating from `Pipeline` to `EtlPipeline`

`Wolfgang.Etl.Abstractions` has two pipeline builders. Both are supported and coexist by design
([ADR-0005](adr/0005-name-etlpipeline-not-pipeline.md)):

- **`Pipeline`**: the staged, typed builder: `Pipeline.Extract(e).Transform(t).Load(l).RunAsync()`.
- **`EtlPipeline`**: the generic builder the format packages and LINQ-style operators extend:
  `EtlPipeline.Create().From(source).Through(t).To(loader).RunAsync()`.

This guide maps every `Pipeline` call shape to its `EtlPipeline` equivalent. It is the parity checklist
from #382. Nothing here deprecates `Pipeline`; that would need a new ADR that supersedes ADR-0005.

## Call-shape map

| `Pipeline` | `EtlPipeline` |
| --- | --- |
| `Pipeline.Extract(extractor)` | `EtlPipeline.Create().From(extractor)` (an `ExtractorBase`) |
| `Pipeline.Extract(extractor).WithProgress(p)` | `EtlPipeline.Create().From(extractor, p)` |
| `Pipeline.Extract(e)` where `e` only implements `IExtractAsync<T>` | `EtlPipeline.Create().From(e.ExtractAsync())` |
| `.Transform(transformer)` (`ITransformAsync` / `ITransformWithCancellationAsync`) | `.Through(transformer)` |
| `.Transform(transformer).WithProgress(p)` (a `TransformerBase`) | `.Through(transformer, p)` |
| `.Transform(t).WithProgress(p)` where `t` only implements `ITransformWithProgressAndCancellationAsync` | `.Through((items, token) => t.TransformAsync(items, p, token))` |
| `.Load(loader)` (a `LoaderBase`) | `.To(loader)` |
| `.Load(loader).WithProgress(p)` (a `LoaderBase`) | `.To(loader, p)` |
| `.Load(l)` where `l` only implements `ILoadAsync<T>` | `await l.LoadAsync(pipeline.AsAsyncEnumerable(token))` (see below) |
| `.WithName(name)` / `.Name` | none, by decision (see below) |
| `.DisposeStagesOnCompletion()` | `.DisposingOwned(extractor, transformer, loader)` |
| `.RunAsync()` / `.RunAsync(token)` | `.RunAsync()` / `.RunAsync(progress, token)` |

`EtlPipeline` also reports an aggregate `EtlPipelineProgress` (extracted, loaded and error counts plus
elapsed time) through `RunAsync(progress)`. That is independent of the per-stage progress above; use
either or both.

## Example

```csharp
// Before
await Pipeline
    .Extract(csvExtractor).WithProgress(extractProgress)
    .Transform(enrich).WithProgress(enrichProgress)
    .Load(sqlLoader).WithProgress(loadProgress)
    .DisposeStagesOnCompletion()
    .RunAsync(token);

// After
await EtlPipeline
    .Create()
    .From(csvExtractor, extractProgress)
    .Through(enrich, enrichProgress)
    .To(sqlLoader, loadProgress)
    .DisposingOwned(csvExtractor, enrich, sqlLoader)
    .RunAsync(progress: null, token);
```

## Decisions

### Naming is not carried over

`Pipeline.WithName` stores a string that `Pipeline` itself never reads: it does not appear in
exceptions, progress reports or logs. The code that builds a pipeline already knows what it is
building, so `EtlPipeline` does not add a name. If a future feature needs one (for example a name in
`EtlPipelineProgress` or in an exception message), it will be added with that feature.

### Stage disposal is `DisposingOwned`

`DisposeStagesOnCompletion()` disposes the extractor, transformers and loader after the run.
`DisposingOwned(params object[])` does the same for whatever it is given: after the run, whether it
succeeds or throws, it disposes each argument (`IAsyncDisposable` first, then `IDisposable`) in reverse
order, which matches `Pipeline`'s reverse-construction-order disposal
([ADR-0006](adr/0006-sequential-reverse-order-stage-disposal.md)). Passing the stages explicitly also
lets a pipeline dispose other resources it owns, such as the stream a loader writes to.

### Loaders that are not `LoaderBase`

`To` takes a `LoaderBase` because the sink reads the loader's error count for
`EtlPipelineProgress.ErrorItemCount`. A loader that only implements `ILoadAsync<T>` consumes the
pipeline's stream directly:

```csharp
var pipeline = EtlPipeline
    .Create()
    .From(extractor)
    .Through(transformer);

await loader.LoadAsync(pipeline.AsAsyncEnumerable(token));
```

This path does not report `EtlPipelineProgress`.
