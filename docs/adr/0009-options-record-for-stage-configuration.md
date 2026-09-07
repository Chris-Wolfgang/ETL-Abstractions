# ADR 0009 — Configure stages with an options record, not properties on the stage

- **Status:** Accepted
- **Date:** 2026-09-05
- **Deciders:** Chris Wolfgang

## Context

The `ETL-*` family had drifted into **two incompatible configuration idioms**, and
new work kept having to pick one without a recorded reason.

| Repo | Model as of 2026-09-05 |
|---|---|
| `Etl-DbClient` | Options record (`DbExtractorOptions` — 8 init props, `DbLoaderOptions` — 9), mid-migration: 18 setters and 7 ctors already `[Obsolete]` |
| `ETL-FixedWidth` | Options records (7 types); loose-parameter ctors `[Obsolete]` |
| `ETL-Xml` | Options records (2 types); 3 ctors still take options positionally-required |
| `ETL-Json` | BCL `JsonSerializerOptions` threaded through; no repo-authored record; 7 live setters |
| `Etl-Csv` | Options records (`CsvExtractorOptions` 17 props, `CsvLoaderOptions` 15); all 37 setters already `[Obsolete]`, none live |
| `ETL-SqlBulkCopy` | Mutable properties on the stage — 13 live setters, none deprecated |
| `ETL-Transformers`, `ETL-Abstractions` | No stage-level configuration |

Three open ETL-FixedWidth issues proposed *different* answers to the same
question (#299 init-only properties, #341 split options records, #342
construction-only config), which is what forced the decision.

Two forces shaped it:

- **Configuration must be fixed before a run.** These are knobs meant to be set
  once, not mutated while a pipeline enumerates. Both candidate models deliver
  that; the disagreement was about *where* the knobs live.
- **Pre-1.0 breaking changes are acceptable.** The migration-cost asymmetry
  between the two models — flipping a shipped `{ get; set; }` to `{ get; init; }`
  is an unavoidable binary break with no possible shim, whereas adding an
  options-record constructor is purely additive — was explicitly **set aside** as
  a deciding factor. It is recorded here because it still governs *sequencing*,
  not because it chose the model.

## Decision

We will configure every extractor, loader, and transformer with a **`sealed record`
of `{ get; init; }` properties passed to the constructor**, rather than with
init-only properties declared on the stage itself.

Three reasons, none of which depend on backward compatibility:

**1. A record can express which options are valid for which input shape.**
`Encoding` is meaningful when a stage is built from a `Stream` and meaningless
when it is built from a `TextReader`, which has already fixed its encoding.
ETL-FixedWidth 0.11.0 removed this footgun from four types by taking `Encoding`
*off* the stage — it exists today only as a constructor parameter and as an init
property on the options record. Declaring configuration on the stage puts it back:
every property exists on every instance regardless of which constructor ran, so
shape-specific options become settable-but-silently-inert. A record hierarchy
(base record for the shape-agnostic settings, derived record adding the
shape-specific ones) makes the invalid combination a **compile error**.

**2. Cross-property validation has somewhere to run.** `init` accessors fire
individually, after the constructor, with no "configuration complete" hook. A rule
such as *"`ServerLimit` requires a `PagingClauseTemplate`"* — which `Etl-DbClient`
genuinely has — therefore cannot be checked at construction under stage-level
init-only properties; it has to be deferred to first enumeration.
`DbExtractor.EnsurePagingClauseTemplateChosen()` exists as exactly that
workaround. A record handed to a constructor is validated once, in one place, and
fails where the mistake was made.

**3. Builders can accumulate partial configuration.** A record composes
incrementally with `with` expressions, and anything left unset keeps the record's
own default. An object initializer cannot conditionally include a member, so a
builder targeting stage-level init-only properties must assign *every* property
*always*, from its own copy of every default — duplicating defaults and letting
the builder silently override the stage's. Both models require the existing
fluent builders to be rewritten; only this one has a sound target.

