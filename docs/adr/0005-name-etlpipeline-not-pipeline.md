# ADR 0005 — Name the generic builder `EtlPipeline`, not `Pipeline`

- **Status:** Accepted
- **Date:** 2026-07-21
- **Deciders:** Chris Wolfgang

## Context

The package already ships a fluent builder named `Pipeline` —
`Pipeline.Extract(extractor).Transform(t).Load(loader).RunAsync()` — which starts
from a *typed extractor* and offers per-stage progress and options
(`.WithProgress`, `.WithName`, `.DisposeStagesOnCompletion`). The new generic,
format-agnostic builder (#147) starts from *any* source and is designed to be
extended by other packages. "Pipeline" is the obvious name for it too — but it is
taken, and both types live in the same namespace (`Wolfgang.Etl.Abstractions`).

## Decision

We will name the generic builder **`EtlPipeline`** and leave `Pipeline` as the
fluent Extract/Transform/Load builder. The two coexist deliberately: they serve
different call sites, and the `Etl` prefix distinguishes the generic, extensible
one without forcing a namespace split or renaming the established `Pipeline`.

## Alternatives considered

- **Rename the existing `Pipeline`** — rejected: it is public, shipped, and used;
  renaming it is a source-breaking change to save the newer type a prefix.
- **Put the generic builder in a sub-namespace and call it `Pipeline`** — rejected:
  two `Pipeline` types resolvable in nearby code is a `using`-ordering foot-gun and
  a documentation hazard; the prefix is cheaper than the ambiguity.
- **A more elaborate name (`GenericPipeline`, `EtlFlow`)** — rejected: `EtlPipeline`
  is the least surprising given the package name and reads well at the call site.

## Consequences

- Two builder types ship side by side; the docs (the getting-started guide carries
  a "`Pipeline` vs `EtlPipeline`" comparison table) must keep steering readers to
  the right one — `Pipeline` when you hold discrete typed stages and want per-stage
  progress, `EtlPipeline` for cross-format flows and the operator/format-package
  extensions.
- Related naming stays consistent under the prefix: `EtlPipelineProgress`,
  `IEtlPipeline<T>`, `IEtlPipelineSink`, `EtlPipelineSourceExtensions`.
