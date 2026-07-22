# ADR 0008 — Ban synchronous `Parallel.For`/`Parallel.ForEach`

- **Status:** Accepted
- **Date:** 2026-07-21
- **Deciders:** Chris Wolfgang

## Context

This is an **async-first ETL library**: extractors, transformers, and loaders are
all `IAsyncEnumerable`-based, and every I/O path is `async`. `System.Threading.Tasks.Parallel.For`
and `Parallel.ForEach` are **synchronous** fan-out primitives — they block the
calling thread until all iterations complete and offer no natural way to `await`
async work inside the loop body. Reaching for them in an async pipeline blocks
threads, defeats the cooperative concurrency model, and tends to produce
`async void`-style lambdas that swallow exceptions. They are easy to type and look
harmless in review.

The repository enforces API hygiene via a `BannedSymbols.txt` consumed by the
`Microsoft.CodeAnalysis.BannedApiAnalyzers` analyzer (warnings-as-errors).

## Decision

We will ban `Parallel.For` and `Parallel.ForEach` (both overload families) in
`BannedSymbols.txt`, with a diagnostic message pointing to the async-friendly
alternatives: `Task.WhenAll`, dataflow, and — on .NET 6+ targets —
`Parallel.ForEachAsync`. The ban makes a synchronous fan-out a **build error**, not
a review nit, so it can't slip in unnoticed.

## Alternatives considered

- **Rely on code review** — rejected: humans miss it, and the whole point of the
  banned-API analyzer is to make "we already decided against this" mechanical.
- **Ban nothing / allow case-by-case** — rejected: there is no place in an
  async-streaming pipeline where the synchronous `Parallel.*` overloads are the
  right tool; a blanket ban has no false positives here.
- **Also ban `Parallel.ForEachAsync`** — rejected: it *is* the async-friendly
  escape hatch on modern targets, so it stays allowed.

## Consequences

- Synchronous parallel fan-out fails the build with a message that names the
  replacement, keeping the library consistently async.
- Contributors on `net462`/`netstandard` (no `Parallel.ForEachAsync`) use
  `Task.WhenAll` or dataflow instead; the ban steers them there.
- Reinforces the sequential-disposal decision in
  [ADR-0006](0006-sequential-reverse-order-stage-disposal.md): parallel teardown
  would have leaned on exactly these banned primitives.
