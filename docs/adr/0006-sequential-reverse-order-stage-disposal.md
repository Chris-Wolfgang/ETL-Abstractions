# ADR 0006 — Dispose pipeline stages sequentially, in reverse construction order

- **Status:** Accepted
- **Date:** 2026-07-21
- **Deciders:** Chris Wolfgang

## Context

`Pipeline.DisposeStagesOnCompletion()` disposes each stage the pipeline owns once
the run finishes, so short-lived stages the call site created don't each need their
own `using`. Two questions arise: in what **order** should stages be disposed, and
should disposal run **concurrently** to shave wall-clock time? A pipeline can hold
an extractor, several transformers, and a loader — some `IAsyncDisposable`, some
`IDisposable`, some neither — and a downstream stage may transitively hold a
resource owned by an upstream stage (e.g. a transformer wrapping the extractor's
reader).

## Decision

We will dispose stages **sequentially, in reverse construction order** (loader →
transformers → extractor), matching the LIFO convention of nested
`using`/`await using` blocks and DI-container scope disposal. A downstream stage is
torn down before the upstream stage whose resources it may reference. Disposal
prefers `IAsyncDisposable` over `IDisposable`, skips stages that are neither,
continues past any failure, and aggregates the failures into an
`AggregateException`. If the run itself threw, that exception is the primary signal
and is rethrown unchanged; disposal still runs regardless.

## Alternatives considered

- **Parallel disposal** (`Task.WhenAll` over the stages) — rejected: correctness of
  shared-resource teardown order outweighs the marginal wall-clock saving. Disposing
  an upstream stage's resource while a downstream stage still holds it is a
  use-after-dispose hazard, and it also collides with this project's ban on
  synchronous parallel primitives (see [ADR-0008](0008-ban-synchronous-parallel-apis.md)).
- **Forward (construction) order** — rejected: tears down upstream resources first,
  the opposite of the `using`-nesting guarantee callers expect.
- **Stop at the first disposal failure** — rejected: would leak the remaining
  stages' resources; disposing all of them and aggregating is safer.

## Consequences

- Teardown order is predictable and matches `using`-block intuition, which is what
  a caller reaching for `DisposeStagesOnCompletion()` is implicitly relying on.
- Disposal is single-threaded and slightly slower than a parallel teardown would
  be; this is an accepted trade for correctness.
- A stage that throws on dispose does not prevent its siblings from being disposed;
  callers see an `AggregateException` unless the run's own exception takes priority.
