using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Abstractions.Benchmarks;

/// <summary>
/// Measures the per-item overhead the <see cref="ExtractorBase{TSource, TProgress}"/>
/// machinery adds on top of a bare in-memory sequence — the async-iterator
/// wrapper, the <c>Interlocked</c>-based item counter, and the progress path
/// (timer creation + a final progress report).
/// </summary>
[MemoryDiagnoser]
public class ExtractorBenchmarks
{
    [Params(1_000, 100_000)]
    public int RecordCount { get; set; }



    [Benchmark(Baseline = true)]
    public async Task<int> Extract_NoProgress()
    {
        var extractor = new SequenceExtractor(RecordCount);

        var count = 0;
        await foreach (var _ in extractor.ExtractAsync())
        {
            count++;
        }

        return count;
    }



    [Benchmark]
    public async Task<int> Extract_WithProgress()
    {
        var extractor = new SequenceExtractor(RecordCount);
        IProgress<Report> progress = new Sink();

        var count = 0;
        await foreach (var _ in extractor.ExtractAsync(progress))
        {
            count++;
        }

        return count;
    }



    // Synchronous no-op progress sink — avoids the SynchronizationContext
    // posting that System.Progress<T> would add as benchmark noise.
    private sealed class Sink : IProgress<Report>
    {
        public void Report(Report value)
        {
        }
    }
}
