type: fix

`TestExtractor`, `FaultyExtractor` and `RetryingExtractor` no longer `await Task.Yield()`, which resumed on the caller's `SynchronizationContext`; a host that blocks on the extraction from such a context can no longer deadlock on the test double.
