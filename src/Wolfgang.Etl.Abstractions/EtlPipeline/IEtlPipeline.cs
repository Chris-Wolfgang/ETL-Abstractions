using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// A generic, format-agnostic ETL pipeline carrying a stream of items of type
/// <typeparamref name="T"/>. Chain the LINQ-flavored operators to reshape the stream, then terminate
/// with <see cref="To{TProgress}(LoaderBase{T, TProgress})"/> (or a format-specific sink terminator)
/// to obtain a runnable <see cref="IEtlPipelineSink"/>.
/// </summary>
/// <typeparam name="T">The type of item currently flowing through the pipeline.</typeparam>
/// <remarks>
/// <para>
/// The pipeline is lazy: operators build up a description of the work and nothing executes until
/// <see cref="IEtlPipelineSink.RunAsync(IProgress{EtlPipelineProgress}, CancellationToken)"/> is
/// called (or the stream is enumerated via <see cref="AsAsyncEnumerable(CancellationToken)"/>).
/// </para>
/// <para>
/// Operators share names with <c>System.Linq.Async</c> but live on a different interface, so there
/// is no conflict. The pipeline operators participate in <see cref="EtlPipelineProgress"/> reporting;
/// the LINQ ones do not.
/// </para>
/// </remarks>
public interface IEtlPipeline<T>
    where T : notnull
{
    /// <summary>Keeps only the records for which <paramref name="predicate"/> returns <see langword="true"/>.</summary>
    /// <param name="predicate">The synchronous predicate to test each record against.</param>
    /// <returns>The pipeline, for further chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
    IEtlPipeline<T> Where(Func<T, bool> predicate);


    /// <summary>Keeps only the records for which <paramref name="asyncPredicate"/> returns <see langword="true"/>.</summary>
    /// <param name="asyncPredicate">The asynchronous predicate to test each record against.</param>
    /// <returns>The pipeline, for further chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="asyncPredicate"/> is <see langword="null"/>.</exception>
    IEtlPipeline<T> Where(Func<T, ValueTask<bool>> asyncPredicate);


    /// <summary>Projects each record to a new value of type <typeparamref name="TOut"/>.</summary>
    /// <typeparam name="TOut">The projected item type.</typeparam>
    /// <param name="selector">The synchronous projection.</param>
    /// <returns>A pipeline carrying <typeparamref name="TOut"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
    IEtlPipeline<TOut> Select<TOut>(Func<T, TOut> selector) where TOut : notnull;


    /// <summary>Projects each record to a new value of type <typeparamref name="TOut"/>.</summary>
    /// <typeparam name="TOut">The projected item type.</typeparam>
    /// <param name="selector">The asynchronous projection.</param>
    /// <returns>A pipeline carrying <typeparamref name="TOut"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
    IEtlPipeline<TOut> Select<TOut>(Func<T, ValueTask<TOut>> selector) where TOut : notnull;


    /// <summary>Projects each record to a stream of <typeparamref name="TOut"/> and flattens the results.</summary>
    /// <typeparam name="TOut">The projected item type.</typeparam>
    /// <param name="selector">The projection producing a stream per input record.</param>
    /// <returns>A pipeline carrying the flattened <typeparamref name="TOut"/> stream.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
    IEtlPipeline<TOut> SelectMany<TOut>(Func<T, IAsyncEnumerable<TOut>> selector) where TOut : notnull;


    /// <summary>Removes records with duplicate keys, keeping the first occurrence of each key.</summary>
    /// <typeparam name="TKey">The key type used to detect duplicates.</typeparam>
    /// <param name="keySelector">Extracts the comparison key from each record.</param>
    /// <param name="comparer">An optional key comparer; the default comparer is used when <see langword="null"/>.</param>
    /// <returns>The pipeline, for further chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="keySelector"/> is <see langword="null"/>.</exception>
    IEtlPipeline<T> Distinct<TKey>(Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer = null);


    /// <summary>Passes through at most <paramref name="count"/> records, then stops enumerating the source.</summary>
    /// <param name="count">The maximum number of records to keep.</param>
    /// <returns>The pipeline, for further chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    IEtlPipeline<T> Take(int count);


    /// <summary>Discards the first <paramref name="count"/> records and passes through the rest.</summary>
    /// <param name="count">The number of leading records to discard.</param>
    /// <returns>The pipeline, for further chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    IEtlPipeline<T> Skip(int count);


    /// <summary>Invokes a side effect for each record without altering the stream.</summary>
    /// <param name="sideEffect">The synchronous side effect to run per record.</param>
    /// <returns>The pipeline, for further chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sideEffect"/> is <see langword="null"/>.</exception>
    IEtlPipeline<T> Tap(Action<T> sideEffect);


    /// <summary>Invokes an asynchronous side effect for each record without altering the stream.</summary>
    /// <param name="asyncSideEffect">The asynchronous side effect to run per record.</param>
    /// <returns>The pipeline, for further chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="asyncSideEffect"/> is <see langword="null"/>.</exception>
    IEtlPipeline<T> Tap(Func<T, ValueTask> asyncSideEffect);


    /// <summary>Groups consecutive records into batches of up to <paramref name="size"/> records.</summary>
    /// <param name="size">The maximum batch size; the final batch may be smaller.</param>
    /// <returns>A pipeline carrying batches as <see cref="IReadOnlyList{T}"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is less than 1.</exception>
    IEtlPipeline<IReadOnlyList<T>> Buffer(int size);


    /// <summary>
    /// Terminates the pipeline with a loader. Format-specific sink terminators (for example
    /// <c>CsvLoader</c> or <c>SqlBulkCopyLoader</c>) are extension methods shipped by their own
    /// packages; this generic overload is the catch-all for callers who already hold a
    /// <see cref="LoaderBase{TDestination, TProgress}"/>.
    /// </summary>
    /// <typeparam name="TProgress">The loader's progress-report type.</typeparam>
    /// <param name="loader">The loader that consumes the pipeline output.</param>
    /// <returns>A runnable <see cref="IEtlPipelineSink"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="loader"/> is <see langword="null"/>.</exception>
    IEtlPipelineSink To<TProgress>(LoaderBase<T, TProgress> loader) where TProgress : notnull;


    /// <summary>
    /// Drops down to the raw <see cref="IAsyncEnumerable{T}"/> for advanced operations not covered by
    /// the built-in operators. Enumerating the returned stream runs the pipeline (without a sink, so
    /// <see cref="EtlPipelineProgress.RecordsLoaded"/> is not tracked).
    /// </summary>
    /// <param name="token">A cancellation token observed while enumerating.</param>
    /// <returns>The composed record stream.</returns>
    IAsyncEnumerable<T> AsAsyncEnumerable(CancellationToken token = default);
}
