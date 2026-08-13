# Wolfgang.Etl.ErrorPolicies

Ready-made item-error policies — skip, log, and dead-letter — for the `OnItemError` hook of
[`Wolfgang.Etl.Abstractions`](https://www.nuget.org/packages/Wolfgang.Etl.Abstractions/) extractors,
loaders, and transformers.

[![NuGet](https://img.shields.io/nuget/v/Wolfgang.Etl.ErrorPolicies.svg?logo=nuget&label=NuGet)](https://www.nuget.org/packages/Wolfgang.Etl.ErrorPolicies/)
[![Downloads](https://img.shields.io/nuget/dt/Wolfgang.Etl.ErrorPolicies.svg?logo=nuget&label=downloads)](https://www.nuget.org/packages/Wolfgang.Etl.ErrorPolicies/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/Chris-Wolfgang/ETL-Abstractions/blob/main/LICENSE)
[![GitHub](https://img.shields.io/badge/GitHub-Repository-181717?logo=github)](https://github.com/Chris-Wolfgang/ETL-Abstractions)

---

## What it is

A base ETL stage decides what to do with a failed item through its `ErrorPolicy` — a
`Func<ItemErrorContext, ItemErrorAction>` returning `Skip` (discard and continue) or `Abort` (re-throw
and stop). This package supplies the common policies so you don't hand-write them:

| Policy | Behaviour |
| --- | --- |
| `ItemErrorPolicy.Skip` | Discard the failed item and continue (the stage still counts it in `CurrentErrorItemCount`). |
| `ItemErrorPolicy.Abort` | Re-throw and stop the run (the default). |
| `ItemErrorPolicy.SkipAndLog(logger)` | Log a warning, then skip. |
| `ItemErrorPolicy.SkipAndDeadLetter(collection \| channel)` | Record the failure in a caller-owned dead-letter sink, then skip. |
| `ItemErrorPolicy.SkipDeadLetterAndLog(collection \| channel, logger)` | Dead-letter **and** log, then skip. |

## Usage

```csharp
using Wolfgang.Etl.ErrorPolicies;

var deadLetters = new List<ItemErrorContext>();

var extractor = new MyExtractor<Record>(source)
{
    ErrorPolicy = ItemErrorPolicy.SkipDeadLetterAndLog(deadLetters, logger)
};
```

The dead-letter overloads write to a caller-owned collection or a
`System.Threading.Channels.ChannelWriter<ItemErrorContext>`, so the memory a bad feed can consume stays
under your control. The channel overloads use the non-blocking `TryWrite` (the hook is synchronous); the
`SkipDeadLetterAndLog(ChannelWriter, ILogger)` overload logs a distinct warning when a full bounded
channel drops the record, so the loss is never silent.

## Install

```
dotnet add package Wolfgang.Etl.ErrorPolicies
```

Part of the [ETL-Abstractions](https://github.com/Chris-Wolfgang/ETL-Abstractions) family — see the
[repository README](https://github.com/Chris-Wolfgang/ETL-Abstractions) for the full package set.
