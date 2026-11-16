type: fix

An exception thrown during a periodic progress tick (from `CreateProgressReport`, a synchronous `IProgress<T>.Report`, or an `Elapsed` subscriber) no longer terminates the process; the built-in timer stops ticking instead, and a persistent fault surfaces through the final report.
