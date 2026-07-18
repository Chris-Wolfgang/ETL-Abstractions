using System;
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


/// <summary>
/// A projecting <see cref="ITransformAsync{TSource, TDestination}"/> for exercising
/// <see cref="IEtlPipeline{T}.Through{TOut}(ITransformAsync{T, TOut})"/>.
/// </summary>
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
        await foreach (var item in items)
        {
            yield return _map(item);
        }
    }
}


/// <summary>
/// A cancellation-aware pass-through transformer that records the token it was handed, for verifying
/// that <see cref="IEtlPipeline{T}.Through{TOut}(ITransformWithCancellationAsync{T, TOut})"/> forwards
/// the run's cancellation token.
/// </summary>
internal sealed class TokenRecordingTransformer<T> : ITransformWithCancellationAsync<T, T>
    where T : notnull
{
    public CancellationToken LastToken { get; private set; }


    public IAsyncEnumerable<T> TransformAsync(IAsyncEnumerable<T> items)
    {
        return TransformAsync(items, CancellationToken.None);
    }


    public async IAsyncEnumerable<T> TransformAsync(IAsyncEnumerable<T> items, [EnumeratorCancellation] CancellationToken token)
    {
        LastToken = token;
        await foreach (var item in items.WithCancellation(token))
        {
            token.ThrowIfCancellationRequested();
            yield return item;
        }
    }
}


/// <summary>
/// A pass-through transformer that cancels the supplied source after emitting a set number of records,
/// for exercising mid-stream cancellation.
/// </summary>
internal sealed class CancelingTransformer<T> : ITransformAsync<T, T>
    where T : notnull
{
    private readonly CancellationTokenSource _cts;
    private readonly int _cancelAfter;


    public CancelingTransformer(CancellationTokenSource cts, int cancelAfter)
    {
        _cts = cts;
        _cancelAfter = cancelAfter;
    }


    public async IAsyncEnumerable<T> TransformAsync(IAsyncEnumerable<T> items)
    {
        var count = 0;
        await foreach (var item in items)
        {
            count++;
            if (count == _cancelAfter)
            {
                _cts.Cancel();
            }

            yield return item;
        }
    }
}
