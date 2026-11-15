# ADR 0013 — Report aggregate item errors and expose stage-specific progress

- **Status:** Accepted
- **Date:** 2026-11-15
- **Deciders:** Chris Wolfgang

## Context

ADR-0004 limited `EtlPipelineProgress` to extracted and loaded counts because
the core could not observe operator-level filtering or errors. The API has since
added `ErrorItemCount`, reported by stages that implement
`IReportsItemErrors`, and the counters were widened to `long`. The progress
snapshot still describes aggregate pipeline activity; it does not claim that
every stage reports errors.

The generic pipeline also now exposes typed stage progress through
`EtlPipelineStageProgressExtensions`. This is independent of the aggregate
`IProgress<EtlPipelineProgress>` sink.

## Decision

We will keep the aggregate snapshot limited to the counts the pipeline and
reporting stages can measure: `ExtractedItemCount`, `LoadedItemCount`,
`Elapsed`, and `ErrorItemCount`. Stages that provide richer typed progress
continue to report through their own progress observers.

## Alternatives considered

- **Return to the three-field snapshot from ADR-0004** — rejected: it would
  omit error skips already reported by participating stages.
- **Treat the aggregate snapshot as a replacement for stage progress** —
  rejected: it cannot describe stage-specific metrics or report errors from
  components that do not implement the error-reporting contract.

## Consequences

- Consumers can use the aggregate snapshot for pipeline-level throughput and
  reported error counts, while attaching stage observers when they need richer
  stage-specific details.
- `ErrorItemCount` covers errors reported by stages implementing
  `IReportsItemErrors`; components without that contract do not contribute
  error counts.
- The count properties use `long`, so large runs do not require a separate
  widening decision.
