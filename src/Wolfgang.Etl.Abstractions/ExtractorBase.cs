using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;



namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Provides a basic implementation for data extractors that extract data of type TSource.
/// Library authors can use this base class to create custom extractors by inheriting from it and implementing
/// ExtractWorkerAsync and CreateProgressReport methods.
/// </summary>
/// <typeparam name="TSource">The type of the object being extracted</typeparam>
/// <typeparam name="TProgress">The type of the progress object</typeparam>
public abstract class ExtractorBase<TSource, TProgress>
    : IExtractWithProgressAndCancellationAsync<TSource, TProgress>
    where TSource : notnull
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
    /// The current number of items extracted so far.
    /// </summary>
    /// <remarks>
    /// It is the responsibility of the derived class to call <see cref="IncrementCurrentItemCount"/>
    /// as each item is extracted. The base class has no way of knowing when an item has been processed.
    /// </remarks>
    public int CurrentItemCount => _currentItemCount;



    /// <summary>
    /// The current number of items skipped so far during extraction.
    /// </summary>
    public int CurrentSkippedItemCount => _currentSkippedItemCount;



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



    /// <summary>
    /// Asynchronously extracts data of type TSource from a source.
    /// </summary>
    /// <returns>
    /// IAsyncEnumerable&lt;TSource&gt;
    /// The result may be an empty sequence if no data is available or if the extraction fails.
    /// </returns>
    public virtual IAsyncEnumerable<TSource> ExtractAsync()
    {
        return ExtractWorkerAsync(CancellationToken.None);
    }



    /// <summary>
    /// Asynchronously extracts data of type TSource from a source.
    /// </summary>
    /// <param name="token">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>
    /// IAsyncEnumerable&lt;TSource&gt;
    /// The result may be an empty sequence if no data is available or if the extraction fails.
    /// </returns>
    /// <remarks>
    /// The extractor should be able to handle cancellation requests gracefully.
    /// If the caller doesn't plan on cancelling the extraction, they can pass CancellationToken.None.
    /// </remarks>
    public virtual IAsyncEnumerable<TSource> ExtractAsync(CancellationToken token)
    {
        return ExtractWorkerAsync(token);
    }



    /// <summary>
    /// Asynchronously extracts data of type TSource from a source.
    /// </summary>
    /// <param name="progress">A provider for progress updates.</param>
    /// <returns>
    /// IAsyncEnumerable&lt;TSource&gt;
    /// The result may be an empty sequence if no data is available or if the extraction fails.
    /// </returns>
    /// <exception cref="ArgumentNullException">The value of progress is null</exception>
    public virtual IAsyncEnumerable<TSource> ExtractAsync(IProgress<TProgress> progress)
    {
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



    /// <summary>
    /// Asynchronously extracts data of type TSource from a source.
    /// </summary>
    /// <param name="progress">A provider for progress updates.</param>
    /// <param name="token">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>
    /// IAsyncEnumerable&lt;TSource&gt;
    /// The result may be an empty sequence if no data is available or if the extraction fails.
    /// </returns>
    /// <remarks>
    /// The extractor should be able to handle cancellation requests gracefully.
    /// If the caller doesn't plan on cancelling the extraction, they can pass CancellationToken.None.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value of progress is null</exception>
    public virtual IAsyncEnumerable<TSource> ExtractAsync(IProgress<TProgress> progress, CancellationToken token)
    {
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



    private async IAsyncEnumerable<TSource> ExtractWithProgressAsync(
        IProgress<TProgress> progress,
        [EnumeratorCancellation] CancellationToken token)
    {
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
