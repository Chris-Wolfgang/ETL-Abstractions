using System;
using System.Collections.Generic;
using System.Threading;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Internal <see cref="ITransformStageWithProgress{TDestination, TProgress}"/> implementation.
/// Holds the upstream source, two transformer entry points (with and without progress) as
/// delegates, and an optional captured <see cref="IProgress{TProgress}"/> sink. Which delegate
/// actually runs is decided by whether <see cref="WithProgress"/> has been called.
/// </summary>
internal sealed class TransformStageWithProgress<TUpstream, TDestination, TProgress>
    : ITransformStageWithProgress<TDestination, TProgress>
    where TUpstream : notnull
    where TDestination : notnull
    where TProgress : notnull
{
    private readonly Func<CancellationToken, IAsyncEnumerable<TUpstream>> _upstream;
    private readonly Func<IAsyncEnumerable<TUpstream>, CancellationToken, IAsyncEnumerable<TDestination>> _noProgressTransform;
    private readonly Func<IAsyncEnumerable<TUpstream>, IProgress<TProgress>, CancellationToken, IAsyncEnumerable<TDestination>> _withProgressTransform;
    private IProgress<TProgress>? _progress;


    internal TransformStageWithProgress
    (
        Func<CancellationToken, IAsyncEnumerable<TUpstream>> upstream,
        Func<IAsyncEnumerable<TUpstream>, CancellationToken, IAsyncEnumerable<TDestination>> noProgressTransform,
        Func<IAsyncEnumerable<TUpstream>, IProgress<TProgress>, CancellationToken, IAsyncEnumerable<TDestination>> withProgressTransform
    )
    {
        _upstream = upstream;
        _noProgressTransform = noProgressTransform;
        _withProgressTransform = withProgressTransform;
    }


    private IAsyncEnumerable<TDestination> Source(CancellationToken token)
    {
        return _progress is null
            ? _noProgressTransform(_upstream(token), token)
            : _withProgressTransform(_upstream(token), _progress, token);
    }


    /// <inheritdoc/>
    public ITransformStage<TDestination> WithProgress(IProgress<TProgress> progress)
    {
        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        _progress = progress;
        return new TransformStage<TDestination>(Source);
    }


    /// <inheritdoc/>
    public ITransformStage<TOut> Transform<TOut>
    (
        ITransformAsync<TDestination, TOut> transformer
    )
        where TOut : notnull
    {
        return new TransformStage<TDestination>(Source).Transform(transformer);
    }


    /// <inheritdoc/>
    public ITransformStage<TOut> Transform<TOut>
    (
        ITransformWithCancellationAsync<TDestination, TOut> transformer
    )
        where TOut : notnull
    {
        return new TransformStage<TDestination>(Source).Transform(transformer);
    }


    /// <inheritdoc/>
    public ITransformStageWithProgress<TOut, TProgressOther> Transform<TOut, TProgressOther>
    (
        ITransformWithProgressAsync<TDestination, TOut, TProgressOther> transformer
    )
        where TOut : notnull
        where TProgressOther : notnull
    {
        return new TransformStage<TDestination>(Source).Transform(transformer);
    }


    /// <inheritdoc/>
    public ITransformStageWithProgress<TOut, TProgressOther> Transform<TOut, TProgressOther>
    (
        ITransformWithProgressAndCancellationAsync<TDestination, TOut, TProgressOther> transformer
    )
        where TOut : notnull
        where TProgressOther : notnull
    {
        return new TransformStage<TDestination>(Source).Transform(transformer);
    }


    /// <inheritdoc/>
    public IPipeline Load(ILoadAsync<TDestination> loader)
    {
        return new TransformStage<TDestination>(Source).Load(loader);
    }


    /// <inheritdoc/>
    public IPipeline Load(ILoadWithCancellationAsync<TDestination> loader)
    {
        return new TransformStage<TDestination>(Source).Load(loader);
    }


    /// <inheritdoc/>
    public IPipelineWithLoadProgress<TProgressOther> Load<TProgressOther>
    (
        ILoadWithProgressAsync<TDestination, TProgressOther> loader
    )
        where TProgressOther : notnull
    {
        return new TransformStage<TDestination>(Source).Load(loader);
    }


    /// <inheritdoc/>
    public IPipelineWithLoadProgress<TProgressOther> Load<TProgressOther>
    (
        ILoadWithProgressAndCancellationAsync<TDestination, TProgressOther> loader
    )
        where TProgressOther : notnull
    {
        return new TransformStage<TDestination>(Source).Load(loader);
    }
}
