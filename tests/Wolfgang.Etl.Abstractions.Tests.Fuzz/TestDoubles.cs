using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Wolfgang.Etl.Abstractions.Tests.Fuzz;

/// <summary>Collects every record delivered to it, for asserting pipeline output.</summary>
internal sealed class CollectingLoader<T> : LoaderBase<T, Report>
    where T : notnull
{
    public List<T> Loaded { get; } = new();


    protected override async Task LoadWorkerAsync(IAsyncEnumerable<T> items, CancellationToken token)
    {
        await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
        {
            Loaded.Add(item);
            IncrementCurrentItemCount();
        }
    }


    protected override Report CreateProgressReport()
    {
        return new Report(CurrentItemCount);
    }
}


/// <summary>Elementwise projecting transformer, for exercising <c>Through</c>.</summary>
internal sealed class MapTransformer<TSource, TDestination> : ITransformAsync<TSource, TDestination>
    where TSource : notnull
    where TDestination : notnull
{
    private readonly Func<TSource, TDestination> _map;


    public MapTransformer(Func<TSource, TDestination> map)
    {
        _map = map;
    }


    public async IAsyncEnumerable<TDestination> TransformAsync(IAsyncEnumerable<TSource> items)
    {
        await foreach (var item in items.ConfigureAwait(false))
        {
            yield return _map(item);
        }
    }
}


/// <summary>Synchronous <see cref="IProgress{T}"/> so the final snapshot is captured before assertions.</summary>
internal sealed class InlineProgress<T> : IProgress<T>
{
    private readonly Action<T> _callback;


    public InlineProgress(Action<T> callback)
    {
        _callback = callback;
    }


    public void Report(T value) => _callback(value);
}
