using System.Diagnostics;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Per-run counter bag for a generic ETL pipeline. Created when a run starts and threaded through the
/// operator chain so each stage can bump the counters that feed <see cref="EtlPipelineProgress"/>.
/// A pipeline is enumerated by a single consumer one item at a time, so the counters are only ever
/// touched on one logical flow and need no synchronization.
/// </summary>
internal sealed class EtlRunState
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    public int RecordsExtracted;

    public int RecordsLoaded;

    public int RecordsFiltered;

    public int RecordsErrored;


    public EtlPipelineProgress Snapshot()
    {
        return new EtlPipelineProgress
        (
            RecordsExtracted,
            RecordsLoaded,
            RecordsFiltered,
            RecordsErrored,
            _stopwatch.Elapsed
        );
    }
}
