using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Internal <see cref="IEtlPipeline{T}"/> implementation. A pipeline is a lazily-composed factory
/// that, given a per-run <see cref="EtlRunState"/> and a <see cref="CancellationToken"/>, produces
/// the record stream. Each appended stage returns a new instance wrapping the previous factory;
/// nothing runs until the terminal sink enumerates the stream.
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
    public IEtlPipeline<TOut> Through<TOut>(ITransformAsync<T, TOut> transformer)
        where TOut : notnull
    {
        if (transformer is null)
        {
            throw new ArgumentNullException(nameof(transformer));
        }

        return new EtlPipelineImpl<TOut>((state, token) => transformer.TransformAsync(_factory(state, token)));
    }


    /// <inheritdoc/>
    public IEtlPipeline<TOut> Through<TOut>(ITransformWithCancellationAsync<T, TOut> transformer)
        where TOut : notnull
    {
        if (transformer is null)
        {
            throw new ArgumentNullException(nameof(transformer));
        }

        return new EtlPipelineImpl<TOut>((state, token) => transformer.TransformAsync(_factory(state, token), token));
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
}
