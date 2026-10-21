# Workflow Security

## Overview

This document describes how the GitHub Actions workflows in this repository keep a pull request from
weakening the checks it is validated by. Two workflows share the job:

- `.github/workflows/pr.yaml` builds and tests the PR's code on `pull_request` (and on `push` to `main`).
- `.github/workflows/protected-files.yaml` is a separate guard on `pull_request_target` that never checks
  out the PR's code and fails any PR that mixes protected configuration files with other changes.

## Security Architecture

### 1. PR validation runs on `pull_request`

```yaml
on:
  pull_request:
    branches:
      - main
  push:
    branches:
      - main
```

`pr.yaml` runs the PR's own code together with the PR's own copies of the workflow and of every
analyzer and build configuration file. That is deliberate: the PR is built and tested with exactly the
files it would merge.

This is safe because a `pull_request` run gets a read-only `GITHUB_TOKEN`, a fork PR gets no secrets, and
nothing in `pr.yaml` uses a secret. The workflow-level permissions are:

```yaml
permissions:
  contents: read
```

The only write scope in `pr.yaml` is `security-events: write` on the `inspectcode-upload` job, which never
checks out PR code; it only uploads the SARIF produced by an earlier job.

### 2. Protected configuration files: `protected-files.yaml`

Because `pr.yaml` uses the PR's own configuration, a PR that changed an analyzer rule and the code that
rule would have flagged could pass its own checks. The protected-files guard closes that gap.

```yaml
on:
  pull_request_target:  # no checkout, no execution of PR content
    branches:
      - main
```

`pull_request_target` runs the workflow from `main`, so a PR cannot edit the guard that judges it. The
guard never checks out or executes anything from the PR: it lists the PR's changed files through the
GitHub API and classifies the PR.

| The PR changes | Result |
|---|---|
| No protected file | Pass. `pr.yaml` validated everything. |
| Only protected files | Pass with a notice. A maintainer reviews the configuration diff on its own. |
| Protected files **and** anything else | **Fail.** Split the PR. Nothing bypasses this. |

**Protected files** (the authoritative list is the `case` statement in `protected-files.yaml`):

- `.editorconfig` (any directory), `*.globalconfig`, `*.ruleset`, `*.DotSettings`
- `Directory.Build.props`, `Directory.Build.targets`
- `BannedSymbols.txt`, `coverlet.runsettings`, `.gitleaks.toml`
- `.config/dotnet-tools.json` (the CI tool manifest)
- `.github/workflows/*.yml` and `.github/workflows/*.yaml`
- `.github/license-audit/*.json`
- The CI scripts `pr.yaml` runs: `scripts/build-pr.ps1`, `scripts/changelog.ps1`,
  `scripts/tfm-parity.ps1`, `scripts/third-party-notices.ps1`

Dependabot PRs skip the guard: its bumps to `Directory.Build.props` and workflow pins are legitimate,
and its identity is controlled by GitHub.

### 3. Credential Protection

All checkout steps set `persist-credentials: false`, so the checkout token is not written to the git
config for later steps:

```yaml
- name: Checkout repository
  uses: actions/checkout@<pinned sha>
  with:
    persist-credentials: false
```

This keeps the token out of git config. It does not stop a step from reading `GITHUB_TOKEN` if a workflow
passes it in explicitly, which is why no `pr.yaml` job that runs PR code is given one with write scope.

### 4. Pinned actions and least privilege

Every third-party action is pinned to a full commit SHA, and every workflow declares the narrowest
`permissions:` it needs. `actions-audit.yaml` runs `actionlint` and `zizmor` over the workflows to check this.

## Attack Scenarios

### Scenario 1: Malicious workflow modification
**Attack**: A PR edits `pr.yaml` to disable a check and adds code that the check would have failed.
**Prevention**: The workflow file is protected, so the mixed PR fails `protected-files.yaml`, which runs
from `main`. A workflow-only PR passes the guard but is reviewed on its own before it merges.

### Scenario 2: Configuration file tampering
**Attack**: A PR relaxes `.editorconfig` or `BannedSymbols.txt` and adds code that the old rule forbids.
**Prevention**: Same guard. Configuration changes must land in their own PR first.

### Scenario 3: Credential theft
**Attack**: PR code tries to read GitHub credentials during the build.
**Prevention**: `pull_request` runs have a read-only token and no secrets, and checkouts use
`persist-credentials: false`.

### Scenario 4: Guard tampering
**Attack**: A PR edits `protected-files.yaml` to allow its own mixed change.
**Prevention**: `pull_request_target` always runs the guard from `main`; the PR's copy is never used.

## Validation

When changing the workflow security model, check these by hand:

1. A PR that changes only a protected file passes the guard with a "Configuration-only PR" notice.
2. A PR that changes a protected file and a source file fails the guard with the split instructions.
3. A PR that changes no protected file passes the guard without a notice.

## Maintenance

### Changing a protected configuration file

1. Open a PR that changes only the protected file(s). `pr.yaml` runs with the new configuration and the
   guard passes it as configuration-only.
2. Review the diff itself: it decides what every later PR is checked against.
3. After it merges, rebase any dependent code change on `main` so it is validated under the reviewed
   configuration.

If you need to relax a rule and add code that needs the relaxed rule, that is two PRs: the configuration
first, then the code.

### Adding a new protected file

1. Add the path to the `case` statement (or the extension regex) in `protected-files.yaml`.
2. If `pr.yaml` runs a new script, add it to the list too.
3. Update the list in this document and the comment at the top of `pr.yaml`.

## References

- [GitHub Actions Security Hardening](https://docs.github.com/en/actions/security-guides/security-hardening-for-github-actions)
- [Keeping your GitHub Actions secure](https://docs.github.com/en/actions/security-guides/security-hardening-for-github-actions#using-third-party-actions)
- [Understanding pull_request_target](https://securitylab.github.com/research/github-actions-preventing-pwn-requests/)
