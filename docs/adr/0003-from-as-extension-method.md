# ADR 0003 — `EtlPipeline.Create().From()` with `From` as an extension method

- **Status:** Accepted
- **Date:** 2026-07-21
- **Deciders:** Chris Wolfgang

## Context

The generic pipeline's entry point reads `EtlPipeline.Create().From(source)`.
`Create()` is a static factory on the `sealed class EtlPipeline` (private ctor),
returning an instance so that *source factories* from format packages can hang off
it uniformly — `Create().CsvExtractor<T>(...)`, `Create().From(...)`, all as
`this EtlPipeline` extensions.

The question was how to declare `From` itself. It has three plausible shapes, and
each has a specific compiler or compatibility problem:

- An **instance method** `From` never touches `this` (it just wraps the argument in
  a new pipeline node), so the analyzer flags **S2325 "make static."**
- A **plain static method** clears S2325 but then `Create().From(...)` fails to
  compile — **CS0176**, "cannot access static member through an instance."
- Shipping `From` as an instance method now and converting it to static later
  would be a **binary-breaking change** for `net462`/`netstandard` consumers.

## Decision

We will declare `From` as a **static extension method** on `EtlPipeline` (in
`EtlPipelineSourceExtensions`). An extension method is static under the hood — so
it clears S2325 — yet is invoked with instance syntax, so `Create().From(...)`
compiles and reads naturally. It also sidesteps the binary-break trap: extension
methods are resolved at the call site and carry no instance-vs-static ABI lock-in.

Format-package source factories follow the same `this EtlPipeline` extension
pattern, giving every source a uniform `Create().Xxx(...)` shape.

The downstream builder methods — `Through`, `To`, `AsAsyncEnumerable` — stay
**interface instance methods** on `IEtlPipeline<T>`, because they genuinely use the
node's `_factory` state (no S2325 there).

## Alternatives considered

- **Instance `From`** — rejected: draws S2325 and, once shipped, locks in an ABI
  that a later "make static" refactor would break.
- **Plain static `From`** — rejected: breaks the `Create().From()` call shape
  (CS0176).
- **A static `EtlPipeline.From(...)` without `Create()`** — rejected: format
  packages need an *instance* to hang source factories off of, so the surface would
  be inconsistent (`From` static, `CsvExtractor` instance).

## Consequences

- `From` lives in a separate `EtlPipelineSourceExtensions` class, not on
  `EtlPipeline` — a reader looking at the `EtlPipeline` class alone won't see it;
  this ADR is the pointer.
- The pattern is now the template every format package copies for its own source
  factories, keeping `Create().Xxx()` uniform across the ecosystem.
- `Through`/`To`/`AsAsyncEnumerable` remaining instance methods is deliberate and
  not an inconsistency — they hold state; `From` and the source factories do not.
