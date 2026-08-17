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

Usage:
    regression-gate.py <current-bdn-report.json> <baseline-data.js> <ratio> <floor_ns>

Exit codes: 0 = no real regression, 1 = at least one benchmark regressed past both
thresholds, 2 = usage/parse error.
"""
import json
import sys


def load_current(path):
    """BDN JSON report -> {full_name: mean_ns}."""
    with open(path, encoding="utf-8") as f:
        data = json.load(f)
    return {b["FullName"]: float(b["Statistics"]["Mean"]) for b in data.get("Benchmarks", [])}


def load_baseline(path):
    """gh-pages data.js (window.BENCHMARK_DATA = {...};) -> {name: value_ns} for the latest entry."""
    with open(path, encoding="utf-8") as f:
        text = f.read().strip()
    prefix = "window.BENCHMARK_DATA"
    if text.startswith(prefix):
        text = text[text.index("=", len(prefix)) + 1:]
    text = text.strip().rstrip(";").strip()
    data = json.loads(text)
    entries = data["entries"]["BenchmarkDotNet"]
    latest = entries[-1]["benches"]
    return {b["name"]: float(b["value"]) for b in latest}


def main(argv):
    if len(argv) != 5:
        print(__doc__)
        return 2
    current = load_current(argv[1])
    baseline = load_baseline(argv[2])
    ratio_threshold = float(argv[3])
    floor_ns = float(argv[4])

    regressions = []
    print(f"Gate: fail only if ratio >= {ratio_threshold:.2f}x AND abs delta >= {floor_ns:,.0f} ns\n")
    print(f"{'benchmark':<70} {'current':>14} {'baseline':>14} {'ratio':>7} {'delta ns':>14}")
    for name, cur in sorted(current.items()):
        base = baseline.get(name)
        if base is None or base <= 0:
            continue
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
    if regressions:
        print(f"::error::{len(regressions)} benchmark(s) regressed past BOTH the ratio and the absolute floor.")
        return 1
    print("No benchmark cleared both the ratio and the absolute floor - gate passes.")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
