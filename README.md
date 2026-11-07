# ETL-Abstractions

The core building blocks for the **Wolfgang.Etl** ETL framework — interfaces and base classes for
extractors, loaders, and transformers, plus companion packages for error handling and testing. This
repository builds and releases four NuGet packages in lockstep.

[![PR build](https://img.shields.io/github/actions/workflow/status/Chris-Wolfgang/ETL-Abstractions/pr.yaml?event=pull_request&label=PR%20build&logo=github)](https://github.com/Chris-Wolfgang/ETL-Abstractions/actions/workflows/pr.yaml)
[![release](https://img.shields.io/github/actions/workflow/status/Chris-Wolfgang/ETL-Abstractions/release.yaml?event=release&label=release&logo=github)](https://github.com/Chris-Wolfgang/ETL-Abstractions/actions/workflows/release.yaml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Multi--Targeted-purple.svg)](https://dotnet.microsoft.com/)
[![OpenSSF Scorecard](https://api.scorecard.dev/projects/github.com/Chris-Wolfgang/ETL-Abstractions/badge)](https://scorecard.dev/viewer/?uri=github.com/Chris-Wolfgang/ETL-Abstractions)

---

## Packages

| Package | What it does | Docs |
| --- | --- | --- |
| **Wolfgang.Etl.Abstractions** [![NuGet](https://img.shields.io/nuget/v/Wolfgang.Etl.Abstractions.svg?logo=nuget&label=)](https://www.nuget.org/packages/Wolfgang.Etl.Abstractions/) | The core `ExtractorBase` / `LoaderBase` / `TransformerBase` types, the `EtlPipeline` composer, progress reporting, per-item error handling, and resilience seams. Zero third-party runtime dependencies. | [README](src/Wolfgang.Etl.Abstractions/README.md) |
| **Wolfgang.Etl.ErrorPolicies** [![NuGet](https://img.shields.io/nuget/v/Wolfgang.Etl.ErrorPolicies.svg?logo=nuget&label=)](https://www.nuget.org/packages/Wolfgang.Etl.ErrorPolicies/) | Ready-made item-error policies — skip, log, and dead-letter — to assign to a stage's `ErrorPolicy`. | [README](src/Wolfgang.Etl.ErrorPolicies/README.md) |
| **Wolfgang.Etl.TestKit** [![NuGet](https://img.shields.io/nuget/v/Wolfgang.Etl.TestKit.svg?logo=nuget&label=)](https://www.nuget.org/packages/Wolfgang.Etl.TestKit/) | Test doubles and harnesses — `TestExtractor`/`TestLoader`/`TestTransformer`, fault injection, deterministic clock and progress timer, and end-to-end scenario helpers. | [README](src/Wolfgang.Etl.TestKit/README.md) |
| **Wolfgang.Etl.TestKit.Xunit** [![NuGet](https://img.shields.io/nuget/v/Wolfgang.Etl.TestKit.Xunit.svg?logo=nuget&label=)](https://www.nuget.org/packages/Wolfgang.Etl.TestKit.Xunit/) | xUnit **contract-test base classes** — inherit one to verify your component against the full behavioural contract of its base type. | [README](src/Wolfgang.Etl.TestKit.Xunit/README.md) |

## Getting started

Build a component on the core package:

```csharp
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

public sealed class NumberExtractor : ExtractorBase<int>
{
    protected override async IAsyncEnumerable<int> ExtractWorkerAsync(
        [EnumeratorCancellation] CancellationToken token)
    {
        for (var i = 1; i <= 3; i++)
        {
            // Stands in for awaiting your real data source
            await Task.Yield();
            token.ThrowIfCancellationRequested();

            IncrementCurrentItemCount();
            yield return i;
        }
    }
}
```

Compose a pipeline:

```csharp
await EtlPipeline
    .Create()
    .From(new NumberExtractor())
    .To(new MyLoader())
    .RunAsync();
```

Then test it — inherit the matching contract base from `Wolfgang.Etl.TestKit.Xunit` and get the whole
conformance suite for free. See each package's README (linked above) for details.

## Supported frameworks

| Package | Target frameworks |
| --- | --- |
| `Wolfgang.Etl.Abstractions`, `Wolfgang.Etl.ErrorPolicies` | `net462`, `net472`, `net48`, `net481`, `netstandard2.0`, `net5.0` through `net10.0` |
| `Wolfgang.Etl.TestKit`, `Wolfgang.Etl.TestKit.Xunit` | `net462`, `net481`, `netstandard2.0`, `net5.0`, `net6.0`, `net7.0`, `net8.0`, `net10.0` |

No package targets `netstandard2.1`; .NET Core 3.x consumers use the `netstandard2.0` build.

## Repository layout

```
src/    — the four shipping packages (each with its own README)
tests/  — unit, contract, concurrency, fuzz, doc-example, SourceLink and AOT-smoke test projects
docs/   — additional documentation and ADRs
```

## History

The `Wolfgang.Etl.TestKit` and `Wolfgang.Etl.TestKit.Xunit` packages previously lived in a separate
`ETL-Test-Kit` repository; they were folded into this repository so the whole ETL core is built,
tested, and released together. Their prior release history remains in the archived
[ETL-Test-Kit](https://github.com/Chris-Wolfgang/ETL-Test-Kit) repo.

## Building from Source

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download) - the current release (10.0); see *Supported Frameworks* for the targets that are built
- [PowerShell 7](https://github.com/PowerShell/PowerShell) (`pwsh`) for the scripts under `scripts/`

### Build Steps

```bash
# Clone the repository
git clone https://github.com/Chris-Wolfgang/ETL-Abstractions.git
cd ETL-Abstractions

# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release

# Run code formatting
pwsh ./scripts/format.ps1

# Run the PR workflow's Windows stage locally (build, tests on every TFM, coverage gates, DevSkim, gitleaks)
pwsh ./scripts/build-pr.ps1
```



## Contributing & license

See [CONTRIBUTING.md](CONTRIBUTING.md) and [SECURITY.md](SECURITY.md). Licensed under the
[MIT License](LICENSE).
