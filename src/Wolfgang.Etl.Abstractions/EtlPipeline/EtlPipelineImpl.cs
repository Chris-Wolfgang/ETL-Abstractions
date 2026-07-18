using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Internal <see cref="IEtlPipeline{T}"/> implementation. A pipeline is a lazily-composed factory
/// that, given a per-run <see cref="EtlRunState"/> and a <see cref="CancellationToken"/>, produces
/// the record stream. Each operator returns a new instance wrapping the previous factory; nothing
/// runs until the terminal sink enumerates the stream.
/// </summary>
internal sealed class EtlPipelineImpl<T> : IEtlPipeline<T>
    where T : notnull
{
    private readonly Func<EtlRunState, CancellationToken, IAsyncEnumerable<T>> _factory;


    private EtlPipelineImpl(Func<EtlRunState, CancellationToken, IAsyncEnumerable<T>> factory)
    {
        _factory = factory;
    }


    /// <summary>
    /// Wraps a raw source factory with the extracted-record counter, producing the head of a pipeline.
    /// </summary>
    internal static EtlPipelineImpl<T> FromStream
    (
        Func<EtlRunState, CancellationToken, IAsyncEnumerable<T>> rawSource
    )
    {
        return new EtlPipelineImpl<T>((state, token) => CountExtracted(rawSource(state, token), state, token));
    }


    /// <inheritdoc/>
    public IEtlPipeline<T> Where(Func<T, bool> predicate)
    {
        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        return new EtlPipelineImpl<T>((state, token) => WhereIterator(_factory(state, token), predicate, state, token));
    }


    /// <inheritdoc/>
    public IEtlPipeline<T> Where(Func<T, ValueTask<bool>> asyncPredicate)
    {
        if (asyncPredicate is null)
        {
            throw new ArgumentNullException(nameof(asyncPredicate));
        }

        return new EtlPipelineImpl<T>((state, token) => WhereAsyncIterator(_factory(state, token), asyncPredicate, state, token));
    }


    /// <inheritdoc/>
    public IEtlPipeline<TOut> Select<TOut>(Func<T, TOut> selector)
        where TOut : notnull
    {
        if (selector is null)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        return new EtlPipelineImpl<TOut>((state, token) => SelectIterator(_factory(state, token), selector, token));
    }


    /// <inheritdoc/>
    public IEtlPipeline<TOut> Select<TOut>(Func<T, ValueTask<TOut>> selector)
        where TOut : notnull
    {
        if (selector is null)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        return new EtlPipelineImpl<TOut>((state, token) => SelectAsyncIterator(_factory(state, token), selector, token));
    }


    /// <inheritdoc/>
    public IEtlPipeline<TOut> SelectMany<TOut>(Func<T, IAsyncEnumerable<TOut>> selector)
        where TOut : notnull
    {
        if (selector is null)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        return new EtlPipelineImpl<TOut>((state, token) => SelectManyIterator(_factory(state, token), selector, token));
    }


    /// <inheritdoc/>
    public IEtlPipeline<T> Distinct<TKey>(Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
    {
        if (keySelector is null)
        {
            throw new ArgumentNullException(nameof(keySelector));
        }

        return new EtlPipelineImpl<T>((state, token) => DistinctIterator(_factory(state, token), keySelector, comparer, token));
    }


    /// <inheritdoc/>
    public IEtlPipeline<T> Take(int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count must not be negative.");
        }

        return new EtlPipelineImpl<T>((state, token) => TakeIterator(_factory(state, token), count, token));
    }


    /// <inheritdoc/>
    public IEtlPipeline<T> Skip(int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count must not be negative.");
        }

        return new EtlPipelineImpl<T>((state, token) => SkipIterator(_factory(state, token), count, token));
    }


    /// <inheritdoc/>
    public IEtlPipeline<T> Tap(Action<T> sideEffect)
    {
        if (sideEffect is null)
        {
            throw new ArgumentNullException(nameof(sideEffect));
        }

        return new EtlPipelineImpl<T>((state, token) => TapIterator(_factory(state, token), sideEffect, token));
    }


    /// <inheritdoc/>
    public IEtlPipeline<T> Tap(Func<T, ValueTask> asyncSideEffect)
    {
        if (asyncSideEffect is null)
        {
            throw new ArgumentNullException(nameof(asyncSideEffect));
        }

        return new EtlPipelineImpl<T>((state, token) => TapAsyncIterator(_factory(state, token), asyncSideEffect, token));
    }


    /// <inheritdoc/>
    public IEtlPipeline<IReadOnlyList<T>> Buffer(int size)
    {
        if (size < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(size), size, "Buffer size must be at least 1.");
        }

        return new EtlPipelineImpl<IReadOnlyList<T>>((state, token) => BufferIterator(_factory(state, token), size, token));
    }


    /// <inheritdoc/>
    public IEtlPipelineSink To<TProgress>(LoaderBase<T, TProgress> loader)
        where TProgress : notnull
    {
        if (loader is null)
        {
            throw new ArgumentNullException(nameof(loader));
        }

        return new EtlPipelineSink<T, TProgress>(_factory, loader);
    }


    /// <inheritdoc/>
    public IAsyncEnumerable<T> AsAsyncEnumerable(CancellationToken token = default)
    {
        return _factory(new EtlRunState(), token);
    }


    // The head of every pipeline: pulls from the raw source, honours cancellation, and counts each
    // record as extracted. WithCancellation covers sources that observe the token via
    // [EnumeratorCancellation]; the explicit throw covers sources that ignore it.
    private static async IAsyncEnumerable<T> CountExtracted
    (
        IAsyncEnumerable<T> source,
        EtlRunState state,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        await foreach (var item in source.WithCancellation(token).ConfigureAwait(false))
        {
            token.ThrowIfCancellationRequested();
            state.RecordsExtracted++;
            yield return item;
        }
    }


    private static async IAsyncEnumerable<T> WhereIterator
    (
        IAsyncEnumerable<T> inner,
        Func<T, bool> predicate,
        EtlRunState state,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        await foreach (var item in inner.ConfigureAwait(false))
        {
            token.ThrowIfCancellationRequested();

            if (predicate(item))
            {
                yield return item;
            }
            else
            {
                state.RecordsFiltered++;
            }
        }
    }


    private static async IAsyncEnumerable<T> WhereAsyncIterator
    (
        IAsyncEnumerable<T> inner,
        Func<T, ValueTask<bool>> predicate,
        EtlRunState state,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        await foreach (var item in inner.ConfigureAwait(false))
        {
            token.ThrowIfCancellationRequested();

            if (await predicate(item).ConfigureAwait(false))
            {
                yield return item;
            }
            else
            {
                state.RecordsFiltered++;
            }
        }
    }


    private static async IAsyncEnumerable<TOut> SelectIterator<TOut>
    (
        IAsyncEnumerable<T> inner,
        Func<T, TOut> selector,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        await foreach (var item in inner.ConfigureAwait(false))
        {
            token.ThrowIfCancellationRequested();
            yield return selector(item);
        }
    }


    private static async IAsyncEnumerable<TOut> SelectAsyncIterator<TOut>
    (
        IAsyncEnumerable<T> inner,
        Func<T, ValueTask<TOut>> selector,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        await foreach (var item in inner.ConfigureAwait(false))
        {
            token.ThrowIfCancellationRequested();
            yield return await selector(item).ConfigureAwait(false);
        }
    }


    private static async IAsyncEnumerable<TOut> SelectManyIterator<TOut>
    (
        IAsyncEnumerable<T> inner,
        Func<T, IAsyncEnumerable<TOut>> selector,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        await foreach (var item in inner.ConfigureAwait(false))
        {
            await foreach (var projected in selector(item).WithCancellation(token).ConfigureAwait(false))
            {
                yield return projected;
            }
        }
    }


    private static async IAsyncEnumerable<T> DistinctIterator<TKey>
    (
        IAsyncEnumerable<T> inner,
        Func<T, TKey> keySelector,
        IEqualityComparer<TKey>? comparer,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        var seen = new HashSet<TKey>(comparer);

        await foreach (var item in inner.ConfigureAwait(false))
        {
            token.ThrowIfCancellationRequested();

            if (seen.Add(keySelector(item)))
            {
                yield return item;
            }
        }
    }


    private static async IAsyncEnumerable<T> TakeIterator
    (
        IAsyncEnumerable<T> inner,
        int count,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        if (count == 0)
        {
            yield break;
        }

        var taken = 0;

        await foreach (var item in inner.ConfigureAwait(false))
        {
            token.ThrowIfCancellationRequested();
            yield return item;

            if (++taken >= count)
            {
                yield break;
            }
        }
    }


    private static async IAsyncEnumerable<T> SkipIterator
    (
        IAsyncEnumerable<T> inner,
        int count,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        var skipped = 0;

        await foreach (var item in inner.ConfigureAwait(false))
        {
            token.ThrowIfCancellationRequested();

            if (skipped < count)
            {
                skipped++;
                continue;
            }

            yield return item;
        }
    }


    private static async IAsyncEnumerable<T> TapIterator
    (
        IAsyncEnumerable<T> inner,
        Action<T> sideEffect,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        await foreach (var item in inner.ConfigureAwait(false))
        {
            token.ThrowIfCancellationRequested();
            sideEffect(item);
            yield return item;
        }
    }


    private static async IAsyncEnumerable<T> TapAsyncIterator
    (
        IAsyncEnumerable<T> inner,
        Func<T, ValueTask> asyncSideEffect,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        await foreach (var item in inner.ConfigureAwait(false))
        {
            token.ThrowIfCancellationRequested();
            await asyncSideEffect(item).ConfigureAwait(false);
            yield return item;
        }
    }


    private static async IAsyncEnumerable<IReadOnlyList<T>> BufferIterator
    (
        IAsyncEnumerable<T> inner,
        int size,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        var batch = new List<T>(size);

        await foreach (var item in inner.ConfigureAwait(false))
        {
            token.ThrowIfCancellationRequested();
            batch.Add(item);

            if (batch.Count >= size)
            {
                yield return batch;
                batch = new List<T>(size);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }
}
