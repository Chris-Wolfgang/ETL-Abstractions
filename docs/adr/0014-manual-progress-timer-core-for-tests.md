# ADR 0014 — Drive stage progress tests through a manual timer core

- **Status:** Accepted
- **Date:** 2026-11-15
- **Deciders:** Chris Wolfgang

## Context

ADR-0012 described contract tests that inject `ManualProgressTimer` into
components and call `Fire()`. The contract-test bases now drive the timer core
used by the stage base classes through `ManualProgressTimerCore` and
`WithManualProgressTimer`. This avoids requiring a timer-injection constructor
for every component under test.

`ManualProgressTimer` remains useful for a component that already supplies its
own `IProgressTimer` implementation.

## Decision

We will use `ManualProgressTimerCore` with `WithManualProgressTimer` when
tests need to trigger a stage base class's progress callback deterministically.
The helper is attached before the run starts, and `Tick()` is called after the
stage has built its timer. Tests for custom injected timers may continue using
`ManualProgressTimer`.

## Alternatives considered

- **Inject `ManualProgressTimer` into every component** — rejected: it adds
  per-component timer plumbing even though the test kit can drive the base
  class's timer-core seam.
- **Remove `ManualProgressTimer`** — rejected: custom components may still
  depend on the public injection seam, and removing it would break existing
  users.

## Consequences

- Base-class progress tests can control callbacks synchronously without sleeps
  or per-component injection constructors.
- Tests must start the stage run before calling `Tick()`, because the timer
  callback is captured when the stage builds its progress timer.
- Both manual approaches remain available for their distinct use cases.
