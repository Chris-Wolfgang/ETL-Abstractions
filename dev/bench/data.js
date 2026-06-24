window.BENCHMARK_DATA = {
  "lastUpdate": 1782264431460,
  "repoUrl": "https://github.com/Chris-Wolfgang/ETL-Abstractions",
  "entries": {
    "BenchmarkDotNet": [
      {
        "commit": {
          "author": {
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com",
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "2c06923535b8d760669970004b7eac2ece2af1f3",
          "message": "Merge pull request #243 from Chris-Wolfgang/protected/abs-benchmarks-workflow\n\nperf: benchmarks gh-pages publish workflow (#164) — protected-only PR",
          "timestamp": "2026-06-23T21:08:14-04:00",
          "tree_id": "7942151e78a4919c34651826baaa0faf241563bd",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/2c06923535b8d760669970004b7eac2ece2af1f3"
        },
        "date": 1782263416100,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 15302.811548868814,
            "unit": "ns",
            "range": "± 58.8631603881769"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34198.547607421875,
            "unit": "ns",
            "range": "± 95.12442227010274"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 1486137.859375,
            "unit": "ns",
            "range": "± 1494.7625710723883"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3403820.5729166665,
            "unit": "ns",
            "range": "± 12863.918750360059"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29492.974939982098,
            "unit": "ns",
            "range": "± 120.47341917221921"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 28909.162358601887,
            "unit": "ns",
            "range": "± 119.94445496259374"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 41442.40933227539,
            "unit": "ns",
            "range": "± 213.02884720260144"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2894067.0208333335,
            "unit": "ns",
            "range": "± 14558.088746106725"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2819699.6666666665,
            "unit": "ns",
            "range": "± 5090.705051601289"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 3635955.2799479165,
            "unit": "ns",
            "range": "± 1730.5515228260963"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com",
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "29b938e468f428f3a2368d93bc1a35fa561e31b5",
          "message": "Merge pull request #247 from Chris-Wolfgang/feature/reset-run-state\n\nfix: reset per-run counters + timing each run (#246)",
          "timestamp": "2026-06-23T21:25:07-04:00",
          "tree_id": "7602a9d1bb487d169804d0644b6209910e855586",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/29b938e468f428f3a2368d93bc1a35fa561e31b5"
        },
        "date": 1782264429617,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 31706.635182698566,
            "unit": "ns",
            "range": "± 395.3932216107171"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34568.5647277832,
            "unit": "ns",
            "range": "± 69.93007120228653"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3076712.4791666665,
            "unit": "ns",
            "range": "± 3843.705544784554"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3393242.0807291665,
            "unit": "ns",
            "range": "± 48960.18219879418"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29934.69176228841,
            "unit": "ns",
            "range": "± 164.82196268167763"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 28941.132227579754,
            "unit": "ns",
            "range": "± 51.297241649865775"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 67522.33813476562,
            "unit": "ns",
            "range": "± 332.81411784215373"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2829918.6341145835,
            "unit": "ns",
            "range": "± 2134.475276187561"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2865575.8333333335,
            "unit": "ns",
            "range": "± 4591.978987410338"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 6642997.669270833,
            "unit": "ns",
            "range": "± 20801.608430180397"
          }
        ]
      }
    ]
  }
}