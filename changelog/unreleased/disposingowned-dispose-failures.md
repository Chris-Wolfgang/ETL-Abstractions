type: fix

`DisposingOwned` now disposes every owned resource even when one throws on dispose, and a disposal failure no longer replaces the exception from a failed run; disposal failures on a successful run surface as an `AggregateException`.
