using System;
using System.Threading;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Represents a mid-chain stage of a fluent ETL pipeline whose current item type is
/// <typeparamref name="TSource"/>. Call one of the <c>Transform</c> overloads to append another
/// transformer, or one of the <c>Load</c> overloads to terminate the chain.
/// </summary>
/// <typeparam name="TSource">The current item type flowing through the pipeline.</typeparam>
public interface ITransformStage<TSource>
    where TSource : notnull
{
    /// <summary>
    /// Appends a transformer that supports neither progress nor cancellation. The pipeline's
    /// <see cref="CancellationToken"/> will not be forwarded into this stage.
    /// </summary>
    /// <typeparam name="TDestination">The type of item produced by the transformer.</typeparam>
    /// <param name="transformer">The transformer to append.</param>
    /// <returns>The next stage in the chain, producing <typeparamref name="TDestination"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="transformer"/> is <see langword="null"/>.</exception>
    ITransformStage<TDestination> Transform<TDestination>
    (
        ITransformAsync<TSource, TDestination> transformer
    )
        where TDestination : notnull;


    /// <summary>
    /// Appends a transformer that supports cancellation but does not report progress.
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
    /// Appends a progress-reporting transformer that does not support cancellation. The pipeline's
    /// <see cref="CancellationToken"/> will not be forwarded into this stage.
    /// </summary>
    /// <typeparam name="TDestination">The type of item produced by the transformer.</typeparam>
    /// <typeparam name="TProgress">The type of progress report emitted by the transformer.</typeparam>
    /// <param name="transformer">The transformer to append.</param>
    /// <returns>The next stage in the chain, producing <typeparamref name="TDestination"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="transformer"/> is <see langword="null"/>.</exception>
    ITransformStageWithProgress<TDestination, TProgress> Transform<TDestination, TProgress>
    (
        ITransformWithProgressAsync<TSource, TDestination, TProgress> transformer
    )
        where TDestination : notnull
        where TProgress : notnull;


    /// <summary>
    /// Appends a progress-reporting transformer that also supports cancellation.
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
    /// Terminates the chain with a loader that supports neither progress nor cancellation.
    /// The pipeline's <see cref="CancellationToken"/> will not be forwarded into this stage.
    /// </summary>
    /// <param name="loader">The loader that consumes the pipeline output.</param>
    /// <returns>A runnable <see cref="IPipeline"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="loader"/> is <see langword="null"/>.</exception>
    IPipeline Load(ILoadAsync<TSource> loader);


    /// <summary>
    /// Terminates the chain with a loader that supports cancellation but does not report progress.
    /// </summary>
    /// <param name="loader">The loader that consumes the pipeline output.</param>
    /// <returns>A runnable <see cref="IPipeline"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="loader"/> is <see langword="null"/>.</exception>
    IPipeline Load(ILoadWithCancellationAsync<TSource> loader);


    /// <summary>
    /// Terminates the chain with a progress-reporting loader that does not support cancellation.
    /// The pipeline's <see cref="CancellationToken"/> will not be forwarded into this stage.
    /// </summary>
    /// <typeparam name="TProgress">The type of progress report emitted by the loader.</typeparam>
    /// <param name="loader">The loader that consumes the pipeline output.</param>
    /// <returns>A runnable <see cref="IPipelineWithLoadProgress{TProgress}"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="loader"/> is <see langword="null"/>.</exception>
    IPipelineWithLoadProgress<TProgress> Load<TProgress>
    (
        ILoadWithProgressAsync<TSource, TProgress> loader
    )
        where TProgress : notnull;


    /// <summary>
    /// Terminates the chain with a progress-reporting loader that also supports cancellation.
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
