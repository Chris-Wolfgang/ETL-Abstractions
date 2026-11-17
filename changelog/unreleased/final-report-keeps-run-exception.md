type: fix

When a run fails or is cancelled and the final progress report also throws (from `CreateProgressReport` or the `IProgress<T>` sink), the caller now gets the run's own exception instead of the report's; this applies to `ExtractorBase`, `TransformerBase`, `LoaderBase` and `EtlPipeline.RunAsync(progress)`. On a run that did not fail, a failing final report still reaches the caller.
