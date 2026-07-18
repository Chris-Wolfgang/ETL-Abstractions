using System;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// An immutable snapshot of a generic <see cref="EtlPipeline"/> run's throughput, reported through
/// the <see cref="IProgress{T}"/> sink passed to
/// <see cref="IEtlPipelineSink.RunAsync(IProgress{EtlPipelineProgress}, System.Threading.CancellationToken)"/>.
/// </summary>
/// <remarks>
/// The counters describe the ends of the pipeline — records pulled from the source and records
/// delivered to the sink — because those are the only points the core pipeline observes. Operators
/// (filtering, projection, batching) live in higher-level packages and change the count between the
/// two ends without the core needing to know how. For rich, stage-specific progress attach an
/// <see cref="IProgress{T}"/> observer directly to the extractor or loader before handing it to the
/// pipeline.
/// </remarks>
/// <param name="RecordsExtracted">The number of records pulled from the source so far.</param>
/// <param name="RecordsLoaded">The number of records delivered to the sink so far.</param>
/// <param name="Elapsed">The wall-clock time elapsed since the run started.</param>
public sealed record EtlPipelineProgress
(
    int RecordsExtracted,
    int RecordsLoaded,
    TimeSpan Elapsed
);
