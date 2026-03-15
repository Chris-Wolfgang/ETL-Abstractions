using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;



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
    : ITransformWithProgressAndCancellationAsync<TSource, TDestination, TProgress>
    where TSource : notnull
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
    /// The current number of items transformed so far.
    /// </summary>
    /// <remarks>
    /// It is the responsibility of the derived class to call <see cref="IncrementCurrentItemCount"/>
    /// as each item is transformed. The base class has no way of knowing when an item has been processed.
    /// </remarks>
    public int CurrentItemCount => _currentItemCount;



    /// <summary>
    /// The current number of items skipped so far during transformation.
    /// </summary>
    public int CurrentSkippedItemCount => _currentSkippedItemCount;



    /// <summary>
    /// The maximum number of items to transform. Once the transformer has reached this limit,
    /// it should stop transforming and signal the end of the sequence.
    /// </summary>
    /// <remarks>
    /// This is useful for transforming a subset of data, especially when the source is large
    /// or infinite or during development.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The specified value is less than 0.</exception>
    /// <example>
    /// <code>
    ///     foreach (var item in items.Skip(SkipItemCount).Take(MaxItemCount))
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
    ///     foreach (var item in items.Skip(SkipItemCount).Take(MaxItemCount))
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



    /// <summary>
    /// Asynchronously transforms data of type TSource to TDestination
    /// </summary>
    /// <param name="items">IAsyncEnumerable&lt;TSource&gt; - A list of 0 or more items to be transformed</param>
    /// <returns>
    /// IAsyncEnumerable&lt;TDestination&gt;
    /// The result may be an empty sequence if no data is available or if the transformation fails.
    /// </returns>
    /// <exception cref="ArgumentNullException">The value of items is null</exception>
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
        return TransformWorkerAsync(items, CancellationToken.None);
    }



    /// <summary>
    /// Asynchronously transforms data of type TSource to TDestination
    /// </summary>
    /// <param name="items">IAsyncEnumerable&lt;TSource&gt; - A list of 0 or more items to be transformed</param>
    /// <param name="token">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>
    /// IAsyncEnumerable&lt;TDestination&gt; - A list of 0 or more transformed items
    /// </returns>
    /// <remarks>
    /// The transformer should be able to handle cancellation requests gracefully.
    /// If the caller doesn't plan on cancelling the transformation, they can pass CancellationToken.None.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value of items is null</exception>
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
        return TransformWorkerAsync(items, token);
    }



    /// <summary>
    /// Asynchronously transforms data of type TSource to TDestination
    /// </summary>
    /// <param name="items">IAsyncEnumerable&lt;TSource&gt; - A list of 0 or more items to be transformed</param>
    /// <param name="progress">A provider for progress updates.</param>
    /// <returns>
    /// IAsyncEnumerable&lt;TDestination&gt; - The result may be an empty sequence if no data is available or if the transformation fails.
    /// </returns>
    /// <exception cref="ArgumentNullException">The value of items is null</exception>
    /// <exception cref="ArgumentNullException">The value of progress is null</exception>
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



    /// <summary>
    /// Asynchronously transforms data of type TSource to TDestination
    /// </summary>
    /// <param name="items">IAsyncEnumerable&lt;TSource&gt; - A list of 0 or more items to be transformed</param>
    /// <param name="progress">A provider for progress updates.</param>
    /// <param name="token">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>
    /// IAsyncEnumerable&lt;TDestination&gt; - The result may be an empty sequence if no data is available or if the transformation fails.
    /// </returns>
    /// <remarks>
    /// The transformer should be able to handle cancellation requests gracefully.
    /// If the caller doesn't plan on cancelling the transformation, they can pass CancellationToken.None.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value of items is null</exception>
    /// <exception cref="ArgumentNullException">The value of progress is null</exception>
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




    private async IAsyncEnumerable<TDestination> TransformWithProgressAsync(
        IAsyncEnumerable<TSource> items, IProgress<TProgress> progress,
        [EnumeratorCancellation] CancellationToken token)
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
            await foreach (var item in TransformWorkerAsync(items, token))
            {
                yield return item;
            }
        }
        finally
        {
            timer.Dispose();
            progress.Report(CreateProgressReport());
        }
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
