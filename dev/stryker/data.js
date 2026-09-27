window.BENCHMARK_DATA = {
  "lastUpdate": 1790490729583,
  "repoUrl": "https://github.com/Chris-Wolfgang/ETL-Abstractions",
  "entries": {
    "Mutation score": [
      {
        "commit": {
          "author": {
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang",
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com"
          },
          "committer": {
            "name": "GitHub",
            "username": "web-flow",
            "email": "noreply@github.com"
          },
          "id": "45799097148b7de8d814fbda158240f01cc067bd",
          "message": "Merge pull request #326 from Chris-Wolfgang/vNext\n\nrelease: Wolfgang.Etl.Abstractions 0.18.0 — per-item error handling (#84) [HOLD until 2026-07-25]",
          "timestamp": "2026-07-25T13:01:09Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/45799097148b7de8d814fbda158240f01cc067bd"
        },
        "date": 1785049251668,
        "tool": "customBiggerIsBetter",
        "benches": [
          {
            "name": "Mutation score",
            "value": 97.23,
            "unit": "%"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang",
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com"
          },
          "committer": {
            "name": "GitHub",
            "username": "web-flow",
            "email": "noreply@github.com"
          },
          "id": "5e5e9585b917785988ace8354c41942e02477cf0",
          "message": "Merge pull request #343 from Chris-Wolfgang/chore/post-release-baseline-0.20.0\n\nPost-release: baseline → 0.20.0",
          "timestamp": "2026-07-30T01:44:04Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/5e5e9585b917785988ace8354c41942e02477cf0"
        },
        "date": 1785653936961,
        "tool": "customBiggerIsBetter",
        "benches": [
          {
            "name": "Mutation score",
            "value": 100,
            "unit": "%"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang",
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com"
          },
          "committer": {
            "name": "GitHub",
            "username": "web-flow",
            "email": "noreply@github.com"
          },
          "id": "0b4e021245d0e9a77325da1f4927d080778529f0",
          "message": "Merge pull request #349 from Chris-Wolfgang/vNext\n\nRelease 0.21.0",
          "timestamp": "2026-08-03T18:13:29Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/0b4e021245d0e9a77325da1f4927d080778529f0"
        },
        "date": 1786256510861,
        "tool": "customBiggerIsBetter",
        "benches": [
          {
            "name": "Mutation score",
            "value": 100,
            "unit": "%"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang",
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com"
          },
          "committer": {
            "name": "GitHub",
            "username": "web-flow",
            "email": "noreply@github.com"
          },
          "id": "23c2c320c65c6abc5d5bc11c1b4272a82404c967",
          "message": "Merge pull request #392 from Chris-Wolfgang/vNext\n\nchore: bump PackageValidation baselines to 0.23.0 (main)",
          "timestamp": "2026-08-15T20:58:03Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/23c2c320c65c6abc5d5bc11c1b4272a82404c967"
        },
        "date": 1786861158115,
        "tool": "customBiggerIsBetter",
        "benches": [
          {
            "name": "Mutation score",
            "value": 76.13,
            "unit": "%"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang",
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com"
          },
          "committer": {
            "name": "GitHub",
            "username": "web-flow",
            "email": "noreply@github.com"
          },
          "id": "38779945e6de3cb00f8edefeee05180217eaa63e",
          "message": "test: InspectCode nits in the unit tests (usings, casts, using-initialisers, closure, override default) (#598)\n\nResolves 18 InspectCode alerts in test code.\n\nVerified locally: Release build 0 errors; unit suites green on net462 / net10.0.\n\nCo-authored-by: Claude Opus 5 <noreply@anthropic.com>",
          "timestamp": "2026-09-20T00:52:47Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/38779945e6de3cb00f8edefeee05180217eaa63e"
        },
        "date": 1789885572574,
        "tool": "customBiggerIsBetter",
        "benches": [
          {
            "name": "Mutation score",
            "value": 98.27,
            "unit": "%"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang",
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com"
          },
          "committer": {
            "name": "GitHub",
            "username": "web-flow",
            "email": "noreply@github.com"
          },
          "id": "f12341468745a07eacb04df3a7d9ee3461a7d49c",
          "message": "test(sourcelink): add the PDB gates across all four shipped packages (#652)\n\n* test(sourcelink): add the PDB gates across all four shipped packages\n\nPorts the ETL-SqlBulkCopy pilot, adapted for a multi-package repo. Each check\nruns per package rather than against a single PDB, because a symbol defect in\nany one of Abstractions, ErrorPolicies, TestKit or TestKit.Xunit is a broken\ndebugging experience for that package's consumers. Three checks x four packages\n= 12 cases, all passing.\n\nThe checks: the PDB is portable (BSJB magic), it carries a SourceLink record\nmapping this repo's sources to GitHub's raw host, and a real source URL built\nfrom that mapping resolves.\n\nCarries the pilot's two corrections to Etl-DbClient's original: the reachability\ncheck actually probes (DbClient's skips unconditionally, because a SourceLink\nmapping always contains the literal '*' it tested for), and the host is asserted\nby URI equality rather than substring, so a look-alike host cannot pass.\n\nAdded via `dotnet sln add` rather than hand-editing, since this repo still uses\nthe GUID-bearing .sln format.\n\nThe 23 build warnings are pre-existing out-of-support-TFM notices from the\nreferenced src projects' net5.0/6.0/7.0 slices: the existing Tests.Unit project\nemits the identical 23 on this branch. This project adds none.\n\nRefs #214.\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n* test(sourcelink): fail in CI on an unbuildable probe URL; clear the analyser findings\n\nPer Copilot on #652, plus the fixes Chris made to the same file in\nETL-Transformers#334.\n\nThe probe test returned successfully whenever BuildProbeUrl produced nothing. On\na developer machine that is correct -- a local unpushed build has no resolvable\nSHA -- but in CI the same path is taken by a malformed local-prefix mapping, a\ndocument-table mismatch and an unresolved SHA, so a broken PDB could satisfy the\ntwo structural checks and silently skip the resolution check this test exists to\nperform. It now asserts in CI, keyed off the CI environment variable, and keeps\nthe local skip.\n\nAlso ports the five analyser fixes from #334, which were my regression: I rewrote\nEtl-DbClient's version and dropped its ReSharper suppressions for the HttpClient\nfindings while keeping the shape that triggers them. Fixed by removing the cause\nrather than suppressing -- one static readonly HttpClient (which clears both the\nshort-lived-client and object-initialiser-inside-using findings), the redundant\nuri! dropped, the unused using removed, and the comment whose trailing ';' read\nas commented-out code rephrased.\n\nVerified: 12/12 tests, also 12/12 with CI=true so the new guard does not fire\nspuriously, 0 errors, and `jb inspectcode` on the project reports 0 findings.\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n* test(sourcelink): retry the probe before concluding the SHA is unresolvable\n\nFound while verifying the previous commit: rebasing produced a new local commit,\nand the four probe tests went red with \"SourceLink URL 404s\" against a path that\ndemonstrably exists at that commit. The commit was pushed, so the URL was correct.\nraw.githubusercontent.com simply had not indexed it yet -- measured propagation\nwas under two minutes, after which the identical URL returned 200.\n\nCI never saw this because a workflow starts minutes after the push. A developer\ncommitting and testing locally hits it every time, which is a false failure on\ncorrect code.\n\nThe probe now tries three times, five seconds apart, and a persistent 404 fails\nonly in CI, where the commit under test is always pushed and a dead URL is a real\ndefect. Locally it is almost always \"not pushed yet\", which is not something to\nfail a developer for. That is the same asymmetry as the unbuildable-URL guard\nadded in the previous commit.\n\nOne further S125: the first draft of the new comment ended a line with ';', which\nreads as commented-out code -- the exact trigger Chris rephrased away in\nETL-Transformers#334. Reworded, and the script now asserts no comment line in the\nfile ends in ';'.\n\nVerified: 12/12, 12/12 with CI=true, 0 errors, and `jb inspectcode` on the\nproject reports 0 findings.\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n* test(sourcelink): fail the probe on any status that is not a resolved file\n\nPer Copilot: only 404 was treated as a failure, so 400, 401, 500 and 503 all made\nthe reachability gate pass. A malformed URL or an unreadable repository would have\nbeen reported as success, which is the same silent-pass class as the two guards\nadded in the previous commit.\n\nStatuses are now classified rather than compared against one value:\n\n  2xx           resolved -- pass\n  403 / 429     GitHub rate-limiting the runner: infra, tolerated\n  5xx           server-side fault: infra, tolerated\n  404           retried, then fails in CI only (propagation delay is real)\n  other 4xx     the URL itself is wrong -- fail immediately, nothing to wait for\n\nTolerating 403/429/5xx is deliberate: those say nothing about SourceLink, and the\ntwo structural checks still carry the gate. Failing other 4xx immediately is also\ndeliberate -- a malformed URL will not become well-formed by waiting, so retrying\nit would only slow the suite down.\n\nSwitching to integer comparison made `using System.Net;` unused, which the\nanalyser flagged; removed.\n\nVerified per repo: 0 errors, tests green, tests green with CI=true, and\n`jb inspectcode` on the project reports 0 findings.\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n* test(sourcelink): only retry the probe in CI\n\nObserved while re-verifying: the suite took 40 s on ETL-Abstractions instead of\nunder a second. The retry was firing on a local unpushed commit and waiting ten\nseconds per package for a 404 that cannot fail the test anyway, since a persistent\n404 only fails in CI.\n\nThe retry now runs only in CI, where waiting is the point. Locally a 404 skips\nimmediately. ETL-Abstractions goes 40 s back to 656 ms; the single-package suites\nare ~300 ms.\n\nVerified per repo: 0 errors, tests green, and `jb inspectcode` reports 0 findings.\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n---------\n\nCo-authored-by: Claude Opus 5 <noreply@anthropic.com>",
          "timestamp": "2026-09-25T14:55:23Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Abstractions/commit/f12341468745a07eacb04df3a7d9ee3461a7d49c"
        },
        "date": 1790490716754,
        "tool": "customBiggerIsBetter",
        "benches": [
          {
            "name": "Mutation score",
            "value": 98.63,
            "unit": "%"
          },
          {
            "name": "Mutation score (tests/Wolfgang.Etl.TestKit.Tests.Unit)",
            "value": 98.9,
            "unit": "%"
          }
        ]
      }
    ]
  }
}