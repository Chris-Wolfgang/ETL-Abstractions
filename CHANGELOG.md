# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **Convenience base classes (#344):** `ExtractorBase<TSource>`, `LoaderBase<TDestination>`, and
  `TransformerBase<TSource, TDestination>` fix the progress type to the built-in `Report` and supply a
  default `CreateProgressReport()`, so a component that doesn't need a custom progress-report type
  implements only its worker method — no progress record and no `CreateProgressReport` override.
  Override it to enrich the report (for example a known total). The existing two/three-type-parameter
  bases are unchanged. Additive.

### Changed

### Deprecated

### Removed

### Fixed

### Security

## [0.20.0] - 2026-07-30

Minor release: a dependency-free retry seam, composable per-item middleware, and pipeline-wide
error aggregation on the base stages. Purely additive — no breaking change (validates against the
0.19.0 baseline).

### Added

- **`IReportsItemErrors` (#335):** a small interface (`int CurrentErrorItemCount { get; }`) implemented
  by `ExtractorBase`, `LoaderBase`, and `TransformerBase`, letting a pipeline read any stage's
  error-item count uniformly regardless of concrete type.
- **Middleware / interceptor (#93):** a composable per-item hook — `IItemMiddleware<T>` returning
  `MiddlewareResult<T>` (`Continue` to keep/replace an item, `Drop` to remove it) — attached to any
  stream with the `WithMiddleware(...)` extensions (single or ordered chain). Lets cross-cutting
  concerns (logging, validation, metrics, throttling, dedup) decorate an extractor/transformer output
  or loader input without changing the component, and composes inside an `EtlPipeline` via
  `Through(s => s.WithMiddleware(...))`. Dependency-free.
- **Retry seam (#94):** `ExtractorBase`, `LoaderBase`, and `TransformerBase` gained a
  `protected virtual WrapWorkerExecution(...)` hook wrapped around every worker invocation. The
  default implementation is a no-op, so behaviour is unchanged; override it to run the worker through
  a retry / resilience strategy. The override receives a re-invocable worker factory (call it again to
  retry) and stream-level semantics are documented on the method. Kept dependency-free — a ready-made
  Polly integration will ship as a separate opt-in `Wolfgang.Etl.Polly` package (#332).

### Changed

- **`EtlPipelineProgress.ErrorItemCount` now aggregates every stage (#335).** Previously it reported
  only the extractor's error-item count; it now sums the error-item counts of the source, every
  transformer, and the loader (each stage that implements `IReportsItemErrors`), so an item any
  stage's error policy discarded is reflected in the total. Pre-1.0 behaviour change.
- _Internal (no public API change):_ the base classes' `StartedAt` / `Elapsed` timing now reads
  through an injectable time source (#338), so downstream test kits can drive the `Report`
  throughput / ETA metrics from a fake clock (unblocks ETL-Test-Kit#262).

## [0.19.0] - 2026-07-29

Minor release: makes the `EtlPipelineProgress` record counters overflow-safe.

### Changed

- **Breaking (#285):** `EtlPipelineProgress`'s counters — `ExtractedItemCount`, `LoadedItemCount`, and
  `ErrorItemCount` — are now `long` instead of `int`, so a long-running pipeline can report more than
  `int.MaxValue` (~2.1 billion) records without overflow. This changes the record's getters, positional
  constructor, and `Deconstruct` from `int` to `long`; Package Validation against the 0.18.1 baseline
  waives the change via `CompatibilitySuppressions.xml`. `AssemblyVersion` stays pinned at `1.0.0.0`.
  Pre-1.0.

## [0.18.1] - 2026-07-27

Patch release. Purely additive — no breaking change.

### Added

- `Report(int currentItemCount, DateTimeOffset? startedAt, TimeSpan elapsed, int? totalItemCount = null)`
  constructor, a cross-assembly-safe way to build a `Report` with its timing/total snapshot values.

### Changed

- The `Report.StartedAt` / `Elapsed` / `TotalItemCount` documentation now steers cross-assembly
  callers to the new constructor instead of the object-initializer (`init`) form, and the AOT
  smoke test builds its report through the constructor.

### Fixed

- A `netstandard2.0`-compiled consumer that set `Report`'s `StartedAt` / `Elapsed` /
  `TotalItemCount` via the object-initializer (`init`) form and then ran on **.NET 6 or .NET 7**
  (which resolve this package's modern assembly) hit a `MissingMethodException`: an `init`
  setter carries an `IsExternalInit` modreq whose identity differs between the `netstandard2.0`
  polyfill and the built-in `net5.0+` type, so the compiled call did not match the runtime
  method. The new constructor takes the values as plain parameters — its signature is identical
  on every target framework — so consumers can set them without tripping the mismatch. The
  `init` setters are unchanged and remain safe within a single target framework.

## [0.18.0] - 2026-07-25

Minor release: adds an **opt-in per-item error-handling mechanism** (#84) to the
three base stages, so a worker can skip a bad item and keep going instead of
aborting the whole run. Also renames the `EtlPipelineProgress` counters for a
consistent `...ItemCount` naming (see *Changed* — a small, pre-1.0 breaking
change bundled here while `EtlPipelineProgress` is only days old).
`AssemblyVersion` remains `1.0.0.0`.

### Added

- `ItemErrorAction` (`Abort`, `Skip`) and `ItemErrorContext` (item number,
  exception, and an optional lazy raw-content accessor) describing a failed item.
- `ExtractorBase`, `LoaderBase`, and `TransformerBase` each gain a protected
  `HandleItemError(ItemErrorContext)` helper — call it from a worker's `catch`
  block and re-throw when it returns `Abort` — plus a `virtual OnItemError`
  policy hook (default `Abort`) that a derived stage overrides to surface its own
  error-handling knob.
- `CurrentErrorItemCount` on each base stage, counting items discarded by an
  error-`Skip`. It is kept distinct from `CurrentSkippedItemCount` (intentional
  skip-budget skips) so a failure is never silently absorbed into the skip total.
- `EtlPipelineProgress.ErrorItemCount`, surfacing an extractor's error-item
  count in the pipeline progress snapshot.

### Changed

- **Breaking:** renamed the `EtlPipelineProgress` counters for a consistent
  `...ItemCount` scheme — `RecordsExtracted` → `ExtractedItemCount` and
  `RecordsLoaded` → `LoadedItemCount` (the new error counter is `ErrorItemCount`,
  not `RecordsErrored`), aligning them with the base stages' `Current*ItemCount`. `EtlPipelineProgress` was introduced in 0.16.0; renaming now,
  while adoption is negligible, is far cheaper than doing it later. Package Validation
  against the 0.17.0 baseline waives the two removed accessors via
  `CompatibilitySuppressions.xml`.

## [0.17.0] - 2026-07-24

Minor release: a new **use-after-dispose contract** plus a substantial testing-depth
pass. The only behavioural change is the dispose guard described under *Changed* —
no public signatures were added, removed, or altered, so Package Validation passes
against 0.16.1 and no binding redirect is needed (`AssemblyVersion` remains
`1.0.0.0`).

### Changed

- **Using a disposed component now throws `ObjectDisposedException`.** Every public
  entry point on `ExtractorBase`, `LoaderBase`, and `TransformerBase`
  (`ExtractAsync` / `LoadAsync` / `TransformAsync`, all overloads) rejects calls made
  after `Dispose()` or `DisposeAsync()` instead of silently running. **Upgrade note:**
  code that reused a component after disposing it previously "worked" and will now
  throw — construct a new instance per run instead. `Dispose` remains idempotent.

### Added

- **Mutation testing is now a release gate.** Stryker runs on every PR with a
  `break` threshold; the suite's mutation score is **~97%**. Coverage added along the
  way pins down per-run counter/timestamp reset, `StartedAt`/`Elapsed` semantics,
  dispose-stage behaviour across all `Extract()` overloads, aggregate disposal
  failures, and exact exception messages.
- **The library is verified context-agnostic.** A counting `SynchronizationContext`
  test proves no internal await marshals a continuation back to the caller's context,
  so `.Result`/`.Wait()` from a UI or legacy-ASP.NET thread cannot deadlock on it.
- **Concurrency stress testing** with Microsoft Coyote, exercising the base classes'
  interleavings for races in the counter/timestamp machinery.
- **Allocation-free hot-path verification** — the documented per-record and
  per-progress-tick members are asserted to allocate zero bytes per call.
- **Long-running GC / allocation profiling** (`tools/GcProfileWorkload`) to catch
  steady-state leaks and Gen2 growth over sustained runs.
- **End-to-end SourceLink debug-step-into verification**, so consumers can step from
  their code into this library's sources.
- **Consumer-side reproducible-build verification** — releases publish a manifest of
  each shipped assembly's SHA-256 so a third party can rebuild from the tag and
  confirm byte-for-byte equality. See [`docs/REPRODUCIBLE-BUILD.md`](docs/REPRODUCIBLE-BUILD.md).

### Fixed

- De-flaked `SystemProgressTimerTests.StopTimer_prevents_further_callbacks`, which
  could fail on a slow or loaded runner.

### Security

- **SLSA build-provenance attestation** is now generated for release artifacts, in
  addition to the SBOM that already shipped. (Package signing remains outstanding —
  tracked in #208.)

## [0.16.1] - 2026-07-21

Patch release: maintenance, testing, and supply-chain hardening. **No API or
behavioural change** — the compiled assembly is identical to 0.16.0; consumers can
upgrade without any code change and without a binding redirect (`AssemblyVersion`
remains `1.0.0.0`).

### Added

- Verified **Native-AOT and trim** compatibility — a `PublishAot` + `PublishTrimmed`
  smoke consumer is published for `linux-x64` and run on every PR, so AOT/trim
  regressions are caught before they reach a consumer's published app.
- Verified **globalization / `CultureInfo` invariance** — the suite now runs under
  `tr-TR`, `de-DE`, `zh-CN`, `ar-SA`, and `ja-JP` in addition to `en-US`.
- Architecture Decision Records under [`docs/adr/`](docs/adr/index.md), a
  migration-guide convention under `docs/migrations/`, and a "Release path &
  compromise scope" appendix in `SECURITY.md`.

### Security

- **ABI-compatibility gate**: Package Validation now fails the pack if a non-major
  release breaks binary/API compatibility against the previously published version.
- All GitHub Actions are pinned to commit SHAs, and a workflow-security audit
  (zizmor + actionlint) runs on every PR to keep them pinned and hardened.

## [0.16.0] - 2026-07-20

Minor release: adds a generic, format-agnostic ETL pipeline. No breaking change.

### Added

- `EtlPipeline` — a generic, format-agnostic pipeline that composes any source, transformer
  stages, and a loader into a single runnable flow, complementing the existing fluent
  `Pipeline` (Extract/Transform/Load) builder:
  - `EtlPipeline.Create()` returns a fresh builder seed; `From(IAsyncEnumerable<T>)` and
    `From(ExtractorBase<T, TProgress>)` factories start the chain from any source. Format
    packages extend the `EtlPipeline` instance with class-named source factories, e.g.
    `EtlPipeline.Create().CsvExtractor<Order>("orders.csv")`.
  - `IEtlPipeline<T>` with `Through` (four overloads — an `ITransformAsync<T, TOut>` or
    `ITransformWithCancellationAsync<T, TOut>` transformer, or a stream-to-stream delegate,
    with or without a `CancellationToken`), `To<TProgress>(LoaderBase<T, TProgress>)`, and
    `AsAsyncEnumerable()`.
  - `IEtlPipelineSink.RunAsync(IProgress<EtlPipelineProgress>?, CancellationToken)` and the
    `EtlPipelineProgress` record (`RecordsExtracted`, `RecordsLoaded`, `Elapsed`).
  - The pipeline is lazy and streaming. The LINQ-flavored operators (`Where`, `Select`, …) are
    provided separately by `Wolfgang.Etl.Transformers` as extension methods layered over
    `Through`, so the core takes no dependency on them.

## [0.15.0] - 2026-06-28

Minor release: adds an opt-in dry-run contract. No breaking change.

### Added

- `ISupportDryRun` — an opt-in interface exposing `bool IsDryRun { get; set; }` for
  ETL stages that support a dry run: the full pipeline is exercised but the external
  side effect that mutates a destination or source is skipped. Implemented by the
  stage that honours it (not by the base classes). (#259)

## [0.14.1] - 2026-06-25

Patch release: a robustness fix and documentation accuracy. No public API change.

### Changed

- **Docs** — README corrected: generated HTML is written to
  `docfx_project/_site/` and published to the `gh-pages` branch (the `docs/`
  folder holds supplementary markdown guides, not generated output). Added the
  v0.14.0 `Report` timing/throughput, disposal, and per-run-reset capabilities
  to the Features table and Quick Start. (#254)

### Fixed

- `Report.EstimatedRemaining` no longer throws `OverflowException` for a
  pathologically low throughput (a single item after a very long elapsed time
  with a large total); the projected estimate is clamped to
  `TimeSpan.MaxValue`. (#255)

## [0.14.0] - 2026-06-24

Adds timing/throughput reporting and resource-lifecycle management to the
base classes. MINOR per SemVer — additions are source- and binary-additive,
with one behavioral change (per-run counter reset) and one binary-sensitive
addition: the base classes now implement `IDisposable`/`IAsyncDisposable`,
so consumers that wrap a component in a `using` will now dispose it.

### Added

- `Report` now surfaces timing and throughput metrics: `StartedAt`,
  `Elapsed`, `TotalItemCount`, `ItemsPerSecond`, `PercentComplete`, and
  `EstimatedRemaining`. (#144, #91)
- `ExtractorBase`, `LoaderBase`, and `TransformerBase` implement
  `IAsyncDisposable` and `IDisposable`, with overridable `DisposeAsync()`,
  `Dispose()`, and `Dispose(bool disposing)` so derived components can
  release resources deterministically. (#92)
- Protected `StartedAt` and `Elapsed` members on the three base classes,
  populated automatically once the first item is processed. (#144)

### Changed

- Per-run counters and timing now reset at the start of each enumeration,
  so a reused extractor, loader, or transformer reports the current run
  rather than cumulative totals across runs. (#246)

## [0.13.1] - 2026-06-19

Canonical maintenance round + binding-stability fix. No public API or
runtime behavior change vs v0.13.0. This release is the prerequisite
the downstream ETL family (`ETL-Test-Kit`, `ETL-Xml`, `ETL-Json`,
`ETL-FixedWidth`, `Etl-DbClient`, the in-development
`ETL-Csv`/`ETL-SqlBulkCopy`/`ETL-Transformers`) consumes by NuGet
reference — bumping it first lets each downstream pilot inherit the
canonical fixes cleanly rather than each fighting mixed-state
dependencies.

### Added

- **D8** — `verify-docs-build` job in `release.yaml` runs DocFX during
  the release pipeline before the NuGet push, so a docs build failure
  now blocks the package from shipping.
- **D8** — docs site version picker assets
  (`docfx_project/public/version-picker.js`,
  `docfx_project/versions.json`,
  `docs/DOCFX-VERSION-PICKER.md`).
- **A1** — `PublicApiAnalyzers` scaffolding (analyzers activate when
  `PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt` are present
  alongside the csproj).
- **CI3** — canonical NuGet package metadata: `Authors`, `Copyright`,
  `RepositoryType`, SourceLink, snupkg symbol packages, deterministic
  CI build flag, and `EmbedUntrackedSources` hoisted to
  `Directory.Build.props`.
- **T3** — Stryker mutation-testing workflow (`stryker.yaml`).
- **T1** — coverage report published to docs site.
- **S1** — CodeQL `security-extended` query pack.
- **D6** — versions.json preservation guard on the docs deploy.

### Changed

- **C1** — fleet-wide template-drift sync: workflow files (`pr.yaml`,
  `release.yaml`, `docfx.yaml`, `codeql.yaml`,
  `build-all-versions.yaml`, `stryker.yaml`), `.editorconfig`,
  `BannedSymbols.txt`, `Directory.Build.props`, and per-context
  `tests/Directory.Build.props` consolidated to the canonical baseline.
- **Nullable** — `<Nullable>enable</Nullable>` consolidated into
  `Directory.Build.props` (was per-csproj); per-project opt-out via
  override still supported.
- **CI2** — Dependabot `github-actions` ecosystem added.
- **D3** — repo scripts hardened (`Setup-Labels.ps1`,
  `Fix-BranchRuleset.ps1`).
- `github/codeql-action/init` and `analyze` bumped v3 → v4 (Node.js
  20 → 24 deprecation).
- **Docs** — README accuracy pass: corrected the Target Frameworks
  table (dropped the untargeted .NET 4.7.0 / 4.7.1 rows, added the
  missing .NET Standard 2.0 row), analyzer count 7 → 8
  (`Microsoft.CodeAnalysis.PublicApiAnalyzers`), and the build
  prerequisite (.NET 8.0 → .NET 10.0 SDK). `CONTRIBUTING.md` analyzer
  list updated to match.

### Removed

- `REPO-INSTRUCTIONS.md` — the repo-template post-setup bootstrap
  checklist ("once you have completed the checklist below you can
  delete this file"); setup is long complete.

### Fixed

- **Docs** — corrected stale XML-doc `<example>` references found in a
  code-review pass: `LoaderBase` / `TransformerBase` examples referenced
  a non-existent `MaxItemCount` (corrected to `MaximumItemCount`), and
  `SystemProgressTimer` pointed at a non-existent `ManualProgressTimer`
  type (corrected to a resolvable `IProgressTimer` reference).
- **C4** — restored explicit `<AssemblyVersion>1.0.0.0</AssemblyVersion>`
  and added a prerelease-safe `<FileVersion>` (regex-strip property
  function) to the src csproj. The original C4 fanout had dropped
  these on the rationale that the hardcoded values were "stale"
  relative to released package versions — but that staleness was the
  correct binding-stability behaviour for libraries that ship a
  `net462` TFM. Without an explicit pin, SDK-derived `AssemblyVersion`
  would change on every minor/patch release, breaking .NET Framework
  consumers without a binding redirect. (See DateTime-Extensions v1.3.1
  for the post-mortem on what happens when this regression reaches a
  release.)
