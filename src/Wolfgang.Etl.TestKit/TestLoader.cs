using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit;

/// <summary>
/// A simple in-memory loader for use in tests, examples, and benchmarks.
/// Iterates all items from the source stream and, when constructed with
/// <c>collectItems: true</c>, makes them available via <see cref="GetCollectedItems"/>
/// after the load completes.
/// </summary>
/// <typeparam name="T">The type of item to load.</typeparam>
/// <remarks>
/// <para>
/// When <c>collectItems</c> is <see langword="false"/>, the loader still
/// enumerates every item so that benchmarks measure realistic throughput across
/// the full pipeline, but nothing is stored. <see cref="GetCollectedItems"/>
/// returns <see langword="null"/> in this mode.
/// </para>
/// <para>
/// When <c>collectItems</c> is <see langword="true"/>, items are accumulated in
/// an internal buffer throughout the load. <see cref="GetCollectedItems"/> returns
/// a snapshot of that buffer at the moment it is called — callers may call it
/// mid-load to inspect items received so far, or post-load for the full result.
/// Each new <c>LoadAsync</c> call clears the buffer before it begins.
/// </para>
/// <para>
/// Set <see cref="LoaderBase{TDestination,TProgress}.SkipItemCount"/> to skip the first
/// N items before loading. Set
/// <see cref="LoaderBase{TDestination,TProgress}.MaximumItemCount"/> to stop after
/// loading that many items.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Test scenario — collect and assert after load:
/// var loader = new TestLoader&lt;MyRecord&gt;(collectItems: true);
/// await loader.LoadAsync(extractor.ExtractAsync());
/// var results = loader.GetCollectedItems();
/// Assert.Equal(expected, results);
///
/// // Benchmark scenario — measure throughput without storing items:
/// var loader = new TestLoader&lt;MyRecord&gt;(collectItems: false);
/// await loader.LoadAsync(extractor.ExtractAsync());
/// // loader.GetCollectedItems() returns null in this mode
/// </code>
/// </example>
public class TestLoader<T> : LoaderBase<T, Report>, ISupportDryRun
    where T : notnull
{
    // ------------------------------------------------------------------
    // Fields
    // ------------------------------------------------------------------

    private readonly bool _collectItems;
    private readonly List<T> _buffer = new List<T>();
    private readonly IProgressTimer? _progressTimer;
    private bool _progressTimerWired;
    private Action? _elapsedHandler;



    // ------------------------------------------------------------------
    // Constructors
    // ------------------------------------------------------------------

    /// <summary>
    /// Initializes a new <see cref="TestLoader{T}"/>.
    /// </summary>
    /// <param name="collectItems">
    /// When <see langword="true"/>, items are accumulated in an internal buffer
    /// during each load operation and made available via <see cref="GetCollectedItems"/>.
    /// When <see langword="false"/>, items are enumerated but not stored —
    /// <see cref="GetCollectedItems"/> returns <see langword="null"/>.
    /// </param>
    public TestLoader(bool collectItems)
    {
        _collectItems = collectItems;
    }



    /// <summary>
    /// Initializes a new <see cref="TestLoader{T}"/> with the supplied
    /// <see cref="IProgressTimer"/> to drive progress callbacks.
    /// </summary>
    /// <param name="collectItems">
    /// When <see langword="true"/>, loaded items are accumulated and accessible
    /// via <see cref="GetCollectedItems"/>.
    /// </param>
    /// <param name="timer">
    /// The timer used to drive progress callbacks. Inject a
    /// <c>ManualProgressTimer</c> in tests to fire callbacks on demand.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="timer"/> is <see langword="null"/>.
    /// </exception>
    protected TestLoader(bool collectItems, IProgressTimer timer)
    {
        _collectItems  = collectItems;
        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));
    }



    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    /// <summary>
    /// Returns a snapshot of the items collected so far, or <see langword="null"/>
    /// if the loader was constructed with <c>collectItems: false</c>.
    /// </summary>
    /// <remarks>
    /// The snapshot is taken at the moment this method is called. It may be called
    /// mid-load to inspect items received so far, or post-load for the complete result.
    /// Each new <c>LoadAsync</c> call clears the internal buffer before enumeration
    /// begins, so a post-load call always reflects the most recent run only.
    /// </remarks>
    /// <returns>
    /// A <see cref="IReadOnlyList{T}"/> containing a point-in-time copy of the
    /// collected items, or <see langword="null"/> when collection is disabled.
    /// </returns>
    public IReadOnlyList<T>? GetCollectedItems() =>
        _collectItems
            ? _buffer.ToArray()
            : null;



    /// <summary>
    /// Gets or sets a value indicating whether the loader runs in dry-run mode.
    /// </summary>
    /// <value>
    /// When <see langword="true"/>, the loader still enumerates the full source and
    /// advances its progress counters, but skips its side effect — items are not added
    /// to the collection buffer, so <see cref="GetCollectedItems"/> reports nothing was
    /// loaded. Defaults to <see langword="false"/>.
    /// </value>
    public bool IsDryRun { get; set; }



    // ------------------------------------------------------------------
    // LoaderBase overrides
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

                if (_collectItems && !IsDryRun)
                {
                    _buffer.Add(item);
                }

                IncrementCurrentItemCount();
            }
        }
        finally
        {
            _progressTimer?.StopTimer();
        }
    }
}
