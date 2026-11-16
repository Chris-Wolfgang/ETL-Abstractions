# Mutation testing (#205)

Mutation testing seeds deliberate faults ("mutants") into the source and checks
that the test suite catches them. A *survived* mutant is a change to behaviour
that **no test noticed** — a hole in the suite that line coverage can't see.

This repo runs [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/introduction/)
as an **enforced release-gate quality bar**, not just a report.

## The floors

There is one Stryker configuration per mutated package, next to the unit-test project that
exercises it. Each sets `thresholds.break`, which makes `dotnet stryker` exit non-zero (failing
the job) when the mutation score drops below it.

| Mutated project | Configuration | `break` | `low` | `high` |
|---|---|---|---|---|
| `Wolfgang.Etl.Abstractions` | `tests/Wolfgang.Etl.Abstractions.Tests.Unit/stryker-config.json` | **80 %** | 80 | 90 |
| `Wolfgang.Etl.TestKit` | `tests/Wolfgang.Etl.TestKit.Tests.Unit/stryker-config.json` | **70 %** | 70 | 90 |

`Wolfgang.Etl.TestKit.Xunit` and `Wolfgang.Etl.ErrorPolicies` have no configuration and are not
mutation-tested.

Scores for `Wolfgang.Etl.Abstractions`:

| When | Score |
|---|---|
| Baseline (2026-07-22, Stryker 4.16.0) | 74.4 % — 248 killed / 83 survived / 2 timeout of 333 tested |
| After the #205 test pass (2026-07-22) | ~86.6 % |
| After the #817 survivor pass (2026-11, Stryker 5.0.0, local run) | 98.66 % |

A current score for `Wolfgang.Etl.TestKit` is not recorded here; read it from the latest
`Stryker (mutation testing)` workflow run.

A floor sits below the measured score so normal CI-runner variance (a slow runner can turn a
killed mutant into a timeout, nudging the score) doesn't cause a spurious failure, while a real
regression — deleting a test, or adding untested behaviour — trips it. Both floors are now well
below the measured scores and are due to be raised under the policy below.

**Policy: ratchet the floor UP, never down.** As survivors are killed and the
score climbs, raise `break` to lock in the gain. Lowering it to make a red build
pass defeats the point — fix the test gap instead.

## How it runs

- **Pull requests** run the gate before merge (`.github/workflows/stryker.yaml`) when they
  touch `src/**`, a test source file (`tests/**/*.cs`), a `stryker-config.json`, or the workflow
  itself. It is scoped that way because a full run takes tens of minutes; a PR that changes only
  documentation, examples or other workflows skips it.
- **Weekly schedule + `workflow_dispatch`** run it on demand / for the trend.

## Running locally

The tool version is pinned in `.config/dotnet-tools.json`. Run Stryker from the unit-test
project's folder: that is where its `stryker-config.json` lives, and a run from the repository
root finds no configuration and applies no floor.

```bash
dotnet tool restore
cd tests/Wolfgang.Etl.Abstractions.Tests.Unit    # or tests/Wolfgang.Etl.TestKit.Tests.Unit
dotnet stryker                           # full project
dotnet stryker --mutate "**/Report.cs"   # a single file, faster
```

The HTML report under `StrykerOutput/**/reports/` in that folder lists every survived mutant with
its file, line, and the mutation applied — the worklist for raising the score.

## Score history & survivor tracking

On the weekly schedule (and manual `workflow_dispatch`) runs — not on PRs — the
workflow also:

- **Charts the score trend** on the `gh-pages` branch under `/dev/stryker/`, using
  the same `github-action-benchmark` action as the BDN benchmark charts
  (`customBiggerIsBetter`, one data point per run). This is trend visibility only —
  it never fails the run; the `break` floor is the gate.
- **Tracks survivors** by writing the survivor worklist (file:line + mutator) to the
  run's job summary and keeping **one** rolling `kind:mutation-survives` issue up to
  date. It is deliberately a single rolling issue, not one issue per survivor —
  there are dozens, and per-mutant issues would be noise. Work the list down from
  the issue (or the uploaded HTML report), then raise the floor.
