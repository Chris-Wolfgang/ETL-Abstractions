using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit;

/// <summary>
/// An in-memory extractor for testing error-handling paths. It yields items from an
/// <see cref="IEnumerable{T}"/> like a normal extractor, but can be configured to inject
/// deterministic faults — throwing at a given item, throwing after completion, or
/// duplicating an item — so consumers can exercise mid-stream failure, finalization
/// failure, and de-duplication logic without hand-rolling broken fakes.
/// </summary>
/// <typeparam name="T">The type of item to extract.</typeparam>
/// <remarks>
/// <para>
/// Faults are configured through the fluent <see cref="ThrowAt"/>,
/// <see cref="ThrowAfterCompletion"/>, and <see cref="DuplicateAt"/> methods, each of
/// which returns the same instance so calls can be chained. Multiple faults stack on a
/// single instance — for example <c>ThrowAt(50, ex)</c> and <c>DuplicateAt(10)</c> both
/// take effect in the same run.
/// </para>
/// <para>
/// Fault indices are zero-based and refer to the position in the emitted (post-skip)
/// sequence. A configured fault fires <em>after</em>
/// <see cref="ExtractorBase{TSource,TProgress}.IncrementCurrentItemCount"/> for that item,
/// so a progress report reflects the item that caused the failure. When a
/// <see cref="ThrowAt"/> and a <see cref="DuplicateAt"/> are configured for the same index,
/// the throw takes precedence and the duplicate is not emitted. Calling <see cref="ThrowAt"/>
/// twice for the same index replaces the earlier exception (last-wins).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var items     = new List&lt;int&gt; { 1, 2, 3, 4, 5 };
/// var extractor = new FaultyExtractor&lt;int&gt;(items)
///     .ThrowAt(index: 3, new System.IO.IOException("disk read failure"))
///     .DuplicateAt(index: 1);
///
/// // Enumerates 1, 2, 2 (duplicate), 3, then throws IOException reaching index 3.
/// await foreach (var item in extractor.ExtractAsync()) { /* ... */ }
/// </code>
/// </example>
public class FaultyExtractor<T> : ExtractorBase<T, Report>
    where T : notnull
{
    // ------------------------------------------------------------------
    // Fields
    // ------------------------------------------------------------------

    private readonly IEnumerable<T> _items;
    private readonly Dictionary<int, Exception> _throwAt = new Dictionary<int, Exception>();
    private readonly HashSet<int> _duplicateAt = new HashSet<int>();
    private Exception? _throwAfterCompletion;
    private Func<ItemErrorContext, ItemErrorAction>? _onItemErrorPolicy;
    private readonly List<ItemErrorContext> _capturedErrors = new List<ItemErrorContext>();
    private readonly IProgressTimer? _progressTimer;
    private bool _progressTimerWired;
    private Action? _elapsedHandler;



    // ------------------------------------------------------------------
    // Constructors
    // ------------------------------------------------------------------

    /// <summary>
    /// Initializes a new <see cref="FaultyExtractor{T}"/> that yields items from the
    /// specified <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="items">
    /// The sequence of items to extract. The enumerable is evaluated on each extraction
    /// run, so the same extractor instance can be reused.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items"/> is <see langword="null"/>.
    /// </exception>
    public FaultyExtractor(IEnumerable<T> items)
    {
        _items = items ?? throw new ArgumentNullException(nameof(items));
    }



    /// <summary>
    /// Initializes a new <see cref="FaultyExtractor{T}"/> that yields items from the
    /// specified <see cref="IEnumerable{T}"/> and uses the supplied
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
    protected FaultyExtractor(IEnumerable<T> items, IProgressTimer timer)
    {
        _items         = items ?? throw new ArgumentNullException(nameof(items));
        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));
    }



    // ------------------------------------------------------------------
    // Fluent fault configuration
    // ------------------------------------------------------------------

    /// <summary>
    /// Configures the extractor to throw <paramref name="exception"/> when it reaches the
    /// item at the specified zero-based <paramref name="index"/> in the emitted sequence.
    /// The failing item is counted (its
    /// <see cref="ExtractorBase{TSource,TProgress}.IncrementCurrentItemCount"/> runs) before
    /// the exception is thrown, so progress reflects the item that caused the failure, but
    /// the item itself is not yielded.
    /// </summary>
    /// <param name="index">The zero-based index of the item to fail on.</param>
    /// <param name="exception">The exception to throw.</param>
    /// <returns>The same <see cref="FaultyExtractor{T}"/> instance, to allow chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="exception"/> is <see langword="null"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// var extractor = new FaultyExtractor&lt;int&gt;(items)
    ///     .ThrowAt(50, new System.IO.IOException("disk read failure"));
    /// </code>
    /// </example>
    public FaultyExtractor<T> ThrowAt(int index, Exception exception)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _throwAt[index] = exception ?? throw new ArgumentNullException(nameof(exception));

        return this;
    }



    /// <summary>
    /// Configures the extractor to throw <paramref name="exception"/> after all items have
    /// been yielded successfully, simulating a cleanup or finalization failure.
    /// </summary>
    /// <param name="exception">The exception to throw after completion.</param>
    /// <returns>The same <see cref="FaultyExtractor{T}"/> instance, to allow chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="exception"/> is <see langword="null"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// var extractor = new FaultyExtractor&lt;int&gt;(items)
    ///     .ThrowAfterCompletion(new System.InvalidOperationException("finalize failed"));
    /// </code>
    /// </example>
    public FaultyExtractor<T> ThrowAfterCompletion(Exception exception)
    {
        _throwAfterCompletion = exception ?? throw new ArgumentNullException(nameof(exception));

        return this;
    }



    /// <summary>
    /// Configures the extractor to yield the item at the specified zero-based
    /// <paramref name="index"/> twice. The duplicate is a real second emission and is
    /// counted, so the total number of yielded items grows by one per configured duplicate.
    /// </summary>
    /// <param name="index">The zero-based index of the item to duplicate.</param>
    /// <returns>The same <see cref="FaultyExtractor{T}"/> instance, to allow chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// var extractor = new FaultyExtractor&lt;int&gt;(items)
    ///     .DuplicateAt(10);
    /// </code>
    /// </example>
    public FaultyExtractor<T> DuplicateAt(int index)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _duplicateAt.Add(index);

        return this;
    }



    // ------------------------------------------------------------------
    // Error-hook configuration (Abstractions 0.18.0)
    // ------------------------------------------------------------------

    /// <summary>
    /// Configures the extractor to route every injected <see cref="ThrowAt"/> fault through
    /// the base <see cref="ExtractorBase{TSource,TProgress}.HandleItemError"/> hook with a
    /// policy of <see cref="ItemErrorAction.Skip"/>, so the failing item is discarded and
    /// counted as an error (<see cref="ExtractorBase{TSource,TProgress}.CurrentErrorItemCount"/>)
    /// rather than propagating. Use this to exercise a resumable stage's skip-and-continue path.
    /// </summary>
    /// <returns>The same <see cref="FaultyExtractor{T}"/> instance, to allow chaining.</returns>
    /// <remarks>
    /// Without an error policy an injected fault propagates (fail-fast), preserving the
    /// default behaviour. The failing item is not yielded, and its
    /// <see cref="ItemErrorContext"/> is recorded in <see cref="CapturedErrors"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// var extractor = new FaultyExtractor&lt;int&gt;(items)
    ///     .ThrowAt(2, new System.FormatException("bad row"))
    ///     .SkipErrors();
    ///
    /// // Item at index 2 is skipped (counted as an error); enumeration continues. /* ... */
    /// </code>
    /// </example>
    public FaultyExtractor<T> SkipErrors()
    {
        _onItemErrorPolicy = _ => ItemErrorAction.Skip;

        return this;
    }



    /// <summary>
    /// Configures a custom per-item error policy. When an injected <see cref="ThrowAt"/> fault
    /// fires, <paramref name="policy"/> is invoked with the failing item's
    /// <see cref="ItemErrorContext"/> and decides whether to
    /// <see cref="ItemErrorAction.Skip"/> the item (counting it as an error and continuing) or
    /// <see cref="ItemErrorAction.Abort"/> the run (re-throwing).
    /// </summary>
    /// <param name="policy">The policy invoked for each failed item.</param>
    /// <returns>The same <see cref="FaultyExtractor{T}"/> instance, to allow chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="policy"/> is <see langword="null"/>.
    /// </exception>
    public FaultyExtractor<T> HandleErrorsWith(Func<ItemErrorContext, ItemErrorAction> policy)
    {
        _onItemErrorPolicy = policy ?? throw new ArgumentNullException(nameof(policy));

        return this;
    }



    /// <summary>
    /// The <see cref="ItemErrorContext"/> for every item whose injected fault was routed
    /// through the error hook, in the order they occurred. Empty when no error policy is
    /// configured (faults propagate) or when no fault has fired.
    /// </summary>
    public IReadOnlyList<ItemErrorContext> CapturedErrors => _capturedErrors.ToArray();



    // ------------------------------------------------------------------
    // ExtractorBase overrides
    // ------------------------------------------------------------------

    /// <summary>
    /// Records the failed item and applies the configured error policy. Invoked by the base
    /// <see cref="ExtractorBase{TSource,TProgress}.HandleItemError"/> when a fault is routed
    /// through the hook; falls back to the base (<see cref="ItemErrorAction.Abort"/>) when no
    /// policy is configured.
    /// </summary>
    /// <param name="context">Describes the failed item.</param>
    /// <returns>The action to take for the failed item.</returns>
    protected override ItemErrorAction OnItemError(ItemErrorContext context)
    {
        _capturedErrors.Add(context);

        return _onItemErrorPolicy is not null
            ? _onItemErrorPolicy(context)
            : base.OnItemError(context);
    }



    // Applies the configured error policy to an injected fault. Returns true when the
    // failing item should be skipped — routed through the base HandleItemError hook,
    // which counts it as an error (not a processed item). Throws to abort the run, or,
    // when no policy is configured, to preserve fail-fast behaviour (counting the
    // failing item first, matching the ThrowAt contract).
    private bool HandleInjectedFault(int index, Exception exception)
    {
        if (_onItemErrorPolicy is not null)
        {
            if (HandleItemError(new ItemErrorContext(index + 1, exception)) == ItemErrorAction.Skip)
            {
                return true;
            }

            throw exception;
        }

        IncrementCurrentItemCount();
        throw exception;
    }

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
        // PercentComplete / EstimatedRemaining can be computed. The timing constructor
        // (Abstractions 0.18.1) sets these via plain parameters, avoiding the init-setter
        // cross-assembly modreq mismatch that broke netstandard2.0 consumers on .NET 6/7.
        new(CurrentItemCount, StartedAt, Elapsed, (_items as ICollection<T>)?.Count);



    /// <inheritdoc/>
    protected override async IAsyncEnumerable<T> ExtractWorkerAsync
    (
        [EnumeratorCancellation] CancellationToken token
    )
    {
        token.ThrowIfCancellationRequested();

        // The wrapped source is synchronous; yield once up front to honour the
        // async-iterator contract on every exit path however the loop terminates.
        await Task.Yield();

        var enumerator = _items.GetEnumerator();

        try
        {
            var index = 0;

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

                var item = enumerator.Current;

                if (_throwAt.TryGetValue(index, out var exception)
                    && HandleInjectedFault(index, exception))
                {
                    index++;
                    continue;
                }

                IncrementCurrentItemCount();

                yield return item;

                if (_duplicateAt.Contains(index) && CurrentItemCount < MaximumItemCount)
                {
                    IncrementCurrentItemCount();
                    yield return item;
                }

                index++;
            }
        }
        finally
        {
            _progressTimer?.StopTimer();
            enumerator.Dispose();
        }

        if (_throwAfterCompletion is not null)
        {
            throw _throwAfterCompletion;
        }
    }
}
