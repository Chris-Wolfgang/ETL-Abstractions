# ADR 0002 — EtlPipeline core lives in Abstractions; operators live in Transformers

- **Status:** Accepted
- **Date:** 2026-07-21
- **Deciders:** Chris Wolfgang

## Context

The generic `EtlPipeline` (issue #147) is a format-agnostic builder:
`Create().From(source).Through(stage).To(loader).RunAsync()`. The natural next
question is where the *operators* — `Where`, `Select`, `Distinct`, `Take`,
`Buffer`, … — should live. The first implementation put the full LINQ-style
operator surface directly on the pipeline in this package.

That was wrong for a structural reason: the companion package
`Wolfgang.Etl.Transformers` **already ships** those operators as transformers
(`WhereTransformer<T>`, `SelectTransformer<TIn,TOut>`, …), and its dependency
arrow points *at* Abstractions (`Transformers → Abstractions`). Abstractions
therefore cannot reference Transformers to reuse them. Re-implementing the
operators in the core would create a **third** copy of the same logic (after
`System.Linq.Async` and `ETL-Transformers`), with three places to fix a bug.

## Decision

We will keep only the **plumbing** in Abstractions — `From`, `Through`, `To`,
`AsAsyncEnumerable` — and provide the **operators** in `Wolfgang.Etl.Transformers`
as extension methods over `Through`, reusing the transformers that package already
owns. **Format factories** (`CsvExtractor<T>`, sink terminators) live in the
individual format packages, again as extension methods. This is "Option B" from
the #147 discussion.

The layering is: `EtlPipeline.Create().From(...)` (Abstractions) →
`.Where(...).Select(...)` (Transformers) → `.To(...)` /
`.CsvLoader(...)` (format packages).

## Alternatives considered

- **Operators in the core (Option A)** — rejected: forces a third reimplementation
  of `Where`/`Select`/… and couples the format-agnostic core to LINQ semantics it
  doesn't need.
- **Move the transformers down into Abstractions** — rejected: Abstractions is the
  dependency root; pulling concrete transformers into it inverts the intended
  layering and bloats the base package every consumer takes.

## Consequences

- The core stays small and has no operator surface of its own; `Through` is the
  single extension seam that both Transformers and format packages build on.
- Operator extensions are gated on Abstractions **0.16.0+** being published
  (tracked as ETL-Transformers #150) — downstream work waits on the core shipping.
- A caller who wants `Where`/`Select` must reference `Wolfgang.Etl.Transformers`;
  the core alone gives them `Through` with an explicit transformer or delegate.
- See [ADR-0004](0004-etlpipelineprogress-extracted-loaded-elapsed.md): because the
  core never sees per-record operator decisions, its progress model reports only
  extracted/loaded counts.
