using System;
using System.Collections.Generic;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Per-run counter bag for a generic ETL pipeline. Created when a run starts and threaded through the
/// factory chain so the source can count extracted records and the sink can count loaded records.
/// A pipeline is enumerated by a single consumer one item at a time, so the counters are only ever
/// touched on one logical flow and need no synchronization.
/// </summary>
internal sealed class EtlRunState
{
    /// <summary>
    /// The shortest gap between two in-flight <see cref="EtlPipelineProgress"/> reports (#735). The
    /// first delivered record is always reported, and the sink reports the exact final snapshot after
    /// the loader finishes, so the throttle only thins out the reports in between.
    /// </summary>
    internal static readonly TimeSpan ProgressReportInterval = TimeSpan.FromMilliseconds(100);

    private readonly ITimeSource _timeSource;
    private readonly long _startTimestamp;
    private readonly long _reportIntervalTicks;
    private long _nextReportTimestamp;
    private bool _reportedOnce;



    public long ExtractedItemCount;

    public long LoadedItemCount;



    // Error-item-count readers, one per stage that reports errors (source, transformers, sink). Their
    // values are summed into the snapshot so an item any stage's error policy discarded is reported,
    // not just the source's. Empty for a pipeline of stages that don't report errors, which reads as 0.
    private readonly List<Func<int>> _errorCountReaders = new();



    // Registers a stage's error-item-count reader. Called once per stage as the factory chain runs.
    public void AddErrorCountReader(Func<int> reader) => _errorCountReaders.Add(reader);



    public EtlRunState()
        : this(SystemTimeSource.Instance)
    {
    }



    // Test seam (#338): inject a fake time source so the elapsed metric is deterministic.
    internal EtlRunState(ITimeSource timeSource)
    {
        _timeSource = timeSource;
        _startTimestamp = timeSource.GetTimestamp();
        _reportIntervalTicks = (long)(ProgressReportInterval.TotalSeconds * timeSource.TimestampFrequency);
    }



    private TimeSpan Elapsed
    {
        get
        {
            var ticks = _timeSource.GetTimestamp() - _startTimestamp;
            return TimeSpan.FromSeconds(ticks / (double)_timeSource.TimestampFrequency);
        }
    }



    // True when an in-flight progress report is due: always for the first call, then at most once per
    // ProgressReportInterval. Reading the timestamp is allocation-free, so the per-record cost of a
    // throttled run is one clock read instead of one EtlPipelineProgress allocation.
    public bool IsProgressReportDue()
    {
        var now = _timeSource.GetTimestamp();
        if (_reportedOnce && now < _nextReportTimestamp)
        {
            return false;
        }

        _reportedOnce = true;
        _nextReportTimestamp = now + _reportIntervalTicks;
        return true;
    }



    public EtlPipelineProgress Snapshot()
    {
        long errorItemCount = 0;
        foreach (var reader in _errorCountReaders)
        {
            errorItemCount += reader();
        }

        return new EtlPipelineProgress(ExtractedItemCount, LoadedItemCount, Elapsed)
        {
            ErrorItemCount = errorItemCount,
        };
    }
}
