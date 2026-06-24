using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;



namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Provides a basic implementation for data transformers that convert data from TSource to TDestination.
/// Library authors can use this base class to create custom transformers by inheriting from it and implementing
/// TransformWorkerAsync and CreateProgressReport methods.
/// </summary>
/// <typeparam name="TSource">The type of the source object</typeparam>
/// <typeparam name="TDestination">The type of the destination object</typeparam>
/// <typeparam name="TProgress">The type of the progress object</typeparam>
public abstract class TransformerBase<TSource, TDestination, TProgress>
    : ITransformWithProgressAndCancellationAsync<TSource, TDestination, TProgress>,
      IAsyncDisposable,
      IDisposable
    where TSource : notnull
    where TDestination : notnull
    where TProgress : notnull
{
    private int _currentItemCount;
    private int _currentSkippedItemCount;
    private long _startTimestamp;
    private DateTimeOffset _startedAtUtc;
    private bool _disposed;



    /// <summary>
    /// The UTC time at which the first item was processed (transformed or skipped), or
    /// <c>null</c> if transformation has not produced any items yet. Captured automatically
    /// the first time <see cref="IncrementCurrentItemCount"/> or
    /// <see cref="IncrementCurrentSkippedItemCount"/> is called, so derived classes can
    /// surface it on their progress report (see <see cref="Report.StartedAt"/>).
    /// </summary>
    protected DateTimeOffset? StartedAt =>
        Volatile.Read(ref _startTimestamp) == 0 ? null : _startedAtUtc;



    /// <summary>
    /// The monotonic wall-clock time elapsed since the first item was processed, or
    /// <see cref="TimeSpan.Zero"/> if transformation has not produced any items yet.
    /// Read this when building a progress report (see <see cref="Report.Elapsed"/>) to
    /// snapshot how long transformation has been running.
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

            var ticks = Stopwatch.GetTimestamp() - start;
            return TimeSpan.FromSeconds(ticks / (double)Stopwatch.Frequency);
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
    /// The current number of items transformed so far.
    /// </summary>
    /// <remarks>
    /// It is the responsibility of the derived class to call <see cref="IncrementCurrentItemCount"/>
    /// as each item is transformed. The base class has no way of knowing when an item has been processed.
    /// <para>
    /// This count is <b>per run</b>: it is reset to zero at the start of each run (when enumeration
    /// of a <c>TransformAsync</c> result begins). Running the same instance more than once therefore
    /// reports the count for the current run, not a cumulative total across runs. Re-enumerating a
    /// single instance concurrently is not supported.
    /// </para>
    /// </remarks>
    public int CurrentItemCount => Volatile.Read(ref _currentItemCount);



    /// <summary>
    /// The current number of items skipped so far during transformation.
    /// </summary>
    public int CurrentSkippedItemCount => Volatile.Read(ref _currentSkippedItemCount);



    /// <summary>
    /// The maximum number of items to transform. Once the transformer has reached this limit,
    /// it should stop transforming and signal the end of the sequence.
    /// </summary>
    /// <remarks>
    /// This is useful for transforming a subset of data, especially when the source is large
    /// or infinite or during development.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The specified value is less than 1.</exception>
    /// <example>
    /// <code>
    ///     foreach (var item in items.Skip(SkipItemCount).Take(MaximumItemCount))
    ///     {
    ///         // Transform each item and return it
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
    /// The number of items to skip before transforming.
    /// The transformer should skip the specified number of items before starting to yield results.
    /// </summary>
    /// <remarks>
    /// This is useful for transforming a subset of data, especially when the source is large
    /// or infinite or during development.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The specified value is less than 0.</exception>
    /// <example>
    /// <code>
    ///     foreach (var item in items.Skip(SkipItemCount).Take(MaximumItemCount))
    ///     {
    ///         // Transform each item and return it
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
    public virtual IAsyncEnumerable<TDestination> TransformAsync(IAsyncEnumerable<TSource> items)
    {
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
        return TransformWithResetAsync(items, CancellationToken.None);
    }



    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TDestination> TransformAsync(IAsyncEnumerable<TSource> items, CancellationToken token)
    {
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
        return TransformWithResetAsync(items, token);
    }



    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TDestination> TransformAsync(IAsyncEnumerable<TSource> items, IProgress<TProgress> progress)
    {
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

        return TransformWithProgressAsync(items, progress, CancellationToken.None);
    }



    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TDestination> TransformAsync(IAsyncEnumerable<TSource> items, IProgress<TProgress> progress, CancellationToken token)
    {
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

        return TransformWithProgressAsync(items, progress, token);
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
        var timer = new SystemProgressTimer(ReportProgress, progress);
        timer.Start(ReportingInterval);
        return timer;
    }



    private async IAsyncEnumerable<TDestination> TransformWithResetAsync
    (
        IAsyncEnumerable<TSource> items,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        ResetRunState();

        await foreach (var item in TransformWorkerAsync(items, token))
        {
            yield return item;
        }
    }



    private async IAsyncEnumerable<TDestination> TransformWithProgressAsync(
        IAsyncEnumerable<TSource> items, IProgress<TProgress> progress,
        [EnumeratorCancellation] CancellationToken token)
    {
        ResetRunState();

        var timer = CreateProgressTimer(progress);

        try
        {
            await foreach (var item in TransformWorkerAsync(items, token))
            {
                yield return item;
            }
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
        Volatile.Write(ref _startTimestamp, 0L);
    }



    /// <summary>
    /// The worker method that performs the actual transformation.
    /// </summary>
    /// <param name="items">IAsyncEnumerable&lt;TSource&gt; - A list of 0 or more items to be transformed</param>
    /// <param name="token">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>
    /// IAsyncEnumerable&lt;TDestination&gt; - The result may be an empty sequence if no data is available or if the transformation fails.
    /// </returns>
    protected abstract IAsyncEnumerable<TDestination> TransformWorkerAsync(IAsyncEnumerable<TSource> items, CancellationToken token);



    /// <summary>
    /// Creates a progress report object of type TProgress.
    /// </summary>
    /// <returns>
    /// TProgress - A new instance of the progress report object.
    /// </returns>
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



    // Captures the start timestamp (monotonic) and wall-clock StartedAt the first
    // time any item is processed. Idempotent and thread-safe: the first caller to
    // win the CompareExchange records the start; later calls are a cheap volatile read.
    private void EnsureStarted()
    {
        if (Volatile.Read(ref _startTimestamp) != 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var timestamp = Stopwatch.GetTimestamp();
        if (Interlocked.CompareExchange(ref _startTimestamp, timestamp, 0) == 0)
        {
            _startedAtUtc = now;
        }
    }



    /// <summary>
    /// Asynchronously releases the resources held by this transformer. The base implementation is a
    /// no-op (the base owns no unmanaged resources); derived classes that hold resources override
    /// <see cref="Dispose(bool)"/> to release them. Enables <c>await using</c> on any transformer.
    /// </summary>
    /// <returns>A completed <see cref="ValueTask"/> for the default no-op implementation.</returns>
    public virtual ValueTask DisposeAsync()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
        return default;
    }



    /// <summary>
    /// Releases the resources held by this transformer. The base implementation is a no-op; derived
    /// classes that hold resources override <see cref="Dispose(bool)"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }



    /// <summary>
    /// Releases resources held by this transformer. Override in a derived class to dispose resources
    /// it owns, then call <c>base.Dispose(disposing)</c>. The base implementation only marks the
    /// instance disposed and is idempotent.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> when called from <see cref="Dispose()"/> or <see cref="DisposeAsync"/>
    /// (dispose managed resources); <see langword="false"/> when called from a finalizer.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
    }
}
