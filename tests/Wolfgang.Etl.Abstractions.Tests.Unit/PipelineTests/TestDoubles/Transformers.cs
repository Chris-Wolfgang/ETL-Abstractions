using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests.TestDoubles;

/// <summary>
/// Transformer implementing only <see cref="ITransformAsync{TSource, TDestination}"/>.
/// Applies a user-supplied mapping function. No progress, no cancellation.
/// </summary>
internal sealed class BareTransformer<TSource, TDestination> : ITransformAsync<TSource, TDestination>
    where TSource : notnull
    where TDestination : notnull
{
    private readonly Func<TSource, TDestination> _map;
    public int TransformAsyncCallCount { get; private set; }


    public BareTransformer(Func<TSource, TDestination> map)
    {
        _map = map;
    }


    public async IAsyncEnumerable<TDestination> TransformAsync(IAsyncEnumerable<TSource> items)
    {
        TransformAsyncCallCount++;
        await foreach (var item in items.ConfigureAwait(false))
        {
            yield return _map(item);
        }
    }
}


/// <summary>
/// Transformer implementing <see cref="ITransformWithCancellationAsync{TSource, TDestination}"/> —
/// cancellation only.
/// </summary>
internal sealed class CancelOnlyTransformer<TSource, TDestination>
    : ITransformWithCancellationAsync<TSource, TDestination>
    where TSource : notnull
    where TDestination : notnull
{
    private readonly Func<TSource, TDestination> _map;
    public CancellationToken LastReceivedToken { get; private set; }
    public bool TokenOverloadWasCalled { get; private set; }


    public CancelOnlyTransformer(Func<TSource, TDestination> map)
    {
        _map = map;
    }


    public IAsyncEnumerable<TDestination> TransformAsync(IAsyncEnumerable<TSource> items)
        => throw new InvalidOperationException();


    public async IAsyncEnumerable<TDestination> TransformAsync
    (
        IAsyncEnumerable<TSource> items,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        TokenOverloadWasCalled = true;
        LastReceivedToken = token;
        await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
        {
            yield return _map(item);
        }
    }
}


/// <summary>
/// Transformer implementing <see cref="ITransformWithProgressAsync{TSource, TDestination, TProgress}"/> —
/// progress only, no cancellation.
/// </summary>
internal sealed class ProgressOnlyTransformer<TSource, TDestination, TProgress>
    : ITransformWithProgressAsync<TSource, TDestination, TProgress>
    where TSource : notnull
    where TDestination : notnull
    where TProgress : notnull
{
    private readonly Func<TSource, TDestination> _map;
    private readonly TProgress _reportValue;
    public IProgress<TProgress>? LastReceivedProgress { get; private set; }
    public bool ProgressOverloadWasCalled { get; private set; }
    public bool ParameterlessOverloadWasCalled { get; private set; }


    public ProgressOnlyTransformer(Func<TSource, TDestination> map, TProgress reportValue)
    {
        _map = map;
        _reportValue = reportValue;
    }


    public async IAsyncEnumerable<TDestination> TransformAsync(IAsyncEnumerable<TSource> items)
    {
        ParameterlessOverloadWasCalled = true;
        await foreach (var item in items.ConfigureAwait(false))
        {
            yield return _map(item);
        }
    }


    public async IAsyncEnumerable<TDestination> TransformAsync
    (
        IAsyncEnumerable<TSource> items,
        IProgress<TProgress> progress
    )
    {
        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        ProgressOverloadWasCalled = true;
        LastReceivedProgress = progress;
        await foreach (var item in items.ConfigureAwait(false))
        {
            progress.Report(_reportValue);
            yield return _map(item);
        }
    }
}


/// <summary>
/// Transformer implementing <see cref="ITransformWithProgressAndCancellationAsync{TSource, TDestination, TProgress}"/> —
/// both progress and cancellation.
/// </summary>
internal sealed class FullTransformer<TSource, TDestination, TProgress>
    : ITransformWithProgressAndCancellationAsync<TSource, TDestination, TProgress>
    where TSource : notnull
    where TDestination : notnull
    where TProgress : notnull
{
    private readonly Func<TSource, TDestination> _map;
    private readonly TProgress _reportValue;
    public IProgress<TProgress>? LastReceivedProgress { get; private set; }
    public CancellationToken LastReceivedToken { get; private set; }
    public bool FullOverloadWasCalled { get; private set; }


    public FullTransformer(Func<TSource, TDestination> map, TProgress reportValue)
    {
        _map = map;
        _reportValue = reportValue;
    }


    public bool ParameterlessOverloadWasCalled { get; private set; }
    public bool TokenOnlyOverloadWasCalled { get; private set; }
    public bool ProgressOnlyOverloadWasCalled { get; private set; }


    public async IAsyncEnumerable<TDestination> TransformAsync(IAsyncEnumerable<TSource> items)
    {
        ParameterlessOverloadWasCalled = true;
        await foreach (var item in items.ConfigureAwait(false))
        {
            yield return _map(item);
        }
    }


    public async IAsyncEnumerable<TDestination> TransformAsync
    (
        IAsyncEnumerable<TSource> items,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        TokenOnlyOverloadWasCalled = true;
        LastReceivedToken = token;
        await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
        {
            yield return _map(item);
        }
    }


    public async IAsyncEnumerable<TDestination> TransformAsync
    (
        IAsyncEnumerable<TSource> items,
        IProgress<TProgress> progress
    )
    {
        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        ProgressOnlyOverloadWasCalled = true;
        LastReceivedProgress = progress;
        await foreach (var item in items.ConfigureAwait(false))
        {
            progress.Report(_reportValue);
            yield return _map(item);
        }
    }


    public async IAsyncEnumerable<TDestination> TransformAsync
    (
        IAsyncEnumerable<TSource> items,
        IProgress<TProgress> progress,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        FullOverloadWasCalled = true;
        LastReceivedProgress = progress;
        LastReceivedToken = token;
        await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
        {
            progress.Report(_reportValue);
            yield return _map(item);
        }
    }
}