**Where a stage already threads a third-party options type** — `ETL-Json` and its
`JsonSerializerOptions` — the repo-authored record **carries** that type as one of
its properties rather than sitting beside it as a second constructor parameter. A
constructor taking two different options objects is a confusing signature and
leaves the caller guessing which one owns a given setting. One options parameter
per stage, always.

We will also **narrow `ISupportDryRun.IsDryRun` to `{ get; }`**. It is declared
`{ get; set; }` today, and an `init` accessor cannot implement a `set` interface
member (CS8854), so the interface currently forces one mutable knob onto every
loader. Narrowing it lets `IsDryRun` become a read-only projection of
`options.IsDryRun` with no second write path.

## Alternatives considered

- **Init-only properties on the stage** — rejected on the three points above. It
  is genuinely better on ergonomics: one concept instead of two, no extra type per
  stage (ETL-FixedWidth would go from 7 options types toward ~11 under its split
  design), full IntelliSense discoverability from the stage itself, and no
  property duplication between record and stage. Those costs are accepted
  deliberately: shape-validity and
  construction-time validation are correctness problems in code that already
  exists, whereas the ergonomic losses are friction.
- **Keep both idioms, per repo** — rejected explicitly. It is the zero-cost option
  today, but it leaves the family with two ways to configure a stage and leaves
  ETL-FixedWidth #299/#341/#342 unresolved indefinitely.
- **Leave `ISupportDryRun` as `{ get; set; }`** and seed it from the record while
  keeping the interface-mandated setter — rejected once narrowing was on the
  table. It works, but leaves `IsDryRun` as the single knob with two write paths
  and preserves mid-run mutation for it alone.

## Consequences

**Obligations this creates**

- Every stage gains a companion options record. `ETL-SqlBulkCopy` is the only
  repo with no record at all (13 live setters). `Etl-Csv` already completed this
  migration in 0.7.x — records exist and all 37 setters are `[Obsolete]` — so it
  needs only the removal stage, not adoption. `Etl-Csv#200`, which kept
  property-based configuration, was already superseded by that work and is not
  reversed by this ADR.
- The fluent builders must be rewritten to accumulate into a record and construct
  once. ETL-FixedWidth #342 records that this is where a prototype silently lost
  an `IsDryRun` assignment: it compiled clean under warnings-as-errors and only a
  test caught that a "dry run" had written real output to a real file. Any
  implementation must keep a test asserting dry-run **through the builder**.
- `ISupportDryRun` narrowing is a breaking change to `Wolfgang.Etl.Abstractions`.
  The blast radius is small and bounded: there is **no polymorphic assignment of
  `IsDryRun` anywhere in production code** across the family — every writer is a
  concrete type, a fluent builder, or `SupportsDryRunContractTests`, which asserts
  the setter in two tests.

**Costs to expect**

- Deprecating the existing setters makes every internal use a build error
  immediately, because this fleet builds Release with `TreatWarningsAsErrors`.
  ETL-FixedWidth #341 estimates ~240 call sites in that repo alone, and that cost
  lands in the *additive* release, not the removal one.
- Mechanical folding of setters into record initializers is **not** safe to
  automate unreviewed. #341 records that prototyping produced silent
  mis-attributions; a mis-fold between properties present on both the extractor
  and loader records compiles and quietly changes what a test asserts. A green
  build is not sufficient evidence — human diff review is required.
- Two places now describe one setting: an `init` property on the record and a
  read-only getter on the stage. That duplication is intentional (instance reads
  stay convenient) but it is real surface growth.

**Explicitly out of scope**

This ADR governs **stage-level** configuration on derived types. The base-stage
properties on `ExtractorBase` / `LoaderBase` / `TransformerBase`
(`SkipItemCount`, `MaximumItemCount`, `ReportingInterval`, `ErrorPolicy`) are
`{ get; set; }` and remain so for now — see #351 and #438, deferred until the
derived-type work settles. Note that a derived constructor *can* assign a base
`init` property, so the two decisions compose when that work resumes;
`ReportingInterval` additionally has a live defect (read once at `timer.Start`,
silently inert thereafter) that argues for taking it up before long.
