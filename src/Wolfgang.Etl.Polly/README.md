# Wolfgang.Etl.Polly

Ready-made [Polly](https://www.pollydocs.org/) resilience for Wolfgang.Etl stages. Build a Polly v8
`ResiliencePipeline` and assign it to an extractor, transformer or loader through its
`WorkerResilience` property; no subclassing.

`Wolfgang.Etl.Abstractions` has no third-party runtime dependencies. This package is where the
`Polly.Core` dependency lives, so only the projects that want Polly carry it.

## Install

```
dotnet add package Wolfgang.Etl.Polly
```

## Use

```csharp
using Polly;
using Polly.Retry;
using Wolfgang.Etl.Polly;

var pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 3 })
    .AddTimeout(TimeSpan.FromMinutes(5))
    .Build();

var extractor = new CsvExtractor(path)
{
    WorkerResilience = PollyResilience.ForStream<CsvRow>(pipeline),
};

var loader = new SqlBulkCopyLoader(connectionString)
{
    WorkerResilience = PollyResilience.ForLoader(pipeline),
};
```

## What it retries

This is **stream-level** resilience: the pipeline wraps the stage's whole worker, so a retry re-runs
the whole extract, transform or load. It suits whole-batch retry, timeouts, circuit breakers and rate
limiting.

- **Extractors and transformers** (`ForStream<T>`): each attempt runs the worker to completion and
  buffers its items; items are yielded only after an attempt succeeds, so a failed attempt never
  sends part of its output downstream and nothing is yielded twice. One attempt's items are held in
  memory.
- **Loaders** (`ForLoader`): each attempt runs the whole load. A retry enumerates the loader's input
  again, so use retry only when the input can be read more than once and the destination tolerates a
  repeated write (an idempotent upsert, or a transaction the failed attempt rolled back).

Not in scope:

- **Per-item transient-fault retry** belongs inside the worker, around the call that can fail.
- **Skipping or dead-lettering a bad item** is the stage's `ErrorPolicy`
  (see [`Wolfgang.Etl.ErrorPolicies`](https://www.nuget.org/packages/Wolfgang.Etl.ErrorPolicies/)).

The stage's counters (`CurrentItemCount` and friends) reset once per run, not per retry, so items
counted by a failed attempt stay counted.

## Supported frameworks

`net462`, `net472`, `net48`, `net481`, `netstandard2.0`, and `net5.0` through `net10.0`.

---

Part of the [ETL-Abstractions](https://github.com/Chris-Wolfgang/ETL-Abstractions) family; see the
[repository README](https://github.com/Chris-Wolfgang/ETL-Abstractions) for the full package set.
