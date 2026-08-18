window.BENCHMARK_DATA = {
  "lastUpdate": 1787076690894,
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
          "id": "e61f0de91b612722f465ffe0a276feba5f6a6a49",
          "message": "Merge pull request #256 from Chris-Wolfgang/release/v0.14.1\n\nchore: prep v0.14.1 release (version bump + changelog)",
          "timestamp": "2026-06-25T12:44:42-04:00",
          "tree_id": "b5b0d59a336643629a38787e4bbb12a2a32f52ef",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/e61f0de91b612722f465ffe0a276feba5f6a6a49"
        },
        "date": 1782406005471,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 31136.177124023438,
            "unit": "ns",
            "range": "± 23.321958794622816"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 33795.330881754555,
            "unit": "ns",
            "range": "± 150.87375183122174"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3094101.4596354165,
            "unit": "ns",
            "range": "± 1783.8359608792875"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3395195.6809895835,
            "unit": "ns",
            "range": "± 2409.487620028279"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29486.08614095052,
            "unit": "ns",
            "range": "± 99.78360366943589"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29338.104110717773,
            "unit": "ns",
            "range": "± 47.89527083909093"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 67393.64742024739,
            "unit": "ns",
            "range": "± 520.7973881491806"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2842683.8411458335,
            "unit": "ns",
            "range": "± 6745.750024706914"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2918058.7630208335,
            "unit": "ns",
            "range": "± 1170.7136076514787"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 6799240.9765625,
            "unit": "ns",
            "range": "± 70553.48256466596"
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
          "id": "6324df7fa2afb25ed16f6a06ec43d7702da3e972",
          "message": "Merge pull request #261 from Chris-Wolfgang/vNext\n\nrelease: v0.15.0 — ISupportDryRun",
          "timestamp": "2026-06-28T13:02:37-04:00",
          "tree_id": "734ebc7ceee3246c0f878b807e9ef4ff4b975153",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/6324df7fa2afb25ed16f6a06ec43d7702da3e972"
        },
        "date": 1782666282271,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 31607.01611328125,
            "unit": "ns",
            "range": "± 1023.7712354886028"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34506.33264160156,
            "unit": "ns",
            "range": "± 60.92196881849472"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3066810.97265625,
            "unit": "ns",
            "range": "± 3498.6290283245353"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3426570.078125,
            "unit": "ns",
            "range": "± 1356.8575655315701"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29361.308675130207,
            "unit": "ns",
            "range": "± 516.814444266043"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 28933.14805094401,
            "unit": "ns",
            "range": "± 38.399494753729414"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 67607.79602050781,
            "unit": "ns",
            "range": "± 167.8383279443381"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2850791.27734375,
            "unit": "ns",
            "range": "± 6579.038125445611"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2880390.16796875,
            "unit": "ns",
            "range": "± 3824.729241206419"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 6688480.6640625,
            "unit": "ns",
            "range": "± 6183.073922613842"
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
          "id": "e984573915d4cb92ce10a3388baddb3c7d4a53a2",
          "message": "Merge pull request #266 from Chris-Wolfgang/dependabot/github_actions/github-actions-640176b5ab\n\nchore(deps): bump actions/checkout from 6 to 7 in the github-actions group",
          "timestamp": "2026-07-09T16:32:52-04:00",
          "tree_id": "2a328ad6bc721904727b663f1f8ee5415a204be1",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/e984573915d4cb92ce10a3388baddb3c7d4a53a2"
        },
        "date": 1783629282913,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 25443.17409769694,
            "unit": "ns",
            "range": "± 61.16949716024827"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 27197.960255940754,
            "unit": "ns",
            "range": "± 106.76278428703739"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 2500247.44140625,
            "unit": "ns",
            "range": "± 4437.474362689682"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 2675990.37109375,
            "unit": "ns",
            "range": "± 4560.433118395221"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 25356.40705362956,
            "unit": "ns",
            "range": "± 76.10505747384981"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 24628.09654744466,
            "unit": "ns",
            "range": "± 239.80465021571274"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 55291.084940592445,
            "unit": "ns",
            "range": "± 870.2207663050373"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2486951.4088541665,
            "unit": "ns",
            "range": "± 777.8228987549776"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2472634.80859375,
            "unit": "ns",
            "range": "± 2505.078771066135"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 5103962.158854167,
            "unit": "ns",
            "range": "± 1336.900933996289"
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
          "id": "50f632659fe494aa2ebed8e243dff1d766a47aa8",
          "message": "Merge pull request #245 from Chris-Wolfgang/feature/dispose-stages\n\nfeat: opt-in Pipeline.DisposeStagesOnCompletion() (#133)",
          "timestamp": "2026-07-18T13:01:41-04:00",
          "tree_id": "b9436e8c9e35a9dfcfd99f39fbee95447ad7c399",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/50f632659fe494aa2ebed8e243dff1d766a47aa8"
        },
        "date": 1784394222430,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 30957.572347005207,
            "unit": "ns",
            "range": "± 95.00534139757345"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34574.882497151695,
            "unit": "ns",
            "range": "± 420.651981121649"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3089347.8619791665,
            "unit": "ns",
            "range": "± 33104.221525684596"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3327156.8828125,
            "unit": "ns",
            "range": "± 2352.830114467763"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29587.582041422527,
            "unit": "ns",
            "range": "± 280.466564475289"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29465.469889322918,
            "unit": "ns",
            "range": "± 98.07399399706873"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 67300.5927734375,
            "unit": "ns",
            "range": "± 310.2930901873712"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2856656.240234375,
            "unit": "ns",
            "range": "± 239.4358178709205"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2964943.6145833335,
            "unit": "ns",
            "range": "± 4659.982270866581"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 6924789.651041667,
            "unit": "ns",
            "range": "± 6241.741232685318"
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
          "id": "22c7d450a4ab73ae2e3e98f8dde4fd8eacc8d985",
          "message": "Merge pull request #278 from Chris-Wolfgang/vNext\n\nrelease: Wolfgang.Etl.Abstractions 0.16.0 (vNext → main)",
          "timestamp": "2026-07-20T20:42:04-04:00",
          "tree_id": "a999ed440b8392f2731a23fd699eb0808e763f6c",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/22c7d450a4ab73ae2e3e98f8dde4fd8eacc8d985"
        },
        "date": 1784594648444,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 31213.568400065105,
            "unit": "ns",
            "range": "± 299.6521235887271"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 36572.21645100912,
            "unit": "ns",
            "range": "± 158.33052842976466"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3070944.2565104165,
            "unit": "ns",
            "range": "± 2678.7419978898206"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3380431.5950520835,
            "unit": "ns",
            "range": "± 40667.08439236489"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29966.721135457356,
            "unit": "ns",
            "range": "± 81.57410316157195"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29706.11447652181,
            "unit": "ns",
            "range": "± 85.83290253492184"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 68654.68408203125,
            "unit": "ns",
            "range": "± 262.3170934846805"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2863981.015625,
            "unit": "ns",
            "range": "± 8638.422022496778"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2943644.8385416665,
            "unit": "ns",
            "range": "± 1837.2466959193214"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 6691726.28125,
            "unit": "ns",
            "range": "± 11809.09849091847"
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
          "id": "0657b9e4283f3a9fb4de826c6b67d647ec2357d4",
          "message": "Merge pull request #306 from Chris-Wolfgang/release/ci-infra-0.16.1\n\nci: workflow / CI-infrastructure bundle for 0.16.1 (admin-bypass)",
          "timestamp": "2026-07-22T08:44:01-04:00",
          "tree_id": "64dee58436a866ff8ed96100eddf17aa577c7068",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/0657b9e4283f3a9fb4de826c6b67d647ec2357d4"
        },
        "date": 1784724362602,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 32059.649709065754,
            "unit": "ns",
            "range": "± 137.8791638677358"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34195.585133870445,
            "unit": "ns",
            "range": "± 93.66076639136202"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3097850.85546875,
            "unit": "ns",
            "range": "± 6485.680518413335"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3317207.0234375,
            "unit": "ns",
            "range": "± 33937.996216461244"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 30232.70036824544,
            "unit": "ns",
            "range": "± 187.50046764015784"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29509.600875854492,
            "unit": "ns",
            "range": "± 104.46812929861298"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 72690.12552897136,
            "unit": "ns",
            "range": "± 178.1186879740516"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2900995.9231770835,
            "unit": "ns",
            "range": "± 16611.657529566797"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2940362.0247395835,
            "unit": "ns",
            "range": "± 33835.18344458681"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 7102165.481770833,
            "unit": "ns",
            "range": "± 53081.30094510381"
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
          "id": "732efdde8618fea4bc263feda0037e2d9fb97ab2",
          "message": "Merge pull request #305 from Chris-Wolfgang/release/prep-0.16.1\n\nrelease: 0.16.1",
          "timestamp": "2026-07-22T10:04:06-04:00",
          "tree_id": "578e384a8ccb94e3c9320491470f716b06bea543",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/732efdde8618fea4bc263feda0037e2d9fb97ab2"
        },
        "date": 1784729162082,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 32688.512502034504,
            "unit": "ns",
            "range": "± 213.01288290577438"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34913.1728108724,
            "unit": "ns",
            "range": "± 92.90601829761779"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3227835.359375,
            "unit": "ns",
            "range": "± 5331.793855491133"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3403880.4401041665,
            "unit": "ns",
            "range": "± 29793.796946240178"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 31655.328531901043,
            "unit": "ns",
            "range": "± 117.35331683022335"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 32588.058044433594,
            "unit": "ns",
            "range": "± 149.83187729728172"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 70625.31201171875,
            "unit": "ns",
            "range": "± 1269.4931495812575"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 3210122.2981770835,
            "unit": "ns",
            "range": "± 297.86708111041185"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 3240046.2122395835,
            "unit": "ns",
            "range": "± 4670.818271378057"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 6896450.057291667,
            "unit": "ns",
            "range": "± 10313.052399783468"
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
          "id": "33992ffac5727bfc63c62fd6d7301186fad152c8",
          "message": "Merge pull request #320 from Chris-Wolfgang/release/prep-0.17.0\n\nrelease: 0.17.0",
          "timestamp": "2026-07-23T21:39:13-04:00",
          "tree_id": "3acdc379432bf9b06f8ae4482e9ba7e9dad6ccb2",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/33992ffac5727bfc63c62fd6d7301186fad152c8"
        },
        "date": 1784857259931,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 32635.177775065105,
            "unit": "ns",
            "range": "± 95.51647773189825"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 35028.11218261719,
            "unit": "ns",
            "range": "± 387.8657479408494"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3220211.7493489585,
            "unit": "ns",
            "range": "± 5734.59641892182"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3439904.2486979165,
            "unit": "ns",
            "range": "± 5769.397030124281"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 33119.71512858073,
            "unit": "ns",
            "range": "± 40.01966686404218"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 31863.15596516927,
            "unit": "ns",
            "range": "± 98.21299228608584"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 69701.81351725261,
            "unit": "ns",
            "range": "± 118.3873780955796"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 3182391.859375,
            "unit": "ns",
            "range": "± 3983.3645830260316"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 3207955.4934895835,
            "unit": "ns",
            "range": "± 7473.333760456796"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 6936332.84375,
            "unit": "ns",
            "range": "± 12973.308060016172"
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
          "id": "45799097148b7de8d814fbda158240f01cc067bd",
          "message": "Merge pull request #326 from Chris-Wolfgang/vNext\n\nrelease: Wolfgang.Etl.Abstractions 0.18.0 — per-item error handling (#84) [HOLD until 2026-07-25]",
          "timestamp": "2026-07-25T09:01:09-04:00",
          "tree_id": "7d597ea3c0643907c43fa7db075a91cec67ee978",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/45799097148b7de8d814fbda158240f01cc067bd"
        },
        "date": 1784984610248,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 31681.81527709961,
            "unit": "ns",
            "range": "± 92.93472870511734"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34341.70011393229,
            "unit": "ns",
            "range": "± 141.33996407345737"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3036697.7369791665,
            "unit": "ns",
            "range": "± 4708.394707196401"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3413362.4596354165,
            "unit": "ns",
            "range": "± 24700.402795805367"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29214.95098368327,
            "unit": "ns",
            "range": "± 85.95318478018915"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29373.47754414876,
            "unit": "ns",
            "range": "± 700.2322193031289"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 68595.50526936848,
            "unit": "ns",
            "range": "± 423.2903092461912"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 3054795.6002604165,
            "unit": "ns",
            "range": "± 4768.193788072737"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2922521.51171875,
            "unit": "ns",
            "range": "± 2840.262605891472"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 7287177.130208333,
            "unit": "ns",
            "range": "± 38994.55034843781"
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
          "id": "3a14ba987efec2b7c0e65fdc0aae89bc33f8f5b8",
          "message": "Merge pull request #331 from Chris-Wolfgang/fix/report-timing-ctor\n\nfix: cross-assembly-safe Report timing constructor (0.18.1)",
          "timestamp": "2026-07-27T20:06:37-04:00",
          "tree_id": "8bdb1ced7eeae2d1d36ab263f92347fb1e402240",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/3a14ba987efec2b7c0e65fdc0aae89bc33f8f5b8"
        },
        "date": 1785197326245,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 31930.534067789715,
            "unit": "ns",
            "range": "± 658.2371578009119"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34010.91799926758,
            "unit": "ns",
            "range": "± 129.0833785595842"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3106942.7135416665,
            "unit": "ns",
            "range": "± 3078.2601995964997"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3369861.2369791665,
            "unit": "ns",
            "range": "± 3214.804757548345"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29861.78623453776,
            "unit": "ns",
            "range": "± 94.08997512637652"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29908.161814371746,
            "unit": "ns",
            "range": "± 70.64600581703343"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 68483.69946289062,
            "unit": "ns",
            "range": "± 285.6650714675182"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2854886.4140625,
            "unit": "ns",
            "range": "± 1462.1467840974312"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2870112.7330729165,
            "unit": "ns",
            "range": "± 326.26898943322766"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 6728828.515625,
            "unit": "ns",
            "range": "± 4243.902731655375"
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
          "id": "d4f55e21d36483278355efdcad57d48eea279c34",
          "message": "Merge pull request #337 from Chris-Wolfgang/vNext\n\nRelease v0.19.0 — overflow-safe progress counters",
          "timestamp": "2026-07-28T20:54:06-04:00",
          "tree_id": "39224f14ae5d7b1d4a2e8c5c15dbbee6ce02137c",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/d4f55e21d36483278355efdcad57d48eea279c34"
        },
        "date": 1785286563632,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 32910.385080973305,
            "unit": "ns",
            "range": "± 246.59303003672085"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34004.371744791664,
            "unit": "ns",
            "range": "± 117.01027694801975"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3169858.515625,
            "unit": "ns",
            "range": "± 5500.413298171225"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3394756.78125,
            "unit": "ns",
            "range": "± 3067.723086076393"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29437.69014485677,
            "unit": "ns",
            "range": "± 180.6071452454448"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29571.55258178711,
            "unit": "ns",
            "range": "± 74.8637522365654"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 67904.77030436198,
            "unit": "ns",
            "range": "± 161.19700302019683"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2847659.9375,
            "unit": "ns",
            "range": "± 472.1922286296104"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2848226.8541666665,
            "unit": "ns",
            "range": "± 5792.383492941568"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 6883109.989583333,
            "unit": "ns",
            "range": "± 30238.32394807222"
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
          "id": "5e5eff855352bb5ee95792a62df438772271635e",
          "message": "Merge pull request #341 from Chris-Wolfgang/chore/post-release-baseline-0.19.0\n\nPost-release: baseline → 0.19.0 (drop int→long suppressions)",
          "timestamp": "2026-07-28T21:51:08-04:00",
          "tree_id": "0736dc53373adf4496b21934bd9fe74b6a8adfe1",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/5e5eff855352bb5ee95792a62df438772271635e"
        },
        "date": 1785289988073,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 34346.27861531576,
            "unit": "ns",
            "range": "± 767.7271166066424"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 35690.69081624349,
            "unit": "ns",
            "range": "± 121.99168762232769"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3607541.7135416665,
            "unit": "ns",
            "range": "± 4820.553954320889"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3485849.25,
            "unit": "ns",
            "range": "± 1446.7723543303268"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 31901.971710205078,
            "unit": "ns",
            "range": "± 107.64253574413279"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 32595.139221191406,
            "unit": "ns",
            "range": "± 132.60242047288244"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 74138.58180745442,
            "unit": "ns",
            "range": "± 339.1681844140048"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 3219619.8020833335,
            "unit": "ns",
            "range": "± 3141.7580180829386"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 3208296.451171875,
            "unit": "ns",
            "range": "± 1993.4468905203664"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8852946.604166666,
            "unit": "ns",
            "range": "± 19787.514036475975"
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
          "id": "9bbcaed318006bc970b34e9430583b1e12098914",
          "message": "Merge pull request #342 from Chris-Wolfgang/vNext-plus-one\n\nRelease v0.20.0 — retry seam, middleware, error aggregation",
          "timestamp": "2026-07-29T20:59:36-04:00",
          "tree_id": "6d0f66ea27f741762df0d8b106e21fa794d64a14",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/9bbcaed318006bc970b34e9430583b1e12098914"
        },
        "date": 1785373304561,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 33264.83210245768,
            "unit": "ns",
            "range": "± 858.3482366946234"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34792.25638834635,
            "unit": "ns",
            "range": "± 64.48475901895517"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3117587.5625,
            "unit": "ns",
            "range": "± 78193.90704888884"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3403166.25390625,
            "unit": "ns",
            "range": "± 3988.759794655839"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29909.5511525472,
            "unit": "ns",
            "range": "± 138.77199709350862"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29191.49988301595,
            "unit": "ns",
            "range": "± 134.40664293862665"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 81575.67272949219,
            "unit": "ns",
            "range": "± 1298.3885923522498"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2939807.4361979165,
            "unit": "ns",
            "range": "± 6446.269778291123"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2862787.6510416665,
            "unit": "ns",
            "range": "± 2886.522125540834"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 6625909.760416667,
            "unit": "ns",
            "range": "± 5634.876130157398"
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
          "id": "5e5e9585b917785988ace8354c41942e02477cf0",
          "message": "Merge pull request #343 from Chris-Wolfgang/chore/post-release-baseline-0.20.0\n\nPost-release: baseline → 0.20.0",
          "timestamp": "2026-07-29T21:44:04-04:00",
          "tree_id": "a4a03457e162855e2850e7a4f00aea4039ce212c",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/5e5e9585b917785988ace8354c41942e02477cf0"
        },
        "date": 1785375973439,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 31583.873189290363,
            "unit": "ns",
            "range": "± 115.28689686427228"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 35160.96792602539,
            "unit": "ns",
            "range": "± 104.53835631316898"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3107534.1640625,
            "unit": "ns",
            "range": "± 17584.772051305485"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3448047.61328125,
            "unit": "ns",
            "range": "± 4145.027564723044"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29626.396423339844,
            "unit": "ns",
            "range": "± 122.33354576748134"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 28745.347229003906,
            "unit": "ns",
            "range": "± 56.763752733836895"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 69584.28100585938,
            "unit": "ns",
            "range": "± 220.36328762445214"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2835179.5182291665,
            "unit": "ns",
            "range": "± 5152.038095929195"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2925295.2018229165,
            "unit": "ns",
            "range": "± 3396.484964473814"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 6793926.755208333,
            "unit": "ns",
            "range": "± 24027.364375768884"
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
          "id": "0b4e021245d0e9a77325da1f4927d080778529f0",
          "message": "Merge pull request #349 from Chris-Wolfgang/vNext\n\nRelease 0.21.0",
          "timestamp": "2026-08-03T14:13:29-04:00",
          "tree_id": "8afdfd7100f226e5f0dcf9fc19117733741a11d6",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/0b4e021245d0e9a77325da1f4927d080778529f0"
        },
        "date": 1785780948422,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 31739.802724202473,
            "unit": "ns",
            "range": "± 301.7022761879442"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34488.93621826172,
            "unit": "ns",
            "range": "± 93.33746794560396"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3124715.6940104165,
            "unit": "ns",
            "range": "± 9282.169049371676"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3391397.7330729165,
            "unit": "ns",
            "range": "± 370.5264434506989"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29440.852060953777,
            "unit": "ns",
            "range": "± 101.35993870949059"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29524.820373535156,
            "unit": "ns",
            "range": "± 161.11819784502399"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 69229.44856770833,
            "unit": "ns",
            "range": "± 174.2017859194949"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2909190.2252604165,
            "unit": "ns",
            "range": "± 10940.777310382022"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2910457.6940104165,
            "unit": "ns",
            "range": "± 2853.2617680686776"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 7096662.15625,
            "unit": "ns",
            "range": "± 29807.687845401626"
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
          "id": "a03ef2fedf6c89a31bb1cd7c9936cbb36f628c02",
          "message": "Merge pull request #357 from Chris-Wolfgang/chore/fold-testkit\n\nRelease 0.22.0 — fold ETL-Test-Kit into this repo (#356)",
          "timestamp": "2026-08-13T09:34:05-04:00",
          "tree_id": "51e4b88cfe29fbea79eef3dbc28cee1818b301e2",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/a03ef2fedf6c89a31bb1cd7c9936cbb36f628c02"
        },
        "date": 1786628163253,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 33471.8779703776,
            "unit": "ns",
            "range": "± 20.18028438729984"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34943.3779296875,
            "unit": "ns",
            "range": "± 1031.666046386766"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3231163.9069010415,
            "unit": "ns",
            "range": "± 7840.546767812861"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3397719.4654947915,
            "unit": "ns",
            "range": "± 28863.376913568358"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 32600.56509399414,
            "unit": "ns",
            "range": "± 115.09383110930203"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 31778.29941813151,
            "unit": "ns",
            "range": "± 225.02069810452966"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 81979.81184895833,
            "unit": "ns",
            "range": "± 421.7887800130081"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 3218792.5442708335,
            "unit": "ns",
            "range": "± 2829.9844480008574"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 3215802.41796875,
            "unit": "ns",
            "range": "± 1533.5868509907941"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8103550.541666667,
            "unit": "ns",
            "range": "± 2795.965619633963"
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
          "id": "9f7a22382c7e9c14e57c50983a9206e1c872c549",
          "message": "Merge pull request #384 from Chris-Wolfgang/chore/baseline-0.22.0\n\nchore(release): align all four PackageValidation baselines to 0.22.0",
          "timestamp": "2026-08-14T15:20:57-04:00",
          "tree_id": "60131ce865720bf0ec5b76b065ba9b25343fd5c5",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/9f7a22382c7e9c14e57c50983a9206e1c872c549"
        },
        "date": 1786735374605,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 32261.896341959637,
            "unit": "ns",
            "range": "± 364.15799521714877"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 35381.66305541992,
            "unit": "ns",
            "range": "± 290.01444589193056"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3147298.1979166665,
            "unit": "ns",
            "range": "± 4080.1891642462997"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3460913.9778645835,
            "unit": "ns",
            "range": "± 12980.283555153379"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 30769.176920572918,
            "unit": "ns",
            "range": "± 67.1035212022148"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29653.87747701009,
            "unit": "ns",
            "range": "± 69.75862046871877"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 84119.70609537761,
            "unit": "ns",
            "range": "± 199.43699171620807"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2858407.7447916665,
            "unit": "ns",
            "range": "± 718.8118438209126"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2921320.4205729165,
            "unit": "ns",
            "range": "± 6424.736099927367"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8379163.734375,
            "unit": "ns",
            "range": "± 2341.3678513388713"
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
          "id": "14b501ec91d5b9fbd80bdb0a2f12591dc7b98cf8",
          "message": "Merge pull request #387 from Chris-Wolfgang/vNext\n\nRelease 0.23.0 — WorkerResilience (#348) + code-scanning cleanup",
          "timestamp": "2026-08-15T15:10:19-04:00",
          "tree_id": "76dbc4cd9d9837465be4ba61e290813ae96bbd92",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/14b501ec91d5b9fbd80bdb0a2f12591dc7b98cf8"
        },
        "date": 1786821141292,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 31437.94061279297,
            "unit": "ns",
            "range": "± 27.96641033811806"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 35441.39617919922,
            "unit": "ns",
            "range": "± 192.632709964561"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3098821.1002604165,
            "unit": "ns",
            "range": "± 9825.677890218283"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3367759.9244791665,
            "unit": "ns",
            "range": "± 13412.899773968795"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29370.526423136394,
            "unit": "ns",
            "range": "± 120.41734342174317"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 28920.124740600586,
            "unit": "ns",
            "range": "± 55.27250065737482"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 83099.82495117188,
            "unit": "ns",
            "range": "± 222.44344040847193"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2850184.8151041665,
            "unit": "ns",
            "range": "± 11499.41639210105"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2887731.9557291665,
            "unit": "ns",
            "range": "± 15540.280690202235"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8131420.734375,
            "unit": "ns",
            "range": "± 6753.498115722863"
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
          "id": "23c2c320c65c6abc5d5bc11c1b4272a82404c967",
          "message": "Merge pull request #392 from Chris-Wolfgang/vNext\n\nchore: bump PackageValidation baselines to 0.23.0 (main)",
          "timestamp": "2026-08-15T16:58:03-04:00",
          "tree_id": "6f28a413495a08fd1b62609dbf120480d645b218",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/23c2c320c65c6abc5d5bc11c1b4272a82404c967"
        },
        "date": 1786827605742,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 31800.580780029297,
            "unit": "ns",
            "range": "± 307.54830997169506"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 36281.488749186195,
            "unit": "ns",
            "range": "± 253.8979487087474"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3127990.4661458335,
            "unit": "ns",
            "range": "± 11429.375798740686"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3916759.8196614585,
            "unit": "ns",
            "range": "± 15055.495493975353"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29872.644785563152,
            "unit": "ns",
            "range": "± 118.63128704029951"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 28800.820571899414,
            "unit": "ns",
            "range": "± 74.11664119421287"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 82518.81917317708,
            "unit": "ns",
            "range": "± 99.99541646215732"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2845985.0143229165,
            "unit": "ns",
            "range": "± 2552.770328339048"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2865319.8111979165,
            "unit": "ns",
            "range": "± 20383.721023751135"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8191735.723958333,
            "unit": "ns",
            "range": "± 1123.8189825546328"
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
          "id": "e876d612b36696607440db69ca6b6c01d310a44c",
          "message": "Merge pull request #407 from Chris-Wolfgang/fix/benchmark-lockfile-churn\n\nfix: clean lockfile churn breaking the benchmark workflows (gh-pages switch)",
          "timestamp": "2026-08-17T08:38:12-04:00",
          "tree_id": "093218982f2d56985dad99c65d911fab322db47b",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/e876d612b36696607440db69ca6b6c01d310a44c"
        },
        "date": 1786970414042,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 31903.78672281901,
            "unit": "ns",
            "range": "± 144.72965526012857"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34751.544413248695,
            "unit": "ns",
            "range": "± 203.21047918256886"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3173447.02734375,
            "unit": "ns",
            "range": "± 5514.84562726297"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3456607.87890625,
            "unit": "ns",
            "range": "± 4092.332152079959"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29015.653350830078,
            "unit": "ns",
            "range": "± 66.90881584431567"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29097.257415771484,
            "unit": "ns",
            "range": "± 34.33747458756494"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 85416.88513183594,
            "unit": "ns",
            "range": "± 1048.0419648363127"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2875124.4192708335,
            "unit": "ns",
            "range": "± 2820.0048603268488"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2873499.3216145835,
            "unit": "ns",
            "range": "± 1566.7688225288678"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8068340.53125,
            "unit": "ns",
            "range": "± 9875.29665358656"
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
          "id": "5186db0108d70de1e05bdd7c80fead246b414179",
          "message": "Merge pull request #406 from Chris-Wolfgang/vNext\n\nRelease 0.23.1",
          "timestamp": "2026-08-17T18:27:23-04:00",
          "tree_id": "512f3f4a93b74566cf729632273079e8019d7df1",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/5186db0108d70de1e05bdd7c80fead246b414179"
        },
        "date": 1787005768447,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 33511.38164265951,
            "unit": "ns",
            "range": "± 121.16583148247912"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 35223.92938232422,
            "unit": "ns",
            "range": "± 174.25090942486918"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3226846.8424479165,
            "unit": "ns",
            "range": "± 1713.7169739208118"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3335411.8841145835,
            "unit": "ns",
            "range": "± 2517.3013795519155"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29808.294474283855,
            "unit": "ns",
            "range": "± 131.02264257398073"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29762.065439860027,
            "unit": "ns",
            "range": "± 99.2941933996938"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 84486.51436360677,
            "unit": "ns",
            "range": "± 336.75281547310556"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2855788.0455729165,
            "unit": "ns",
            "range": "± 2140.497536501959"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2959976.84765625,
            "unit": "ns",
            "range": "± 8008.527434948032"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 9407115.291666666,
            "unit": "ns",
            "range": "± 21711.610142582238"
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
          "id": "2b9ca55989c934f9cc2793121086a44a0da16da1",
          "message": "Merge pull request #421 from Chris-Wolfgang/vNext\n\nRelease 0.23.2",
          "timestamp": "2026-08-18T14:09:28-04:00",
          "tree_id": "818e990de6a3a9ead077dbc823b7c8c6927a55ed",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/2b9ca55989c934f9cc2793121086a44a0da16da1"
        },
        "date": 1787076686611,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 31941.615783691406,
            "unit": "ns",
            "range": "± 531.698054050727"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34885.07734171549,
            "unit": "ns",
            "range": "± 111.25141048986065"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3165365.1953125,
            "unit": "ns",
            "range": "± 3272.5724322457777"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3466908.1744791665,
            "unit": "ns",
            "range": "± 2160.342846739653"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 30409.424173990887,
            "unit": "ns",
            "range": "± 821.5457191306077"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29108.52246602376,
            "unit": "ns",
            "range": "± 56.43634834694361"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 82429.61051432292,
            "unit": "ns",
            "range": "± 309.00074671186957"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2855155.029296875,
            "unit": "ns",
            "range": "± 7420.317434770811"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2826378.8815104165,
            "unit": "ns",
            "range": "± 2601.2001881317738"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8001211.21875,
            "unit": "ns",
            "range": "± 33412.231794936335"
          }
        ]
      }
    ]
  }
}