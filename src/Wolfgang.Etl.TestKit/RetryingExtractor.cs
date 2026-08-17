using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit;

/// <summary>
/// An extractor double that throws a transient fault on its first <c>failFirstAttempts</c> worker
/// invocations and then succeeds, driven through a retry override of the
/// <see cref="Wolfgang.Etl.Abstractions"/> 0.20 <c>WrapWorkerExecution</c> resilience seam. It both
/// demonstrates how to build stream-level retry on a stage and serves as the reference component
/// exercised by <c>RetryContractTests</c> (in <c>Wolfgang.Etl.TestKit.Xunit</c>).
/// </summary>
/// <typeparam name="T">The type of item to extract. Must be <c>notnull</c>.</typeparam>
/// <remarks>
/// <para>
/// Each retry re-invokes the worker from the start (stream-level resilience, per the base contract).
/// Because the transient fault is thrown at the <em>start</em> of the worker — before any item is
/// yielded — a retried attempt never double-yields. After <c>failFirstAttempts</c> failures the
/// worker yields every item; if the retry budget (<c>maxAttempts</c>) is exhausted first, the last
/// fault propagates. <see cref="AttemptCount"/> records how many worker invocations occurred.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Fails twice, then succeeds on the third attempt (budget of 5):
/// var extractor = new RetryingExtractor&lt;int&gt;(Enumerable.Range(0, 10), failFirstAttempts: 2, maxAttempts: 5);
/// await foreach (var _ in extractor.ExtractAsync()) { }   // succeeds; extractor.AttemptCount == 3
/// </code>
/// </example>
public class RetryingExtractor<T> : ExtractorBase<T, Report>
    where T : notnull
{
    // ------------------------------------------------------------------
    // Fields
    // ------------------------------------------------------------------

    private readonly IEnumerable<T> _items;
    private readonly int _failFirstAttempts;
    private readonly int _maxAttempts;
    private int _attemptCount;



    // ------------------------------------------------------------------
    // Constructor
    // ------------------------------------------------------------------

    /// <summary>
    /// Initializes a new <see cref="RetryingExtractor{T}"/>.
    /// </summary>
    /// <param name="items">The items to extract once a worker invocation succeeds.</param>
    /// <param name="failFirstAttempts">
    /// The number of leading worker invocations that throw a transient fault before one succeeds.
    /// Use a value ≥ <paramref name="maxAttempts"/> to model a permanent fault.
    /// </param>
    /// <param name="maxAttempts">The maximum number of worker invocations (initial try plus retries).</param>
    /// <exception cref="ArgumentNullException"><paramref name="items"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="failFirstAttempts"/> is negative, or <paramref name="maxAttempts"/> is less than 1.
    /// </exception>
    public RetryingExtractor(IEnumerable<T> items, int failFirstAttempts, int maxAttempts)
    {
        _items = items ?? throw new ArgumentNullException(nameof(items));

        if (failFirstAttempts < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(failFirstAttempts), failFirstAttempts, "Must not be negative.");
        }

        if (maxAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), maxAttempts, "Must be at least 1.");
        }

        _failFirstAttempts = failFirstAttempts;
        _maxAttempts       = maxAttempts;
    }



    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    /// <summary>Gets the number of worker invocations (attempts) made by the most recent run.</summary>
    public int AttemptCount => _attemptCount;



    // ------------------------------------------------------------------
    // ExtractorBase overrides
    // ------------------------------------------------------------------

    /// <inheritdoc/>
    protected override Report CreateProgressReport() =>
        new(CurrentItemCount, StartedAt, Elapsed, (_items as ICollection<T>)?.Count);



    /// <inheritdoc/>
    protected override async IAsyncEnumerable<T> ExtractWorkerAsync
    (
        [EnumeratorCancellation] CancellationToken token
    )
    {
        var attempt = Interlocked.Increment(ref _attemptCount);

        token.ThrowIfCancellationRequested();

        // Stryker disable once Statement : Task.Yield only forces the continuation to run
        // asynchronously; removing it changes scheduling, not observable behaviour, so the
        // statement-removal mutant is equivalent and unkillable.
        await Task.Yield();

        if (attempt <= _failFirstAttempts)
        {
            throw new InvalidOperationException($"Transient fault on attempt {attempt}.");
        }

        foreach (var item in _items)
        {
            token.ThrowIfCancellationRequested();

            if (CurrentSkippedItemCount < SkipItemCount)
            {
                IncrementCurrentSkippedItemCount();
                continue;
            }

            if (CurrentItemCount >= MaximumItemCount)
            {
                yield break;
            }

            IncrementCurrentItemCount();
            yield return item;
        }
    }



    /// <summary>
    /// Retries the worker up to <c>maxAttempts</c> times: each attempt re-invokes
    /// <paramref name="workerFactory"/> to produce a fresh stream, and a failed attempt is retried
    /// until the budget is exhausted, at which point the last fault propagates. Cancellation is never
    /// retried.
    /// </summary>
    /// <param name="workerFactory">A factory producing a fresh worker stream for the supplied token.</param>
    /// <param name="token">A token to observe.</param>
    /// <returns>The retried stream of extracted items.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="workerFactory"/> is <see langword="null"/>.</exception>
    protected override IAsyncEnumerable<T> WrapWorkerExecution
    (
        Func<CancellationToken, IAsyncEnumerable<T>> workerFactory,
        CancellationToken token
    )
    {
        if (workerFactory is null)
        {
            throw new ArgumentNullException(nameof(workerFactory));
        }

        // Split so the null check runs eagerly at call time rather than being
        // deferred to the first MoveNextAsync of the iterator (S4456).
        return WrapWorkerExecutionCore(workerFactory, token);
    }



    private async IAsyncEnumerable<T> WrapWorkerExecutionCore
    (
        Func<CancellationToken, IAsyncEnumerable<T>> workerFactory,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        for (var attempt = 1; ; attempt++)
        {
            var buffer  = new List<T>();
            ExceptionDispatchInfo? failure = null;

            var enumerator = workerFactory(token).GetAsyncEnumerator(token);
            try
            {
                while (true)
                {
                    try
                    {
                        // Stryker disable once Boolean : ConfigureAwait(false) vs (true) is equivalent under the test host.
                        if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                        {
                            break;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
#pragma warning disable CA1031 // a test double deliberately captures any worker fault to drive retry
                    catch (Exception ex)
                    {
                        failure = ExceptionDispatchInfo.Capture(ex);
                        // Stryker disable once Statement : the worker only faults at stream start, so its iterator is
                        // already done here; dropping this break just costs one MoveNextAsync that returns false.
                        break;
                    }
#pragma warning restore CA1031

                    buffer.Add(enumerator.Current);
                }
            }
            finally
            {
                // Stryker disable once Statement,Boolean : the iterator is always completed here so DisposeAsync
                // is a no-op; ConfigureAwait(false) vs (true) is equivalent under the test host. Both unkillable.
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }

            if (failure is null)
            {
                foreach (var item in buffer)
                {
                    yield return item;
                }

                yield break;
            }

            if (attempt >= _maxAttempts)
            {
                failure.Throw();
            }
        }
    }
}
