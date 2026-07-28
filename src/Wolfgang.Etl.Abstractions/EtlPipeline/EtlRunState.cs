using System;
using System.Collections.Generic;
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

    public long ExtractedItemCount;

    public long LoadedItemCount;

    // Error-item-count readers, one per stage that reports errors (source, transformers, sink). Their
    // values are summed into the snapshot so an item any stage's error policy discarded is reported,
    // not just the source's. Empty for a pipeline of stages that don't report errors, which reads as 0.
    private readonly List<Func<int>> _errorCountReaders = new();


    // Registers a stage's error-item-count reader. Called once per stage as the factory chain runs.
    public void AddErrorCountReader(Func<int> reader) => _errorCountReaders.Add(reader);


    public EtlPipelineProgress Snapshot()
    {
        long errorItemCount = 0;
        foreach (var reader in _errorCountReaders)
        {
            errorItemCount += reader();
        }

        return new EtlPipelineProgress(ExtractedItemCount, LoadedItemCount, _stopwatch.Elapsed)
        {
            ErrorItemCount = errorItemCount,
        };
    }
}
