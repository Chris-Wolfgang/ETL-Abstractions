using System;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Represents the extractor stage of a fluent ETL pipeline for items of type
/// <typeparamref name="TSource"/>. Call <see cref="Transform{TDestination}"/> to append a
/// transformer, or <see cref="Load"/> to terminate the chain with a loader.
/// </summary>
/// <typeparam name="TSource">The type of item produced by the extractor.</typeparam>
/// <remarks>
/// Stages are obtained from <see cref="Pipeline.Extract{TSource}"/>. Each stage is a thin,
/// compile-time-typed wrapper that defers execution until
/// <see cref="IPipeline.RunAsync()"/> is invoked.
/// </remarks>
public interface IExtractStage<TSource>
    where TSource : notnull
{
    /// <summary>
    /// Appends a transformer that does not report progress.
    /// </summary>
    /// <typeparam name="TDestination">The type of item produced by the transformer.</typeparam>
    /// <param name="transformer">The transformer to append.</param>
    /// <returns>The next stage in the chain, producing <typeparamref name="TDestination"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="transformer"/> is <see langword="null"/>.</exception>
    ITransformStage<TDestination> Transform<TDestination>
    (
        ITransformWithCancellationAsync<TSource, TDestination> transformer
    )
        where TDestination : notnull;


    /// <summary>
    /// Appends a progress-reporting transformer. The returned stage exposes
    /// <see cref="ITransformStageWithProgress{TDestination, TProgress}.WithProgress"/>.
    /// </summary>
    /// <typeparam name="TDestination">The type of item produced by the transformer.</typeparam>
    /// <typeparam name="TProgress">The type of progress report emitted by the transformer.</typeparam>
    /// <param name="transformer">The transformer to append.</param>
    /// <returns>The next stage in the chain, producing <typeparamref name="TDestination"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="transformer"/> is <see langword="null"/>.</exception>
    ITransformStageWithProgress<TDestination, TProgress> Transform<TDestination, TProgress>
    (
        ITransformWithProgressAndCancellationAsync<TSource, TDestination, TProgress> transformer
    )
        where TDestination : notnull
        where TProgress : notnull;


    /// <summary>
    /// Terminates the chain with a loader that does not report progress.
    /// </summary>
    /// <param name="loader">The loader that consumes the pipeline output.</param>
    /// <returns>A runnable <see cref="IPipeline"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="loader"/> is <see langword="null"/>.</exception>
    IPipeline Load(ILoadWithCancellationAsync<TSource> loader);


    /// <summary>
    /// Terminates the chain with a progress-reporting loader. The returned pipeline exposes
    /// <see cref="IPipelineWithLoadProgress{TProgress}.WithProgress"/>.
    /// </summary>
    /// <typeparam name="TProgress">The type of progress report emitted by the loader.</typeparam>
    /// <param name="loader">The loader that consumes the pipeline output.</param>
    /// <returns>A runnable <see cref="IPipelineWithLoadProgress{TProgress}"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="loader"/> is <see langword="null"/>.</exception>
    IPipelineWithLoadProgress<TProgress> Load<TProgress>
    (
        ILoadWithProgressAndCancellationAsync<TSource, TProgress> loader
    )
        where TProgress : notnull;
}
