using System;
using System.Collections.Generic;
using System.Threading;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Internal <see cref="IExtractStageWithProgress{TSource, TProgress}"/> implementation. Holds
/// the two extractor entry points (with and without progress) as delegates. The object itself
/// is immutable: <see cref="WithProgress"/> and the <c>Transform</c>/<c>Load</c> overloads each
/// return a fresh <see cref="ExtractStage{TSource}"/> that captures the relevant delegate, so
/// repeated or branched calls cannot alias shared state.
/// </summary>
internal sealed class ExtractStageWithProgress<TSource, TProgress> : IExtractStageWithProgress<TSource, TProgress>
    where TSource : notnull
    where TProgress : notnull
{
    private readonly Func<CancellationToken, IAsyncEnumerable<TSource>> _noProgressSource;
    private readonly Func<IProgress<TProgress>, CancellationToken, IAsyncEnumerable<TSource>> _withProgressSource;


    internal ExtractStageWithProgress
    (
        Func<CancellationToken, IAsyncEnumerable<TSource>> noProgressSource,
        Func<IProgress<TProgress>, CancellationToken, IAsyncEnumerable<TSource>> withProgressSource
    )
    {
        _noProgressSource = noProgressSource;
        _withProgressSource = withProgressSource;
    }


    /// <inheritdoc/>
    public IExtractStage<TSource> WithProgress(IProgress<TProgress> progress)
    {
        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        var withProgressSource = _withProgressSource;
        return new ExtractStage<TSource>(token => withProgressSource(progress, token));
    }


    /// <inheritdoc/>
    public ITransformStage<TDestination> Transform<TDestination>
    (
        ITransformAsync<TSource, TDestination> transformer
    )
        where TDestination : notnull
    {
        return new ExtractStage<TSource>(_noProgressSource).Transform(transformer);
    }


    /// <inheritdoc/>
    public ITransformStage<TDestination> Transform<TDestination>
    (
        ITransformWithCancellationAsync<TSource, TDestination> transformer
    )
        where TDestination : notnull
    {
        return new ExtractStage<TSource>(_noProgressSource).Transform(transformer);
    }


    /// <inheritdoc/>
    public ITransformStageWithProgress<TDestination, TProgressOther> Transform<TDestination, TProgressOther>
    (
        ITransformWithProgressAsync<TSource, TDestination, TProgressOther> transformer
    )
        where TDestination : notnull
        where TProgressOther : notnull
    {
        return new ExtractStage<TSource>(_noProgressSource).Transform(transformer);
    }


    /// <inheritdoc/>
    public ITransformStageWithProgress<TDestination, TProgressOther> Transform<TDestination, TProgressOther>
    (
        ITransformWithProgressAndCancellationAsync<TSource, TDestination, TProgressOther> transformer
    )
        where TDestination : notnull
        where TProgressOther : notnull
    {
        return new ExtractStage<TSource>(_noProgressSource).Transform(transformer);
    }


    /// <inheritdoc/>
    public IPipeline Load(ILoadAsync<TSource> loader)
    {
        return new ExtractStage<TSource>(_noProgressSource).Load(loader);
    }


    /// <inheritdoc/>
    public IPipeline Load(ILoadWithCancellationAsync<TSource> loader)
    {
        return new ExtractStage<TSource>(_noProgressSource).Load(loader);
    }


    /// <inheritdoc/>
    public IPipelineWithLoadProgress<TProgressOther> Load<TProgressOther>
    (
        ILoadWithProgressAsync<TSource, TProgressOther> loader
    )
        where TProgressOther : notnull
    {
        return new ExtractStage<TSource>(_noProgressSource).Load(loader);
    }


    /// <inheritdoc/>
    public IPipelineWithLoadProgress<TProgressOther> Load<TProgressOther>
    (
        ILoadWithProgressAndCancellationAsync<TSource, TProgressOther> loader
    )
        where TProgressOther : notnull
    {
        return new ExtractStage<TSource>(_noProgressSource).Load(loader);
    }
}
