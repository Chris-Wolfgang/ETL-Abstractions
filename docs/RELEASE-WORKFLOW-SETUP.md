# Release Workflow Setup Guide

This guide explains how this repository's `release.yaml` workflow is configured and how a release moves through it. The repository ships four packages (`Wolfgang.Etl.Abstractions`, `Wolfgang.Etl.ErrorPolicies`, `Wolfgang.Etl.TestKit`, `Wolfgang.Etl.TestKit.Xunit`) from one lockstep version, and one run of the workflow publishes all of them.

## Overview

The release workflow triggers when you **publish a GitHub Release** and implements a comprehensive validation and automatic deployment process that:
- ✅ Tests all target frameworks per test project on Windows
- ✅ Enforces the line-coverage thresholds set at the top of `release.yaml`: `CODECOV_MINIMUM` (95%) for the production assemblies and `CODECOV_TEST_MINIMUM` (99%) for the test assemblies
- ✅ Verifies the documentation builds and that the symbol packages carry SourceLink
- ✅ Validates NuGet package integrity with smoke tests
- ✅ Automatically publishes to NuGet.org after validation passes
- ✅ Eliminates duplicate build work for faster releases

## Required Configuration

Complete the following one-time setup so that the workflow can publish releases:

### Configure NuGet trusted publishing (no API key)

`release.yaml` publishes with [NuGet trusted publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing):
the `publish-nuget` job requests a GitHub OIDC token (`permissions: id-token: write`), exchanges it for a
short-lived NuGet API key with the `NuGet/login` action (`user: Chris-Wolfgang`), and pushes with that key.
There is **no `NUGET_API_KEY` repository secret** to create or rotate.

**Location:** nuget.org → your account → **Trusted Publishing**

1. Click **"Add"** (create a trusted publishing policy).
2. **Repository owner:** `Chris-Wolfgang`
3. **Repository:** the repository name (e.g. `ETL-Abstractions`)
4. **Workflow file:** `release.yaml`
5. **Environment:** leave empty (the job does not use a GitHub environment).
6. Save. For a package ID that has never been published, the policy can be created first; the first
   publish from the workflow then claims the ID.

**What this does:** Lets only `release.yaml` in this repository obtain a push key, and only for the few
minutes the job runs. Nothing long-lived is stored in the repository.

### Verify Branch Protection Rules

**Location:** Settings → Branches → main (or Settings → Rules → Rulesets)

> **Note:** This repository carries `scripts/Fix-BranchRuleset.ps1`, which replaces the "Protect main branch" ruleset with a fresh one when the canonical definition has changed. It delegates the creation to `Setup-BranchRuleset.ps1`, which is **not** in this repository's `scripts/` folder — it comes from `repo-template`. Without it, configure the equivalent settings manually using the checklist below.

Ensure the following settings are enabled:

- ✅ **Require a pull request before merging**
  - **Single developer repos:** 0 approvals (default)
  - **Multi-developer repos:** 1+ approvals (recommended)
- ✅ **Require status checks to pass before merging**
  - Required checks should include the following status check contexts:
    - "Stage 1: Linux Tests (.NET 5.0-10.0) + Coverage Gate"
    - "Stage 2: Windows Tests (.NET 5.0-10.0, Framework 4.6.2-4.8.1)"
    - "Stage 3: macOS Tests (.NET 6.0-10.0)"
    - "Security Scan (DevSkim)"
    - "Security Scan (CodeQL)"
- ✅ **Require branches to be up to date before merging**
- ✅ **Require conversation resolution before merging**
- ✅ **Do not allow bypassing the above settings** (recommended, even for admins)
- ✅ **Restrict deletions**
- ✅ **Require linear history** (optional but recommended)

**What this does:** Ensures all code merged to `main` has passed comprehensive validation, preventing broken releases.

## Testing the Release Workflow

After completing the setup, test the workflow by creating a GitHub Release:

1. Go to your repository's **Releases** page
2. Click **"Draft a new release"**
3. Choose or create a tag (e.g., `v0.0.1-test`)
4. Add a title and description (optional for a test)
5. Check **"Set as a pre-release"** for test releases
6. Click **"Publish release"**

The workflow triggers automatically when the release is published.

### Expected Workflow Behavior

The workflow has eight jobs. Nothing is published unless the first four pass.

1. **`validate-release`**
   - Runs all framework tests with coverage
   - Enforces the production (95%) and test-assembly (99%) coverage thresholds
   - Uploads the coverage report

2. **`pack-and-validate`** (needs 1)
   - Packs the four NuGet packages and their symbol packages
   - Performs a smoke-test installation
   - Generates one CycloneDX SBOM per package
   - Uploads packages as artifacts

3. **`verify-docs-build`** — "Verify Documentation Builds" (needs 1 and 2)

4. **`sourcelink-snupkg`** — "SourceLink structural verify (.snupkg)" (needs 2)

5. **`publish-nuget`** (needs 2, 3 and 4)
   - Exchanges the job's OIDC token for a short-lived NuGet key (`NuGet/login`)
   - Publishes the packages to NuGet.org

6. **`sourcelink-symbol-server-smoke`** — "SourceLink symbol-server smoke (post-publish)" (needs 5)

7. **`trigger-docs`** — "Build & Deploy Documentation" (needs 1)

8. **`update-release-artifacts`** — "Attach Artifacts to Release" (needs 1, 2 and 5): attaches the
   packages, symbol packages, SBOMs and the reproducible-build manifest to the GitHub Release

### Monitoring the Workflow

