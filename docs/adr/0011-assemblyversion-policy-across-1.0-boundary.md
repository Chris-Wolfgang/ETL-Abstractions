# ADR 0011 — `<AssemblyVersion>` policy across the 1.0 boundary and for new libraries

- **Status:** Accepted
- **Date:** 2026-08-04
- **Deciders:** Chris Wolfgang
- **Amends:** [ADR-0007](0007-pin-assemblyversion.md) (which stays in force for the existing packages)

## Context

[ADR-0007](0007-pin-assemblyversion.md) pins `<AssemblyVersion>` at `1.0.0.0` for
these packages while the NuGet `Version` moves through `0.x`. That keeps
`net462` consumers off the binding-redirect treadmill on every release. Two
questions were left open:

1. What happens to the pin when the packages actually reach **1.0.0**?
2. Should a **new** Wolfgang.Etl library copy the pin, or track the version?

The subtlety is that `<AssemblyVersion>` is part of strong-name **binding
identity on .NET Framework** but is largely ignored by .NET 5+/.NET Core. So a
binary-breaking change (e.g. the `int→long` counter change, or a `set`→`init`
flip — see the modreq trap) is invisible at bind time when the AssemblyVersion is
frozen: a `net462` consumer that upgrades the *package* without recompiling gets a
runtime `MissingMethodException` instead of a load-time version mismatch. Freezing
AssemblyVersion during `0.x` — a phase where SemVer *permits* breaking changes on
every minor — trades that load-time signal away.

## Decision

**Existing packages (Abstractions, ErrorPolicies, TestKit, TestKit.Xunit):** keep
the `1.0.0.0` pin from ADR-0007. **Do not retrofit** a `0.x`-based AssemblyVersion
now — dropping `1.0.0.0` → `0.22.0.0` is a numeric *downgrade* that breaks
existing `net462` binding redirects and can fail to load. The pin becomes
semantically correct at the 1.0.0 release, and from 1.0 onward the policy is
`Major.0.0.0` (all `1.x` share `1.0.0.0`; a breaking `2.0.0` bumps to `2.0.0.0`).

**New libraries — depends on the target set:**

- **Multi-targets .NET Framework (`net46x`/`net48x`):** set
  `<AssemblyVersion>0.{Minor}.0.0</AssemblyVersion>` during `0.x` — the minor is
  the SemVer "breaking" position pre-1.0, so a breaking release bumps the assembly
  identity and `net462` consumers get an actionable **load-time** signal (add a
  redirect / re-reference) instead of a surprise runtime `MissingMethodException`.
  Switch to `Major.0.0.0` at 1.0.
- **Modern .NET only (`net5.0`+ / `netstandard`):** **pin** it (e.g. `1.0.0.0`).
  AssemblyVersion binding is lax there, so the signal buys nothing and the pin is
  simpler.

`<AssemblyVersion>` is centralized in `Directory.Build.props`, so the choice is a
one-line change per repo. The `0.{Minor}.0.0` form is derived from `$(Version)`:

```xml
<AssemblyVersion>0.$([System.Version]::Parse($(Version.Split('-')[0])).Minor).0.0</AssemblyVersion>
```

## Alternatives considered

- **Pin everywhere, including new netfx libraries** — simplest, but breaking `0.x`
  changes stay invisible at bind time on `net462`, surfacing as runtime
  `MissingMethodException`. Acceptable only when netfx consumers are not a concern.
- **AssemblyVersion = the full NuGet version** (`0.22.0.0`, bumped every release) —
  maximal clarity but forces a binding-redirect update on every patch, including
  non-breaking ones. Overkill: patches don't break.
- **Retrofit the existing packages to `0.{Minor}.0.0` now** — rejected: it is a
  numeric downgrade from the shipped `1.0.0.0` and breaks existing `net462`
  binding, for a signal that only matters going forward.

## Consequences

- The 1.0.0 release of the existing packages is a clean, no-op AssemblyVersion
  transition (it was already `1.0.0.0`); the pin simply becomes correct.
- New netfx-targeting libraries give their consumers honest, load-time breakage
  signals during `0.x`, at the cost of redirect updates on breaking minors — which
  is the correct cost, because those releases genuinely broke.
- Modern-.NET-only libraries stay on the simple pin.
- The policy is a single centralized property per repo.
