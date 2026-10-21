using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Wolfgang.Etl.Abstractions.Benchmarks;

/// <summary>
/// Measures a full <see cref="EtlPipeline"/> run (<c>Create().From(...).Through(...).To(...).RunAsync</c>)
/// over the base-class components, with and without an <see cref="EtlPipelineProgress"/> sink (#735).
/// The gap between the two is the cost of the pipeline's own progress reporting, which is throttled to
/// one snapshot per <c>EtlRunState.ProgressReportInterval</c> rather than one per record.
/// </summary>
[MemoryDiagnoser]
public class EtlPipelineBenchmarks
{
    // A synchronous no-op sink: no SynchronizationContext post, so the benchmark measures the
    // snapshot cost itself rather than Progress<T>'s thread-pool dispatch.
    private static readonly IProgress<EtlPipelineProgress> NoOpProgress = new NoOpProgressSink();



    [Params(1_000, 100_000)]
    public int RecordCount { get; set; }



    [Benchmark(Baseline = true)]
    public async Task EtlPipeline_without_progress()
    {
        await EtlPipeline
            .Create()
            .From(new SequenceExtractor(RecordCount))
            .Through(new PassThroughTransformer())
            .To(new CountingLoader())
            .RunAsync();
    }



    [Benchmark]
    public async Task EtlPipeline_with_progress()
    {
        await EtlPipeline
            .Create()
            .From(new SequenceExtractor(RecordCount))
            .Through(new PassThroughTransformer())
            .To(new CountingLoader())
            .RunAsync(NoOpProgress);
    }



    private sealed class NoOpProgressSink : IProgress<EtlPipelineProgress>
    {
        public void Report(EtlPipelineProgress value)
        {
        }
    }
}
