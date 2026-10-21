using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace Wolfgang.Etl.Polly;

/// <summary>
/// Builds <c>WorkerResilience</c> wrappers that run a Wolfgang.Etl stage's worker through a Polly
/// <see cref="ResiliencePipeline"/>. Assign the result to the stage's <c>WorkerResilience</c> property;
/// no subclassing is needed.
/// </summary>
/// <remarks>
/// <para>
/// This is <b>stream-level</b> resilience, the contract of the <c>WorkerResilience</c> seam: the
/// pipeline spans the whole worker, so a retry re-runs the entire extract, transform or load. Use it
/// for whole-batch retry, timeouts, circuit breakers and rate limiting. Per-item transient-fault retry
/// belongs inside the worker, and the decision to skip or dead-letter a bad item belongs to the
/// stage's <c>ErrorPolicy</c>; this package does not reimplement either.
/// </para>
/// <para>
/// The stage's counters (<c>CurrentItemCount</c> and friends) are reset once per run, not per retry,
/// so records counted by a failed attempt stay counted.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var pipeline = new ResiliencePipelineBuilder()
///     .AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 3 })
///     .AddTimeout(TimeSpan.FromMinutes(5))
///     .Build();
///
/// var extractor = new CsvExtractor(path)
/// {
///     WorkerResilience = PollyResilience.ForStream&lt;CsvRow&gt;(pipeline),
/// };
///
/// var loader = new SqlBulkCopyLoader(connectionString)
/// {
///     WorkerResilience = PollyResilience.ForLoader(pipeline),
/// };
/// </code>
/// </example>
public static class PollyResilience
{
    /// <summary>
    /// Creates a <c>WorkerResilience</c> wrapper for an extractor or transformer. Each attempt runs the
    /// worker to completion inside <paramref name="pipeline"/> and buffers its items; the items are
    /// yielded only after an attempt succeeds.
    /// </summary>
    /// <typeparam name="T">The item type the stage produces.</typeparam>
    /// <param name="pipeline">The Polly pipeline to run each attempt through.</param>
    /// <returns>A wrapper to assign to <c>ExtractorBase.WorkerResilience</c> or <c>TransformerBase.WorkerResilience</c>.</returns>
    /// <remarks>
    /// Buffering is what makes a retry safe: a failed attempt never leaks part of its output downstream,
    /// so nothing is yielded twice. The cost is that one attempt's items are held in memory and the first
    /// item arrives only when the worker finishes. For a large or unbounded stream, put the retry inside
    /// the worker (around the call that can fail transiently) instead.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> is <see langword="null"/>.</exception>
    public static Func<Func<CancellationToken, IAsyncEnumerable<T>>, CancellationToken, IAsyncEnumerable<T>> ForStream<T>
    (
        ResiliencePipeline pipeline
    )
    {
        if (pipeline is null)
        {
            throw new ArgumentNullException(nameof(pipeline));
        }

        return (workerFactory, token) => BufferedAsync(pipeline, workerFactory, token);
    }



    /// <summary>
    /// Creates a <c>WorkerResilience</c> wrapper for a loader: each attempt runs the whole load inside
    /// <paramref name="pipeline"/>.
    /// </summary>
    /// <param name="pipeline">The Polly pipeline to run each attempt through.</param>
    /// <returns>A wrapper to assign to <c>LoaderBase.WorkerResilience</c>.</returns>
    /// <remarks>
    /// A retry re-runs the loader's worker, which enumerates its input again from the start. That is
    /// safe only when the input can be enumerated more than once (for example a list, or a pipeline
    /// source that re-reads its data) and the destination tolerates a repeated write (an idempotent
    /// upsert, or a load inside a transaction the failed attempt rolled back). For a single-pass input,
    /// use a pipeline without retry (timeout, circuit breaker, rate limiter).
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> is <see langword="null"/>.</exception>
    public static Func<Func<CancellationToken, Task>, CancellationToken, Task> ForLoader(ResiliencePipeline pipeline)
    {
        if (pipeline is null)
        {
            throw new ArgumentNullException(nameof(pipeline));
        }

        return (workerFactory, token) => pipeline
            .ExecuteAsync
            (
                static async (factory, ct) => await factory(ct).ConfigureAwait(false),
                workerFactory,
                token
            )
            .AsTask();
    }



    private static async IAsyncEnumerable<T> BufferedAsync<T>
    (
        ResiliencePipeline pipeline,
        Func<CancellationToken, IAsyncEnumerable<T>> workerFactory,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        var items = await pipeline
            .ExecuteAsync
            (
                static async (factory, ct) =>
                {
                    var buffer = new List<T>();
                    await foreach (var item in factory(ct).WithCancellation(ct).ConfigureAwait(false))
                    {
                        buffer.Add(item);
                    }

                    return buffer;
                },
                workerFactory,
                token
            )
            .ConfigureAwait(false);

        foreach (var item in items)
        {
            yield return item;
        }
    }
}
