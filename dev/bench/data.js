window.BENCHMARK_DATA = {
  "lastUpdate": 1789921432330,
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
          "id": "0e843945b98370e3973c8f0d229a0cc1b497e7e2",
          "message": "Merge pull request #425 from Chris-Wolfgang/chore/baseline-0.23.2\n\nchore: bump PackageValidation baselines to 0.23.2 (no version change)",
          "timestamp": "2026-08-18T21:08:05-04:00",
          "tree_id": "3e4e44523e2a19eedc7a52729dd48b2e3ea608c5",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/0e843945b98370e3973c8f0d229a0cc1b497e7e2"
        },
        "date": 1787101809236,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 32193.258728027344,
            "unit": "ns",
            "range": "± 656.9553182984853"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 35698.08504231771,
            "unit": "ns",
            "range": "± 137.5391388325523"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3162559.00390625,
            "unit": "ns",
            "range": "± 4527.364080952584"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3376237.1223958335,
            "unit": "ns",
            "range": "± 8282.149531846426"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29751.945119222004,
            "unit": "ns",
            "range": "± 155.089617437925"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 28961.55145263672,
            "unit": "ns",
            "range": "± 120.576117905684"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 81686.82682291667,
            "unit": "ns",
            "range": "± 163.44912601062592"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2875148.3125,
            "unit": "ns",
            "range": "± 6159.261124918694"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2932113.671875,
            "unit": "ns",
            "range": "± 10276.737699830326"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8228873.229166667,
            "unit": "ns",
            "range": "± 16100.182434317143"
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
          "id": "d8a846d07ac4ead047bc74f3604b1553b0a33d43",
          "message": "Merge pull request #430 from Chris-Wolfgang/vNext\n\nRelease 0.23.3",
          "timestamp": "2026-08-20T22:38:36-04:00",
          "tree_id": "488d5288e3296d4b5b818a103f0c81f57a741fc6",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/d8a846d07ac4ead047bc74f3604b1553b0a33d43"
        },
        "date": 1787280037194,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 32639.121470133465,
            "unit": "ns",
            "range": "± 478.5666780075057"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 35884.88543701172,
            "unit": "ns",
            "range": "± 85.63791382131966"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3105382.15625,
            "unit": "ns",
            "range": "± 1091.871989750329"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3388120.4752604165,
            "unit": "ns",
            "range": "± 1873.5742712000936"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 30123.428064982098,
            "unit": "ns",
            "range": "± 377.62531952574585"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29345.31621805827,
            "unit": "ns",
            "range": "± 92.86166167974733"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 92536.99104817708,
            "unit": "ns",
            "range": "± 142.31413280578664"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2899623.9765625,
            "unit": "ns",
            "range": "± 4123.284480542903"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2902067.9869791665,
            "unit": "ns",
            "range": "± 43121.239055283506"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8305891.96875,
            "unit": "ns",
            "range": "± 6335.406558614803"
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
          "id": "a6987f4cae434922e189c0d0d1bcf713343a39b5",
          "message": "Merge pull request #439 from Chris-Wolfgang/chore/baseline-0.23.3\n\nchore(pack): advance PackageValidation baseline to 0.23.3",
          "timestamp": "2026-08-28T09:17:29-04:00",
          "tree_id": "36b7341fa8650901422b8a753d6130ab3ec82229",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/a6987f4cae434922e189c0d0d1bcf713343a39b5"
        },
        "date": 1787923176444,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 32405.371083577473,
            "unit": "ns",
            "range": "± 105.3064308353556"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 35510.485107421875,
            "unit": "ns",
            "range": "± 152.8583812973163"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3200594.2122395835,
            "unit": "ns",
            "range": "± 3615.8032911276573"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3551832.28515625,
            "unit": "ns",
            "range": "± 3053.2579160516007"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29770.02052307129,
            "unit": "ns",
            "range": "± 68.8173078574188"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29147.501200358074,
            "unit": "ns",
            "range": "± 67.10825606912064"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 82903.7920735677,
            "unit": "ns",
            "range": "± 208.6027206506947"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2843321.8802083335,
            "unit": "ns",
            "range": "± 1721.7832741027366"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2884368.9479166665,
            "unit": "ns",
            "range": "± 10026.268405142719"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8161874.380208333,
            "unit": "ns",
            "range": "± 61661.71536463339"
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
          "id": "2a8dcc9e50d5d5495449a9b731c4497f74fca808",
          "message": "Merge pull request #446 from Chris-Wolfgang/protected/423-workflow-timeouts\n\nmaint(ci): add fail-fast timeout-minutes to 11 workflows — protected-only",
          "timestamp": "2026-08-28T19:41:48-04:00",
          "tree_id": "4f89399a76a09db06cd2c75cb05c11de01884e8e",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/2a8dcc9e50d5d5495449a9b731c4497f74fca808"
        },
        "date": 1787960619012,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 33152.15584309896,
            "unit": "ns",
            "range": "± 74.42253095545618"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34883.86951700846,
            "unit": "ns",
            "range": "± 270.63914765271215"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3311788.1315104165,
            "unit": "ns",
            "range": "± 16195.801878302338"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3353642.3404947915,
            "unit": "ns",
            "range": "± 4964.934733314425"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 32737.667622884113,
            "unit": "ns",
            "range": "± 99.61515510201956"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 32530.289510091145,
            "unit": "ns",
            "range": "± 176.80798570955477"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 83308.96950276692,
            "unit": "ns",
            "range": "± 272.8580502981411"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 3208386.046875,
            "unit": "ns",
            "range": "± 8818.216003460584"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 3493050.4427083335,
            "unit": "ns",
            "range": "± 3205.84455295388"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8063269.708333333,
            "unit": "ns",
            "range": "± 6942.753565050598"
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
          "id": "93fc736132cb96c8d9822771d2b01a96ba4420c6",
          "message": "Merge pull request #448 from Chris-Wolfgang/vNext\n\nRelease 0.23.4 — coverage, CI hardening, and a real zero-tests fix",
          "timestamp": "2026-08-29T18:26:37-04:00",
          "tree_id": "68a7807251e69ee4e0a75c75437cb60ecfcefbdf",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/93fc736132cb96c8d9822771d2b01a96ba4420c6"
        },
        "date": 1788042515857,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 33889.70786539713,
            "unit": "ns",
            "range": "± 830.3864578999606"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34609.53389485677,
            "unit": "ns",
            "range": "± 116.88833969552442"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3256373.8372395835,
            "unit": "ns",
            "range": "± 5051.665926651925"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3337344.7220052085,
            "unit": "ns",
            "range": "± 2504.3866437117863"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 31630.97471110026,
            "unit": "ns",
            "range": "± 102.53484705465846"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 32540.54865519206,
            "unit": "ns",
            "range": "± 144.961480388436"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 78093.75136311848,
            "unit": "ns",
            "range": "± 221.86144122701356"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 3209895.8268229165,
            "unit": "ns",
            "range": "± 6091.457162889243"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 3285814.0598958335,
            "unit": "ns",
            "range": "± 2550.154631184043"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8180048.708333333,
            "unit": "ns",
            "range": "± 23244.817269094987"
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
          "id": "b223d1a3aeda11a502010c82a1a8394af6e97f2d",
          "message": "Merge pull request #450 from Chris-Wolfgang/chore/baseline-0.23.4\n\nchore(pack): advance PackageValidation baseline to 0.23.4",
          "timestamp": "2026-08-29T19:27:56-04:00",
          "tree_id": "7b12dc398aa5240aa388348165de32cb8c11f5d5",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/b223d1a3aeda11a502010c82a1a8394af6e97f2d"
        },
        "date": 1788046190713,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 33349.0617574056,
            "unit": "ns",
            "range": "± 384.3338719710043"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 35389.802185058594,
            "unit": "ns",
            "range": "± 108.66662028329391"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3105726.2838541665,
            "unit": "ns",
            "range": "± 1800.530410795096"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3395312.1341145835,
            "unit": "ns",
            "range": "± 15189.550764052632"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29613.215260823566,
            "unit": "ns",
            "range": "± 82.52343596946655"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29486.92791748047,
            "unit": "ns",
            "range": "± 109.78649254373985"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 83015.55013020833,
            "unit": "ns",
            "range": "± 458.0728976113841"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2892520.7161458335,
            "unit": "ns",
            "range": "± 4727.303384370142"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2883051.57421875,
            "unit": "ns",
            "range": "± 16073.432288863438"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8202205.151041667,
            "unit": "ns",
            "range": "± 16044.558794692397"
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
          "id": "e1c083ba99a1426984f13311527266a5dddea853",
          "message": "Merge pull request #458 from Chris-Wolfgang/fix/sourcelink-cve-2026-62900\n\nfix(deps): Microsoft.SourceLink.GitHub 8.0.0 → 10.0.401 (CVE-2026-62900)",
          "timestamp": "2026-09-11T08:44:44-04:00",
          "tree_id": "0b169db3a4c7a44f40250bb5fd27e1bb99d452e7",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/e1c083ba99a1426984f13311527266a5dddea853"
        },
        "date": 1789130810692,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 31796.615234375,
            "unit": "ns",
            "range": "± 88.62777469048338"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34752.14018758138,
            "unit": "ns",
            "range": "± 116.49667773471708"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3153651.12109375,
            "unit": "ns",
            "range": "± 6090.108902540985"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3482737.3463541665,
            "unit": "ns",
            "range": "± 46017.864015236686"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29626.53611755371,
            "unit": "ns",
            "range": "± 101.57271284831161"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29243.02215576172,
            "unit": "ns",
            "range": "± 205.48304822062852"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 97217.30033365886,
            "unit": "ns",
            "range": "± 369.69257184016243"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2877790.87890625,
            "unit": "ns",
            "range": "± 6491.063861268904"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2870840.91015625,
            "unit": "ns",
            "range": "± 2844.665193797695"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8429465.088541666,
            "unit": "ns",
            "range": "± 55572.00887950597"
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
          "id": "5745bf45337b30da463ae362d8536c369592baff",
          "message": "Merge pull request #457 from Chris-Wolfgang/feat/456-isupportdryrun-readonly\n\nfeat!: remove ISupportDryRun",
          "timestamp": "2026-09-11T12:36:25-04:00",
          "tree_id": "05c12aeade32daaad0414d615402aacabb4ae43b",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/5745bf45337b30da463ae362d8536c369592baff"
        },
        "date": 1789144703863,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 33211.41327921549,
            "unit": "ns",
            "range": "± 144.704654190219"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34319.38602701823,
            "unit": "ns",
            "range": "± 101.9742468093821"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3241258.0859375,
            "unit": "ns",
            "range": "± 571.4761305903097"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3389418.9192708335,
            "unit": "ns",
            "range": "± 4862.928965460322"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 32598.009684244793,
            "unit": "ns",
            "range": "± 171.45138200376672"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 31863.78057861328,
            "unit": "ns",
            "range": "± 111.67640171903086"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 82200.68927001953,
            "unit": "ns",
            "range": "± 375.3757725515198"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 3235270.1354166665,
            "unit": "ns",
            "range": "± 41087.175726277506"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 3232338.4576822915,
            "unit": "ns",
            "range": "± 4640.6165039582065"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8154517.744791667,
            "unit": "ns",
            "range": "± 5175.779299394287"
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
          "id": "f59739eecb7120128638f38bcbbad225e04fba87",
          "message": "Merge pull request #460 from Chris-Wolfgang/normalized-ctor\n\nfeat: per-stage options records and base constructors that accept them (ADR-0009)",
          "timestamp": "2026-09-13T19:02:37-04:00",
          "tree_id": "88f725334bb5dbe7c79aa749f53be6569ea7ffb1",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/f59739eecb7120128638f38bcbbad225e04fba87"
        },
        "date": 1789340674817,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 27612.17901611328,
            "unit": "ns",
            "range": "± 457.42849624744264"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 29881.994862874348,
            "unit": "ns",
            "range": "± 391.7440246436953"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 2773177.1790364585,
            "unit": "ns",
            "range": "± 41716.784575071986"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 2932528.7213541665,
            "unit": "ns",
            "range": "± 24573.84522330158"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 21874.726704915363,
            "unit": "ns",
            "range": "± 499.8092208318859"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 21767.207000732422,
            "unit": "ns",
            "range": "± 349.40064960805415"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 69833.89241536458,
            "unit": "ns",
            "range": "± 741.8331796728368"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2173298.396484375,
            "unit": "ns",
            "range": "± 32794.03627948321"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2163320.2819010415,
            "unit": "ns",
            "range": "± 16084.653581029384"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 6837642.986979167,
            "unit": "ns",
            "range": "± 14127.058345506144"
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
          "id": "65bd49cbbd38fd25a6c0b581178266ccb75a7e4d",
          "message": "Merge pull request #464 from Chris-Wolfgang/keep-hidden-ctors\n\nrefactor(ctors): keep the hidden parameterless base ctors permanently; chain to this(options: null); prove identity by reflection",
          "timestamp": "2026-09-14T20:22:07-04:00",
          "tree_id": "46f0a40da3f2088ef8633bb0baa3c1ee2e977615",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/65bd49cbbd38fd25a6c0b581178266ccb75a7e4d"
        },
        "date": 1789431852997,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 33804.43271891276,
            "unit": "ns",
            "range": "± 801.4899169932419"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 35381.165364583336,
            "unit": "ns",
            "range": "± 91.13464482935477"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3157720.7884114585,
            "unit": "ns",
            "range": "± 7403.180515759541"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3772987.9153645835,
            "unit": "ns",
            "range": "± 11463.50273448644"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29242.609771728516,
            "unit": "ns",
            "range": "± 122.82220673217705"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29167.88383992513,
            "unit": "ns",
            "range": "± 186.3082158822811"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 84280.28068033855,
            "unit": "ns",
            "range": "± 318.5380330471152"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2961440.5110677085,
            "unit": "ns",
            "range": "± 558.8244480496504"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2873747.7259114585,
            "unit": "ns",
            "range": "± 3057.5955537858845"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8300260.03125,
            "unit": "ns",
            "range": "± 9900.02991799506"
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
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com",
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang"
          },
          "distinct": true,
          "id": "4f1ec5e9c76a9952f14b803ba9b1e7581318986c",
          "message": "release: 0.24.0\n\nMINOR from 0.23.4. Construction-time configuration for every base stage\n(ADR-0009): ExtractorOptions / LoaderOptions / TransformerOptions records\nand constructors accepting them on all six base classes, with the\nparameterless constructors retained permanently and hidden. Guard\nexceptions unified across target frameworks via C# 14 static extension\npolyfills. Pre-1.0 removals: ISupportDryRun, and the generic\nSupportsDryRunContractTests<TSut> (now non-generic).\n\nThis commit folds PublicAPI.Unshipped into Shipped for Abstractions and\nTestKit.Xunit and dates the CHANGELOG. The Version bump lives in the\nprotected-file PR (Directory.Build.props), as for 0.23.4.\n\nCo-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>",
          "timestamp": "2026-09-16T17:30:19-04:00",
          "tree_id": "296c8e0c4f6d6b279f7d4ec314e2f152c50c0726",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/4f1ec5e9c76a9952f14b803ba9b1e7581318986c"
        },
        "date": 1789594336169,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 33227.087565104164,
            "unit": "ns",
            "range": "± 169.24929311363303"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34641.823333740234,
            "unit": "ns",
            "range": "± 136.80285866999688"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3318417.5859375,
            "unit": "ns",
            "range": "± 31701.36502756203"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3409707.2721354165,
            "unit": "ns",
            "range": "± 28564.504183587203"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 32093.320170084637,
            "unit": "ns",
            "range": "± 256.93580851019937"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 32444.529205322266,
            "unit": "ns",
            "range": "± 135.74492000830534"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 83292.06341552734,
            "unit": "ns",
            "range": "± 232.92296281083176"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 3196354.5872395835,
            "unit": "ns",
            "range": "± 1454.6146657275647"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 3254231.3893229165,
            "unit": "ns",
            "range": "± 13333.763366999401"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8160440.890625,
            "unit": "ns",
            "range": "± 6561.657675238894"
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
          "id": "55a3018429a6dde4678f283258e256fc2dcd1b96",
          "message": "build: drop NuGet lock files and RestorePackagesWithLockFile (#477)\n\nThe 39 packages.lock.json files were never enforced (RestoreLockedMode was\nnever set), so they were pure restore churn. Fleet decision 2026-09-16: lock\nfiles add only a content hash on exact-pinned nuget.org packages and, where\nenforced, break every PR on each SDK patch (D20-Dice NU1004 on the injected\nMicrosoft.NET.ILLink.Tasks). The Scorecard nugetCommand finding is suppressed\nfleet-wide in repo-template#557.\n\nRemoved the property from Directory.Build.props and deleted the lock files.\nVerified a restore creates none. The \"restore tracked files\" comments in\nbenchmarks.yaml / pr-benchmarks.yaml still mention the lock files; harmless,\nleft for the next workflow PR.\n\nCo-authored-by: Chris Wolfgang <cwolfgan@ptd.net>\nCo-authored-by: Claude Opus 5 <noreply@anthropic.com>",
          "timestamp": "2026-09-16T20:57:55-04:00",
          "tree_id": "ca3ebc68654c8040fb4815d0db10222468714249",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/55a3018429a6dde4678f283258e256fc2dcd1b96"
        },
        "date": 1789606787953,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 33240.106282552086,
            "unit": "ns",
            "range": "± 120.70627040181812"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34659.209340413414,
            "unit": "ns",
            "range": "± 116.54088676991505"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3256850.77734375,
            "unit": "ns",
            "range": "± 1991.1497916640094"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3391937.650390625,
            "unit": "ns",
            "range": "± 1783.3050432137204"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 32048.78584798177,
            "unit": "ns",
            "range": "± 604.5208294832626"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 31844.447408040363,
            "unit": "ns",
            "range": "± 155.87988434140843"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 79208.46779378255,
            "unit": "ns",
            "range": "± 262.53913146457336"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 3222946.1497395835,
            "unit": "ns",
            "range": "± 7126.934439180095"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 3235754.71875,
            "unit": "ns",
            "range": "± 46454.77102661182"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8218857.239583333,
            "unit": "ns",
            "range": "± 77241.85214186192"
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
          "id": "eedec58d80275de7a32bcedcf959997b04a3156c",
          "message": "ci: pin every workflow action to a commit SHA with an exact # vX.Y.Z comment (#476)\n\nRan repo-template's scripts/pin-actions.ps1 -PinTags: tag references become\nSHA pins and major-only comments (# v7) become the exact tag on the pinned\ncommit (# v7.0.1), so zizmor's ref-version-mismatch stops firing when the\nmajor tag moves on. Only the ref/comment text changed. Dependabot keeps the\nprecision it finds, so this stays converted.\n\n13 already exact, 71 line(s) rewritten, 0 tag reference(s), 0 pinned SHA(s) with no tag\n\nRefs Chris-Wolfgang/repo-template#447\n\nCo-authored-by: Chris Wolfgang <cwolfgan@ptd.net>\nCo-authored-by: Claude Opus 5 <noreply@anthropic.com>",
          "timestamp": "2026-09-16T21:31:09-04:00",
          "tree_id": "942663bde285f083cc071a7b9bbf761717235fdf",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/eedec58d80275de7a32bcedcf959997b04a3156c"
        },
        "date": 1789608791530,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 32280.99863688151,
            "unit": "ns",
            "range": "± 100.75668824690568"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34784.139709472656,
            "unit": "ns",
            "range": "± 88.5430422918405"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3161952.2005208335,
            "unit": "ns",
            "range": "± 4654.2790647417005"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3386020.0911458335,
            "unit": "ns",
            "range": "± 6476.923909596963"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 30244.0239054362,
            "unit": "ns",
            "range": "± 141.44041764943236"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29517.173431396484,
            "unit": "ns",
            "range": "± 49.878528907423515"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 85846.08439127605,
            "unit": "ns",
            "range": "± 185.3444319214805"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2850144.2350260415,
            "unit": "ns",
            "range": "± 3474.4232776987537"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2901980.0026041665,
            "unit": "ns",
            "range": "± 7316.521670522797"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8285179.697916667,
            "unit": "ns",
            "range": "± 31904.111304753143"
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
          "id": "1e40d76a3fee20e212cd4ef2efeceb629250f186",
          "message": "chore: take template updates via scripts/upgrade.ps1 (safe bucket) + .template-version (#478)\n\n* chore: take template updates via scripts/upgrade.ps1 (safe bucket) and stamp .template-version\n\nBase = repo-template a5b7a4f7a (last template sync in this repo's history).\nIn sync : 3 file(s);Safe    : 25 file(s);Review  : 18 file(s);Removed : 1 file(s) no longer in the template;\n\nApplied the safe bucket (template changed, local untouched since the base, or\nnew in the template), plus: license-audit.yaml taken from the template where\nthe local allowlist was a subset (old .github/license/ layout removed),\ntfm-parity wired into pr.yaml Stage 2 + build-pr.ps1 where the anchors exist,\nand .template-version stamped with this repo's placeholder values. Review\nbucket (customised here AND changed upstream) untouched; sidecars discarded.\n\nRelease build of the solution after the change: ok (with .editorconfig HELD BACK: template version fails the Release build - see log)\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n* ci: inline zizmor ignore for pull_request_target (accepted design) so the new actions-audit gate passes\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n* build: take the template .editorconfig (CA2007 enforced in src/); ConfigureAwait(false) on the six await-using enumerator sites in TestKit.Xunit\n\nThe sweep held .editorconfig back because the template's version (CA2007 =\nwarning in src/, TreatWarningsAsErrors in Release) failed on six\n`await using var enumerator = ….GetAsyncEnumerator();` lines in the shipped\nTestKit.Xunit contract-test bases. Split each into `var enumerator = …;` +\n`await using var enumeratorDisposal = enumerator.ConfigureAwait(false);` so\nthe enumerator keeps its type and disposal does not capture the caller's\nsynchronization context — the library-code rule the .editorconfig enforces.\nNo behaviour change for consumers on a context-free test host; correct for\nconsumers that block on a context. Release build clean; Abstractions and\nTestKit unit suites pass on every TFM.\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n* fix(license-audit): map xunit's license.txt URL to Apache-2.0 (TestKit.Xunit pulls xunit.abstractions; same as repo-template)\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n* chore: review follow-ups — Validate-DocsDeploy resolves links (as repo-template#563); doc says the validator is a manual post-deploy check\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n* chore: review follow-ups (round 2)\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n---------\n\nCo-authored-by: Chris Wolfgang <cwolfgan@ptd.net>\nCo-authored-by: Claude Opus 5 <noreply@anthropic.com>",
          "timestamp": "2026-09-17T17:10:32-04:00",
          "tree_id": "3ecb8af0bbe2a463791a96c1069ba15f84f6153e",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/1e40d76a3fee20e212cd4ef2efeceb629250f186"
        },
        "date": 1789679556752,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 32279.670633951824,
            "unit": "ns",
            "range": "± 402.225202155271"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 35534.12316894531,
            "unit": "ns",
            "range": "± 114.57460445033043"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3167401.578125,
            "unit": "ns",
            "range": "± 3480.1351296530343"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3410260.7591145835,
            "unit": "ns",
            "range": "± 6140.697683895801"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29644.99819437663,
            "unit": "ns",
            "range": "± 116.93136368171547"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29181.739552815754,
            "unit": "ns",
            "range": "± 91.3628702829186"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 82610.88431803386,
            "unit": "ns",
            "range": "± 354.9310820529048"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2921453.59765625,
            "unit": "ns",
            "range": "± 2923.0113285222333"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2886328.6953125,
            "unit": "ns",
            "range": "± 1947.3337494626505"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8107503.541666667,
            "unit": "ns",
            "range": "± 8254.651883771316"
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
          "id": "732de01f9e5540609dc8c23686ed79165e1f5cd5",
          "message": "chore(publicapi): record the 18 synthesized members of Report and EtlPipelineProgress (#479)\n\n* chore(publicapi): record the synthesized members of Report and EtlPipelineProgress\n\nThe 18 compiler-synthesized members of the two shipped records (<Clone>$, copy ctor, Deconstruct, Equals ×2, GetHashCode, ToString, PrintMembers, EqualityContract, == / !=) have shipped unrecorded since the records were introduced because RS0016 is muzzled by the blanket analyzer severity (#459). Harvested by raising RS0016 to warning on this project alone across all 11 target frameworks (identical set on every one) and appended to PublicAPI.Shipped.txt — they are already public, so Shipped, not Unshipped. RS0017 accepts the <Clone>$ lines in the form RS0016 emits on PublicApiAnalyzers 5.6.0 (the blocker recorded in Chris-Wolfgang/Etl-Csv#263 no longer reproduces). ErrorPolicies, TestKit and TestKit.Xunit report nothing.\n\nRefs #459.\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n* chore: changelog fragment (internal) for the PublicAPI backfill\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n---------\n\nCo-authored-by: Claude Opus 5 <noreply@anthropic.com>",
          "timestamp": "2026-09-17T19:24:03-04:00",
          "tree_id": "cdb7740f29362790d2f71a9d74cc6444d442d537",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/732de01f9e5540609dc8c23686ed79165e1f5cd5"
        },
        "date": 1789687559325,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 33342.73376464844,
            "unit": "ns",
            "range": "± 106.53539338959853"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 34876.67784627279,
            "unit": "ns",
            "range": "± 991.9455625322026"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3352254.9537760415,
            "unit": "ns",
            "range": "± 76548.07728985148"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3360102.7102864585,
            "unit": "ns",
            "range": "± 5174.696048400712"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 33041.55806477865,
            "unit": "ns",
            "range": "± 116.71954475744627"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 32504.323099772137,
            "unit": "ns",
            "range": "± 226.46133760845788"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 79203.97163899739,
            "unit": "ns",
            "range": "± 256.2544293646029"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 3208742.4752604165,
            "unit": "ns",
            "range": "± 2696.621808946528"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 3211542.4713541665,
            "unit": "ns",
            "range": "± 4153.568057663132"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8486986.302083334,
            "unit": "ns",
            "range": "± 370882.00087997626"
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
          "id": "64526f48c9b6c8383e660ccacc8556047a961c93",
          "message": "Release v0.25.0 — base-stage setters deprecated (constructor-only ahead); CreateSut takes the base config; bulk increments; TestKit ships net5–7 (#481)\n\n* feat!: make the base-stage configuration init-only (ReportingInterval / MaximumItemCount / SkipItemCount) (#480)\n\n* feat!: make the base-stage configuration init-only (ReportingInterval / MaximumItemCount / SkipItemCount)\n\nCompletes ADR-0009 for the base classes (#351, #438): the nine configuration properties on ExtractorBase, LoaderBase and TransformerBase become { get; init; }, so a stage's configuration is fixed at construction — through the options record or an object initializer — and can no longer be mutated mid-run. ReportingInterval in particular was read once at start-up, so a later assignment was silently inert. Validation and defaults are unchanged.\n\nset → init is a binary break with no possible shim (the accessor gains a modreq(IsExternalInit) signature); pre-1.0, so a MINOR bump, and the ETL-* family republishes against it in lockstep.\n\nTestKit.Xunit: the three base contract classes can no longer assign the properties after construction, so CreateSut takes the base configuration — CreateSut(int itemCount, int maximumItemCount, int skipItemCount, int reportingInterval), no defaults so overrides stay honest — with a protected CreateSut(int itemCount) forwarder for derived tests and a private CreateConfiguredSut for the 41 contract sites; CreateSutOverSource gains maximumItemCount. Six contract tests renamed (set → configured). Abstractions' own tests and the TestKit doubles' tests fold their 30 post-construction assignments into object initializers; GuardParityTests probe the init accessors through construction.\n\nTestKit and TestKit.Xunit now also ship net5.0/net6.0/net7.0 assets: both assign inherited init properties, and the IsExternalInit modreq differs between Abstractions' netstandard2.0 and net5.0+ builds (the Etl-Csv 0.9.0 defect).\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n* chore: changelog fragments instead of a CHANGELOG.md edit; TestKit.Xunit ships net5.0/6.0/7.0 too\n\nThe template upgrade (#478) moved this repo to changelog/unreleased fragments assembled at release time. The TestKit.Xunit TargetFrameworks change the CHANGELOG entry described had not actually been applied (review catch on #480).\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n---------\n\nCo-authored-by: Claude Opus 5 <noreply@anthropic.com>\n\n* release: v0.25.0\n\nMINOR (pre-1.0) with breaking changes: the base-stage configuration (ReportingInterval / MaximumItemCount / SkipItemCount) is init-only on ExtractorBase, LoaderBase and TransformerBase (#351, #438, completing ADR-0009), and the TestKit.Xunit base contract classes take that configuration through CreateSut. TestKit and TestKit.Xunit ship net5.0/6.0/7.0 assets. PublicAPI folded (Abstractions: 9 set accessors removed, 9 init added; TestKit.Xunit: CreateSut signatures + 28 renamed contract tests); CHANGELOG assembled from the 4 fragments; PackageValidationBaselineVersion 0.23.4 -> 0.24.0 (the post-0.24 bump was never done) and CompatibilitySuppressions.xml regenerated against it: Abstractions 99 x CP0002 (9 accessors x 11 TFMs), TestKit.Xunit CP0002/CP0005/CP0012 for the CreateSut change, TestKit none.\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n* feat: bulk IncrementCurrentItemCount(int) / IncrementCurrentSkippedItemCount(int) on the three bases (#475)\n\nOne interlocked add for stages whose source skips or batches for them (DbClient's server-side OFFSET paging, a seek past a header block) instead of count per-item calls. Zero is a no-op; a negative count throws ArgumentOutOfRangeException (ThrowIfLessThan, polyfilled on the older targets). Placed beside the parameterless overloads (S4136), and every cref to the parameterless members now names the overload — the same CS0419 will meet consumers that reference them by bare name, noted in the CHANGELOG entry. Rides the 0.25.0 cut (additive; PublicAPI entries added straight to Shipped, no new PackageValidation suppressions).\n\nCloses #475.\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n* docs: review fixes on the 0.25.0 cut — ctor remarks no longer say the setters remain assignable; contract-test examples forward the base config; 24 renamed tests, not six\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n* refactor!: deprecate the base-stage setters instead of flipping them to init (same shape as the consumers)\n\nReverses the set -> init flip in this cut: ReportingInterval / MaximumItemCount / SkipItemCount on the three bases keep their setters with [Obsolete] on the accessor (\"Configure X through ExtractorOptions passed to the constructor\"), backed by explicit fields the constructor writes directly (the record already validated). No binary break in 0.25; the setters — and the configuration properties as public surface — go in the fleet-wide removal wave (not before 2026-12-15), after which the constructor is the only way to configure a stage.\n\nTestKit's own ADR-0009 step: every double gains constructor overloads taking the base options record (public and protected timer-injecting ones; 27 new PublicAPI entries), and every test that configured a double through an object initializer now passes the record. Test-local doubles take an optional record too. GuardParityTests keep probing the deprecated setters' guards under a narrow CS0618 suppression until removal. PublicAPI: the 9 init lines revert to set; CompatibilitySuppressions.xml for Abstractions is empty again.\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n* docs: the deprecated setters go, the getters stay (read-only after the removal wave)\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n* test: run each bulk-increment double once — counts added before a run are cleared at start\n\nAbstractions.Tests.Unit is gated at 99 % and had slipped to 98.9 %: the three doubles in BulkIncrementTests never ran, leaving their worker bodies and CreateProgressReport uncovered. One real test covers them (bulk-added counts reset when a run starts; every stage reports on completion). Assembly back to 99.1 %.\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n---------\n\nCo-authored-by: Claude Opus 5 <noreply@anthropic.com>",
          "timestamp": "2026-09-18T12:18:32-04:00",
          "tree_id": "6ee4ce1a2d79decfcce3661ce81b9eee24550780",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/64526f48c9b6c8383e660ccacc8556047a961c93"
        },
        "date": 1789748432661,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 31988.785247802734,
            "unit": "ns",
            "range": "± 190.5340239278981"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 35647.11022949219,
            "unit": "ns",
            "range": "± 62.668600672917215"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3123230.5026041665,
            "unit": "ns",
            "range": "± 1155.9797510289798"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3510482.0247395835,
            "unit": "ns",
            "range": "± 2433.1896914586346"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29858.053161621094,
            "unit": "ns",
            "range": "± 62.2608864232474"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29153.439127604168,
            "unit": "ns",
            "range": "± 119.39431906478522"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 79589.09193929036,
            "unit": "ns",
            "range": "± 170.2617222772989"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2849419.10546875,
            "unit": "ns",
            "range": "± 7356.3410167182365"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2882699.3854166665,
            "unit": "ns",
            "range": "± 1900.3095536260018"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8213344.411458333,
            "unit": "ns",
            "range": "± 9193.722245963341"
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
          "id": "9d04577eb39b37ae4e8ac337bb626cc17fdaaaaa",
          "message": "chore(pack): advance PackageValidation baseline to 0.25.0 (#535)\n\nv0.25.0 is published and indexed on nuget.org; CompatibilitySuppressions.xml regenerated against it (the CreateSut entries were one-release-lived and are now pruned).\n\nCo-authored-by: Claude Opus 5 <noreply@anthropic.com>",
          "timestamp": "2026-09-19T07:57:26-04:00",
          "tree_id": "bbd68a4d0a9eb938dbaa22515e6c0160d2acd924",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/9d04577eb39b37ae4e8ac337bb626cc17fdaaaaa"
        },
        "date": 1789819167393,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 31180.103546142578,
            "unit": "ns",
            "range": "± 196.17754625906903"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 35573.23967488607,
            "unit": "ns",
            "range": "± 87.84085851180481"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3141227.6380208335,
            "unit": "ns",
            "range": "± 1241.517830405067"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3430947.14453125,
            "unit": "ns",
            "range": "± 20404.736347658727"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29789.39808654785,
            "unit": "ns",
            "range": "± 46.340612841869174"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29376.90855407715,
            "unit": "ns",
            "range": "± 149.27277966172034"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 100973.73067220052,
            "unit": "ns",
            "range": "± 3809.1442735259698"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2945701.9505208335,
            "unit": "ns",
            "range": "± 1556.5189644286959"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2853493.5143229165,
            "unit": "ns",
            "range": "± 3597.1303357084767"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8173186.713541667,
            "unit": "ns",
            "range": "± 8434.29203084674"
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
          "id": "e7da54fe0dd5a81bc5eeb6141ba3d029928f1859",
          "message": "docs(testkit): constructor crefs in the Type{T}(…) form InspectCode resolves (#594)\n\nResolves 27 InspectCode alerts in the TestKit doubles — doc comments only.\n\nVerified locally: Release build 0 errors.\n\nCo-authored-by: Claude Opus 5 <noreply@anthropic.com>",
          "timestamp": "2026-09-19T14:29:57-04:00",
          "tree_id": "b32c5020ec9e69905b570968ea8f82a1af48da57",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/e7da54fe0dd5a81bc5eeb6141ba3d029928f1859"
        },
        "date": 1789842885246,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 31653.489400227863,
            "unit": "ns",
            "range": "± 96.91145079439244"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 35212.66280110677,
            "unit": "ns",
            "range": "± 100.30714030165832"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3159333.041015625,
            "unit": "ns",
            "range": "± 4702.327283263205"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3707703.2265625,
            "unit": "ns",
            "range": "± 30964.27846354012"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29780.711029052734,
            "unit": "ns",
            "range": "± 136.47427769577652"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29072.24282836914,
            "unit": "ns",
            "range": "± 69.86499060013894"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 82045.8427734375,
            "unit": "ns",
            "range": "± 216.4969215776102"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2872579.3489583335,
            "unit": "ns",
            "range": "± 5231.575677697615"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2837780.6848958335,
            "unit": "ns",
            "range": "± 273.8162163716821"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8207637.072916667,
            "unit": "ns",
            "range": "± 108970.89801271526"
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
          "id": "3dc5941886aa404f3d6d9a350902b8ed2f828752",
          "message": "refactor: drop the dead `= null` default on the six options constructors (S3427) (#595)\n\nResolves the 6 S3427 alerts on the base stages' options constructors.\n\nVerified locally: Release build 0 errors; unit suites green on net462 / net10.0.\n\nCo-authored-by: Claude Opus 5 <noreply@anthropic.com>",
          "timestamp": "2026-09-19T15:27:14-04:00",
          "tree_id": "84f4e7a3eaab5860e405e4aef9783dafe20e0596",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/3dc5941886aa404f3d6d9a350902b8ed2f828752"
        },
        "date": 1789846158446,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 44257.37466430664,
            "unit": "ns",
            "range": "± 120.01520668257537"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 45660.46260579427,
            "unit": "ns",
            "range": "± 151.47891544704467"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 4370885.231770833,
            "unit": "ns",
            "range": "± 9707.433115085481"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 4592653.817708333,
            "unit": "ns",
            "range": "± 1951.0093205612395"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 36217.97878011068,
            "unit": "ns",
            "range": "± 82.80645915037333"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 36529.26510620117,
            "unit": "ns",
            "range": "± 166.29206882365338"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 103448.23838297527,
            "unit": "ns",
            "range": "± 483.4903657122217"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 3586988.4765625,
            "unit": "ns",
            "range": "± 4176.989877688084"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 3590625.2864583335,
            "unit": "ns",
            "range": "± 4156.028943337068"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 10242864.171875,
            "unit": "ns",
            "range": "± 23190.124868086867"
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
          "id": "225bd062488ad48c4e259a963565cee45a772f8f",
          "message": "chore(analyzers): S1133 off for the base stages' scheduled [Obsolete] setters (#596)\n\nResolves the 9 S1133 alerts on the deliberate base-stage obsolete setters.\n\nVerified locally: Release build 0 errors.\n\nCo-authored-by: Claude Opus 5 <noreply@anthropic.com>",
          "timestamp": "2026-09-19T18:28:34-04:00",
          "tree_id": "2b2b1fea1d3311366a2c368a4dcc032a5b7b5719",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/225bd062488ad48c4e259a963565cee45a772f8f"
        },
        "date": 1789857038147,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 32318.462595621746,
            "unit": "ns",
            "range": "± 162.73311563129434"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 35525.18418375651,
            "unit": "ns",
            "range": "± 82.74727209422836"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3132119.28125,
            "unit": "ns",
            "range": "± 20091.06301590814"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3414274.125,
            "unit": "ns",
            "range": "± 1979.6389613352046"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29996.60759480794,
            "unit": "ns",
            "range": "± 101.98501816322162"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29580.75942484538,
            "unit": "ns",
            "range": "± 99.38620880904156"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 83541.12532552083,
            "unit": "ns",
            "range": "± 328.8088781119434"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2963248.8958333335,
            "unit": "ns",
            "range": "± 1852.3984513318244"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2875435.01953125,
            "unit": "ns",
            "range": "± 4380.1624353484"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8464893.854166666,
            "unit": "ns",
            "range": "± 13310.86454981636"
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
          "id": "57e8ee4672833c8c0284b7a778aff4e83b914ce3",
          "message": "chore: CheckNamespace — polyfills disabled once, three test files moved to the folder namespace (#597)\n\nResolves the 4 CheckNamespace alerts.\n\nVerified locally: Release build 0 errors; unit suites green on net462 / net10.0.\n\nCo-authored-by: Claude Opus 5 <noreply@anthropic.com>",
          "timestamp": "2026-09-19T19:04:32-04:00",
          "tree_id": "7ee7a4ad6a8301e87e965ad3be232c5ac8120e63",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/57e8ee4672833c8c0284b7a778aff4e83b914ce3"
        },
        "date": 1789859186393,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 25814.194478352863,
            "unit": "ns",
            "range": "± 41.94079656666468"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 26619.223022460938,
            "unit": "ns",
            "range": "± 97.32526638805696"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 2514468.4192708335,
            "unit": "ns",
            "range": "± 1543.7726179031538"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 2726815.48046875,
            "unit": "ns",
            "range": "± 2390.466429237891"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 24553.623565673828,
            "unit": "ns",
            "range": "± 137.68756848804327"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 24518.875615437824,
            "unit": "ns",
            "range": "± 57.244657948608385"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 62466.78497314453,
            "unit": "ns",
            "range": "± 252.8960418806012"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2471510.4596354165,
            "unit": "ns",
            "range": "± 1053.7358508721668"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2496481.6471354165,
            "unit": "ns",
            "range": "± 2342.839228183183"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 6307302.940104167,
            "unit": "ns",
            "range": "± 2060.7307099471464"
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
          "id": "a445023137786d7976005e7eb130b374293288e3",
          "message": "fix(benchmarks): gate against the median of the last five main runs, not the single latest (#606)\n\nThe gate compared a PR run against only the newest gh-pages data point. Identical code on main has measured 2.5-4.4 ms for Extract_NoProgress(100000) across its last eight runs; when the newest happened to be the 2.5 ms outlier, every following PR (three in a row, none touching src/) tripped the 1.5x ratio and the absolute floor. The baseline is now the per-benchmark median of the last BASELINE_RUNS (5) main entries. Checked against the failing run's real numbers: 1.27x -> passes; a synthetic 1.6x over the median still fails.\n\nCo-authored-by: Claude Opus 5 <noreply@anthropic.com>",
          "timestamp": "2026-09-20T12:21:46-04:00",
          "tree_id": "01a451e805eea4b3d72238d0090cf0bbe0617454",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/a445023137786d7976005e7eb130b374293288e3"
        },
        "date": 1789921427540,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 1000)",
            "value": 31849.83039347331,
            "unit": "ns",
            "range": "± 272.90040911017553"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 1000)",
            "value": 35795.2223815918,
            "unit": "ns",
            "range": "± 55.40819382710325"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_NoProgress(RecordCount: 100000)",
            "value": 3130408.1276041665,
            "unit": "ns",
            "range": "± 16338.51383938957"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.ExtractorBenchmarks.Extract_WithProgress(RecordCount: 100000)",
            "value": 3437195.6223958335,
            "unit": "ns",
            "range": "± 4645.004147025489"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 1000)",
            "value": 29591.8162689209,
            "unit": "ns",
            "range": "± 135.2491901731577"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 1000)",
            "value": 29487.110107421875,
            "unit": "ns",
            "range": "± 66.42865405427189"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 1000)",
            "value": 84031.43322753906,
            "unit": "ns",
            "range": "± 379.54526768367566"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.FluentPipeline(RecordCount: 100000)",
            "value": 2844778.837890625,
            "unit": "ns",
            "range": "± 2153.1248260152493"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.ManualComposition(RecordCount: 100000)",
            "value": 2923127.9192708335,
            "unit": "ns",
            "range": "± 1932.4256568706255"
          },
          {
            "name": "Wolfgang.Etl.Abstractions.Benchmarks.PipelineBenchmarks.BaseClassComposition(RecordCount: 100000)",
            "value": 8138999.515625,
            "unit": "ns",
            "range": "± 11575.377718478108"
          }
        ]
      }
    ]
  }
}