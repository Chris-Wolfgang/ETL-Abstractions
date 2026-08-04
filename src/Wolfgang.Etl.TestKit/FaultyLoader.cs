using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit;

/// <summary>
/// An in-memory loader for testing error-handling paths. It consumes the source stream
/// like a normal loader, but can be configured to inject deterministic faults — throwing
/// at a given item, throwing after completion, or duplicating an item — so consumers can
/// exercise mid-stream failure, finalization failure, and idempotency logic without
/// hand-rolling broken fakes.
/// </summary>
/// <typeparam name="T">The type of item to load.</typeparam>
/// <remarks>
/// <para>
/// When constructed with <c>collectItems: true</c>, every loaded item — including any
/// duplicates — is accumulated and exposed via <see cref="GetCollectedItems"/>.
/// </para>
/// <para>
/// Faults are configured through the fluent <see cref="ThrowAt"/>,
/// <see cref="ThrowAfterCompletion"/>, and <see cref="DuplicateAt"/> methods, each of which
/// returns the same instance so calls can be chained. Multiple faults stack on a single
/// instance.
/// </para>
/// <para>
/// Fault indices are zero-based and refer to the position in the loaded (post-skip)
/// sequence. A configured fault fires <em>after</em>
/// <see cref="LoaderBase{TDestination,TProgress}.IncrementCurrentItemCount"/> for that item,
/// so a progress report reflects the item that caused the failure. When a
/// <see cref="ThrowAt"/> and a <see cref="DuplicateAt"/> are configured for the same index,
/// the throw takes precedence and the duplicate is not loaded. Calling <see cref="ThrowAt"/>
/// twice for the same index replaces the earlier exception (last-wins).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var loader = new FaultyLoader&lt;int&gt;(collectItems: true)
///     .ThrowAt(index: 25, new System.TimeoutException("connection lost"));
///
/// await loader.LoadAsync(extractor.ExtractAsync()); // throws reaching index 25
/// </code>
/// </example>
public class FaultyLoader<T> : LoaderBase<T, Report>
    where T : notnull
{
    // ------------------------------------------------------------------
    // Fields
    // ------------------------------------------------------------------

    private readonly bool _collectItems;
    private readonly List<T> _buffer = new List<T>();
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
    /// Initializes a new <see cref="FaultyLoader{T}"/>.
    /// </summary>
    /// <param name="collectItems">
    /// When <see langword="true"/>, loaded items (including duplicates) are accumulated in
    /// an internal buffer during each load operation and made available via
    /// <see cref="GetCollectedItems"/>. When <see langword="false"/>, items are consumed but
    /// not stored — <see cref="GetCollectedItems"/> returns <see langword="null"/>.
    /// </param>
    public FaultyLoader(bool collectItems)
    {
        _collectItems = collectItems;
    }



    /// <summary>
    /// Initializes a new <see cref="FaultyLoader{T}"/> with the supplied
    /// <see cref="IProgressTimer"/> to drive progress callbacks.
    /// </summary>
    /// <param name="collectItems">
    /// When <see langword="true"/>, loaded items are accumulated and accessible via
    /// <see cref="GetCollectedItems"/>.
    /// </param>
    /// <param name="timer">
    /// The timer used to drive progress callbacks. Inject a
    /// <c>ManualProgressTimer</c> in tests to fire callbacks on demand.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="timer"/> is <see langword="null"/>.
    /// </exception>
    protected FaultyLoader(bool collectItems, IProgressTimer timer)
    {
        _collectItems  = collectItems;
        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));
    }



    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    /// <summary>
    /// Returns a snapshot of the items loaded so far, or <see langword="null"/> if the
    /// loader was constructed with <c>collectItems: false</c>.
    /// </summary>
    /// <returns>
    /// A <see cref="IReadOnlyList{T}"/> containing a point-in-time copy of the loaded items
    /// (including any injected duplicates), or <see langword="null"/> when collection is
    /// disabled.
    /// </returns>
    public IReadOnlyList<T>? GetCollectedItems() =>
        _collectItems
            ? _buffer.ToArray()
            : null;



    // ------------------------------------------------------------------
    // Fluent fault configuration
    // ------------------------------------------------------------------

    /// <summary>
    /// Configures the loader to throw <paramref name="exception"/> when it reaches the item
    /// at the specified zero-based <paramref name="index"/> in the loaded sequence. The
    /// failing item is counted (its
    /// <see cref="LoaderBase{TDestination,TProgress}.IncrementCurrentItemCount"/> runs) before
    /// the exception is thrown, so progress reflects the item that caused the failure, but
    /// the item itself is not stored.
    /// </summary>
    /// <param name="index">The zero-based index of the item to fail on.</param>
    /// <param name="exception">The exception to throw.</param>
    /// <returns>The same <see cref="FaultyLoader{T}"/> instance, to allow chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="exception"/> is <see langword="null"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// var loader = new FaultyLoader&lt;int&gt;(collectItems: true)
    ///     .ThrowAt(25, new System.TimeoutException("connection lost"));
    /// </code>
    /// </example>
    public FaultyLoader<T> ThrowAt(int index, Exception exception)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _throwAt[index] = exception ?? throw new ArgumentNullException(nameof(exception));

        return this;
    }



    /// <summary>
    /// Configures the loader to throw <paramref name="exception"/> after all items have been
    /// loaded successfully, simulating a cleanup or finalization failure.
    /// </summary>
    /// <param name="exception">The exception to throw after completion.</param>
    /// <returns>The same <see cref="FaultyLoader{T}"/> instance, to allow chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="exception"/> is <see langword="null"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// var loader = new FaultyLoader&lt;int&gt;(collectItems: false)
    ///     .ThrowAfterCompletion(new System.InvalidOperationException("commit failed"));
    /// </code>
    /// </example>
    public FaultyLoader<T> ThrowAfterCompletion(Exception exception)
    {
        _throwAfterCompletion = exception ?? throw new ArgumentNullException(nameof(exception));

        return this;
    }



    /// <summary>
    /// Configures the loader to load the item at the specified zero-based
    /// <paramref name="index"/> twice. The duplicate is a real second load and is counted,
    /// so the total number of loaded items grows by one per configured duplicate.
    /// </summary>
    /// <param name="index">The zero-based index of the item to duplicate.</param>
    /// <returns>The same <see cref="FaultyLoader{T}"/> instance, to allow chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// var loader = new FaultyLoader&lt;int&gt;(collectItems: true)
    ///     .DuplicateAt(10);
    /// </code>
    /// </example>
    public FaultyLoader<T> DuplicateAt(int index)
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
    /// Configures the loader to route every injected <see cref="ThrowAt"/> fault through the
    /// base <see cref="LoaderBase{TDestination,TProgress}.HandleItemError"/> hook with a policy
    /// of <see cref="ItemErrorAction.Skip"/>, so the failing item is discarded and counted as
    /// an error (<see cref="LoaderBase{TDestination,TProgress}.CurrentErrorItemCount"/>) rather
    /// than propagating. Use this to exercise a resumable stage's skip-and-continue path.
    /// </summary>
    /// <returns>The same <see cref="FaultyLoader{T}"/> instance, to allow chaining.</returns>
    /// <remarks>
    /// Without an error policy an injected fault propagates (fail-fast), preserving the
    /// default behaviour. The failing item is neither stored nor counted as loaded, and its
    /// <see cref="ItemErrorContext"/> is recorded in <see cref="CapturedErrors"/>.
    /// </remarks>
    public FaultyLoader<T> SkipErrors()
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
    /// <returns>The same <see cref="FaultyLoader{T}"/> instance, to allow chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="policy"/> is <see langword="null"/>.
    /// </exception>
    public FaultyLoader<T> HandleErrorsWith(Func<ItemErrorContext, ItemErrorAction> policy)
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
    // LoaderBase overrides
    // ------------------------------------------------------------------

    /// <summary>
    /// Records the failed item and applies the configured error policy. Invoked by the base
    /// <see cref="LoaderBase{TDestination,TProgress}.HandleItemError"/> when a fault is routed
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
    // which counts it as an error (not a loaded item). Throws to abort the run, or,
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
        new(CurrentItemCount, StartedAt, Elapsed);



    /// <inheritdoc/>
    protected override async Task LoadWorkerAsync
    (
        IAsyncEnumerable<T> items,
        CancellationToken token
    )
    {
        token.ThrowIfCancellationRequested();

        _buffer.Clear();

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
                    break;
                }

                if (_throwAt.TryGetValue(index, out var exception)
                    && HandleInjectedFault(index, exception))
                {
                    index++;
                    continue;
                }

                IncrementCurrentItemCount();

                if (_collectItems)
                {
                    _buffer.Add(item);
                }

                if (_duplicateAt.Contains(index) && CurrentItemCount < MaximumItemCount)
                {
                    IncrementCurrentItemCount();

                    if (_collectItems)
                    {
                        _buffer.Add(item);
                    }
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
