using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;



namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Provides a basic implementation for data loaders that write data of type TDestination to a target destination.
/// Library authors can use this base class to create custom loaders by inheriting from it and implementing
/// LoadWorkerAsync and CreateProgressReport methods.
/// </summary>
/// <typeparam name="TDestination">The type of the destination object being written</typeparam>
/// <typeparam name="TProgress">The type of the progress object</typeparam>
public abstract class LoaderBase<TDestination, TProgress>
    : ILoadWithProgressAndCancellationAsync<TDestination, TProgress>,
      IReportsItemErrors,
      IAsyncDisposable,
      IDisposable
    where TDestination : notnull
    where TProgress : notnull
{
    private int _currentItemCount;
    private int _currentSkippedItemCount;
    private int _currentErrorItemCount;
    private long _startTimestamp;
    private DateTimeOffset _startedAtUtc;
    private bool _disposed;



    // Test seam (#338): when set, StartedAt/Elapsed derive from this time source instead of the
    // system clock, so the timing-derived Report metrics can be driven deterministically. Left null
    // in production (real clock). Internal + InternalsVisibleTo, mirroring the IProgressTimer
    // injection pattern, so Test-Kit doubles can advance a fake clock.
    internal ITimeSource? TimeSource;




    /// <summary>
    /// The UTC time at which the first item was processed (loaded or skipped), or
    /// <c>null</c> if loading has not produced any items yet. Captured automatically
    /// the first time <see cref="IncrementCurrentItemCount"/> or
    /// <see cref="IncrementCurrentSkippedItemCount"/> is called, so derived classes can
    /// surface it on their progress report (see <see cref="Report.StartedAt"/>).
    /// </summary>
    protected DateTimeOffset? StartedAt =>
        Volatile.Read(ref _startTimestamp) == 0 ? null : _startedAtUtc;



    /// <summary>
    /// The monotonic wall-clock time elapsed since the first item was processed, or
    /// <see cref="TimeSpan.Zero"/> if loading has not produced any items yet.
    /// Read this when building a progress report (see <see cref="Report.Elapsed"/>) to
    /// snapshot how long loading has been running.
    /// </summary>
    protected TimeSpan Elapsed
    {
        get
        {
            var start = Volatile.Read(ref _startTimestamp);
            if (start == 0)
            {
                return TimeSpan.Zero;
            }

            var source = TimeSource ?? SystemTimeSource.Instance;
            var ticks = source.GetTimestamp() - start;
            return TimeSpan.FromSeconds(ticks / (double)source.TimestampFrequency);
        }
    }



    /// <summary>
    /// The number of milliseconds between progress updates.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Value cannot be less than 1.</exception>
    public int ReportingInterval
    {
        get;
        set
        {
#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
#else
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Reporting interval must be greater than 0.");
            }
#endif
            field = value;
        }
    } = 1_000;



    /// <summary>
    /// The current number of items loaded so far.
    /// </summary>
    /// <remarks>
    /// It is the responsibility of the derived class to call <see cref="IncrementCurrentItemCount"/>
    /// as each item is loaded. The base class has no way of knowing when an item has been processed.
    /// <para>
    /// This count is <b>per run</b>: it is reset to zero at the start of each <c>LoadAsync</c> call.
    /// Running the same instance more than once therefore reports the count for the current run, not
    /// a cumulative total across runs. Running a single instance concurrently is not supported.
    /// </para>
    /// </remarks>
    public int CurrentItemCount => Volatile.Read(ref _currentItemCount);



    /// <summary>
    /// The current number of items skipped so far during loading.
    /// </summary>
    public int CurrentSkippedItemCount => Volatile.Read(ref _currentSkippedItemCount);



    /// <summary>
    /// The number of items that raised an error and were discarded by the stage's error policy
    /// (<see cref="OnItemError"/> returned <see cref="ItemErrorAction.Skip"/>) so far. This is distinct
    /// from <see cref="CurrentSkippedItemCount"/>, which counts items skipped intentionally (for
    /// example by <c>SkipItemCount</c>): a failed record is counted here, never silently dropped.
    /// </summary>
    public int CurrentErrorItemCount => Volatile.Read(ref _currentErrorItemCount);



    /// <summary>
    /// The maximum number of items to load. Once the loader has reached this limit,
    /// it should stop loading items as if it had reached the end of the sequence.
    /// </summary>
    /// <remarks>
    /// This is useful for partially loading data from a source, especially when the source is large
    /// or infinite or during development.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The specified value is less than 1.</exception>
    /// <example>
    /// <code>
    ///     foreach (var item in items.Skip(SkipItemCount).Take(MaximumItemCount))
    ///     {
    ///         // Process the item
    ///     }
    /// </code>
    /// </example>
    public int MaximumItemCount
    {
        get;
        set
        {
#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
#else
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Maximum item count cannot be less than 1.");
            }
#endif
            field = value;
        }
    } = int.MaxValue;



    /// <summary>
    /// The number of items skipped before loading.
    /// The loader should skip the specified number of items before starting to process the remaining items.
    /// </summary>
    /// <remarks>
    /// This is useful for skipping the beginning of the list during testing or because it may already be loaded.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The specified value is less than 0.</exception>
    /// <example>
    /// <code>
    ///     foreach (var item in items.Skip(SkipItemCount).Take(MaximumItemCount))
    ///     {
    ///         // Process the item
    ///     }
    /// </code>
    /// </example>
    public int SkipItemCount
    {
        get;
        set
        {
#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 0);
#else
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Skip item count cannot be less than 0.");
            }
#endif
            field = value;
        }
    }



    /// <inheritdoc/>
    public virtual Task LoadAsync(IAsyncEnumerable<TDestination> items)
    {
        ThrowIfDisposed();
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(items);
#else
#pragma warning disable RCS1140 // Roslynator does not associate throw inside #else block with method XML doc
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }
#pragma warning restore RCS1140
#endif
        return LoadWithResetAsync(items, CancellationToken.None);
    }



    /// <inheritdoc/>
    public virtual Task LoadAsync(IAsyncEnumerable<TDestination> items, CancellationToken token)
    {
        ThrowIfDisposed();
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(items);
#else
#pragma warning disable RCS1140 // Roslynator does not associate throw inside #else block with method XML doc
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }
#pragma warning restore RCS1140
#endif
        return LoadWithResetAsync(items, token);
    }



    /// <inheritdoc/>
    public virtual Task LoadAsync(IAsyncEnumerable<TDestination> items, IProgress<TProgress> progress)
    {
        ThrowIfDisposed();
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(progress);
#else
#pragma warning disable RCS1140 // Roslynator does not associate throw inside #else block with method XML doc
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }
        if (progress == null)
        {
            throw new ArgumentNullException(nameof(progress));
        }
