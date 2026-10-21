type: feature

New package `Wolfgang.Etl.Polly`: `PollyResilience.ForStream<T>(pipeline)` and `PollyResilience.ForLoader(pipeline)` build `WorkerResilience` wrappers that run a stage's worker through a Polly v8 `ResiliencePipeline` (whole-batch retry, timeout, circuit breaker, rate limiting), with no subclassing.
