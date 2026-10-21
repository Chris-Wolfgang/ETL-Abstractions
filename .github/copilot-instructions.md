# Copilot Coding Agent Instructions

## Repository Summary

This repository builds four NuGet packages for the Wolfgang ETL family:

| Package | Purpose |
|---|---|
| `Wolfgang.Etl.Abstractions` | Base classes (`ExtractorBase`, `TransformerBase`, `LoaderBase`), the stage interfaces, `Report`, the fluent `Pipeline` and the generic `EtlPipeline` builder. Zero third-party runtime dependencies. |
| `Wolfgang.Etl.ErrorPolicies` | Ready-made per-item error policies (`ItemErrorPolicy`) for the stages' `ErrorPolicy` hook. |
| `Wolfgang.Etl.TestKit` | Framework-agnostic test doubles (`TestExtractor`, `TestLoader`, `FaultyExtractor`, ...). |
| `Wolfgang.Etl.TestKit.Xunit` | xUnit contract-test base classes and helpers (`ExtractorBaseContractTests`, `EtlScenario`, `ProgressCapture`, ...). |

Every downstream ETL library (Csv, Json, Xml, FixedWidth, DbClient, SqlBulkCopy, Transformers) depends on
these packages, so public API changes are deliberate and tracked.

## Layout

- `src/` - the four packages. Each has `PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt`.
- `tests/` - unit, contract, concurrency (Coyote), fuzz, doc-example, SourceLink and AOT-smoke projects.
- `benchmarks/` - BenchmarkDotNet project.
- `examples/` - runnable samples for net4.8 and net8.0.
- `docs/` - guides and the ADRs in `docs/adr/`.
- `changelog/unreleased/` - one fragment per change; `CHANGELOG.md` is assembled from them at release time.

## Build and test

```bash
dotnet build -c Release
dotnet test -c Release
pwsh ./scripts/build-pr.ps1      # the same build, tests, coverage gate and scans as pr.yaml
pwsh ./scripts/format.ps1        # apply the repository's formatting rules
```

Coverage gates: 95% line coverage for each production assembly, 100% for each test assembly.

## Conventions

- Warnings are errors. Fix analyzer findings rather than suppressing them.
- Any change to the public surface goes in `PublicAPI.Unshipped.txt` of that package.
- Any change under `src/` adds a fragment to `changelog/unreleased/` (see its README).
- Allman braces, four-space indentation, file-scoped namespaces, three blank lines between members.
- Tests are named `Method_when_condition_expected_result`.
- No synchronous I/O in library code; async all the way.
- Protected files (`.editorconfig`, `Directory.Build.*`, `BannedSymbols.txt`, `*.globalconfig`,
  `.github/workflows/*`, the CI scripts) go in their own pull request; see `docs/WORKFLOW_SECURITY.md`.
