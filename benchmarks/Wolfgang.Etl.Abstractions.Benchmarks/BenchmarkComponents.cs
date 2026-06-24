using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Abstractions.Benchmarks;

// ----------------------------------------------------------------------
// Base-class-derived components — exercise the ExtractorBase / TransformerBase
// / LoaderBase machinery (Interlocked counters, progress plumbing, the async
// iterator wrapper) over a purely in-memory sequence so the benchmark measures
// the abstraction overhead, not any I/O.
// ----------------------------------------------------------------------

/// <summary>An extractor that yields <c>0..count-1</c> from memory.</summary>
internal sealed class SequenceExtractor : ExtractorBase<int, Report>
{
    private readonly int _count;



    public SequenceExtractor(int count)
    {
        _count = count;
    }



    protected override async IAsyncEnumerable<int> ExtractWorkerAsync
    (
        [EnumeratorCancellation] CancellationToken token
    )
    {
        for (var i = 0; i < _count; i++)
        {
            IncrementCurrentItemCount();
            yield return i;
        }

        await Task.CompletedTask;
    }



    protected override Report CreateProgressReport()
    {
        return new Report(CurrentItemCount);
    }
}



/// <summary>A pass-through transformer that yields each source item unchanged.</summary>
internal sealed class PassThroughTransformer : TransformerBase<int, int, Report>
{
    protected override async IAsyncEnumerable<int> TransformWorkerAsync
    (
        IAsyncEnumerable<int> items,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        await foreach (var item in items.WithCancellation(token))
        {
            IncrementCurrentItemCount();
            yield return item;
        }
    }



    protected override Report CreateProgressReport()
    {
        return new Report(CurrentItemCount);
    }
}



/// <summary>A loader that drains the sequence, counting items.</summary>
internal sealed class CountingLoader : LoaderBase<int, Report>
{
    protected override async Task LoadWorkerAsync
    (
        IAsyncEnumerable<int> items,
        CancellationToken token
    )
    {
        await foreach (var _ in items.WithCancellation(token))
        {
            IncrementCurrentItemCount();
        }
    }



    protected override Report CreateProgressReport()
    {
        return new Report(CurrentItemCount);
    }
}



// ----------------------------------------------------------------------
// Interface-only components — implement just the simplest (no-progress,
// no-cancellation) ETL interfaces so the fluent Pipeline composition resolves
// to its leanest path, isolating the cost of the Pipeline plumbing itself.
// ----------------------------------------------------------------------

/// <summary>A minimal <see cref="IExtractAsync{TSource}"/> over an in-memory range.</summary>
internal sealed class RangeExtractor : IExtractAsync<int>
{
    private readonly int _count;



    public RangeExtractor(int count)
    {
        _count = count;
    }



    public async IAsyncEnumerable<int> ExtractAsync()
    {
        for (var i = 0; i < _count; i++)
        {
            yield return i;
        }

        await Task.CompletedTask;
    }
}



/// <summary>A minimal pass-through <see cref="ITransformAsync{TSource, TDestination}"/>.</summary>
internal sealed class IdentityTransformer : ITransformAsync<int, int>
{
    public async IAsyncEnumerable<int> TransformAsync(IAsyncEnumerable<int> items)
    {
        await foreach (var item in items)
        {
            yield return item;
        }
    }
}



/// <summary>A minimal <see cref="ILoadAsync{TDestination}"/> that drains the sequence.</summary>
internal sealed class SinkLoader : ILoadAsync<int>
{
    public async Task LoadAsync(IAsyncEnumerable<int> items)
    {
        await foreach (var _ in items)
        {
        }
    }
}
