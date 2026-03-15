using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
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
    : ILoadWithProgressAndCancellationAsync<TDestination, TProgress>
    where TDestination : notnull
    where TProgress : notnull
{
    private int _currentItemCount;
    private int _currentSkippedItemCount;



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
    /// </remarks>
    public int CurrentItemCount => _currentItemCount;



    /// <summary>
    /// The current number of items skipped so far during loading.
    /// </summary>
    public int CurrentSkippedItemCount => _currentSkippedItemCount;



    /// <summary>
    /// The maximum number of items to load. Once the loader has reached this limit,
    /// it should stop loading items as if it had reached the end of the sequence
    /// </summary>
    /// <remarks>
    /// This is useful for partially loading data from a source, especially when the source is large
    /// or infinite or during development.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The specified value is less than 0.</exception>
    /// <example>
    /// <code>
    ///     foreach (var item in items.Skip(SkipItemCount).Take(MaxItemCount))
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
    /// This is useful for skipping the beginning of the list during testing or because it may already be loaded
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The specified value is less than 0.</exception>
    /// <example>
    /// <code>
    ///     foreach (var item in items.Skip(SkipItemCount).Take(MaxItemCount))
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



    /// <summary>
    /// Asynchronously loads data of type TDestination into the target destination.
    /// </summary>
    /// <param name="items">The items to be loaded to the destination.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// Items may be an empty sequence if no data is available or if the loading fails.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Argument items is null</exception>
    public virtual Task LoadAsync(IAsyncEnumerable<TDestination> items)
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
        return LoadWorkerAsync(items, CancellationToken.None);
    }



    /// <summary>
    /// Asynchronously loads data of type TDestination into the target destination.
    /// </summary>
    /// <param name="items">The items to be loaded to the destination.</param>
    /// <param name="token">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// Items may be an empty sequence if no data is available or if the loading fails.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Argument items is null</exception>
    public virtual Task LoadAsync(IAsyncEnumerable<TDestination> items, CancellationToken token)
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
        return LoadWorkerAsync(items, token);
    }



    /// <summary>
    /// Asynchronously loads data of type TDestination into the target destination.
    /// </summary>
    /// <param name="items">The items to be loaded to the destination.</param>
    /// <param name="progress">A provider for progress updates.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// Items may be an empty sequence if no data is available or if the loading fails.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Argument items is null</exception>
    /// <exception cref="ArgumentNullException">Argument progress is null</exception>
    public virtual Task LoadAsync(IAsyncEnumerable<TDestination> items, IProgress<TProgress> progress)
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

        return LoadWithProgressAsync(items, progress, CancellationToken.None);
    }



    /// <summary>
    /// Asynchronously loads data of type TDestination into the target destination.
    /// </summary>
    /// <param name="items">The items to be loaded to the destination.</param>
    /// <param name="progress">A provider for progress updates.</param>
    /// <param name="token">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// Items may be an empty sequence if no data is available or if the loading fails.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Argument items is null</exception>
    /// <exception cref="ArgumentNullException">Argument progress is null</exception>
    public virtual Task LoadAsync(IAsyncEnumerable<TDestination> items, IProgress<TProgress> progress, CancellationToken token)
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

        return LoadWithProgressAsync(items, progress, token);
    }



    private async Task LoadWithProgressAsync(
        IAsyncEnumerable<TDestination> items,
        IProgress<TProgress> progress,
        CancellationToken token)
    {
        // MA0042 suppressed: System.Threading.Timer does not implement IAsyncDisposable.
#pragma warning disable MA0042
        var timer = new Timer(
            ReportProgress,
            state: progress,
            TimeSpan.FromMilliseconds(ReportingInterval),
            TimeSpan.FromMilliseconds(ReportingInterval));
#pragma warning restore MA0042

        try
        {
            await LoadWorkerAsync(items, token).ConfigureAwait(false);
        }
        finally
        {
#pragma warning disable CA1849, VSTHRD103 // Timer.Dispose() is correct here; await is not valid in a finally block
            timer.Dispose();
#pragma warning restore CA1849, VSTHRD103
            progress.Report(CreateProgressReport());
        }
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
        _ = Interlocked.Increment(ref _currentSkippedItemCount);
    }
}
