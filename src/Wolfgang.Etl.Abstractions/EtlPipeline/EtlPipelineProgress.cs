using System;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// An immutable snapshot of a generic <see cref="EtlPipeline"/> run's throughput counters,
/// reported through the <see cref="IProgress{T}"/> sink passed to
/// <see cref="IEtlPipelineSink.RunAsync(IProgress{EtlPipelineProgress}, System.Threading.CancellationToken)"/>.
/// </summary>
/// <remarks>
/// The counters are intentionally primitive — they describe the flow through the pipeline as a
/// whole, not the internals of any one stage. For rich, stage-specific progress (per-extractor or
/// per-loader reports, ETA, percentages), attach an <see cref="IProgress{T}"/> observer directly to
/// the extractor or loader before handing it to the pipeline.
/// </remarks>
/// <param name="RecordsExtracted">
/// The number of records produced by the source and pulled into the pipeline so far.
/// </param>
/// <param name="RecordsLoaded">The number of records delivered to the sink so far.</param>
/// <param name="RecordsFiltered">
/// The number of records removed by a <see cref="IEtlPipeline{T}.Where(System.Func{T, bool})"/>
/// operator (a record whose predicate returned <see langword="false"/>) so far.
/// </param>
/// <param name="RecordsErrored">
/// The number of records that failed processing so far. Reserved for a future operator-level error
/// policy; without one, an exception in an operator aborts the run, so this is currently always
/// <c>0</c>.
/// </param>
/// <param name="Elapsed">The wall-clock time elapsed since the run started.</param>
public sealed record EtlPipelineProgress
(
    int RecordsExtracted,
    int RecordsLoaded,
    int RecordsFiltered,
    int RecordsErrored,
    TimeSpan Elapsed
);
