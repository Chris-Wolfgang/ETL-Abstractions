# ADR 0018 — Target .NET Framework 4.6.2 through the current .NET, one build per runtime for the runtime packages

- **Status:** Accepted
- **Date:** 2026-11-17
- **Deciders:** Chris Wolfgang

## Context

ETL code lives in long-lived line-of-business systems. Many of them are still on
.NET Framework 4.x and cannot move to modern .NET on the timetable of a dependency
upgrade. A base library that targeted only current .NET would be unusable in the
codebases that most need it. A library that targeted only `netstandard2.0` would run
everywhere but could not use newer runtime APIs where they exist.

The target framework lists were set long before this ADR, and the only written
reasoning for the matrix was in a downstream repository (ETL-Transformers ADR-0003).

## Decision

We target every runtime a consumer is likely to be on, from `net462` up.

| Packages | Target frameworks |
|---|---|
| `Wolfgang.Etl.Abstractions`, `Wolfgang.Etl.ErrorPolicies` | `net462`, `net472`, `net48`, `net481`, `netstandard2.0`, `net5.0` through `net11.0` |
| `Wolfgang.Etl.TestKit`, `Wolfgang.Etl.TestKit.Xunit` | `net462`, `net481`, `netstandard2.0`, `net5.0`, `net6.0`, `net7.0`, `net8.0`, `net10.0`, `net11.0` |

- **The runtime packages get one build per runtime release**, because they ship into
  production applications on each of those runtimes.
- **The test packages get one build per runtime family.** A `net472` or `net48`
  consumer binds the `net462` build and a `net9.0` consumer binds the `net8.0` build.
- **No package targets `netstandard2.1`.** .NET Core 3.x consumers use the
  `netstandard2.0` build.
- **The unit tests run on every framework in the runtime list**, with `netcoreapp3.1`
  in place of `netstandard2.0` so that the `netstandard2.0` build is exercised too.

## Alternatives considered

- **Current .NET only** — rejected: a simpler build and a smaller test matrix, but it
  excludes the .NET Framework consumers these libraries exist to serve.
- **`netstandard2.0` only** — rejected: one build, but no access to the runtime APIs
  and optimisations available on newer frameworks.
- **The full list for the test packages as well** — rejected: a test package gains
  nothing from a separate build per point release.

## Consequences

- Every target framework is built and tested, so the build and test surface is
  large.
- The public surface is limited to what `netstandard2.0` can express unless a member
  is guarded. The source carries `#if` guards, mostly `NET8_0_OR_GREATER`.
- `IAsyncEnumerable<T>` comes from `Microsoft.Bcl.AsyncInterfaces` on the frameworks
  that do not have it built in.
- The lists include frameworks that are out of support (`net5.0` to `net7.0`, and
  `net9.0`). Recent BCL packages no longer list some of them as tested, so the
  projects set `SuppressTfmSupportBuildWarnings`. Removing a target framework breaks
  the consumers on it, and Package Validation reports it.
- Each new .NET release has to be added to the lists and to the test matrix.
- **The family does not share one list.** When this was written, ETL-Transformers and
  Etl-DbClient used the same list as the runtime packages here; ETL-FixedWidth,
  ETL-Json, ETL-Xml and ETL-SqlBulkCopy used the narrower list of the test packages;
  ETL-Csv used that narrower list with `netstandard2.1` in place of `net481`. This
  ADR records the reasoning for this repository and does not reconcile those.

*Recorded after the fact: the target framework lists were already in force when this
ADR was written.*
