#!/usr/bin/env python3
"""Absolute-floor benchmark-regression gate (#367).

The github-action-benchmark alert is ratio-only, so the smallest (sub-millisecond)
benchmarks trip a 1.50x ratio on runner noise alone and block releases — the
opposite of what the gate is for. A regression is only real if it is BOTH
ratio-significant AND materially large in absolute terms, so this gate fails only
when a benchmark's mean has grown by at least `ratio` x AND by at least `floor_ns`
nanoseconds. A 17 us noise blip on a 30 us benchmark clears the floor check; a
genuine per-item regression (which shows up at the large record counts too) does
not.

The baseline for each benchmark is the MEDIAN of its last `BASELINE_RUNS` main-branch
values, not the single latest one: identical code on main has measured anywhere from
2.5 ms to 4.4 ms for the same 100,000-record benchmark on hosted runners, so a lucky
fast run as the sole baseline made every following PR look 1.5x slower (the
ratio-and-floor check cannot help when the reference point itself is the outlier).

Usage:
    regression-gate.py <current-bdn-report.json> <baseline-data.js> <ratio> <floor_ns>

A gate that compared nothing must not pass: if the current report is empty, or none of
its benchmarks has a baseline to compare against (a renamed class or namespace, changed
[Params], a change in result naming), the gate fails. Benchmarks present on only one side
are listed; a new benchmark beside others that did compare is reported, not failed.

Only the mean time is gated; allocated bytes are not.

Exit codes: 0 = no real regression, 1 = at least one benchmark regressed past both
thresholds or nothing could be compared, 2 = usage/parse error.
"""
import json
import statistics
import sys

BASELINE_RUNS = 5


def load_current(path):
    """BDN JSON report -> {full_name: mean_ns}."""
    with open(path, encoding="utf-8") as f:
        data = json.load(f)
    return {b["FullName"]: float(b["Statistics"]["Mean"]) for b in data.get("Benchmarks", [])}


def load_baseline(path):
    """gh-pages data.js (window.BENCHMARK_DATA = {...};) -> {name: median value_ns over the last BASELINE_RUNS entries}."""
    with open(path, encoding="utf-8") as f:
        text = f.read().strip()
    prefix = "window.BENCHMARK_DATA"
    if text.startswith(prefix):
        text = text[text.index("=", len(prefix)) + 1:]
    text = text.strip().rstrip(";").strip()
    data = json.loads(text)
    entries = data["entries"]["BenchmarkDotNet"]
    history = {}
    for entry in entries[-BASELINE_RUNS:]:
        for b in entry["benches"]:
            history.setdefault(b["name"], []).append(float(b["value"]))
    return {name: statistics.median(values) for name, values in history.items()}


def main(argv):
    if len(argv) != 5:
        print(__doc__)
        return 2
    current = load_current(argv[1])
    baseline = load_baseline(argv[2])
    ratio_threshold = float(argv[3])
    floor_ns = float(argv[4])

    regressions = []
    compared = 0
    unmatched_current = []
    print(f"Gate: fail only if ratio >= {ratio_threshold:.2f}x AND abs delta >= {floor_ns:,.0f} ns; baseline = median of the last {BASELINE_RUNS} main runs\n")
    print(f"{'benchmark':<70} {'current':>14} {'baseline':>14} {'ratio':>7} {'delta ns':>14}")
    for name, cur in sorted(current.items()):
        base = baseline.get(name)
        if base is None or base <= 0:
            unmatched_current.append(name)
            continue
        compared += 1
        ratio = cur / base
        delta = cur - base
        flag = ""
        if ratio >= ratio_threshold and delta >= floor_ns:
            flag = "  <-- REGRESSION (ratio AND floor)"
            regressions.append((name, ratio, delta))
        elif ratio >= ratio_threshold:
            flag = "  (ratio only - under floor, treated as noise)"
        short = name.rsplit(".", 1)[-1]
        print(f"{short:<70} {cur:>14,.0f} {base:>14,.0f} {ratio:>6.2f}x {delta:>14,.0f}{flag}")

    print()
    missing_from_current = sorted(set(baseline) - set(current))
    if unmatched_current:
        print(f"::warning::{len(unmatched_current)} benchmark(s) have no usable baseline and were not compared:")
        for name in unmatched_current:
            print(f"  + {name}")
    if missing_from_current:
        print(f"::warning::{len(missing_from_current)} baseline benchmark(s) are missing from the current run:")
        for name in missing_from_current:
            print(f"  - {name}")
    if compared == 0:
        print("::error::No benchmark was compared against the baseline - the gate could not run, so it fails.")
        return 1
    if regressions:
        print(f"::error::{len(regressions)} benchmark(s) regressed past BOTH the ratio and the absolute floor.")
        return 1
    print("No benchmark cleared both the ratio and the absolute floor - gate passes.")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
