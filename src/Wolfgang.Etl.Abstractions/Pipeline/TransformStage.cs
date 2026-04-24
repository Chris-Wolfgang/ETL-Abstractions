using System;
using System.Collections.Generic;
using System.Threading;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Internal <see cref="ITransformStage{TSource}"/> implementation whose source stream is a
/// pre-captured delegate. Used after a no-progress transformer, or after
/// <see cref="ITransformStageWithProgress{TSource, TProgress}.WithProgress"/> has captured a
/// progress sink to forward to the transformer's <c>TransformAsync</c> call.
/// </summary>
internal sealed class TransformStage<TSource> : ITransformStage<TSource>
    where TSource : notnull
{
    private readonly Func<CancellationToken, IAsyncEnumerable<TSource>> _source;


    internal TransformStage(Func<CancellationToken, IAsyncEnumerable<TSource>> source)
    {
        _source = source;
    }


    /// <inheritdoc/>
    public ITransformStage<TDestination> Transform<TDestination>
    (
        ITransformAsync<TSource, TDestination> transformer
    )
        where TDestination : notnull
    {
        if (transformer is null)
        {
            throw new ArgumentNullException(nameof(transformer));
        }

        var source = _source;
        return new TransformStage<TDestination>
        (
            token => transformer.TransformAsync(source(token))
        );
    }


    /// <inheritdoc/>
    public ITransformStage<TDestination> Transform<TDestination>
    (
        ITransformWithCancellationAsync<TSource, TDestination> transformer
    )
        where TDestination : notnull
    {
        if (transformer is null)
        {
            throw new ArgumentNullException(nameof(transformer));
        }

        var source = _source;
        return new TransformStage<TDestination>
        (
            token => transformer.TransformAsync(source(token), token)
        );
    }


    /// <inheritdoc/>
    public ITransformStageWithProgress<TDestination, TProgress> Transform<TDestination, TProgress>
    (
        ITransformWithProgressAsync<TSource, TDestination, TProgress> transformer
    )
        where TDestination : notnull
        where TProgress : notnull
    {
        if (transformer is null)
        {
            throw new ArgumentNullException(nameof(transformer));
        }

        var source = _source;
        return new TransformStageWithProgress<TSource, TDestination, TProgress>
        (
            upstream: source,
            noProgressTransform: (items, _) => transformer.TransformAsync(items),
            withProgressTransform: (items, progress, _) => transformer.TransformAsync(items, progress)
        );
    }


    /// <inheritdoc/>
    public ITransformStageWithProgress<TDestination, TProgress> Transform<TDestination, TProgress>
    (
        ITransformWithProgressAndCancellationAsync<TSource, TDestination, TProgress> transformer
    )
        where TDestination : notnull
        where TProgress : notnull
    {
        if (transformer is null)
        {
            throw new ArgumentNullException(nameof(transformer));
        }

        var source = _source;
        return new TransformStageWithProgress<TSource, TDestination, TProgress>
        (
            upstream: source,
            noProgressTransform: (items, token) => transformer.TransformAsync(items, token),
            withProgressTransform: (items, progress, token) => transformer.TransformAsync(items, progress, token)
        );
    }


    /// <inheritdoc/>
    public IPipeline Load(ILoadAsync<TSource> loader)
    {
        if (loader is null)
        {
            throw new ArgumentNullException(nameof(loader));
        }

        var source = _source;
        return new PipelineImpl
        (
            token => loader.LoadAsync(source(token))
        );
    }


    /// <inheritdoc/>
    public IPipeline Load(ILoadWithCancellationAsync<TSource> loader)
    {
        if (loader is null)
        {
            throw new ArgumentNullException(nameof(loader));
        }

        var source = _source;
        return new PipelineImpl
        (
            token => loader.LoadAsync(source(token), token)
        );
    }


    /// <inheritdoc/>
    public IPipelineWithLoadProgress<TProgress> Load<TProgress>
    (
        ILoadWithProgressAsync<TSource, TProgress> loader
    )
        where TProgress : notnull
    {
        if (loader is null)
        {
            throw new ArgumentNullException(nameof(loader));
        }

        return new PipelineWithLoadProgress<TSource, TProgress>
        (
            upstream: _source,
            noProgressLoad: (items, _) => loader.LoadAsync(items),
            withProgressLoad: (items, progress, _) => loader.LoadAsync(items, progress)
        );
    }


    /// <inheritdoc/>
    public IPipelineWithLoadProgress<TProgress> Load<TProgress>
    (
        ILoadWithProgressAndCancellationAsync<TSource, TProgress> loader
    )
        where TProgress : notnull
    {
        if (loader is null)
        {
            throw new ArgumentNullException(nameof(loader));
        }

        return new PipelineWithLoadProgress<TSource, TProgress>
        (
            upstream: _source,
            noProgressLoad: (items, token) => loader.LoadAsync(items, token),
            withProgressLoad: (items, progress, token) => loader.LoadAsync(items, progress, token)
        );
    }
}
