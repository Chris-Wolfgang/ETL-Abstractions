# Architecture Decision Records

This folder records the non-obvious design decisions behind
`Wolfgang.Etl.Abstractions` — the *why* that the code and type signatures can't
carry on their own. See [ADR-0001](0001-record-architecture-decisions.md) for the
practice itself, and [`TEMPLATE.md`](TEMPLATE.md) to add a new one.

Each ADR is immutable once **Accepted**. A decision that gets reversed is recorded
as a *new* ADR that supersedes the old one; the superseded ADR stays in place with
its status updated, so the history of the thinking is preserved.

| # | Title | Status |
|---|-------|--------|
| [0001](0001-record-architecture-decisions.md) | Record architecture decisions | Accepted |
| [0002](0002-etlpipeline-core-in-abstractions-operators-in-transformers.md) | EtlPipeline core in Abstractions; operators in Transformers | Accepted |
| [0003](0003-from-as-extension-method.md) | `Create().From()` with `From` as an extension method | Accepted |
| [0004](0004-etlpipelineprogress-extracted-loaded-elapsed.md) | `EtlPipelineProgress` reports extracted / loaded / elapsed only | Accepted |
| [0005](0005-name-etlpipeline-not-pipeline.md) | Name the generic builder `EtlPipeline`, not `Pipeline` | Accepted |
| [0006](0006-sequential-reverse-order-stage-disposal.md) | Dispose stages sequentially, in reverse construction order | Accepted |
| [0007](0007-pin-assemblyversion.md) | Pin `<AssemblyVersion>` at `1.0.0.0` | Accepted |
| [0008](0008-ban-synchronous-parallel-apis.md) | Ban synchronous `Parallel.For`/`Parallel.ForEach` | Accepted |

## Status legend

- **Accepted** — in force.
- **Superseded by ADR-XXXX** — replaced by a later decision; kept for history.
- **Deprecated** — no longer applies, but was not directly replaced.
