# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

### Changed

### Deprecated

### Removed

### Fixed

### Security

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
