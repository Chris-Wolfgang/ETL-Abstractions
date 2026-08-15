using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit;

/// <summary>
/// An extractor double that waits a configurable delay before yielding each item, simulating a
/// latent / backpressured source. Useful for exercising timeout, cancellation-promptness, and
/// progress-cadence behaviour that an instantaneous in-memory source cannot.
/// </summary>
/// <typeparam name="T">The type of item to extract. Must be <c>notnull</c>.</typeparam>
/// <remarks>
/// <para>
/// The delay is awaited with <see cref="Task.Delay(TimeSpan, CancellationToken)"/> <em>before</em>
/// each item is yielded, so a cancelled token interrupts the wait and the extractor stops promptly
/// (throwing <see cref="OperationCanceledException"/>) rather than draining the source. Pair it with
/// <c>CancellationContractTests</c> (in <c>Wolfgang.Etl.TestKit.Xunit</c>) to verify a stage cancels
/// quickly and leaves its counters consistent.
/// </para>
/// <para>
/// Supply a single <see cref="TimeSpan"/> for a uniform delay, or a <c>Func&lt;int, TimeSpan&gt;</c>
/// to vary the delay by zero-based item index. <see cref="LoaderBase{TDestination,TProgress}"/>-style
/// <see cref="ExtractorBase{TSource,TProgress}.SkipItemCount"/> /
/// <see cref="ExtractorBase{TSource,TProgress}.MaximumItemCount"/> bounds are honoured.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Each item arrives 20 ms apart:
/// var extractor = new DelayingExtractor&lt;int&gt;(Enumerable.Range(0, 100), TimeSpan.FromMilliseconds(20));
/// using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
/// // Enumerating with cts.Token throws OperationCanceledException after ~2-3 items.
/// </code>
/// </example>
public class DelayingExtractor<T> : ExtractorBase<T, Report>
    where T : notnull
{
    // ------------------------------------------------------------------
    // Fields
    // ------------------------------------------------------------------

    private readonly IEnumerable<T> _items;
    private readonly Func<int, TimeSpan> _delaySelector;



    // ------------------------------------------------------------------
    // Constructors
    // ------------------------------------------------------------------

    /// <summary>
    /// Initializes a new <see cref="DelayingExtractor{T}"/> that waits <paramref name="delay"/> before
    /// yielding every item.
    /// </summary>
    /// <param name="items">The items to extract.</param>
    /// <param name="delay">The delay to await before each item.</param>
    /// <exception cref="ArgumentNullException"><paramref name="items"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="delay"/> is negative.</exception>
    public DelayingExtractor(IEnumerable<T> items, TimeSpan delay)
        : this(items, ValidatedConstantDelay(delay))
    {
    }



    /// <summary>
    /// Initializes a new <see cref="DelayingExtractor{T}"/> that waits the delay returned by
    /// <paramref name="delaySelector"/> (given the zero-based item index) before yielding each item.
    /// </summary>
    /// <param name="items">The items to extract.</param>
    /// <param name="delaySelector">Maps a zero-based item index to the delay to await before it.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items"/> or <paramref name="delaySelector"/> is <see langword="null"/>.
    /// </exception>
    public DelayingExtractor(IEnumerable<T> items, Func<int, TimeSpan> delaySelector)
    {
        _items         = items ?? throw new ArgumentNullException(nameof(items));
        _delaySelector = delaySelector ?? throw new ArgumentNullException(nameof(delaySelector));
    }



    private static Func<int, TimeSpan> ValidatedConstantDelay(TimeSpan delay)
    {
        if (delay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delay), delay, "Delay must not be negative.");
        }

        return _ => delay;
    }



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
        token.ThrowIfCancellationRequested();

        var index = 0;

        foreach (var item in _items)
        {
            token.ThrowIfCancellationRequested();

            if (CurrentSkippedItemCount < SkipItemCount)
            {
                IncrementCurrentSkippedItemCount();
                index++;
                continue;
            }

            if (CurrentItemCount >= MaximumItemCount)
            {
                yield break;
            }

            await Task.Delay(_delaySelector(index), token).ConfigureAwait(false);

            IncrementCurrentItemCount();
            index++;
            yield return item;
        }
    }
}
