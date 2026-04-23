using System;
using System.Collections.Generic;
using System.Threading;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Internal <see cref="ITransformStageWithProgress{TDestination, TProgress}"/> implementation.
/// Holds the upstream source, the progress-capable transformer, and an optional bound
/// <see cref="IProgress{TProgress}"/>.
/// </summary>
internal sealed class TransformStageWithProgress<TUpstream, TDestination, TProgress>
    : ITransformStageWithProgress<TDestination, TProgress>
    where TUpstream : notnull
    where TDestination : notnull
    where TProgress : notnull
{
    private readonly Func<CancellationToken, IAsyncEnumerable<TUpstream>> _upstream;
    private readonly ITransformWithProgressAndCancellationAsync<TUpstream, TDestination, TProgress> _transformer;
    private IProgress<TProgress>? _progress;


    internal TransformStageWithProgress
    (
        Func<CancellationToken, IAsyncEnumerable<TUpstream>> upstream,
        ITransformWithProgressAndCancellationAsync<TUpstream, TDestination, TProgress> transformer
    )
    {
        _upstream = upstream;
        _transformer = transformer;
    }


    private IAsyncEnumerable<TDestination> Source(CancellationToken token)
    {
        return _progress is null
            ? _transformer.TransformAsync(_upstream(token), token)
            : _transformer.TransformAsync(_upstream(token), _progress, token);
    }


    public ITransformStage<TDestination> WithProgress(IProgress<TProgress> progress)
    {
        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        _progress = progress;
        return new TransformStage<TDestination>(Source);
    }


    public ITransformStage<TOut> Transform<TOut>
    (
        ITransformWithCancellationAsync<TDestination, TOut> transformer
    )
        where TOut : notnull
    {
        return new TransformStage<TDestination>(Source).Transform(transformer);
    }


    public ITransformStageWithProgress<TOut, TProgressOther> Transform<TOut, TProgressOther>
    (
        ITransformWithProgressAndCancellationAsync<TDestination, TOut, TProgressOther> transformer
    )
        where TOut : notnull
        where TProgressOther : notnull
    {
        return new TransformStage<TDestination>(Source).Transform(transformer);
    }


    public IPipeline Load(ILoadWithCancellationAsync<TDestination> loader)
    {
        return new TransformStage<TDestination>(Source).Load(loader);
    }


    public IPipelineWithLoadProgress<TProgressOther> Load<TProgressOther>
    (
        ILoadWithProgressAndCancellationAsync<TDestination, TProgressOther> loader
    )
        where TProgressOther : notnull
    {
        return new TransformStage<TDestination>(Source).Load(loader);
    }
}