- **Actions Tab:** Shows workflow progress in real-time
- **Artifacts:** Each job uploads artifacts (coverage reports, packages)
- **Releases:** Check the Releases page after successful completion

## Troubleshooting

### `NuGet login (OIDC trusted publishing)` fails

**Problem:** The `publish-nuget` job fails at the `NuGet/login` step, or `dotnet nuget push` returns 403.

**Solution:**
1. On nuget.org, check the trusted publishing policy: owner `Chris-Wolfgang`, this repository's name,
   workflow file `release.yaml`, no environment.
2. Check the job still has `permissions: id-token: write`.
3. Re-run the job from the Actions tab (do not re-publish the release)

### Tests Fail on Specific Framework

**Problem:** Tests pass on some frameworks but fail on others (e.g., net462).

**Solution:**
1. Check the test logs for framework-specific issues
2. Fix compatibility issues in your code
3. Test locally: `dotnet test --framework net462`
4. Push fix, then re-publish the release (or re-run the workflow from the Actions tab)

### Coverage Below a Threshold

**Problem:** Workflow fails at coverage validation step.

**Solution:**
1. Review `CoverageReport/Summary.txt` artifact; the failing module is named in the job log, and
   test assemblies are held to the higher test threshold
2. Add tests for uncovered code paths (or, for a test assembly, remove or exercise the unexecuted test code)
3. Ensure tests run on all frameworks
4. Push fix, then re-publish the release (or re-run the workflow from the Actions tab)

### Smoke Test Fails to Install Package

**Problem:** Package packs successfully but fails smoke test installation.

**Solution:**
1. Check package dependencies in `.csproj`
2. Verify framework compatibility in `<TargetFrameworks>`
3. Test locally: `dotnet pack` then try installing in a test project
4. Fix packaging issues and re-publish the release (or re-run the workflow from the Actions tab)

## Production Release Checklist

Before creating a production GitHub Release (e.g., `v1.0.0`):

- [ ] All tests pass on all platforms (pr.yaml workflow)
- [ ] Code coverage meets both thresholds (production 95%; test assemblies 100% on pull requests)
- [ ] Security scan shows no critical issues
- [ ] The lockstep `<Version>` in `Directory.Build.props` is bumped (the projects carry no version of their own)
- [ ] The changelog fragments are assembled: `pwsh ./scripts/changelog.ps1 assemble` writes the new
      version section into `CHANGELOG.md` from `changelog/unreleased/*.md` and deletes the fragments
      (`pwsh ./scripts/changelog.ps1 bump` prints the version the fragments imply)
- [ ] All PRs merged to `main` branch
- [ ] Local build succeeds: `dotnet build --configuration Release`
- [ ] Local tests pass: `dotnet test --configuration Release`

**Create a production release:**
1. Go to your repository's **Releases** page
2. Click **"Draft a new release"**
3. Choose or create the version tag (e.g., `v1.0.0`) targeting `main`
4. Add a title and release notes
5. Click **"Publish release"**

**After workflow completes:**
- [ ] Verify packages appear on NuGet.org
- [ ] Test installing package from NuGet.org in a clean project
- [ ] Announce release (if applicable)

## Workflow Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  Trigger: Published GitHub Release                          │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│  validate-release                                           │
│  • Test all frameworks, collect coverage                    │
│  • Enforce the production and test-assembly thresholds      │
└─────────────────────────────────────────────────────────────┘
          │                                   │
          ▼                                   ▼
┌───────────────────────────────┐   ┌─────────────────────────┐
│  pack-and-validate            │   │  trigger-docs           │
│  • Pack, smoke test, SBOMs    │   │  • Build & deploy docs  │
└───────────────────────────────┘   └─────────────────────────┘
          │                │
          ▼                ▼
┌──────────────────┐  ┌─────────────────────┐
│ verify-docs-build│  │ sourcelink-snupkg   │
└──────────────────┘  └─────────────────────┘
          │                │
          ▼                ▼
┌─────────────────────────────────────────────────────────────┐
│  publish-nuget (needs pack, docs build and SourceLink)      │
│  • NuGet login via OIDC trusted publishing                  │
│  • Publish to NuGet.org                                     │
└─────────────────────────────────────────────────────────────┘
          │                                   │
          ▼                                   ▼
┌───────────────────────────────┐   ┌─────────────────────────┐
│ sourcelink-symbol-server-smoke│   │ update-release-artifacts│
└───────────────────────────────┘   └─────────────────────────┘
```

## Key Improvements Over Previous Workflow

| Issue | Before | After |
|-------|--------|-------|
| **Framework Coverage** | Default framework only | All frameworks (net5.0-10.0, net462-481) |
| **Code Coverage** | Not enforced | Production and test-assembly thresholds enforced |
| **Package Validation** | None | Smoke test installation |
| **Deployment** | Incomplete publish script | Automatic publishing after validation |
| **Publishing credential** | Long-lived API key | OIDC trusted publishing, no stored key |
| **GitHub Releases** | Not used as trigger | Workflow triggered by published release |
| **Build Efficiency** | Duplicate builds in each job | Build once per job with dependencies |
| **Test Logging** | No logger parameter | Console logging with verbosity |
| **Permissions** | Read-only | Write access for releases |

## Support

If you encounter issues not covered in this guide:

1. Check the Actions tab of this repository on GitHub for detailed logs
2. Review artifacts uploaded by failed jobs
3. Consult the [GitHub Actions documentation](https://docs.github.com/en/actions)
4. Open an issue in this repository with:
   - Workflow run URL
   - Error message and logs
   - Steps to reproduce
