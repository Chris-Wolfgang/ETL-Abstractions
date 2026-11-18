# ADR 0016 — Declare the shipped libraries trim- and Native-AOT-compatible, and prove it with a published smoke test

- **Status:** Accepted
- **Date:** 2026-11-17
- **Deciders:** Chris Wolfgang

## Context

The packages in this repository target `net8.0` and later alongside the older
frameworks. A consumer who publishes with `PublishTrimmed` or `PublishAot` gets
`IL2xxx` / `IL3xxx` warnings for every referenced library that does not declare
itself compatible, and a library that reflects over types the trimmer cannot see, or
generates code at run time, fails in the published app rather than at build time.

These packages are the base of the `Wolfgang.Etl.*` family. A downstream library
cannot make a credible trim or AOT claim if the base classes it inherits from make
none.

The shipped code has no reflection over unknown types and no run-time code
generation. The one reflection-shaped member, `PropertySelector.Resolve`, reads an
expression tree to find a `PropertyInfo` and never compiles it.

Two pieces already existed when this ADR was written: a Native AOT smoke test (added
in July 2026) and the compatibility declaration itself (added in September 2026).
Neither had a record of why it is there or what it does and does not cover.

## Decision

We declare compatibility, enforce the declaration at build time, and verify it with
a real native publish.

1. **Declaration.** `Directory.Build.props` sets `IsAotCompatible` and `IsTrimmable`
   for every project under `src/`, on the target frameworks compatible with
   `net8.0`. Projects under `tests/` and `benchmarks/` are exempt.
2. **Enforcement.** `.editorconfig` keeps the `Trimming`, `AOT` and `SingleFile`
   analyzer categories at `warning` while every other category defaults to
   `suggestion`. Release builds treat warnings as errors, so a new reflection or
   dynamic-code use in shipped code fails the build.
3. **Verification.** `tests/Wolfgang.Etl.Abstractions.AotSmoke` is a console app with
   `PublishAot`, `PublishTrimmed` and `TrimmerSingleWarn=false`. It references
   `Wolfgang.Etl.Abstractions` and `Wolfgang.Etl.ErrorPolicies` and exercises both
   pipeline builders, the three base classes, progress reporting, `Report`,
   `PropertySelector` and the error policies. The `aot.yaml` workflow publishes it
   for `linux-x64` and runs the binary on every pull request and on every push to
   the default branch. The workflow has no path filter, so that its job can be a
   required status check.

## Alternatives considered

- **Analyzers only, no smoke test** — rejected: the analyzers see this assembly's
  source, not what the trimmer and the native compiler do with the whole closure.
  Only a publish followed by a run shows that the result works.
- **Smoke test only, no declaration** — rejected: the packages would carry no
  compatibility metadata, so consumers would still get warnings for them, and the
  analyzers would not run on the library projects themselves.
- **Leave the analyzer categories at the default `suggestion` severity** — rejected:
  the declaration would be a claim nothing enforces.

## Consequences

- Shipped code that needs reflection or dynamic code must be redesigned or annotated
  (`RequiresUnreferencedCode`, `RequiresDynamicCode`, `DynamicallyAccessedMembers`).
  An annotation on a public member changes the public surface and is its own
  decision.
- The declaration covers all four packages, but the smoke test references only
  `Wolfgang.Etl.Abstractions` and `Wolfgang.Etl.ErrorPolicies`. `Wolfgang.Etl.TestKit`
  and `Wolfgang.Etl.TestKit.Xunit` are checked by the analyzers only.
- Nothing is claimed for the target frameworks below `net8.0`.
- The native link and run are verified on `linux-x64` only.
- The smoke test runs on every pull request, documentation-only ones included.
- A public API added later is covered by the smoke test only if someone adds a call
  to it there.

*Recorded after the fact: both the declaration and the smoke test were already in
force when this ADR was written.*
