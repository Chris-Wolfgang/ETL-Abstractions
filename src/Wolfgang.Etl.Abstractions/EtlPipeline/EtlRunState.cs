using System;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Per-run counter bag for a generic ETL pipeline. Created when a run starts and threaded through the
/// factory chain so the source can count extracted records and the sink can count loaded records.
/// A pipeline is enumerated by a single consumer one item at a time, so the counters are only ever
/// touched on one logical flow and need no synchronization.
/// </summary>
internal sealed class EtlRunState
{
    private readonly ITimeSource _timeSource;
    private readonly long _startTimestamp;

    public long ExtractedItemCount;

    public long LoadedItemCount;

    // Optional reader that surfaces an error-reporting source's error-item count into the snapshot.
    // Left null for sources that don't report errors (e.g. a raw IAsyncEnumerable), which reads as 0.
    public Func<int>? ErrorCountReader;


    public EtlRunState()
        : this(SystemTimeSource.Instance)
    {
    }


    // Test seam (#338): inject a fake time source so the elapsed metric is deterministic.
    internal EtlRunState(ITimeSource timeSource)
    {
        _timeSource = timeSource;
        _startTimestamp = timeSource.GetTimestamp();
    }


    private TimeSpan Elapsed
    {
        get
        {
            var ticks = _timeSource.GetTimestamp() - _startTimestamp;
            return TimeSpan.FromSeconds(ticks / (double)_timeSource.TimestampFrequency);
        }
    }


    public EtlPipelineProgress Snapshot()
    {
        return new EtlPipelineProgress(ExtractedItemCount, LoadedItemCount, Elapsed)
        {
            ErrorItemCount = ErrorCountReader?.Invoke() ?? 0,
        };
    }
}
