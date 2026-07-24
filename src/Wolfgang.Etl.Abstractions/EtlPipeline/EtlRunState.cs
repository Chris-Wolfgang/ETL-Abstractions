using System;
using System.Diagnostics;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Per-run counter bag for a generic ETL pipeline. Created when a run starts and threaded through the
/// factory chain so the source can count extracted records and the sink can count loaded records.
/// A pipeline is enumerated by a single consumer one item at a time, so the counters are only ever
/// touched on one logical flow and need no synchronization.
/// </summary>
internal sealed class EtlRunState
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    public int RecordsExtracted;

    public int RecordsLoaded;

    // Optional reader that surfaces an error-reporting source's error-item count into the snapshot.
    // Left null for sources that don't report errors (e.g. a raw IAsyncEnumerable), which reads as 0.
    public Func<int>? ErrorCountReader;


    public EtlPipelineProgress Snapshot()
    {
        return new EtlPipelineProgress(RecordsExtracted, RecordsLoaded, _stopwatch.Elapsed)
        {
            RecordsErrored = ErrorCountReader?.Invoke() ?? 0,
        };
    }
}
