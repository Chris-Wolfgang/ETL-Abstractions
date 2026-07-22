# ADR 0007 — Pin `<AssemblyVersion>` at `1.0.0.0`

- **Status:** Accepted
- **Date:** 2026-07-21
- **Deciders:** Chris Wolfgang

## Context

The package version moves on every release (`0.13.0`, `0.16.0`, …). On .NET
Framework, the CLR binds strongly-named references by **assembly version**: if
`AssemblyVersion` tracked the package version, every release would change it, and
every consumer would need a `bindingRedirect` in `app.config` to load the new
build in place of the one they compiled against. That is a notorious source of
"could not load file or assembly … version mismatch" failures for library
consumers, especially in the `net462`/`net47x`/`net48` targets this package
supports.

`AssemblyVersion`, `FileVersion`, and `InformationalVersion` are three separate
knobs: only `AssemblyVersion` participates in binding; the other two are metadata.

## Decision

We will pin `<AssemblyVersion>` to a fixed `1.0.0.0` binding-stability baseline and
let the moving version numbers live on the metadata knobs: `FileVersion` is derived
from `<Version>` (stripped of any pre-release suffix, with a `.0` revision) and
`InformationalVersion` carries the full `<Version>`. `AssemblyVersion` bumps **only**
on a deliberate breaking API change — the one situation where forcing consumers to
recompile/redirect is the *intended* signal.

## Alternatives considered

- **`AssemblyVersion` == package version** — rejected: forces a binding redirect on
  every release for .NET Framework consumers; maximal friction for no benefit while
  the API is compatible.
- **Auto-increment `AssemblyVersion` per build** — rejected: same redirect problem,
  worse (non-deterministic).

## Consequences

- .NET Framework consumers can drop in a newer patch/minor build without touching
  `app.config`; the reference keeps binding to `1.0.0.0`.
- `FileVersion`/`InformationalVersion` remain the source of truth for "which build
  is this?" in Explorer, logs, and diagnostics.
- The pin is load-bearing: bumping `AssemblyVersion` casually (e.g. to match the
  package version) would silently reintroduce the redirect burden. It moves only on
  an intentional breaking change, and that move is itself a decision worth an ADR.
