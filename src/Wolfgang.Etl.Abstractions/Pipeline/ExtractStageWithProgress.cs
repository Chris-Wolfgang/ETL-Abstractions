using System;
using System.Collections.Generic;
using System.Threading;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Internal <see cref="IExtractStageWithProgress{TSource, TProgress}"/> implementation. Holds
/// the progress-capable extractor and an optional bound <see cref="IProgress{TProgress}"/>.
/// </summary>
internal sealed class ExtractStageWithProgress<TSource, TProgress> : IExtractStageWithProgress<TSource, TProgress>
    where TSource : notnull
    where TProgress : notnull
{
    private readonly IExtractWithProgressAndCancellationAsync<TSource, TProgress> _extractor;
    private IProgress<TProgress>? _progress;


    internal ExtractStageWithProgress(IExtractWithProgressAndCancellationAsync<TSource, TProgress> extractor)
    {
        _extractor = extractor;
    }


    private IAsyncEnumerable<TSource> Source(CancellationToken token)
    {
        return _progress is null
            ? _extractor.ExtractAsync(token)
            : _extractor.ExtractAsync(_progress, token);
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
        ITransformWithCancellationAsync<TSource, TDestination> transformer
    )
        where TDestination : notnull
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
    public IPipeline Load(ILoadWithCancellationAsync<TSource> loader)
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
