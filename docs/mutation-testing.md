# Mutation testing (#205)

Mutation testing seeds deliberate faults ("mutants") into the source and checks
that the test suite catches them. A *survived* mutant is a change to behaviour
that **no test noticed** — a hole in the suite that line coverage can't see.

This repo runs [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/introduction/)
as an **enforced release-gate quality bar**, not just a report.

## The floor

`stryker-config.json` sets `thresholds.break`, which makes `dotnet stryker` exit
non-zero (failing the job) when the mutation score drops below it.

| | Value |
|---|---|
| Baseline score (full project, 2026-07-22, Stryker 4.16.0) | **74.4 %** — 248 killed / 83 survived / 2 timeout of 333 tested |
| **Enforced floor (`break`)** | **70 %** |

The floor sits a few points below the measured baseline so normal CI-runner
variance (a slow runner can turn a killed mutant into a timeout, nudging the
score) doesn't cause a spurious failure, while a real regression — deleting a
test, or adding untested behaviour — trips it.

**Policy: ratchet the floor UP, never down.** As survivors are killed and the
score climbs, raise `break` to lock in the gain. Lowering it to make a red build
pass defeats the point — fix the test gap instead.

## How it runs

- **Pull requests that touch `src/**`** run the gate before merge
  (`.github/workflows/stryker.yaml`). It is scoped to source changes because a
  full run takes tens of minutes; docs/test-only/workflow PRs skip it.
- **Weekly schedule + `workflow_dispatch`** run it on demand / for the trend.

## Running locally

```bash
dotnet tool install --global dotnet-stryker --version 4.16.0
dotnet stryker                       # full project (~4 min)
dotnet stryker --mutate "**/Report.cs"   # a single file, faster
```

The HTML report under `StrykerOutput/**/reports/` lists every survived mutant with
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
