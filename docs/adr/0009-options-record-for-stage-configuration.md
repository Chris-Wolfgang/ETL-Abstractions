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
| `ETL-Xml` | Options records (2 types); on 3 ctors the options parameter is still positional and required |
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
  is an unavoidable binary break with no possible shim, whereas an options-record
  constructor *can* be added without breaking anything, provided the parameterless
  constructor is kept explicitly (see "Two constructors" under Decision) — was
  explicitly **set aside** as a deciding factor. It is recorded here because it
  still governs *sequencing*, not because it chose the model.

## Decision

We will configure every extractor, loader, and transformer with a **record of
`{ get; init; }` properties passed to the constructor**, rather than with init-only
properties declared on the stage itself.

**Seal at the leaves; leave open any record meant to be inherited.** Sealing a record
nothing derives from is worth doing — a record's synthesized `Equals` compares
`EqualityContract`, so an unsealed record has subtler equality semantics than most
readers expect. But `sealed` blocks a record from being *derived from*, not from
*deriving*: `public sealed record DbExtractorOptions : ExtractorOptions` is both legal
and the shape we want, so the existing sealed records across the fleet need no change
to adopt a base.

Two kinds of record are therefore open rather than sealed:

- the per-stage-kind base records (`ExtractorOptions`, `LoaderOptions`,
  `TransformerOptions`) that carry the members every stage shares; and
- any record a shape-specific variant derives from — ETL-FixedWidth's
  `FixedWidthExtractorOptions` must be unsealed so `FixedWidthExtractorStreamOptions`
  can add `Encoding` on the `Stream` path only. That is the mechanism reason 1 below
  depends on, so it is not optional.

That makes ETL-FixedWidth a three-level chain (`ExtractorOptions` →
`FixedWidthExtractorOptions` → `FixedWidthExtractorStreamOptions`). Deliberate, but
worth knowing: record equality and `with` across three levels are where the sharp
edges live.

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

### The base stages get their own records, one per stage kind

`ExtractorBase`, `LoaderBase` and `TransformerBase` each carry the same four
configuration members (`ReportingInterval`, `MaximumItemCount`, `SkipItemCount`,
`ErrorPolicy`). They move into `ExtractorOptions`, `LoaderOptions` and
`TransformerOptions` respectively, with their validation moved into the records'
`init` accessors, and each base class gains a constructor that accepts its record.
A derived record inherits the matching base record, so a caller configures the whole
stage — base members included — through one object.

**One record per stage kind, not a shared `StageOptions`.** The type system forces
this; it is not a preference. `WorkerResilience` has a different type on every base:
generic over `TSource` on `ExtractorBase`, non-generic on `LoaderBase`, generic over
`TDestination` on `TransformerBase`. No single record can carry it.

**`WorkerResilience` stays on the stage.** It is the only generic member. Putting it
in the records would make every base record generic, and therefore every derived
record in the fleet — `DbExtractorOptions<TRecord>` — reversing Etl-DbClient's
documented decision that its records are not generic, and costing callers an
explicit type argument at every construction, since record creation cannot infer
one. It is a resilience strategy rather than a setting, the same category as
`ILogger`, and it is already `{ get; init; }`, so it has none of the mid-run-mutation
problem this ADR exists to remove. It remains an init-only property on the stage.

**Two constructors, not one with an optional parameter.** The obvious shape,
`protected ExtractorBase(ExtractorOptions? options = null)`, is wrong. Any explicit
constructor removes the implicit parameterless one, and `ExtractorBase() -> void` is
a *shipped* API member. Source still compiles — an implicit `base()` binds to the
optional-parameter form — but every precompiled derived assembly in the fleet calls
`ExtractorBase::.ctor()` and would throw `MissingMethodException`. `RS0017` catches
it. The correct shape keeps the shipped member explicitly and makes the options
parameter required:

```csharp
protected ExtractorBase() { }
protected ExtractorBase(ExtractorOptions? options) { … }
```

This is what "additive" means in the Context above: additive *because* the
parameterless constructor is retained, not additive by nature.

### `IsDryRun` follows the rule: it lives in the options record

We will **narrow `ISupportDryRun.IsDryRun` to `{ get; }`**. It is declared
`{ get; set; }` today, which forces every implementer to expose a setter — the one
mutable knob the rule above could not remove, because neither a get-only property
nor an `init` accessor can satisfy a `set` interface member (CS8854).

