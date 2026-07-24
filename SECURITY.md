# Security Policy

## Reporting a Vulnerability

If you discover a security vulnerability, please follow these steps:

1. **Do not** create a public issue on this repository.
2. In the top navigation of this repository, click the **Security** tab.
3. In the top right, click the **Report a vulnerability** button.
4. Fill out the provided form with:
   - A description of the vulnerability
   - Steps to reproduce the issue
   - Potential impact
   - Suggested fix (if you have one)

## Response Timeline

We will acknowledge your report within 48 hours and provide an estimated timeline for a fix.

## Thank You

Your help is greatly appreciated!
Responsible disclosure of security vulnerabilities helps protect our entire community.

## Release path & compromise scope

Facts a maintainer would need at 2am if the release identity is compromised. Generic incident-response steps (rotating credentials, revoking OAuth apps, publishing advisories, unlisting NuGet packages) are not duplicated here — GitHub's and NuGet's own docs update faster than a checked-in runbook.

- **Release path**: OIDC / NuGet Trusted Publishing via `NuGet/login@v1` in `.github/workflows/release.yaml`. The workflow mints an ephemeral push token per run via OIDC — the release path does not depend on a long-lived API key stored in GitHub secrets or on the NuGet account. During an incident, check the NuGet account for any long-lived API keys anyway (they can be created outside of CI) and delete anything you don't recognize.
- **Fallback**: none. If Trusted Publishing is compromised, the incident is at the GitHub-account level (the OIDC identity is `Chris-Wolfgang/ETL-Abstractions`).
- **Owner**: @Chris-Wolfgang.
- **Downstream consumers**: this is the framework's foundational package — every `Wolfgang.Etl.*` library depends on it, including `Wolfgang.Etl.TestKit`, `Wolfgang.Etl.Transformers`, and the format packages (`Wolfgang.Etl.Csv`, `.Json`, `.Xml`, `.FixedWidth`, `.SqlBulkCopy`, `.DbClient`). A compromise cascades to all of them. Unknown external consumers may also exist on nuget.org.
- **Package coordinates for unlisting**: `Wolfgang.Etl.Abstractions` — https://www.nuget.org/packages/Wolfgang.Etl.Abstractions/ (single package; symbols publish as the matching `.snupkg`).

## Verifying the supply chain

Each release ships provenance so consumers can verify what they downloaded:

- **SBOM** — a CycloneDX SBOM (`Wolfgang.Etl.Abstractions.bom.json`) is generated at
  release time and attached to the GitHub Release, listing the package's transitive
  dependency set.
- **SLSA build provenance** — each `.nupkg` is attested with
  [`actions/attest-build-provenance`](https://github.com/actions/attest-build-provenance),
  recording a signed statement that the artifact was built by this repo's
  `release.yaml` from a specific commit. Verify a downloaded package with:

  ```bash
  gh attestation verify Wolfgang.Etl.Abstractions.<version>.nupkg \
    --repo Chris-Wolfgang/ETL-Abstractions
  ```

  A pass confirms the package's digest matches an attestation produced by this
  repository's release workflow.

> **Not yet enabled — Authenticode/NuGet package signing.** The `.nupkg` is not yet
> signed with a code-signing certificate, so `nuget verify --signatures` will report
> it as unsigned. That is tracked on issue #208 and requires a code-signing
> certificate; the SBOM + SLSA attestation above are the current verification path.
