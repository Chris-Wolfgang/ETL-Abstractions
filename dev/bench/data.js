window.BENCHMARK_DATA = {
  "lastUpdate": 1782405143615,
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
          "id": "eb3a46349fdde7658dfcb87e857e9a9ffb2da583",
          "message": "Merge pull request #248 from Chris-Wolfgang/feature/async-disposable\n\nfeat: IAsyncDisposable/IDisposable on base classes (#92)",
          "timestamp": "2026-06-23T21:51:54-04:00",
          "tree_id": "9ca989361120e08ecde8a9579985f03efafdd549",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/eb3a46349fdde7658dfcb87e857e9a9ffb2da583"
        },
        "date": 1782266035496,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 31542.394073486328,
            "unit": "ns",
            "range": "± 114.39495081753778"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34034.97401936849,
            "unit": "ns",
            "range": "± 52.65117878895945"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3059995.4609375,
            "unit": "ns",
            "range": "± 9414.11398983909"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3342062.5533854165,
            "unit": "ns",
            "range": "± 3862.403914064617"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29365.838129679363,
            "unit": "ns",
            "range": "± 42.26355721956391"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29074.93191019694,
            "unit": "ns",
            "range": "± 71.34254898278093"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 75282.49051920573,
            "unit": "ns",
            "range": "± 2017.0391993237686"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2878086.86328125,
            "unit": "ns",
            "range": "± 1883.5057170427735"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2894422.4993489585,
            "unit": "ns",
            "range": "± 6894.159109510389"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 6733863.377604167,
            "unit": "ns",
            "range": "± 6341.542743937732"
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
          "id": "9714545ef6aa5460cd978f86b2294dedd9b79956",
          "message": "Merge pull request #251 from Chris-Wolfgang/release/v0.14.0\n\nchore: prep v0.14.0 release (version bump + changelog)",
          "timestamp": "2026-06-24T20:09:59-04:00",
          "tree_id": "47ed6dfb6df867225daa5735e08eac97805b5679",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/9714545ef6aa5460cd978f86b2294dedd9b79956"
        },
        "date": 1782346323508,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 31914.836954752605,
            "unit": "ns",
            "range": "± 96.45684721749096"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 35301.20790608724,
            "unit": "ns",
            "range": "± 68.1215231835136"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3126658.5494791665,
            "unit": "ns",
            "range": "± 9465.355887038977"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3363722.6080729165,
            "unit": "ns",
            "range": "± 17344.22430596229"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29212.665735880535,
            "unit": "ns",
            "range": "± 117.60220174411349"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29249.489018758137,
            "unit": "ns",
            "range": "± 43.15554565575019"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 72242.86814371745,
            "unit": "ns",
            "range": "± 245.43333840054717"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2875114.609375,
            "unit": "ns",
            "range": "± 5929.2768058133925"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2823102.2513020835,
            "unit": "ns",
            "range": "± 5659.149916078309"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 6675932.807291667,
            "unit": "ns",
            "range": "± 7650.270471645234"
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
          "id": "6932e5590bf64f1ea1f22214585df66c0812b481",
          "message": "Merge pull request #254 from Chris-Wolfgang/chore/code-review-fixes\n\nchore: address code-review findings (docs accuracy + minor polish)",
          "timestamp": "2026-06-25T12:18:16-04:00",
          "tree_id": "91babcdd04804edd2d66afc58a81f0bf1789fc6e",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/6932e5590bf64f1ea1f22214585df66c0812b481"
        },
        "date": 1782404424053,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 31208.53564453125,
            "unit": "ns",
            "range": "± 163.74990810645824"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34169.641927083336,
            "unit": "ns",
            "range": "± 131.02847456125437"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3096440.3802083335,
            "unit": "ns",
            "range": "± 6190.594765010549"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3297373.1959635415,
            "unit": "ns",
            "range": "± 3331.1645107508198"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29125.345825195312,
            "unit": "ns",
            "range": "± 102.37795574664851"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 28902.611251831055,
            "unit": "ns",
            "range": "± 48.46392133573189"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 71335.07458496094,
            "unit": "ns",
            "range": "± 203.39696334783056"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2834189.6966145835,
            "unit": "ns",
            "range": "± 19127.42500928163"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2934468.8971354165,
            "unit": "ns",
            "range": "± 21306.21507706952"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 7308094.278645833,
            "unit": "ns",
            "range": "± 15429.25668265705"
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
          "id": "5e48b3c22d014d5b21b1193590b8c68b64bfce46",
          "message": "Merge pull request #255 from Chris-Wolfgang/fix/report-estimatedremaining-overflow\n\nfix: guard Report.EstimatedRemaining against TimeSpan overflow",
          "timestamp": "2026-06-25T12:30:30-04:00",
          "tree_id": "72a6334965a7d0f0ea936885bc38044b86764c3d",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/5e48b3c22d014d5b21b1193590b8c68b64bfce46"
        },
        "date": 1782405141454,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 24867.502451578777,
            "unit": "ns",
            "range": "± 71.73658215357969"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 27379.452870686848,
            "unit": "ns",
            "range": "± 200.40258647925575"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 2620272.0618489585,
            "unit": "ns",
            "range": "± 11370.031080836143"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 2679076.390625,
            "unit": "ns",
            "range": "± 14829.830116845907"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 25283.291056315105,
            "unit": "ns",
            "range": "± 77.92446234952132"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 25149.605524698894,
            "unit": "ns",
            "range": "± 48.279696381483745"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 55325.47889200846,
            "unit": "ns",
            "range": "± 71.69009930583111"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2476396.2552083335,
            "unit": "ns",
            "range": "± 483.4530738431164"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2483940.6341145835,
            "unit": "ns",
            "range": "± 2692.8593727400553"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 5344788.028645833,
            "unit": "ns",
            "range": "± 84485.45200628086"
          }
        ]
      }
    ]
  }
}