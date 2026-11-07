# ADR 0010 — Pin AssemblyVersion at 1.0.0.0 for binding stability

> Imported from the archived ETL-Test-Kit repository, where it was ADR 2. Renumbered when
> TestKit and TestKit.Xunit moved into this repository so the numbers no longer collide.

## Status

Superseded by [ADR-0007](0007-pin-assemblyversion.md). Since the repository consolidation, ADR-0007 records the same decision for all four packages; this record is kept for history.

## Context

Both packages ship to NuGet and target .NET Framework TFMs (net462, net481) in
addition to modern .NET. On .NET Framework, the CLR binds by the assembly's
**strong `AssemblyVersion`**: if `AssemblyVersion` tracks the package version,
every minor/patch bump changes the bind identity, so a consumer that references
`1.2.0` but resolves `1.3.0` at runtime needs an assembly binding redirect or
fails to load. For a *test-support* library pulled transitively into many test
projects, that friction is disproportionate to the value.

## Decision

We will pin `<AssemblyVersion>` at `1.0.0.0` and let `<FileVersion>` and
`<InformationalVersion>` (derived from `<Version>`) carry the real release
version. `AssemblyVersion` is bumped **only** on a deliberate breaking API change
(a new major), never on a minor/patch release.

## Consequences

- Consumers do not need binding redirects when a minor/patch bump flows in
  transitively — the bind identity is stable across the whole `1.x` line.
- The actual shipped version is still discoverable via file/informational
  version and the NuGet package version.
- The pin is load-bearing: a reviewer must not "fix" `AssemblyVersion` to match
  the package version. A major bump is the only time it moves.
- Binary compatibility within a bind identity is additionally guarded by
  PackageValidation (see the ABI-gate decision and `EnablePackageValidation`).
