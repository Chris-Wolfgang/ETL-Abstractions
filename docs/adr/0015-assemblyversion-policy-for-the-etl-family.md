# ADR 0015 — `<AssemblyVersion>` policy for the ETL family: `0.{Minor}.0.0` for new libraries, existing pins stay

- **Status:** Accepted
- **Date:** 2026-11-17
- **Deciders:** Chris Wolfgang

## Context

`AssemblyVersion` is part of an assembly's bind-time identity on .NET Framework and
is ignored for binding on .NET 5 and later. Every library in the `Wolfgang.Etl.*`
family targets `net462`, so the value matters to its .NET Framework consumers.

[ADR-0007](0007-pin-assemblyversion.md) pins the four packages in this repository at
`1.0.0.0`, so that a consumer needs no binding redirect on a patch or minor release.
A pin held through the `0.x` line has two costs that ADR-0007 does not weigh:

- **A breaking minor is invisible at bind time.** SemVer allows a breaking change in
  every `0.x` minor. With a frozen `AssemblyVersion`, a .NET Framework consumer who
  drops in the new package without recompiling loads it successfully and then fails
  at run time with a `MissingMethodException`, instead of getting a version mismatch
  at load time that names the problem.
- **The pin spends `1.0.0.0` before 1.0.** A library pinned at `1.0.0.0` ships its
  whole `0.x` history under that identity. When the real `1.0.0` is released, the
  identity does not change, at the release most likely to break a consumer.

The opposite choice, `AssemblyVersion` equal to the package version, forces a binding
redirect on every patch release for no behavioural reason. That is the cost ADR-0007
was written to avoid.

State of the family when this was written:

| `AssemblyVersion` | Libraries |
|---|---|
| Derived as `0.{Minor}.0.0` | ETL-Csv, ETL-SqlBulkCopy, ETL-Transformers |
| Pinned at `1.0.0.0` | this repository (four packages), ETL-FixedWidth, ETL-Json, ETL-Xml, Etl-DbClient |

The three deriving libraries cite this decision in their project files, and
ETL-SqlBulkCopy's ADR-0007 cites it too, as "ADR-0011 in ETL-Abstractions". No such
record was ever committed here, and 0011 is a different decision. This ADR is that
record.

## Decision

We set `AssemblyVersion` by where a library is in its life, not by one rule for all.

1. **A new library that targets .NET Framework uses `0.{Minor}.0.0` during `0.x`**,
   derived from `<Version>` so that a release cannot forget it:

   ```xml
   <AssemblyVersion>0.$([System.Version]::Parse($(Version.Split('-')[0])).Minor).0.0</AssemblyVersion>
   ```

   It changes on every minor release, which is where SemVer puts a breaking change
   before 1.0, and stays fixed across patch releases.
2. **A new library with no .NET Framework target pins `1.0.0.0`.** Binding ignores
   the value there, so the signal buys nothing and the pin is simpler.
3. **From 1.0 onwards a library uses `{Major}.0.0.0`.** Every `1.x` release shares
   `1.0.0.0`; a breaking `2.0.0` moves to `2.0.0.0`.
4. **A library already pinned at `1.0.0.0` is never moved to a `0.x` value.**
   `1.0.0.0` to `0.N.0.0` is a numeric downgrade: it invalidates the binding
   redirects consumers already have and can fail to load. Those libraries stay
   pinned until their own 1.0.

`FileVersion` and `InformationalVersion` carry the release version in every case.

## Alternatives considered

- **Pin every library at `1.0.0.0`** — rejected for new libraries because of the two
  costs above. It remains in force for the libraries that already ship that way.
- **`AssemblyVersion` equal to the package version** — rejected: a binding redirect
  on every patch release. ETL-SqlBulkCopy started here and moved away from it.
- **A hand-maintained `0.{Minor}.0.0`** — rejected: it depends on every release
  preparation remembering one more edit. The derived form cannot be forgotten.
- **Move the pinned libraries down to `0.{Minor}.0.0`** — rejected: see point 4.

## Consequences

- Two regimes coexist in the family until each library reaches 1.0. A reader has to
  check which one a repository is in; the table above was correct on the date of
  this ADR and is not kept up to date here.
- In a pinned library, a breaking `0.x` minor gives a .NET Framework consumer no
  bind-time signal. Only the package version and Package Validation convey it.
- **Not decided here:** what `AssemblyVersion` a pinned library ships at its own
  `1.0.0`, given that `1.0.0.0` is already in use. That needs its own ADR when the
  first pinned library gets there.
- The derived formula yields `0.{Minor}.0.0` only while the major version is 0. Each
  deriving library must replace it at 1.0, and should record that as an ADR.
- ADR-0007 continues to govern this repository's four packages.
- The project-file comments and the ADR in the sibling repositories that say
  "ADR-0011 in ETL-Abstractions" should be changed to cite this ADR.

*Recorded after the fact: the policy was already in force in the libraries listed
above when this ADR was written.*
