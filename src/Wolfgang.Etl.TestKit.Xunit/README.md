# Wolfgang.Etl.TestKit.Xunit

xUnit **contract-test base classes** for components built on
[`Wolfgang.Etl.Abstractions`](https://www.nuget.org/packages/Wolfgang.Etl.Abstractions/). Inherit one
base class and your extractor / loader / transformer is verified against the full behavioural contract
of its base type — dozens of tests for free.

[![NuGet](https://img.shields.io/nuget/v/Wolfgang.Etl.TestKit.Xunit.svg?logo=nuget&label=NuGet)](https://www.nuget.org/packages/Wolfgang.Etl.TestKit.Xunit/)
[![Downloads](https://img.shields.io/nuget/dt/Wolfgang.Etl.TestKit.Xunit.svg?logo=nuget&label=downloads)](https://www.nuget.org/packages/Wolfgang.Etl.TestKit.Xunit/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/Chris-Wolfgang/ETL-Abstractions/blob/main/LICENSE)
[![GitHub](https://img.shields.io/badge/GitHub-Repository-181717?logo=github)](https://github.com/Chris-Wolfgang/ETL-Abstractions)

---

## What it is

A set of abstract xUnit contract-test bases. Derive from the one matching your stage, implement a few
small factory methods, and inherit the whole conformance suite:

| Base class | Verifies a … |
| --- | --- |
| `ExtractorBaseContractTests<TSut, TItem, TProgress>` | `ExtractorBase` implementation |
| `LoaderBaseContractTests<TSut, TItem, TProgress>` | `LoaderBase` implementation |
| `TransformerBaseContractTests<TSut, TItem, TProgress>` | `TransformerBase` implementation |

Additional contract bases cover cancellation, progress, disposal, per-item error handling, the retry
seam, idempotency, and pipeline composition.

## Usage

```csharp
public class MyExtractorContractTests
    : ExtractorBaseContractTests<MyExtractor, MyRecord, Report>
{
    protected override MyExtractor CreateSut(int itemCount) =>
        new MyExtractor(TestData.Take(itemCount));

    protected override IReadOnlyList<MyRecord> CreateExpectedItems() =>
        TestData;
}
```

Progress-timer behaviour is driven deterministically via `ManualProgressTimerCore` +
`WithManualProgressTimer` from [`Wolfgang.Etl.TestKit`](https://www.nuget.org/packages/Wolfgang.Etl.TestKit/) —
no per-component timer plumbing required.

## Install

```
dotnet add package Wolfgang.Etl.TestKit.Xunit
```

Part of the [ETL-Abstractions](https://github.com/Chris-Wolfgang/ETL-Abstractions) family — see the
[repository README](https://github.com/Chris-Wolfgang/ETL-Abstractions) for the full package set.
