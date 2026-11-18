# ADR 0017 — Constrain record and progress type parameters to `notnull`

- **Status:** Accepted
- **Date:** 2026-11-17
- **Deciders:** Chris Wolfgang

## Context

Every stage is generic over the record type it extracts, transforms or loads, and
most are generic over a progress type as well. Records flow through
`IAsyncEnumerable<T>` from stage to stage.

A `null` record in that stream is ambiguous. It could mean the end of the data, a
row that was skipped, or a real value. If the type system allows it, every
downstream stage has to null-check each item on its hot path, and each library in
the family has to decide for itself what a `null` item means.

The constraint is already on the public surface of all four packages. The only
written record of it was in a downstream repository (ETL-Transformers ADR-0002),
which describes it as inherited from here.

## Decision

We constrain every type parameter that stands for a record to `notnull`: on the
`IExtract*`, `ITransform*` and `ILoad*` interfaces, on `ExtractorBase`,
`TransformerBase` and `LoaderBase`, on the `EtlPipeline` builder, and on the TestKit
doubles and contract-test bases. The progress type parameter `TProgress` carries the
same constraint on the base classes and on the progress interfaces.

"No value" has one representation: the stream does not yield an item.

## Alternatives considered

- **Leave the type parameters unconstrained** — rejected: it compiles, but it brings
  back the ambiguity and the per-stage null checks, and different libraries would
  settle the meaning of a `null` item differently.
- **Check for `null` items at run time instead** — rejected as the primary
  mechanism: it costs a check per item in every stage and reports the mistake when
  the pipeline runs, not when it is written.

## Consequences

- A record type cannot be a nullable reference type (`string?`) or a `Nullable<T>`
  (`int?`). Optional data is modelled inside the record, or filtered out before it
  enters the stream.
- Every downstream generic stage must repeat the constraint on its own type
  parameters to satisfy the base type. Composition across the family then never hits
  a constraint mismatch.
- The constraint is checked at compile time only. A violation is warning `CS8714`,
  and only in a nullable-enabled context; it is an error where warnings are treated
  as errors. A consumer with nullable analysis disabled gets no diagnostic, and the
  constraint alone does not stop a `null` item at run time.
- `IExtractWithProgressAsync<TSource, TProgress>` does not constrain `TProgress`,
  unlike the other progress interfaces. This ADR records what is in force and does
  not change that.

*Recorded after the fact: the constraint was already in force when this ADR was
written.*
