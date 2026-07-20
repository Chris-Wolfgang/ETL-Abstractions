using System;
using System.Collections.Generic;
using System.Threading;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// A generic, format-agnostic ETL pipeline carrying a stream of items of type
/// <typeparamref name="T"/>. Append transformer stages with <see cref="Through{TOut}(ITransformAsync{T, TOut})"/>,
/// then terminate with <see cref="To{TProgress}(LoaderBase{T, TProgress})"/> (or a format-specific
/// sink terminator) to obtain a runnable <see cref="IEtlPipelineSink"/>.
/// </summary>
/// <typeparam name="T">The type of item currently flowing through the pipeline.</typeparam>
/// <remarks>
/// <para>
/// This is the minimal pipeline core. It knows how to pull from a source, pipe through transformers,
/// and push to a sink — nothing more. The LINQ-flavored operators (<c>Where</c>, <c>Select</c>,
/// <c>Distinct</c>, <c>Take</c>, <c>Buffer</c>, …) are <em>extension methods</em> that layer on top
/// of <see cref="Through{TOut}(ITransformAsync{T, TOut})"/> and are shipped by
/// <c>Wolfgang.Etl.Transformers</c>, which already owns those transformers — the core does not
/// re-implement them.
/// </para>
/// <para>
/// The pipeline is lazy: appending a stage builds up a description of the work and nothing executes
/// until <see cref="IEtlPipelineSink.RunAsync(IProgress{EtlPipelineProgress}, CancellationToken)"/>
/// is called (or the stream is enumerated via <see cref="AsAsyncEnumerable(CancellationToken)"/>).
/// </para>
/// </remarks>
public interface IEtlPipeline<T>
    where T : notnull
{
    /// <summary>
    /// Appends a transformer stage, piping the current stream through it. This is the primitive on
    /// which higher-level operators are built.
    /// </summary>
    /// <typeparam name="TOut">The type produced by the transformer.</typeparam>
    /// <param name="transformer">The transformer to append.</param>
    /// <returns>A pipeline carrying <typeparamref name="TOut"/>.</returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="transformer"/> is <see langword="null"/>.</exception>
    IEtlPipeline<TOut> Through<TOut>(ITransformAsync<T, TOut> transformer) where TOut : notnull;


    /// <summary>
    /// Appends a cancellation-aware transformer stage. The pipeline's cancellation token is forwarded
    /// into the transformer.
    /// </summary>
    /// <typeparam name="TOut">The type produced by the transformer.</typeparam>
    /// <param name="transformer">The transformer to append.</param>
    /// <returns>A pipeline carrying <typeparamref name="TOut"/>.</returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="transformer"/> is <see langword="null"/>.</exception>
    IEtlPipeline<TOut> Through<TOut>(ITransformWithCancellationAsync<T, TOut> transformer) where TOut : notnull;


    /// <summary>
    /// Appends a stream-to-stream transformer stage supplied as a delegate — the same contract as
    /// <see cref="ITransformAsync{TSource, TDestination}.TransformAsync(IAsyncEnumerable{TSource})"/>,
    /// but inline, without declaring a class. Note this transforms the <em>whole stream</em>; a
    /// per-element projection (<c>Select</c>) is an operator provided by <c>Wolfgang.Etl.Transformers</c>,
    /// not the core.
    /// </summary>
    /// <typeparam name="TOut">The type produced by the stage.</typeparam>
    /// <param name="stage">The stream-to-stream transform to append.</param>
    /// <returns>A pipeline carrying <typeparamref name="TOut"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stage"/> is <see langword="null"/>.</exception>
    IEtlPipeline<TOut> Through<TOut>(Func<IAsyncEnumerable<T>, IAsyncEnumerable<TOut>> stage) where TOut : notnull;


    /// <summary>
    /// Appends a cancellation-aware stream-to-stream transformer stage supplied as a delegate. The
    /// pipeline's cancellation token is forwarded into the delegate.
    /// </summary>
    /// <typeparam name="TOut">The type produced by the stage.</typeparam>
    /// <param name="stage">The stream-to-stream transform to append.</param>
    /// <returns>A pipeline carrying <typeparamref name="TOut"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stage"/> is <see langword="null"/>.</exception>
    IEtlPipeline<TOut> Through<TOut>(Func<IAsyncEnumerable<T>, CancellationToken, IAsyncEnumerable<TOut>> stage) where TOut : notnull;


    /// <summary>
    /// Terminates the pipeline with a loader. Format-specific sink terminators (for example
    /// <c>CsvLoader</c> or <c>SqlBulkCopyLoader</c>) are extension methods shipped by their own
    /// packages; this generic overload is the catch-all for callers who already hold a
    /// <see cref="LoaderBase{TDestination, TProgress}"/>.
    /// </summary>
    /// <typeparam name="TProgress">The loader's progress-report type.</typeparam>
    /// <param name="loader">The loader that consumes the pipeline output.</param>
    /// <returns>A runnable <see cref="IEtlPipelineSink"/>.</returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="loader"/> is <see langword="null"/>.</exception>
    IEtlPipelineSink To<TProgress>(LoaderBase<T, TProgress> loader) where TProgress : notnull;


    /// <summary>
    /// Drops down to the raw <see cref="IAsyncEnumerable{T}"/> — the escape hatch for consumers who
    /// want to apply <c>System.Linq.Async</c> operators directly or otherwise leave the pipeline.
    /// Enumerating the returned stream runs the pipeline (without a sink, so
    /// <see cref="EtlPipelineProgress.RecordsLoaded"/> is not tracked).
    /// </summary>
    /// <param name="token">A cancellation token observed while enumerating.</param>
    /// <returns>The composed record stream.</returns>
    IAsyncEnumerable<T> AsAsyncEnumerable(CancellationToken token = default);
}
