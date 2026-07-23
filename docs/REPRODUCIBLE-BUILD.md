# Reproducing & verifying the build

`Wolfgang.Etl.Abstractions` is built **deterministically** — `Deterministic` +
`ContinuousIntegrationBuild` + SourceLink — so the compiled assembly is a pure
function of the source at a given commit. That means **you don't have to trust our
CI**: you can rebuild the library yourself from the published tag and confirm, byte
for byte, that the assembly we shipped is the one the source produces.

Our CI already proves the build is reproducible *internally* — `.github/workflows/reproducible-build.yaml`
builds the library twice in independent directories and fails if the two
assemblies' hashes differ (#216). This page is the **consumer side**: how *you*
verify it independently (#225).

## What is verified

The **compiled assembly** (`Wolfgang.Etl.Abstractions.dll`) is the deterministic
artifact. The `.nupkg` itself is a zip and carries non-deterministic container
metadata (entry timestamps), so its outer hash is *not* expected to match — verify
the assembly inside it, not the package envelope.

Each release attaches a **`reproducible-build-manifest.json`** listing the expected
`sha256` of the shipped assembly for every target framework, e.g.:

```json
{
  "package": "Wolfgang.Etl.Abstractions",
  "version": "0.16.1",
  "algorithm": "sha256",
  "assemblies": {
    "lib/net10.0/Wolfgang.Etl.Abstractions.dll": "d34db33f…",
    "lib/net8.0/Wolfgang.Etl.Abstractions.dll": "…"
  }
}
```

## Verify a published package

You need the same **major .NET SDK** the release was built with (see the release's
`global.json` / the SDK line in `release.yaml` — `10.0.x` for current releases).

```bash
# 1. Get the exact published assembly hash (from the package on nuget.org)
dotnet nuget locals -c all >/dev/null   # optional: clean caches
unzip -p Wolfgang.Etl.Abstractions.<version>.nupkg \
  lib/net10.0/Wolfgang.Etl.Abstractions.dll | sha256sum

# 2. Rebuild from source at the tag and hash your own output
git clone https://github.com/Chris-Wolfgang/ETL-Abstractions
cd ETL-Abstractions
git checkout v<version>
dotnet build src/Wolfgang.Etl.Abstractions/Wolfgang.Etl.Abstractions.csproj \
  -c Release -f net10.0 -p:ContinuousIntegrationBuild=true
sha256sum src/Wolfgang.Etl.Abstractions/bin/Release/net10.0/Wolfgang.Etl.Abstractions.dll

# 3. Compare — the two hashes, and the value in reproducible-build-manifest.json,
#    must all match.
```

A match means the published binary is exactly what the tagged source compiles to,
on your machine, under your control.

## If the hashes differ

First rule out environment drift — a different SDK **feature band** or OS can change
codegen. Confirm you used the SDK major/feature band named in the release and built
with `-p:ContinuousIntegrationBuild=true`.

If they still differ with matching tooling, that is a genuine reproducibility
discrepancy worth reporting: open an issue titled `reproducibility: <version> hash
mismatch` with your **SDK version** (`dotnet --version`), **OS**, the **two hashes**,
and the target framework. It is treated as a security-relevant report.

## Third-party verification attestations

Independent verifiers are encouraged to publish their result following the
[Reproducible Builds project](https://reproducible-builds.org/) conventions (a
signed statement that "version X's assembly rebuilds to hash H"), for example via
[vouchsafe.io](https://vouchsafe.io/) or an [`actions/attest-build-provenance`](https://github.com/actions/attest-build-provenance)
attestation over your own rebuild. Link such an attestation on the discrepancy/verification
issue so others can find it.
