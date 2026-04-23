using System;
using System.Collections.Generic;
using System.Threading;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Internal <see cref="IExtractStage{TSource}"/> implementation whose source stream is a
/// pre-captured delegate. Used when the extractor does not report progress or when progress
/// has already been bound.
/// </summary>
internal sealed class ExtractStage<TSource> : IExtractStage<TSource>
    where TSource : notnull
{
    private readonly Func<CancellationToken, IAsyncEnumerable<TSource>> _source;


    internal ExtractStage(Func<CancellationToken, IAsyncEnumerable<TSource>> source)
    {
        _source = source;
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
        return new TransformStageWithProgress<TSource, TDestination, TProgress>(source, transformer);
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
        ILoadWithProgressAndCancellationAsync<TSource, TProgress> loader
    )
        where TProgress : notnull
    {
        if (loader is null)
        {
            throw new ArgumentNullException(nameof(loader));
        }

        return new PipelineWithLoadProgress<TSource, TProgress>(_source, loader);
    }
}
