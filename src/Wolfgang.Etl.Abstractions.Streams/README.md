# Wolfgang.Etl.Abstractions.Streams

The stream tier of the Wolfgang.Etl framework: named stream sources and destinations, and base classes
for extractors and loaders that work over **several streams** in one run (every CSV file in a folder,
one output file per customer). Stream-based libraries (ETL-Csv, ETL-Json, ETL-Xml) reference it;
libraries that do not touch streams (ETL-DbClient, ETL-SqlBulkCopy, ETL-Transformers) depend only on
[`Wolfgang.Etl.Abstractions`](https://www.nuget.org/packages/Wolfgang.Etl.Abstractions/).

```
Wolfgang.Etl.Abstractions            (ExtractorBase / LoaderBase / EtlPipeline: everyone)
        ▲
Wolfgang.Etl.Abstractions.Streams    (this package: stream-based libraries only)
        ▲
   ETL-Csv / ETL-Json / ETL-Xml
```

## Install

```
dotnet add package Wolfgang.Etl.Abstractions.Streams
```

## What's in the box

| Type | Use |
| --- | --- |
| `NamedStreamSource` / `NamedStreamDestination` | A `StreamReader` / `StreamWriter` plus an optional name (file path, blob key, URL) for progress and errors. |
| `MultiStreamExtractorBase<TRecord, TProgress>` | Reads the sources in order. You implement `ExtractSourceAsync(reader, token)`: "read one stream". |
| `MultiStreamLoaderBase<TRecord, TProgress>` | Routes each record to the destination a selector picks. You implement `WriteRecordAsync(record, destination, token)`. |
| `MultiStreamProgress` | `Report` plus `CurrentSourceNumber`, `TotalSourceCount` (-1 when unknown), `CurrentSourceName` and `CurrentFailedSourceCount`. |
| `MultiStreamErrorAction` / `StreamSourceError` | `Stop` (default) rethrows a failing source; `Continue` skips it, counts it and reports it to `SourceErrorHandler`. |

## Extractor

The base class walks the sources, disposes each reader after use, applies `SkipItemCount` and
`MaximumItemCount` across all sources, and counts every record. Your class reads one stream:

```csharp
public sealed class CsvMultiStreamExtractor : MultiStreamExtractorBase<CsvRow, MultiStreamProgress>
{
    public CsvMultiStreamExtractor(IEnumerable<NamedStreamSource> sources) : base(sources)
    {
    }

    protected override IAsyncEnumerable<CsvRow> ExtractSourceAsync(StreamReader reader, CancellationToken token) =>
        new CsvExtractor(reader).ExtractAsync(token);

    protected override MultiStreamProgress CreateProgressReport() => CreateMultiStreamProgress();
}

var sources = Directory
    .EnumerateFiles(folder, "*.csv")
    .Select(path => new NamedStreamSource(new StreamReader(path), path));

var extractor = new CsvMultiStreamExtractor(sources)
{
    SourceErrorAction = MultiStreamErrorAction.Continue,
    SourceErrorHandler = error => logger.LogWarning(error.Exception, "Skipped {File}", error.Name),
};
```

A lazy sequence like the one above reports `TotalSourceCount = -1` and opens each file only when the
extractor reaches it; a list or array reports its count up front.

## Loader

The base class calls the selector for each record, opens each destination the first time it is
chosen (`OnDestinationOpenedAsync`, for a header), closes them all when the input ends
(`OnDestinationClosingAsync`, for a footer), and flushes and disposes every writer, even on failure.
Return the same `NamedStreamDestination` instance for every record that belongs in the same stream.

```csharp
public sealed class CsvMultiStreamLoader : MultiStreamLoaderBase<Order, MultiStreamProgress>
{
    public CsvMultiStreamLoader(Func<Order, NamedStreamDestination> selector) : base(selector)
    {
    }

    protected override Task OnDestinationOpenedAsync(NamedStreamDestination destination, CancellationToken token) =>
        destination.Writer.WriteLineAsync("Id,Customer,Amount");

    protected override Task WriteRecordAsync(Order order, NamedStreamDestination destination, CancellationToken token) =>
        destination.Writer.WriteLineAsync($"{order.Id},{order.Customer},{order.Amount}");

    protected override MultiStreamProgress CreateProgressReport() => CreateMultiStreamProgress();
}
```

## Supported frameworks

`net462`, `net472`, `net48`, `net481`, `netstandard2.0`, and `net5.0` through `net10.0`, the same
matrix as `Wolfgang.Etl.Abstractions`.

---

Part of the [ETL-Abstractions](https://github.com/Chris-Wolfgang/ETL-Abstractions) family; see the
[repository README](https://github.com/Chris-Wolfgang/ETL-Abstractions) for the full package set.
