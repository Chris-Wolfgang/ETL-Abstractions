using System;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Per-stage typed progress for <see cref="EtlPipeline"/> (#382): attach a transformer or loader
/// together with an <see cref="IProgress{T}"/> that receives that stage's own progress type, the
/// counterpart of <c>WithProgress</c> on the fluent <see cref="Pipeline"/> builder. The aggregate
/// <see cref="EtlPipelineProgress"/> passed to <see cref="IEtlPipelineSink.RunAsync"/> is reported as
/// before; the two are independent.
/// </summary>
/// <example>
/// <code>
/// await EtlPipeline
///     .Create()
///     .From(orderExtractor, extractProgress)
///     .Through(enrichTransformer, enrichProgress)
///     .To(sqlLoader, loadProgress)
///     .RunAsync(progress, token);
/// </code>
/// </example>
public static class EtlPipelineStageProgressExtensions
{
    /// <summary>
    /// Adds a <see cref="TransformerBase{TSource, TDestination, TProgress}"/> stage whose own
    /// <typeparamref name="TProgress"/> reports go to <paramref name="progress"/>. The pipeline's
    /// cancellation token is forwarded to the transformer.
    /// </summary>
    /// <typeparam name="T">The type of item flowing into the transformer.</typeparam>
    /// <typeparam name="TOut">The type of item the transformer produces.</typeparam>
    /// <typeparam name="TProgress">The transformer's progress-report type.</typeparam>
    /// <param name="pipeline">The pipeline to extend.</param>
    /// <param name="transformer">The transformer to add.</param>
    /// <param name="progress">Receives the transformer's <typeparamref name="TProgress"/> reports.</param>
    /// <returns>An <see cref="IEtlPipeline{T}"/> of <typeparamref name="TOut"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="pipeline"/>, <paramref name="transformer"/> or <paramref name="progress"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static IEtlPipeline<TOut> Through<T, TOut, TProgress>
    (
        this IEtlPipeline<T> pipeline,
        TransformerBase<T, TOut, TProgress> transformer,
        IProgress<TProgress> progress
    )
        where T : notnull
        where TOut : notnull
        where TProgress : notnull
    {
        if (pipeline is null)
        {
            throw new ArgumentNullException(nameof(pipeline));
        }

        if (transformer is null)
        {
            throw new ArgumentNullException(nameof(transformer));
        }

        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        return AsImpl(pipeline).ThroughWithProgress(transformer, progress);
    }



    /// <summary>
    /// Terminates the pipeline with a <see cref="LoaderBase{TDestination, TProgress}"/> whose own
    /// <typeparamref name="TProgress"/> reports go to <paramref name="progress"/>.
    /// </summary>
    /// <typeparam name="T">The type of item the loader consumes.</typeparam>
    /// <typeparam name="TProgress">The loader's progress-report type.</typeparam>
    /// <param name="pipeline">The pipeline to terminate.</param>
    /// <param name="loader">The loader that consumes the stream.</param>
    /// <param name="progress">Receives the loader's <typeparamref name="TProgress"/> reports.</param>
    /// <returns>An <see cref="IEtlPipelineSink"/> ready to run.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="pipeline"/>, <paramref name="loader"/> or <paramref name="progress"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static IEtlPipelineSink To<T, TProgress>
    (
        this IEtlPipeline<T> pipeline,
        LoaderBase<T, TProgress> loader,
        IProgress<TProgress> progress
    )
        where T : notnull
        where TProgress : notnull
    {
        if (pipeline is null)
        {
            throw new ArgumentNullException(nameof(pipeline));
        }

        if (loader is null)
        {
            throw new ArgumentNullException(nameof(loader));
        }

        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        return AsImpl(pipeline).ToWithProgress(loader, progress);
    }



    // Every pipeline EtlPipeline builds is an EtlPipelineImpl, which shares one run state across its
    // stages. Any other IEtlPipeline implementation is adapted through its AsAsyncEnumerable, so the
    // overloads still work; its own stages just do not contribute to the aggregate counts.
    private static EtlPipelineImpl<T> AsImpl<T>(IEtlPipeline<T> pipeline)
        where T : notnull
    {
        return pipeline as EtlPipelineImpl<T>
            ?? EtlPipelineImpl<T>.FromStream((_, token) => pipeline.AsAsyncEnumerable(token));
    }
}
