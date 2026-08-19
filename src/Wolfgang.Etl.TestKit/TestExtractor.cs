using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit;

/// <summary>
/// An in-memory extractor for use in tests, examples, and benchmarks.
/// Yields items from an <see cref="IEnumerable{T}"/>, an
/// <see cref="IEnumerator{T}"/>, or a factory delegate, depending on which
/// constructor is used.
/// </summary>
/// <typeparam name="T">The type of item to extract.</typeparam>
/// <remarks>
/// <para>
/// Use the <see cref="IEnumerable{T}"/> constructor when your data is already
/// materialized in a collection. The enumerable is re-evaluated on each call to
/// <c>ExtractAsync</c>, so the extractor can be reused across multiple runs.
/// </para>
/// <para>
/// Use the <see cref="IEnumerator{T}"/> constructor when you need to generate large
/// volumes of data on the fly (e.g. via a generator method with <c>yield return</c>)
/// without materializing all items in memory at once. The caller owns the enumerator's
/// lifetime — the extractor does not dispose it.
/// </para>
/// <para>
/// Set <see cref="ExtractorBase{TSource,TProgress}.SkipItemCount"/> to skip the first N
/// items before yielding. Set
/// <see cref="ExtractorBase{TSource,TProgress}.MaximumItemCount"/> to stop after yielding
/// that many items. Both default to their base-class values (0 and
/// <see cref="int.MaxValue"/> respectively).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // From a list:
/// var items = new List&lt;int&gt; { 1, 2, 3, 4, 5 };
/// var extractor = new TestExtractor&lt;int&gt;(items);
///
/// // From an on-the-fly generator — avoids allocating a million-item list:
/// static IEnumerator&lt;int&gt; Generate(int count)
/// {
///     for (var i = 0; i &lt; count; i++)
///         yield return i;
/// }
///
/// var extractor = new TestExtractor&lt;int&gt;(Generate(1_000_000));
/// </code>
/// </example>
public class TestExtractor<T> : ExtractorBase<T, Report>
    where T : notnull
{
    // ------------------------------------------------------------------
    // Fields
    // ------------------------------------------------------------------

    private readonly IEnumerable<T>? _enumerable;
    private readonly IEnumerator<T>? _enumerator;
    private readonly IProgressTimer? _progressTimer;
    private bool _progressTimerWired;
    private Action? _elapsedHandler;



    // ------------------------------------------------------------------
    // Constructors
    // ------------------------------------------------------------------

    /// <summary>
    /// Initializes a new <see cref="TestExtractor{T}"/> that yields items from
    /// the specified <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="items">
    /// The sequence of items to extract. The enumerable is evaluated lazily on
    /// each extraction run, so the same extractor instance can be reused.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items"/> is <see langword="null"/>.
    /// </exception>
    public TestExtractor(IEnumerable<T> items)
    {
        _enumerable = items ?? throw new ArgumentNullException(nameof(items));
    }



    /// <summary>
    /// Initializes a new <see cref="TestExtractor{T}"/> that yields items from
    /// the specified <see cref="IEnumerator{T}"/>.
    /// </summary>
    /// <param name="enumerator">
    /// The enumerator to draw items from. Useful for generator methods that produce
    /// large volumes of data on demand without allocating a full collection in memory.
    /// The caller is responsible for the enumerator's lifetime — the extractor does
    /// not dispose it.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="enumerator"/> is <see langword="null"/>.
    /// </exception>
    public TestExtractor(IEnumerator<T> enumerator)
    {
        _enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
    }



    /// <summary>
    /// Initializes a new <see cref="TestExtractor{T}"/> that yields items from
    /// the specified <see cref="IEnumerable{T}"/> and uses the supplied
    /// <see cref="IProgressTimer"/> to drive progress callbacks.
    /// </summary>
    /// <param name="items">The sequence of items to extract.</param>
    /// <param name="timer">
    /// The timer used to drive progress callbacks. Inject a
    /// <c>ManualProgressTimer</c> in tests to fire callbacks on demand.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items"/> or <paramref name="timer"/> is <see langword="null"/>.
    /// </exception>
    protected TestExtractor(IEnumerable<T> items, IProgressTimer timer)
    {
        _enumerable    = items ?? throw new ArgumentNullException(nameof(items));
        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));
    }



    /// <summary>
    /// Initializes a new <see cref="TestExtractor{T}"/> that yields items from
    /// the specified <see cref="IEnumerator{T}"/> and uses the supplied
    /// <see cref="IProgressTimer"/> to drive progress callbacks.
    /// </summary>
    /// <param name="enumerator">
    /// The enumerator to draw items from. The caller is responsible for the
    /// enumerator's lifetime — the extractor does not dispose it.
    /// </param>
    /// <param name="timer">
    /// The timer used to drive progress callbacks. Inject a
    /// <c>ManualProgressTimer</c> in tests to fire callbacks on demand.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="enumerator"/> or <paramref name="timer"/> is <see langword="null"/>.
    /// </exception>
    protected TestExtractor(IEnumerator<T> enumerator, IProgressTimer timer)
    {
        _enumerator    = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
        _progressTimer = timer      ?? throw new ArgumentNullException(nameof(timer));
    }



    /// <summary>
    /// Initializes a new <see cref="TestExtractor{T}"/> that yields items produced
    /// by repeatedly invoking the specified factory delegate.
    /// </summary>
    /// <param name="factory">
    /// A delegate invoked once per item to produce the next value to yield.
    /// </param>
    /// <remarks>
    /// This constructor produces an <b>unbounded</b> sequence — the factory is invoked
    /// indefinitely. The caller <b>must</b> bound the run by setting
    /// <see cref="ExtractorBase{TSource,TProgress}.MaximumItemCount"/> or by cancelling
    /// the extraction; otherwise extraction never completes.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="factory"/> is <see langword="null"/>.
    /// </exception>
    public TestExtractor(Func<T> factory)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        _enumerable = Generate(factory);
    }



    /// <summary>
    /// Initializes a new <see cref="TestExtractor{T}"/> that yields
    /// <paramref name="count"/> items, each produced by invoking the specified
    /// factory delegate.
    /// </summary>
    /// <param name="factory">
    /// A delegate invoked once per item to produce the next value to yield.
    /// </param>
    /// <param name="count">The number of items to produce. Must be non-negative.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="factory"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="count"/> is less than zero.
    /// </exception>
    public TestExtractor(Func<T> factory, int count)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        _enumerable = Generate(factory, count);
    }



    /// <summary>
    /// Initializes a new <see cref="TestExtractor{T}"/> that yields items produced
    /// by repeatedly invoking the specified index-aware factory delegate. The
    /// factory receives the zero-based index of the item being produced.
    /// </summary>
    /// <param name="factory">
    /// A delegate invoked once per item, receiving the zero-based item index, to
    /// produce the next value to yield.
    /// </param>
    /// <remarks>
    /// This constructor produces an <b>unbounded</b> sequence — the factory is invoked
    /// indefinitely. The caller <b>must</b> bound the run by setting
    /// <see cref="ExtractorBase{TSource,TProgress}.MaximumItemCount"/> or by cancelling
    /// the extraction; otherwise extraction never completes.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="factory"/> is <see langword="null"/>.
    /// </exception>
    public TestExtractor(Func<int, T> factory)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        _enumerable = GenerateIndexed(factory);
    }



    /// <summary>
    /// Initializes a new <see cref="TestExtractor{T}"/> that yields
    /// <paramref name="count"/> items, each produced by invoking the specified
    /// index-aware factory delegate. The factory receives the zero-based index of
    /// the item being produced.
    /// </summary>
    /// <param name="factory">
    /// A delegate invoked once per item, receiving the zero-based item index, to
    /// produce the next value to yield.
    /// </param>
    /// <param name="count">The number of items to produce. Must be non-negative.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="factory"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="count"/> is less than zero.
    /// </exception>
    public TestExtractor(Func<int, T> factory, int count)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        _enumerable = GenerateIndexed(factory, count);
    }



    /// <summary>
    /// Initializes a new <see cref="TestExtractor{T}"/> that yields items produced
    /// by repeatedly invoking the specified factory delegate and uses the supplied
    /// <see cref="IProgressTimer"/> to drive progress callbacks.
    /// </summary>
    /// <param name="factory">
    /// A delegate invoked once per item to produce the next value to yield.
    /// </param>
    /// <param name="timer">
    /// The timer used to drive progress callbacks. Inject a
    /// <c>ManualProgressTimer</c> in tests to fire callbacks on demand.
    /// </param>
    /// <remarks>
    /// This constructor produces an <b>unbounded</b> sequence — the factory is invoked
    /// indefinitely. The caller <b>must</b> bound the run by setting
    /// <see cref="ExtractorBase{TSource,TProgress}.MaximumItemCount"/> or by cancelling
    /// the extraction; otherwise extraction never completes.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="factory"/> or <paramref name="timer"/> is <see langword="null"/>.
    /// </exception>
    protected TestExtractor(Func<T> factory, IProgressTimer timer)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));
        _enumerable    = Generate(factory);
    }



    /// <summary>
    /// Initializes a new <see cref="TestExtractor{T}"/> that yields
    /// <paramref name="count"/> items, each produced by invoking the specified
    /// factory delegate, and uses the supplied <see cref="IProgressTimer"/> to
    /// drive progress callbacks.
    /// </summary>
    /// <param name="factory">
    /// A delegate invoked once per item to produce the next value to yield.
    /// </param>
    /// <param name="count">The number of items to produce. Must be non-negative.</param>
    /// <param name="timer">
    /// The timer used to drive progress callbacks. Inject a
    /// <c>ManualProgressTimer</c> in tests to fire callbacks on demand.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="factory"/> or <paramref name="timer"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="count"/> is less than zero.
    /// </exception>
    protected TestExtractor(Func<T> factory, int count, IProgressTimer timer)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));
        _enumerable    = Generate(factory, count);
    }



    /// <summary>
    /// Initializes a new <see cref="TestExtractor{T}"/> that yields items produced
    /// by repeatedly invoking the specified index-aware factory delegate and uses
    /// the supplied <see cref="IProgressTimer"/> to drive progress callbacks. The
    /// factory receives the zero-based index of the item being produced.
    /// </summary>
    /// <param name="factory">
    /// A delegate invoked once per item, receiving the zero-based item index, to
    /// produce the next value to yield.
    /// </param>
    /// <param name="timer">
    /// The timer used to drive progress callbacks. Inject a
    /// <c>ManualProgressTimer</c> in tests to fire callbacks on demand.
    /// </param>
    /// <remarks>
    /// This constructor produces an <b>unbounded</b> sequence — the factory is invoked
    /// indefinitely. The caller <b>must</b> bound the run by setting
    /// <see cref="ExtractorBase{TSource,TProgress}.MaximumItemCount"/> or by cancelling
    /// the extraction; otherwise extraction never completes.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="factory"/> or <paramref name="timer"/> is <see langword="null"/>.
    /// </exception>
    protected TestExtractor(Func<int, T> factory, IProgressTimer timer)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));
        _enumerable    = GenerateIndexed(factory);
    }



    /// <summary>
    /// Initializes a new <see cref="TestExtractor{T}"/> that yields
    /// <paramref name="count"/> items, each produced by invoking the specified
    /// index-aware factory delegate, and uses the supplied
    /// <see cref="IProgressTimer"/> to drive progress callbacks. The factory
    /// receives the zero-based index of the item being produced.
    /// </summary>
    /// <param name="factory">
    /// A delegate invoked once per item, receiving the zero-based item index, to
    /// produce the next value to yield.
    /// </param>
    /// <param name="count">The number of items to produce. Must be non-negative.</param>
    /// <param name="timer">
    /// The timer used to drive progress callbacks. Inject a
    /// <c>ManualProgressTimer</c> in tests to fire callbacks on demand.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="factory"/> or <paramref name="timer"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="count"/> is less than zero.
    /// </exception>
    protected TestExtractor(Func<int, T> factory, int count, IProgressTimer timer)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));
        _enumerable    = GenerateIndexed(factory, count);
    }



    // ------------------------------------------------------------------
    // Factory generators
    // ------------------------------------------------------------------

    private static IEnumerable<T> Generate(Func<T> factory)
    {
        while (true)
        {
            yield return factory();
        }
    }



    private static IEnumerable<T> Generate(Func<T> factory, int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return factory();
        }
    }



    private static IEnumerable<T> GenerateIndexed(Func<int, T> factory)
    {
        for (var i = 0; ; i++)
        {
            yield return factory(i);
        }
    }



    private static IEnumerable<T> GenerateIndexed(Func<int, T> factory, int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return factory(i);
        }
    }



    // ------------------------------------------------------------------
    // ExtractorBase overrides
    // ------------------------------------------------------------------

    /// <inheritdoc/>
    protected override IProgressTimer CreateProgressTimer(IProgress<Report> progress)
    {
        if (_progressTimer is null)
        {
            return base.CreateProgressTimer(progress);
        }

        if (!_progressTimerWired)
        {
            _progressTimerWired = true;
            _elapsedHandler = () => progress.Report(CreateProgressReport());
            _progressTimer.Elapsed += _elapsedHandler;
        }

        return _progressTimer;
    }



    /// <summary>
    /// Unsubscribes the <see cref="IProgressTimer.Elapsed"/> handler from an injected
    /// timer. The injected timer is owned by the caller and is therefore not disposed here.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> when called from <see cref="IDisposable.Dispose"/>;
    /// <see langword="false"/> when called from the finalizer.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && _progressTimer is not null && _elapsedHandler is not null)
        {
            _progressTimer.Elapsed -= _elapsedHandler;
            _elapsedHandler = null;
        }

        base.Dispose(disposing);
    }



    /// <inheritdoc/>
    protected override Report CreateProgressReport() =>
        // When the source is a materialized collection its size is a cheap, known total, so
        // PercentComplete / EstimatedRemaining can be computed. An enumerator- or factory-backed
        // source has no known total (stays null). The timing constructor (Abstractions 0.18.1)
        // sets these via plain parameters, avoiding the init-setter cross-assembly modreq
        // mismatch that broke netstandard2.0 consumers running on .NET 6/7.
        new(CurrentItemCount, StartedAt, Elapsed, (_enumerable as ICollection<T>)?.Count);



    /// <inheritdoc/>
    protected override async IAsyncEnumerable<T> ExtractWorkerAsync
    (
        [EnumeratorCancellation] CancellationToken token
    )
    {
        token.ThrowIfCancellationRequested();

        // The wrapped source is synchronous, so this iterator would otherwise
        // contain no await. Yield once up front to honour the async-iterator
        // contract on every exit path (including the MaximumItemCount
        // yield-break) and to give callers an asynchronous hop. Placing it here
        // rather than after the loop keeps it reachable regardless of how the
        // loop terminates.
        // Stryker disable once Statement : equivalent — Task.Yield is a scheduling hop only; removing
        // it changes no observable extraction result (the source is synchronous).
        await Task.Yield();

        var enumerator     = (_enumerator ?? _enumerable!.GetEnumerator());
        var ownsEnumerator = _enumerator == null;

        try
        {
            while (enumerator.MoveNext())
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
                yield return enumerator.Current;
            }
        }
        finally
        {
            _progressTimer?.StopTimer();
            if (ownsEnumerator)
            {
                enumerator.Dispose();
            }
        }
    }
}
