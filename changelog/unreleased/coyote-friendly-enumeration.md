type: internal

`ExtractorBase` and `TransformerBase` enumerate the worker with a manual `MoveNextAsync().ConfigureAwait(false)` loop instead of `await foreach (... .ConfigureAwait(false))`, so the Coyote dispose-race test can run. Same behaviour, including the no-context continuations.