With the interface narrowed, `IsDryRun` is configured exactly like every other
setting: the **derived options record** of each stage that implements
`ISupportDryRun` carries `IsDryRun { get; init; }`, and the stage exposes
`public bool IsDryRun { get; }`, assigned once in its constructor from that record.

```csharp
new DbLoader<T>(conn, sql, new DbLoaderOptions { IsDryRun = true })
loader.IsDryRun = true;   // does not compile
```

**On the derived record, not the base.** `ISupportDryRun` is opt-in. A member on
`LoaderOptions` would give every loader the knob whether or not it honours it —
the settable-but-inert footgun reason 1 exists to prevent. Today all six loaders
implement the interface and no extractor or transformer does.

**One write path.** An earlier revision of this ADR also kept `{ get; init; }` on the
stage, so the value could be set by an object initializer on the loader as well as
through the record, and justified the duplication as a "modreq-free path" around
the `IsExternalInit` cross-TFM hazard. That justification was mistaken: a record's
`init` accessor emits `set_IsDryRun` with the same `modreq(IsExternalInit)`, so
`new DbLoaderOptions { IsDryRun = true }` is exposed identically. The only
modreq-free route is a constructor *parameter*, which the fleet's records do not
offer. With no safety argument left, the second path bought nothing but a
precedence rule ("the object initializer wins because it runs after the
constructor") that every reader would have had to carry. It is withdrawn. The
object-initializer spelling survives — on the record, where every other setting's
already is.

The `IsExternalInit` hazard itself is worth stating plainly, since the withdrawn
argument mis-described it. It is a property of **every `init` record in the fleet**
— all eleven existing ones ship this way — not of `IsDryRun`. It bites a
`netstandard2.0`-compiled consumer that resolves the net6.0/net7.0 assembly at
runtime (both out of support), it shipped as a real defect in
`Wolfgang.Etl.TestKit` 0.11.0, and it is pre-existing: this ADR neither introduces
nor fixes it. A parameterised record constructor would, and is a separate decision.

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
  table. A `set` accessor permits assignment *at any time*, preserving mid-run
  mutation for this one member, which is the very thing this ADR exists to remove.
- **`{ get; init; }` on the stage as well as in the record** — adopted in an earlier
  revision of this ADR to keep an object-initializer spelling on the loader, then
  withdrawn. Its "modreq-free path" justification was mistaken (see the `IsDryRun`
  section), leaving two write paths and a precedence rule with no safety gain. The
  record's own object initializer provides the same ergonomics.

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
- Narrowing the interface **enables** the fix; it does not deliver it. Until each
  repo moves `IsDryRun` into its options record and exposes it as `{ get; }`, the
  interface documents a read-only contract that every implementation still
  contradicts with a live `set` accessor. That transitional state is what makes the
  interface change non-breaking for implementers, but it is not a resting place: if
  the adopting issues stall, the fleet has paid for a breaking change and received a
  documentation promise. Each loader's move to `{ get; }` is the real deliverable.
- Each loader's move to `{ get; }` is itself a second break, distinct from the
  interface change and wider in reach — it removes the setter that precompiled
  consumers call, object-initializer users included. Sequence it into the same
  release as that repo's options-record adoption rather than shipping two
  recompile-forcing releases.

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

The base-stage properties (`SkipItemCount`, `MaximumItemCount`, `ReportingInterval`,
`ErrorPolicy`) gain a record-based construction path under this ADR, but their
existing `{ get; set; }` accessors are **not** deprecated or removed here. That
follow-on — deprecate, then remove, on the same staged schedule as the derived
types — is tracked in #351 and #438.

With the records in place, #438's hardest problem largely dissolves. Its blocker was
that the shipped contract tests assert validation by *assigning* to the property,
and could not be rewritten against `init` without a generic `new()` constraint that
cannot be added. A concrete record needs no such constraint: a test can write
`Assert.Throws<ArgumentOutOfRangeException>(() => new ExtractorOptions { ReportingInterval = 0 })`
directly, with no factory change and no threading of values through an abstract
`CreateSut`. And `ReportingInterval`'s live defect — read once at `timer.Start`,
silently inert thereafter — stops being a defect once the property is
construction-only, which argues for not letting that follow-on sit.