#pragma warning restore RCS1140
#endif

        return LoadWithProgressAsync(items, progress, CancellationToken.None);
    }



    /// <inheritdoc/>
    public virtual Task LoadAsync(IAsyncEnumerable<TDestination> items, IProgress<TProgress> progress, CancellationToken token)
    {
        ThrowIfDisposed();
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(progress);
#else
#pragma warning disable RCS1140 // Roslynator does not associate throw inside #else block with method XML doc
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }
        if (progress == null)
        {
            throw new ArgumentNullException(nameof(progress));
        }
#pragma warning restore RCS1140
#endif

        return LoadWithProgressAsync(items, progress, token);
    }



    /// <summary>
    /// Creates the <see cref="IProgressTimer"/> used to drive progress callbacks.
    /// Override this method in a derived class to inject a custom timer
    /// (for example, a custom implementation that allows manual control in unit tests).
    /// </summary>
    /// <param name="progress">The progress sink that will receive callbacks.</param>
    /// <returns>A started <see cref="IProgressTimer"/> instance.</returns>
    protected virtual IProgressTimer CreateProgressTimer(IProgress<TProgress> progress)
    {
        var timer = TimerCoreFactory is null
            ? new SystemProgressTimer(ReportProgress, progress)
            : new SystemProgressTimer(ReportProgress, progress, TimerCoreFactory);
        timer.Start(ReportingInterval);
        return timer;
    }


    // Test seam: when set, CreateProgressTimer builds its SystemProgressTimer over this timer core
    // (a deterministic fake) instead of a real System.Threading.Timer.
    internal Func<System.Threading.TimerCallback, ITimerCore>? TimerCoreFactory;




    private Task LoadWithResetAsync
    (
        IAsyncEnumerable<TDestination> items,
        CancellationToken token
    )
    {
        ResetRunState();
        return WrapWorkerExecution(ct => LoadWorkerAsync(items, ct), token);
    }



    private async Task LoadWithProgressAsync(
        IAsyncEnumerable<TDestination> items,
        IProgress<TProgress> progress,
        CancellationToken token)
    {
        ResetRunState();

        var timer = CreateProgressTimer(progress);

        try
        {
            await WrapWorkerExecution(ct => LoadWorkerAsync(items, ct), token).ConfigureAwait(false);
        }
        finally
        {
#pragma warning disable CA1849, VSTHRD103 // Timer.Dispose() is correct here; await is not valid in a finally block
            timer.Dispose();
#pragma warning restore CA1849, VSTHRD103
            progress.Report(CreateProgressReport());
        }
    }



    // Resets the per-run counters and timing to their initial state. Fired at the start of every
    // run, so running the same instance more than once reports counts and timing for the current
    // run rather than cumulatively across runs.
    private void ResetRunState()
    {
        Volatile.Write(ref _currentItemCount, 0);
        Volatile.Write(ref _currentSkippedItemCount, 0);
        Volatile.Write(ref _currentErrorItemCount, 0);
        Volatile.Write(ref _startTimestamp, 0L);
    }



    /// <summary>
    /// This method is the core implementation of the loading logic and should be
    /// overridden by derived classes.
    /// </summary>
    /// <param name="items">The items to be loaded to the destination.</param>
    /// <param name="token">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// Items may be an empty sequence if no data is available or if the loading fails.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Argument items is null</exception>
    protected abstract Task LoadWorkerAsync(IAsyncEnumerable<TDestination> items, CancellationToken token);



    /// <summary>
    /// A resilience seam wrapped around every invocation of <see cref="LoadWorkerAsync"/>. The
    /// default implementation simply invokes <paramref name="workerFactory"/> once, so loading
    /// behaves exactly as if the seam were absent. Override it to run the worker through a retry /
    /// resilience strategy (for example a Polly <c>ResiliencePipeline</c>): the strategy can invoke
    /// <paramref name="workerFactory"/> more than once to retry a transient failure.
    /// </summary>
    /// <remarks>
    /// This is <b>stream-level</b> resilience: a retry re-runs the whole worker, which re-enumerates
    /// the source <c>items</c> from the start — so retry is only safe when that source can be
    /// enumerated more than once. The per-run counters (<see cref="CurrentItemCount"/>,
    /// <see cref="CurrentSkippedItemCount"/>, <see cref="CurrentErrorItemCount"/>) are reset once at
    /// the start of the run, <b>not</b> on each retry, so they accumulate across attempts unless the
    /// override resets them. Any delay the override introduces must observe <paramref name="token"/>.
    /// Kept dependency-free by design — a concrete Polly integration lives in a separate opt-in
    /// package rather than in this library.
    /// </remarks>
    /// <param name="workerFactory">A factory that runs the worker for the supplied token. Re-invocable — call it again to retry.</param>
    /// <param name="token">A <see cref="CancellationToken"/> to observe, including during any retry delay.</param>
    /// <exception cref="ArgumentNullException"><paramref name="workerFactory"/> is <see langword="null"/>.</exception>
    /// <returns>A task representing the (possibly resilience-wrapped) load operation.</returns>
    protected virtual Task WrapWorkerExecution
    (
        Func<CancellationToken, Task> workerFactory,
        CancellationToken token
    )
    {
        if (workerFactory is null)
        {
            throw new ArgumentNullException(nameof(workerFactory));
        }

        return workerFactory(token);
    }



    /// <summary>
    /// Creates a progress report of type TProgress. This gives the derived class the opportunity to
    /// implement a custom progress report that is specific to the loading process.
    /// </summary>
    /// <returns>Progress of type TProgress</returns>
    protected abstract TProgress CreateProgressReport();



    // Named Timer callback: receives the IProgress<TProgress> instance as state,
    // avoiding a lambda capture that would generate a compiler display class and
    // produce a phantom (object) constructor entry in code-coverage reports.
    // ExcludeFromCodeCoverage: TimerCallback requires object? state; the cast and
    // null-forgiving operator produce an untakeable null branch in coverage tools.
    [ExcludeFromCodeCoverage]
    private void ReportProgress(object? state)
    {
        ((IProgress<TProgress>)state!).Report(CreateProgressReport());
    }



    /// <summary>
    /// Increments the CurrentItemCount in a thread safe manner.
    /// </summary>
    /// <remarks>
    /// Simply calling CurrentItemCount++ or CurrentItemCount += 1 is not
    /// thread safe. This method ensures that CurrentItemCount is incremented safely.
    /// </remarks>
    [SuppressMessage("IDE0058", "IDE0058:Expression value is never used",
        Justification = "Interlocked.Increment return value intentionally discarded; only the side-effect matters.")]
    protected void IncrementCurrentItemCount()
    {
        EnsureStarted();
        _ = Interlocked.Increment(ref _currentItemCount);
    }



    /// <summary>
    /// Increments the CurrentSkippedItemCount in a thread safe manner.
    /// </summary>
    /// <remarks>
    /// Simply calling CurrentSkippedItemCount++ or CurrentSkippedItemCount += 1 is not
    /// thread safe. This method ensures that CurrentSkippedItemCount is incremented safely.
    /// </remarks>
    [SuppressMessage("IDE0058", "IDE0058:Expression value is never used",
        Justification = "Interlocked.Increment return value intentionally discarded; only the side-effect matters.")]
    protected void IncrementCurrentSkippedItemCount()
    {
        EnsureStarted();
        _ = Interlocked.Increment(ref _currentSkippedItemCount);
    }



    private static readonly Func<ItemErrorContext, ItemErrorAction> AbortPolicy =
        static _ => ItemErrorAction.Abort;

    private Func<ItemErrorContext, ItemErrorAction> _errorPolicy = AbortPolicy;



    /// <summary>
    /// Gets or sets the policy invoked when an item fails to process. Return <see cref="ItemErrorAction.Skip"/>
    /// to discard the item and continue, or <see cref="ItemErrorAction.Abort"/> to re-throw and stop
    /// the run. Defaults to fail-fast: every failed item aborts the run until a policy is assigned.
    /// Ready-made policies are provided by <c>Wolfgang.Etl.ErrorPolicies.ItemErrorPolicy</c>.
    /// </summary>
    /// <exception cref="ArgumentNullException">The assigned value is <see langword="null"/>.</exception>
    public Func<ItemErrorContext, ItemErrorAction> ErrorPolicy
    {
        get => _errorPolicy;
        set => _errorPolicy = value ?? throw new ArgumentNullException(nameof(value));
    }



    /// <summary>
    /// Decides what to do when an item fails to process. The base implementation delegates to
    /// <see cref="ErrorPolicy"/> (fail-fast by default). Override in a derived stage instead only when
    /// the decision needs stage-internal state; a worker does not call this directly.
    /// </summary>
    /// <param name="context">
    /// Describes the failed item — its ordinal, the exception, and optional raw content.
    /// </param>
    /// <returns>Whether to skip the item or abort the run.</returns>
    /// <remarks>
    /// A worker calls <see cref="HandleItemError"/>, which invokes this method and performs the skip
    /// bookkeeping. The worker still owns the <c>try</c>/<c>catch</c> — a C# async iterator cannot
    /// resume after it throws — and calls <see cref="HandleItemError"/> from it. On a format that
    /// cannot genuinely resume after a bad record, <see cref="ItemErrorAction.Skip"/> means "swallow
    /// the failure and stop at that point" rather than "skip and continue"; such a stage documents
    /// that on its own type.
    /// </remarks>
    protected virtual ItemErrorAction OnItemError(ItemErrorContext context)
    {
        return ErrorPolicy(context);
    }



    /// <summary>
    /// Applies the stage's error policy to a failed item: invokes <see cref="OnItemError"/> and, when
    /// it returns <see cref="ItemErrorAction.Skip"/>, increments the error-item count so the failure is
    /// never silent. Call this from a worker's <c>catch</c> block and re-throw when it returns
    /// <see cref="ItemErrorAction.Abort"/>.
    /// </summary>
    /// <param name="context">Describes the failed item.</param>
    /// <returns>
    /// <see cref="ItemErrorAction.Skip"/> to discard the item and continue, or
    /// <see cref="ItemErrorAction.Abort"/> to re-throw.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null"/>.</exception>
    protected ItemErrorAction HandleItemError(ItemErrorContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var action = OnItemError(context);
        if (action == ItemErrorAction.Skip)
        {
            EnsureStarted();
            _ = Interlocked.Increment(ref _currentErrorItemCount);
        }

        return action;
    }



    // Captures the start timestamp (monotonic) and wall-clock StartedAt the first
    // time any item is processed. Idempotent and thread-safe: the first caller to
    // win the CompareExchange records the start; later calls are a cheap volatile read.
    private void EnsureStarted()
    {
        if (Volatile.Read(ref _startTimestamp) != 0)
        // Stryker disable once all: equivalent — dropping this fast-path block only skips a cheap
        // early-out. The CompareExchange below is the real guard and assigns only on the winning
        // exchange, so a re-entrant caller changes nothing either way. (The CONDITION itself is not
        // disabled — flipping it returns before the first timestamp is recorded, which the
        // StartedAt/Elapsed tests catch.)
        {
            // Stryker disable once all: equivalent — same reasoning as the block above.
            return;
        }

        var source = TimeSource ?? SystemTimeSource.Instance;
        var now = source.UtcNow;
        var timestamp = source.GetTimestamp();
        if (Interlocked.CompareExchange(ref _startTimestamp, timestamp, 0) == 0)
        {
            _startedAtUtc = now;
        }
    }



    /// <summary>
    /// Asynchronously releases the resources held by this loader. The base implementation is a
    /// no-op (the base owns no unmanaged resources); derived classes that hold resources such as
    /// connections or streams override <see cref="Dispose(bool)"/> to release them. Enables
    /// <c>await using</c> on any loader.
    /// </summary>
    /// <returns>A completed <see cref="ValueTask"/> for the default no-op implementation.</returns>
    public virtual ValueTask DisposeAsync()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
        return default;
    }



    /// <summary>
    /// Releases the resources held by this loader. The base implementation is a no-op; derived
    /// classes that hold resources override <see cref="Dispose(bool)"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }



    /// <summary>
    /// Releases resources held by this loader. Override in a derived class to dispose resources
    /// it owns (connections, streams, etc.), then call <c>base.Dispose(disposing)</c>. The base
    /// implementation only marks the instance disposed and is idempotent.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> when called from <see cref="Dispose()"/> or <see cref="DisposeAsync"/>
    /// (dispose managed resources); <see langword="false"/> when called from a finalizer.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        // Stryker disable once all: equivalent — dropping this guard block only skips a redundant,
        // idempotent re-assignment of _disposed. The guard's negation and the assignment below are
        // real and killable (covered by the use-after-dispose tests).
        {
            // Stryker disable once all: equivalent — same reasoning; skipping the early return just
            // re-runs the idempotent `_disposed = true`.
            return;
        }

        _disposed = true;
    }


    // Throws if this loader has already been disposed. Reads _disposed, so the public entry points
    // reject use-after-dispose (and give the Dispose(bool) idempotency guard an observable effect).
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
