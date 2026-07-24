# GC / allocation profile

BDN benchmarks (see [`benchmarks/`](../benchmarks/)) measure single-method
micro-perf. This workflow measures the **sustained-load** metrics — the ones that
only surface after minutes of continuous ETL traffic:

- gen0 → gen1 → gen2 promotion rates
- Large Object Heap (LOH) growth and pinning
- Finalizer queue depth
- Thread-pool starvation events
- Working-set growth vs allocated bytes

## What runs

[`.github/workflows/gc-profile.yaml`](../.github/workflows/gc-profile.yaml) runs on
**workflow_dispatch** and on a weekly Sunday 07:00 UTC schedule (Stryker runs at
06:00, so the two don't fight for the same GitHub-hosted runner pool).

The workload — [`tools/GcProfileWorkload/`](../tools/GcProfileWorkload/) — runs an
in-memory extract → transform → load `Pipeline` (base-class `ExtractorBase` →
`TransformerBase` → `LoaderBase`, with per-stage progress and stage disposal) in a
loop for 10 minutes (configurable via the `duration_seconds` input). ServerGC +
concurrent GC are enabled to match a realistic long-running ETL host.
`dotnet-counters` attaches by PID and samples the `System.Runtime` counter set
every 5 seconds; results write to a CSV artifact.

## Gate mode: informational

Every run uploads:

- `reports/workload.log` — the workload's stdout (per-cycle `gen0`/`gen1`/`gen2`
  counts, allocated MB, heap MB, and a final summary).
- `reports/counters.csv` — every sampled `System.Runtime` counter (heap size per
  generation, GC count per gen, working set, thread-pool queue depth,
  exceptions/sec, …).

**No regression gate today.** A meaningful gate needs a stable baseline, which the
first several runs establish (GC metrics vary more run-to-run than BDN benchmarks —
differently-timed collections dominate short samples). Follow-up: once ~10 baseline
runs exist, add a threshold gate (e.g. "gen2 collections/minute > 2× rolling median
→ fail + open a maintenance issue").

## Baseline (first local run, 2026-07-22)

For reference, an 8-second local run processed **~90M records** through the
pipeline with only **2 gen0 collections, 0 gen1, 0 gen2**, ~6 MB total allocated,
~1 MB steady heap — i.e. the streaming pipeline is near-zero-alloc per record
(consistent with the allocation-free hot-path tests, #217). The sustained profile
should stay in that shape; a linear heap climb or rising gen2 is the regression to
catch.

## Reading a report

The counter CSV has columns like:

```
Timestamp,Metadata,Provider,Name,Value
2026-07-19T07:00:15Z,,System.Runtime,gc-heap-size,1.1
2026-07-19T07:00:15Z,,System.Runtime,gen-0-gc-count,2.0
...
```

Metrics worth watching:

- **`gc-heap-size`** — total heap MB. Should stabilise, not grow linearly. Linear
  growth = leak.
- **`gen-2-gc-count` / `loh-size`** — high gen2 or LOH growth = large-object
  pinning or long-lived allocations. For a streaming ETL library we expect
  near-zero gen2.
- **`threadpool-queue-length`** — should stay near zero. A rising queue = the
  workload is blocking a thread-pool thread somewhere.
- **`allocation-rate`** — MB/sec allocated. Cross-reference the workload log's
  per-cycle line for allocations-per-record instead.

## Ratchet policy

Same shape as [`mutation-testing.md`](mutation-testing.md)'s ratchet:

- Baseline: whatever the first stable run gives.
- Improvement: tighter thresholds.
- Regression: never quietly relaxed — flag in review.

Refs #212.
