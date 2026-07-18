using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.EtlPipelineTests;

/// <summary>
/// Minimal <see cref="LoaderBase{TDestination, TProgress}"/> that collects everything it is handed,
/// for exercising <see cref="IEtlPipeline{T}.To{TProgress}(LoaderBase{T, TProgress})"/>.
/// </summary>
internal sealed class CollectingLoader<T> : LoaderBase<T, EtlProgress>
    where T : notnull
{
    public List<T> Loaded { get; } = new();


    protected override async Task LoadWorkerAsync(IAsyncEnumerable<T> items, CancellationToken token)
    {
        await foreach (var item in items.WithCancellation(token))
        {
            token.ThrowIfCancellationRequested();
            Loaded.Add(item);
            IncrementCurrentItemCount();
        }
    }


    protected override EtlProgress CreateProgressReport()
    {
        return new EtlProgress(CurrentItemCount);
    }
}


/// <summary>
/// Minimal <see cref="ExtractorBase{TSource, TProgress}"/> that yields a fixed sequence, for
/// exercising <see cref="EtlPipeline.From{T, TProgress}(ExtractorBase{T, TProgress})"/>.
/// </summary>
internal sealed class SeededExtractor<T> : ExtractorBase<T, EtlProgress>
    where T : notnull
{
    private readonly IEnumerable<T> _items;


    public SeededExtractor(IEnumerable<T> items)
    {
        _items = items;
    }


    protected override async IAsyncEnumerable<T> ExtractWorkerAsync([EnumeratorCancellation] CancellationToken token)
    {
        foreach (var item in _items)
        {
            token.ThrowIfCancellationRequested();
            yield return item;
            IncrementCurrentItemCount();
            await Task.Yield();
        }
    }


    protected override EtlProgress CreateProgressReport()
    {
        return new EtlProgress(CurrentItemCount);
    }
}
