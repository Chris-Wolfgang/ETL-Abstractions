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


    public EtlPipelineProgress Snapshot()
    {
        return new EtlPipelineProgress(RecordsExtracted, RecordsLoaded, _stopwatch.Elapsed);
    }
}
