# Architecture Decision Records

This directory records the significant architecture / design decisions for
**Wolfgang.Etl.TestKit** and **Wolfgang.Etl.TestKit.Xunit**, using lightweight
[Architecture Decision Records (ADRs)](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions).

An ADR captures a single decision: the context that forced it, the decision
itself, and the consequences (good and bad). ADRs are immutable once accepted —
when a decision changes, add a **new** ADR that supersedes the old one rather
than editing history.

- **[index.md](index.md)** — the list of all ADRs and their status.
- **[TEMPLATE.md](TEMPLATE.md)** — the skeleton to copy when adding one.

## Adding an ADR

1. Copy [`TEMPLATE.md`](TEMPLATE.md) to `NNNN-short-title.md`, numbering it with
   the next free 4-digit sequence.
2. Fill in Context / Decision / Consequences (Nygard style).
3. Set the status to `Proposed`, then `Accepted` once agreed (or
   `Superseded by ADR-NNNN`).
4. Add a row to [`index.md`](index.md).
5. Land the ADR alongside the PR that introduces the decision, so it is part of
   the review.
