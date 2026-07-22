#!/usr/bin/env python3
"""Normalize VSTest .trx output to a sorted, platform-independent list of
"test name<TAB>outcome" lines, for the cross-platform differential (#209).

Usage: python scripts/normalize-trx.py <trx-dir> <output-file>

Only the test name and outcome are emitted — durations, machine names, and
timestamps are intentionally excluded so the list is identical across platforms
when behaviour is identical.
"""
import glob
import os
import sys
import xml.etree.ElementTree as ET

NS = "{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}"


def main(trx_dir: str, out_file: str) -> None:
    rows = []
    for path in glob.glob(os.path.join(trx_dir, "**", "*.trx"), recursive=True):
        tree = ET.parse(path)
        for result in tree.iter(NS + "UnitTestResult"):
            rows.append(f"{result.get('testName')}\t{result.get('outcome')}")

    rows.sort()
    with open(out_file, "w", encoding="utf-8", newline="\n") as handle:
        handle.write("\n".join(rows))
        handle.write("\n")

    print(f"wrote {len(rows)} outcomes to {out_file}")


if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("usage: normalize-trx.py <trx-dir> <output-file>", file=sys.stderr)
        sys.exit(2)
    main(sys.argv[1], sys.argv[2])
