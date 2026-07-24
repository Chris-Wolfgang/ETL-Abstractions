using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;



namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Provides a basic implementation for data extractors that extract data of type TSource.
/// Library authors can use this base class to create custom extractors by inheriting from it and implementing
/// ExtractWorkerAsync and CreateProgressReport methods.
/// </summary>
/// <typeparam name="TSource">The type of the object being extracted</typeparam>
/// <typeparam name="TProgress">The type of the progress object</typeparam>
public abstract class ExtractorBase<TSource, TProgress>
    : IExtractWithProgressAndCancellationAsync<TSource, TProgress>,
      IAsyncDisposable,
      IDisposable
    where TSource : notnull
    where TProgress : notnull
{
    private int _currentItemCount;
    private int _currentSkippedItemCount;
    private long _startTimestamp;
    private DateTimeOffset _startedAtUtc;
    private bool _disposed;



    /// <summary>
    /// The UTC time at which the first item was processed (extracted or skipped), or
    /// <c>null</c> if extraction has not produced any items yet. Captured automatically
    /// the first time <see cref="IncrementCurrentItemCount"/> or
    /// <see cref="IncrementCurrentSkippedItemCount"/> is called, so derived classes can
    /// surface it on their progress report (see <see cref="Report.StartedAt"/>).
    /// </summary>
    protected DateTimeOffset? StartedAt =>
        Volatile.Read(ref _startTimestamp) == 0 ? null : _startedAtUtc;



    /// <summary>
    /// The monotonic wall-clock time elapsed since the first item was processed, or
    /// <see cref="TimeSpan.Zero"/> if extraction has not produced any items yet.
    /// Read this when building a progress report (see <see cref="Report.Elapsed"/>) to
    /// snapshot how long extraction has been running.
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
    /// The current number of items extracted so far.
    /// </summary>
    /// <remarks>
    /// It is the responsibility of the derived class to call <see cref="IncrementCurrentItemCount"/>
    /// as each item is extracted. The base class has no way of knowing when an item has been processed.
    /// <para>
    /// This count is <b>per run</b>: it is reset to zero at the start of each run (when enumeration
    /// of an <c>ExtractAsync</c> result begins). Running the same instance more than once therefore
    /// reports the count for the current run, not a cumulative total across runs. Re-enumerating a
    /// single instance concurrently is not supported.
    /// </para>
    /// </remarks>
    public int CurrentItemCount => Volatile.Read(ref _currentItemCount);



    /// <summary>
    /// The current number of items skipped so far during extraction.
    /// </summary>
    public int CurrentSkippedItemCount => Volatile.Read(ref _currentSkippedItemCount);



    /// <summary>
    /// The maximum number of items to extract. Once the extractor has reached this limit,
    /// it should stop extracting and signal the end of the sequence.
    /// </summary>
    /// <remarks>
    /// This is useful for partially extracting data from a source, especially when the source is large
    /// or infinite or during development.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The specified value is less than 1.</exception>
    /// <example>
    /// <code>
    ///     var count = 0;
    ///     using (var reader = new StreamReader(filePath))
    ///     {
    ///         while (!reader.EndOfStream)
    ///         {
    ///             yield return await reader.ReadLineAsync();
    ///             count++;
    ///             if (count >= MaximumItemCount)
    ///             {
    ///                 Console.WriteLine("Maximum item count reached. Stopping extraction.");
    ///                 break; // Stop extracting if the maximum item count is reached
    ///             }
    ///         }
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
    /// The number of items to skip before extracting.
    /// The extractor should skip the specified number of items before starting to yield results.
    /// </summary>
    /// <remarks>
    /// This is useful for partially extracting data from a source during development, or to skip
    /// items that were already processed or are not relevant for the current extraction.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The specified value is less than 0.</exception>
    /// <example>
    /// <code>
    ///     using (var reader = new StreamReader(filePath))
    ///     {
    ///         // Skip the specified number of items before starting to yield results
    ///
    ///         var skipCount = 0;
    ///         while (!reader.EndOfStream &amp;&amp; skipCount &lt; SkipItemCount)
    ///         {
    ///             await reader.ReadLineAsync();
    ///             skipCount++;
    ///         }
    ///
    ///         // Now start yielding results
    ///
    ///         var count = 0;
    ///         while (!reader.EndOfStream)
    ///         {
    ///             yield return await reader.ReadLineAsync();
    ///             count++;
    ///         }
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
    public virtual IAsyncEnumerable<TSource> ExtractAsync()
    {
        ThrowIfDisposed();
        return ExtractWithResetAsync(CancellationToken.None);
    }



    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TSource> ExtractAsync(CancellationToken token)
    {
        ThrowIfDisposed();
        return ExtractWithResetAsync(token);
    }



    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TSource> ExtractAsync(IProgress<TProgress> progress)
    {
        ThrowIfDisposed();
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(progress);
#else
#pragma warning disable RCS1140 // Roslynator does not associate throw inside #else block with method XML doc
        if (progress == null)
        {
            throw new ArgumentNullException(nameof(progress));
        }
#pragma warning restore RCS1140
#endif

        return ExtractWithProgressAsync(progress, CancellationToken.None);
    }



    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TSource> ExtractAsync(IProgress<TProgress> progress, CancellationToken token)
    {
        ThrowIfDisposed();
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(progress);
#else
#pragma warning disable RCS1140 // Roslynator does not associate throw inside #else block with method XML doc
        if (progress == null)
        {
            throw new ArgumentNullException(nameof(progress));
        }
#pragma warning restore RCS1140
#endif

        return ExtractWithProgressAsync(progress, token);
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



    private async IAsyncEnumerable<TSource> ExtractWithResetAsync
    (
        [EnumeratorCancellation] CancellationToken token
    )
    {
        ResetRunState();

        await foreach (var item in ExtractWorkerAsync(token))
        {
            yield return item;
        }
    }



    private async IAsyncEnumerable<TSource> ExtractWithProgressAsync(
        IProgress<TProgress> progress,
        [EnumeratorCancellation] CancellationToken token)
    {
        ResetRunState();

        var timer = CreateProgressTimer(progress);

        try
        {
            await foreach (var item in ExtractWorkerAsync(token))
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
    // run (when enumeration begins), so running the same instance more than once reports counts
    // and timing for the current run rather than cumulatively across runs.
    private void ResetRunState()
    {
        Volatile.Write(ref _currentItemCount, 0);
        Volatile.Write(ref _currentSkippedItemCount, 0);
        Volatile.Write(ref _startTimestamp, 0L);
    }



    /// <summary>
    /// This method is the core implementation of the extraction logic and should be
    /// overridden by derived classes.
    /// </summary>
    /// <param name="token">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>
    /// IAsyncEnumerable&lt;TSource&gt;
    /// The result may be an empty sequence if no data is available or if the extraction fails.
    /// </returns>
    protected abstract IAsyncEnumerable<TSource> ExtractWorkerAsync(CancellationToken token);



    /// <summary>
    /// Creates a progress report of type TProgress. This gives the derived class the opportunity to
    /// implement a custom progress report that is specific to the extraction process.
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



    /// <summary>
    /// Decides what to do when an item fails to process. Override in a derived stage to record the
    /// failure and return <see cref="ItemErrorAction.Skip"/> to discard the item and continue, or
    /// <see cref="ItemErrorAction.Abort"/> to re-throw and stop the run. The base implementation
    /// always returns <see cref="ItemErrorAction.Abort"/>, so a stage that does not opt in keeps its
    /// fail-fast behaviour.
    /// </summary>
    /// <param name="context">
    /// Describes the failed item — its ordinal, the exception, and optional raw content.
    /// </param>
    /// <returns>Whether to skip the item or abort the run.</returns>
    /// <remarks>
    /// This is the policy hook a derived stage overrides; a worker does not call it directly. A worker
    /// calls <see cref="HandleItemError"/>, which invokes this method and performs the skip
    /// bookkeeping. The base classes deliberately expose no public error-handling property: a base
    /// class cannot catch a per-item failure on the worker's behalf — a C# async iterator cannot
    /// resume after it throws — so the worker owns the <c>try</c>/<c>catch</c>, and only a format that
    /// can genuinely resume after a bad record overrides this and surfaces its own public knob.
    /// </remarks>
    protected virtual ItemErrorAction OnItemError(ItemErrorContext context)
    {
        return ItemErrorAction.Abort;
    }



    /// <summary>
    /// Applies the stage's error policy to a failed item: invokes <see cref="OnItemError"/> and, when
    /// it returns <see cref="ItemErrorAction.Skip"/>, increments the skipped-item count so the skip is
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
            IncrementCurrentSkippedItemCount();
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

        var now = DateTimeOffset.UtcNow;
        var timestamp = Stopwatch.GetTimestamp();
        if (Interlocked.CompareExchange(ref _startTimestamp, timestamp, 0) == 0)
        {
            _startedAtUtc = now;
        }
    }



    /// <summary>
    /// Asynchronously releases the resources held by this extractor. The base implementation is a
    /// no-op (the base owns no unmanaged resources); derived classes that hold resources such as
    /// streams or connections override <see cref="Dispose(bool)"/> to release them. Enables
    /// <c>await using</c> on any extractor.
    /// </summary>
    /// <returns>A completed <see cref="ValueTask"/> for the default no-op implementation.</returns>
    public virtual ValueTask DisposeAsync()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
        return default;
    }



    /// <summary>
    /// Releases the resources held by this extractor. The base implementation is a no-op; derived
    /// classes that hold resources override <see cref="Dispose(bool)"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }



    /// <summary>
    /// Releases resources held by this extractor. Override in a derived class to dispose resources
    /// it owns (streams, connections, etc.), then call <c>base.Dispose(disposing)</c>. The base
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


    // Throws if this extractor has already been disposed. Reads _disposed, so the public entry
    // points reject use-after-dispose (and give the Dispose(bool) idempotency guard an observable
    // effect).
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
