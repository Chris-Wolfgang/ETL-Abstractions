type: fix

The TestKit doubles (`TestExtractor`, `TestTransformer`, `TestLoader`, `SnapshotTestLoader`, `DelayingExtractor`, `RetryingExtractor`, `FaultyExtractor`, `FaultyTransformer`, `FaultyLoader`) now stop at `MaximumItemCount` without reading one more record from their source and discarding it; the `ExtractorBase.MaximumItemCount` example shows the same check-after-counting pattern.
