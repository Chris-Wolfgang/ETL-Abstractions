type: docs

The `MaximumItemCount` and `SkipItemCount` examples on `LoaderBase` and `TransformerBase` now compile against the real worker signature (`await foreach` over the `IAsyncEnumerable<T>`) and call `IncrementCurrentSkippedItemCount()` / `IncrementCurrentItemCount()`, as the `ExtractorBase` examples already did.
