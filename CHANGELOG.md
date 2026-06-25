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
