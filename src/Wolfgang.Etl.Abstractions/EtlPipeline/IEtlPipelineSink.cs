using System;
using System.Threading;
using System.Threading.Tasks;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// A terminated, runnable generic ETL pipeline. Obtained from
/// <see cref="IEtlPipeline{T}.To{TProgress}(LoaderBase{T, TProgress})"/> or a format-specific sink
/// terminator (for example a <c>CsvLoader</c> extension shipped by a format package).
/// </summary>
public interface IEtlPipelineSink
{
    /// <summary>
    /// Runs the pipeline to completion: pulls records from the source, threads them through every
    /// operator, and delivers them to the sink.
    /// </summary>
    /// <param name="progress">
    /// An optional sink for <see cref="EtlPipelineProgress"/> snapshots. When supplied, a snapshot is
    /// reported as records are delivered to the loader and once more when the run completes. Pass
    /// <see langword="null"/> to skip progress reporting.
    /// </param>
    /// <param name="token">
    /// A cancellation token observed while pulling from the source and threading records through the
    /// pipeline. It is also forwarded to the sink.
    /// </param>
    /// <returns>A task that completes when the loader has finished consuming the stream.</returns>
    Task RunAsync(IProgress<EtlPipelineProgress>? progress = null, CancellationToken token = default);
}
