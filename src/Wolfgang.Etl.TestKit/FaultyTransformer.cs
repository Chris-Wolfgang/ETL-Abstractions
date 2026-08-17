using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit;

/// <summary>
/// A pass-through transformer for testing error-handling paths. It returns each item
/// unchanged like a normal transformer, but can be configured to inject deterministic
/// faults — throwing at a given item, throwing after completion, or duplicating an item —
/// so consumers can exercise mid-stream failure, finalization failure, and de-duplication
/// logic without hand-rolling broken fakes.
/// </summary>
/// <typeparam name="T">The type of item to transform.</typeparam>
/// <remarks>
/// <para>
/// Faults are configured through the fluent <see cref="ThrowAt"/>,
/// <see cref="ThrowAfterCompletion"/>, and <see cref="DuplicateAt"/> methods, each of which
/// returns the same instance so calls can be chained. Multiple faults stack on a single
/// instance.
/// </para>
/// <para>
/// Fault indices are zero-based and refer to the position in the emitted (post-skip)
/// sequence. A configured fault fires <em>after</em>
/// <see cref="TransformerBase{TSource,TDestination,TProgress}.IncrementCurrentItemCount"/>
/// for that item, so a progress report reflects the item that caused the failure. When a
/// <see cref="ThrowAt"/> and a <see cref="DuplicateAt"/> are configured for the same index,
/// the throw takes precedence and the duplicate is not emitted. Calling <see cref="ThrowAt"/>
/// twice for the same index replaces the earlier exception (last-wins).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var transformer = new FaultyTransformer&lt;int&gt;()
///     .ThrowAt(index: 50, new System.InvalidOperationException("bad record"))
///     .DuplicateAt(index: 10);
///
/// await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync()));
/// </code>
/// </example>
public class FaultyTransformer<T> : TransformerBase<T, T, Report>
    where T : notnull
{
    // ------------------------------------------------------------------
    // Fields
    // ------------------------------------------------------------------

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
    /// Initializes a new <see cref="FaultyTransformer{T}"/> using the default
    /// base-class progress timer.
    /// </summary>
    public FaultyTransformer() { }



    /// <summary>
    /// Initializes a new <see cref="FaultyTransformer{T}"/> with the supplied
    /// <see cref="IProgressTimer"/> to drive progress callbacks.
    /// </summary>
    /// <param name="timer">
    /// The timer used to drive progress callbacks. Inject a
    /// <c>ManualProgressTimer</c> in tests to fire callbacks on demand.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="timer"/> is <see langword="null"/>.
    /// </exception>
    protected FaultyTransformer(IProgressTimer timer)
    {
        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));
    }



    // ------------------------------------------------------------------
    // Fluent fault configuration
    // ------------------------------------------------------------------

    /// <summary>
    /// Configures the transformer to throw <paramref name="exception"/> when it reaches the
    /// item at the specified zero-based <paramref name="index"/> in the emitted sequence.
    /// The failing item is counted (its
    /// <see cref="TransformerBase{TSource,TDestination,TProgress}.IncrementCurrentItemCount"/>
    /// runs) before the exception is thrown, so progress reflects the item that caused the
    /// failure, but the item itself is not emitted.
    /// </summary>
    /// <param name="index">The zero-based index of the item to fail on.</param>
    /// <param name="exception">The exception to throw.</param>
    /// <returns>The same <see cref="FaultyTransformer{T}"/> instance, to allow chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="exception"/> is <see langword="null"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// var transformer = new FaultyTransformer&lt;int&gt;()
    ///     .ThrowAt(50, new System.InvalidOperationException("bad record"));
    /// </code>
    /// </example>
    public FaultyTransformer<T> ThrowAt(int index, Exception exception)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _throwAt[index] = exception ?? throw new ArgumentNullException(nameof(exception));

        return this;
    }



    /// <summary>
    /// Configures the transformer to throw <paramref name="exception"/> after all items have
    /// been emitted successfully, simulating a cleanup or finalization failure.
    /// </summary>
    /// <param name="exception">The exception to throw after completion.</param>
    /// <returns>The same <see cref="FaultyTransformer{T}"/> instance, to allow chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="exception"/> is <see langword="null"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// var transformer = new FaultyTransformer&lt;int&gt;()
    ///     .ThrowAfterCompletion(new System.InvalidOperationException("flush failed"));
    /// </code>
    /// </example>
    public FaultyTransformer<T> ThrowAfterCompletion(Exception exception)
    {
        _throwAfterCompletion = exception ?? throw new ArgumentNullException(nameof(exception));

        return this;
    }



    /// <summary>
    /// Configures the transformer to emit the item at the specified zero-based
    /// <paramref name="index"/> twice. The duplicate is a real second emission and is
    /// counted, so the total number of emitted items grows by one per configured duplicate.
    /// </summary>
    /// <param name="index">The zero-based index of the item to duplicate.</param>
    /// <returns>The same <see cref="FaultyTransformer{T}"/> instance, to allow chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// var transformer = new FaultyTransformer&lt;int&gt;()
    ///     .DuplicateAt(10);
    /// </code>
    /// </example>
    public FaultyTransformer<T> DuplicateAt(int index)
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
    /// Configures the transformer to route every injected <see cref="ThrowAt"/> fault through
    /// the base <see cref="TransformerBase{TSource,TDestination,TProgress}.HandleItemError"/>
    /// hook with a policy of <see cref="ItemErrorAction.Skip"/>, so the failing item is
    /// discarded and counted as an error
    /// (<see cref="TransformerBase{TSource,TDestination,TProgress}.CurrentErrorItemCount"/>)
    /// rather than propagating. Use this to exercise a resumable stage's skip-and-continue path.
    /// </summary>
    /// <returns>The same <see cref="FaultyTransformer{T}"/> instance, to allow chaining.</returns>
    /// <remarks>
    /// Without an error policy an injected fault propagates (fail-fast), preserving the
    /// default behaviour. The failing item is not emitted, and its
    /// <see cref="ItemErrorContext"/> is recorded in <see cref="CapturedErrors"/>.
    /// </remarks>
    public FaultyTransformer<T> SkipErrors()
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
    /// <returns>The same <see cref="FaultyTransformer{T}"/> instance, to allow chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="policy"/> is <see langword="null"/>.
    /// </exception>
    public FaultyTransformer<T> HandleErrorsWith(Func<ItemErrorContext, ItemErrorAction> policy)
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
    // TransformerBase overrides
    // ------------------------------------------------------------------

    /// <summary>
    /// Records the failed item and applies the configured error policy. Invoked by the base
    /// <see cref="TransformerBase{TSource,TDestination,TProgress}.HandleItemError"/> when a
    /// fault is routed through the hook; falls back to the base
    /// (<see cref="ItemErrorAction.Abort"/>) when no policy is configured.
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
        // Stryker disable once Logical : equivalent. `_elapsedHandler` is non-null only when
        // `_progressTimer` is non-null, so the extra states an `&&`->`||` flip would enter execute
        // only `_progressTimer.Elapsed -= null` (a harmless no-op) or differ solely on the
        // untestable finalizer (disposing == false) path — no observable behaviour changes.
        if (disposing && _progressTimer is not null && _elapsedHandler is not null)
        {
            _progressTimer.Elapsed -= _elapsedHandler;
            _elapsedHandler = null;
        }

        base.Dispose(disposing);
    }



    /// <inheritdoc/>
    protected override Report CreateProgressReport() =>
        new(CurrentItemCount, StartedAt, Elapsed);



    /// <inheritdoc/>
    protected override async IAsyncEnumerable<T> TransformWorkerAsync
    (
        IAsyncEnumerable<T> items,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        token.ThrowIfCancellationRequested();

        var index = 0;

        try
        {
            await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
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
        }

        if (_throwAfterCompletion is not null)
        {
            throw _throwAfterCompletion;
        }
    }
}
