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
/// <param name="ExtractedItemCount">The number of records pulled from the source so far.</param>
/// <param name="LoadedItemCount">The number of records delivered to the sink so far.</param>
/// <param name="Elapsed">The wall-clock time elapsed since the run started.</param>
public sealed record EtlPipelineProgress
(
    long ExtractedItemCount,
    long LoadedItemCount,
    TimeSpan Elapsed
)
{
    /// <summary>
    /// The number of records discarded via an error policy so far, summed across every stage that
    /// reports errors: the source extractor, each transformer that implements
    /// <see cref="IReportsItemErrors"/>, and the loader. A stage whose policy returned
    /// <see cref="ItemErrorAction.Skip"/> for a bad record adds one; a stage that does not report
    /// errors (for example a raw <see cref="System.Collections.Generic.IAsyncEnumerable{T}"/> source or
    /// a delegate transform) adds <c>0</c>. Reported separately from <see cref="ExtractedItemCount"/>
    /// and <see cref="LoadedItemCount"/>, which count only the records that flowed through, so a
    /// failed record is never silent. Distinct from
    /// intentional skips (an extractor's <c>SkipItemCount</c> budget), which are not surfaced here.
    /// </summary>
    public long ErrorItemCount { get; init; }
}
