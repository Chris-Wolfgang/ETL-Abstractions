# 4. Make progress reporting deterministic via an injectable timer

## Status

Accepted

## Context

The Abstractions base classes report progress on a wall-clock interval
(`ReportingInterval`) driven by an internal timer. A test that wants to assert
"a progress report was raised" cannot depend on a real timer firing: the test
would either sleep (slow, flaky) or race the timer against a fast synchronous
source that completes — and unsubscribes the timer's `Elapsed` handler in the
worker's `finally` — before the timer ever fires.

## Decision

We will expose progress-timer injection as a first-class part of the doubles and
the contract-test base classes:

- Every double offers a constructor overload taking an `IProgressTimer`, and the
  kit ships **`ManualProgressTimer`** whose `Start`/`Stop` are no-ops and which
  only raises `Elapsed` when `Fire()` is called explicitly.
- The contract-test base classes drive progress assertions by pulling the first
  item (so the pipeline is mid-flight), calling `Fire()` deterministically, then
  draining the rest — never by waiting on wall-clock time.
- Implementations wiring an injected timer guard against duplicate `Elapsed`
  subscriptions when `CreateProgressTimer` is overridden.

## Consequences

- Progress-callback tests are fast and deterministic — no sleeps, no timing races.
- The injection seam is public API surface (the `IProgressTimer` constructor
  overloads and `ManualProgressTimer`) and is therefore guarded by the
  PublicAPI baseline and PackageValidation; it cannot be removed without a
  breaking-change bump.
- Test authors must fire the timer while the pipeline is mid-flight; firing it
  after a synchronous source has drained observes no callback (documented in the
  `ManualProgressTimer` example).
