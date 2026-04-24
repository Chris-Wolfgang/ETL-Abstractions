using System;
using System.Collections.Generic;
using System.Threading;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Internal <see cref="IExtractStageWithProgress{TSource, TProgress}"/> implementation. Holds
/// the two extractor entry points (with and without progress) as delegates, plus an optional
/// captured <see cref="IProgress{TProgress}"/> sink. Which delegate actually runs is decided by
/// whether <see cref="WithProgress"/> has been called.
/// </summary>
internal sealed class ExtractStageWithProgress<TSource, TProgress> : IExtractStageWithProgress<TSource, TProgress>
    where TSource : notnull
    where TProgress : notnull
{
    private readonly Func<CancellationToken, IAsyncEnumerable<TSource>> _noProgressSource;
    private readonly Func<IProgress<TProgress>, CancellationToken, IAsyncEnumerable<TSource>> _withProgressSource;
    private IProgress<TProgress>? _progress;


    internal ExtractStageWithProgress
    (
        Func<CancellationToken, IAsyncEnumerable<TSource>> noProgressSource,
        Func<IProgress<TProgress>, CancellationToken, IAsyncEnumerable<TSource>> withProgressSource
    )
    {
        _noProgressSource = noProgressSource;
        _withProgressSource = withProgressSource;
    }


    private IAsyncEnumerable<TSource> Source(CancellationToken token)
    {
        return _progress is null
            ? _noProgressSource(token)
            : _withProgressSource(_progress, token);
    }


    /// <inheritdoc/>
    public IExtractStage<TSource> WithProgress(IProgress<TProgress> progress)
    {
        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        _progress = progress;
        return new ExtractStage<TSource>(Source);
    }


    /// <inheritdoc/>
    public ITransformStage<TDestination> Transform<TDestination>
    (
        ITransformAsync<TSource, TDestination> transformer
    )
        where TDestination : notnull
    {
        return new ExtractStage<TSource>(Source).Transform(transformer);
    }


    /// <inheritdoc/>
    public ITransformStage<TDestination> Transform<TDestination>
    (
        ITransformWithCancellationAsync<TSource, TDestination> transformer
    )
        where TDestination : notnull
    {
        return new ExtractStage<TSource>(Source).Transform(transformer);
    }


    /// <inheritdoc/>
    public ITransformStageWithProgress<TDestination, TProgressOther> Transform<TDestination, TProgressOther>
    (
        ITransformWithProgressAsync<TSource, TDestination, TProgressOther> transformer
    )
        where TDestination : notnull
        where TProgressOther : notnull
    {
        return new ExtractStage<TSource>(Source).Transform(transformer);
    }


    /// <inheritdoc/>
    public ITransformStageWithProgress<TDestination, TProgressOther> Transform<TDestination, TProgressOther>
    (
        ITransformWithProgressAndCancellationAsync<TSource, TDestination, TProgressOther> transformer
    )
        where TDestination : notnull
        where TProgressOther : notnull
    {
        return new ExtractStage<TSource>(Source).Transform(transformer);
    }


    /// <inheritdoc/>
    public IPipeline Load(ILoadAsync<TSource> loader)
    {
        return new ExtractStage<TSource>(Source).Load(loader);
    }


    /// <inheritdoc/>
    public IPipeline Load(ILoadWithCancellationAsync<TSource> loader)
    {
        return new ExtractStage<TSource>(Source).Load(loader);
    }


    /// <inheritdoc/>
    public IPipelineWithLoadProgress<TProgressOther> Load<TProgressOther>
    (
        ILoadWithProgressAsync<TSource, TProgressOther> loader
    )
        where TProgressOther : notnull
    {
        return new ExtractStage<TSource>(Source).Load(loader);
    }


    /// <inheritdoc/>
    public IPipelineWithLoadProgress<TProgressOther> Load<TProgressOther>
    (
        ILoadWithProgressAndCancellationAsync<TSource, TProgressOther> loader
    )
        where TProgressOther : notnull
    {
        return new ExtractStage<TSource>(Source).Load(loader);
    }
}
