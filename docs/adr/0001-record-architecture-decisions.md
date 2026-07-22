# ADR 0001 — Record architecture decisions

- **Status:** Accepted
- **Date:** 2026-07-21
- **Deciders:** Chris Wolfgang

## Context

`Wolfgang.Etl.Abstractions` carries a number of design choices that are not
obvious from the code and not recoverable from the type signatures — why the
generic pipeline's operators live in a *different* package, why the entry point
is an extension method rather than an instance method, why `<AssemblyVersion>` is
frozen at `1.0.0.0`. Six months after the PR that introduced each one, the
rationale is gone: the commit message is terse, the PR discussion is buried, and
the next maintainer (often the same person) re-derives the trade-off from scratch,
sometimes landing on a different answer and regressing the original intent.

## Decision

We will keep short **Architecture Decision Records** under `docs/adr/`, one file
per non-obvious decision, using [Michael Nygard's format](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions):
Context / Decision / Alternatives / Consequences. Each ADR is immutable once
Accepted — a reversal is a *new* ADR that supersedes the old one, and the old one
is marked `Superseded by` rather than deleted, so the history of the thinking
survives.

New decisions land **in the same PR** as the code that implements them, so the ADR
is part of the review rather than an afterthought.

## Alternatives considered

- **Wiki / external doc site** — drifts out of sync with the code because it is
  not versioned alongside it and is not part of the PR review.
- **Long XML-doc comments on the types** — good for *how to use* an API, poor for
  *why the API is shaped this way*; they also can't capture cross-cutting or
  rejected-alternative reasoning that spans multiple types.
- **Nothing (status quo)** — the rationale keeps evaporating; this ADR exists
  precisely because that cost is real.

## Consequences

- Every reviewer of a non-trivial design PR now expects an accompanying ADR; this
  is a small, deliberate tax on such PRs.
- The `docs/adr/` folder becomes the canonical answer to "why is it like this?"
  and the first place to look before changing a load-bearing decision.
- ADRs are retroactive where useful: the initial set (0002–0008) documents choices
  made before this practice began.
