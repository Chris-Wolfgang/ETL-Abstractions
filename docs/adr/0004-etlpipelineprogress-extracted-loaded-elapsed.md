# ADR 0004 — `EtlPipelineProgress` reports extracted / loaded / elapsed only

- **Status:** Accepted
- **Date:** 2026-07-21
- **Deciders:** Chris Wolfgang

## Context

`EtlPipeline.RunAsync` accepts an `IProgress<EtlPipelineProgress>`. An earlier
draft of the progress record carried richer counters — `RecordsFiltered` and
`RecordsErrored` alongside extracted/loaded — on the assumption the pipeline could
report how many records each stage dropped or failed.

Once operators moved out of the core ([ADR-0002](0002-etlpipeline-core-in-abstractions-operators-in-transformers.md)),
that assumption broke. The core no longer sees per-record operator decisions: a
`Where` that filters a record is an opaque transformer stage from the pipeline's
point of view. The only two points the core observes every record are the
**source** (where it counts what's extracted) and the **sink** (where it counts
what's loaded). "Filtered" and "errored" are semantics only a specific transformer
knows, and the core can't populate them honestly.

## Decision

We will make `EtlPipelineProgress` a record of exactly three fields —
`RecordsExtracted` (counted at the source), `RecordsLoaded` (counted at the sink),
and `Elapsed` — and nothing the core cannot measure directly. A field the pipeline
cannot fill accurately is worse than an absent field: it invites callers to trust
a number that is always zero or wrong.

## Alternatives considered

- **Keep `RecordsFiltered` / `RecordsErrored`** — rejected: after Option B the core
  cannot populate them; they would report 0 regardless of what transformers did,
  which is actively misleading.
- **Have transformers push their own counters up into the progress record** —
  rejected: requires every transformer to know about `EtlPipelineProgress`,
  coupling the operator layer to the core's reporting type. Per-stage progress is
  the fluent `Pipeline`'s job, not the generic `EtlPipeline`'s.

## Consequences

- The progress snapshot is honest and cheap: two counters the core owns end-to-end,
  plus elapsed time, independent of how many stages sit between source and sink.
- Callers who need per-stage or per-record diagnostics use the fluent `Pipeline`
  API (per-stage `IProgress<T>`) or an explicit transformer that reports its own
  metrics.
- The counters are currently `int`; widening them to `long` for very large runs is
  tracked as follow-up #285 and would be a deliberate, separate decision.
