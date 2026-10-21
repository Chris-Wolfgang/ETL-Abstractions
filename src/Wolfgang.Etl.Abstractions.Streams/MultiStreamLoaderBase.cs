using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Wolfgang.Etl.Abstractions.Streams;

/// <summary>
/// Base class for a loader that writes records to several streams, routing each record to a
/// destination chosen by a selector, for example one output file per customer. The base class owns
/// the orchestration: it calls the selector per record, opens each destination the first time it is
/// chosen (<see cref="OnDestinationOpenedAsync"/>, for a header), applies <c>SkipItemCount</c> and
/// <c>MaximumItemCount</c>, counts every record, closes every destination when the input ends
/// (<see cref="OnDestinationClosingAsync"/>, for a footer), and flushes and disposes every writer it
/// opened, even when the load fails. A concrete library supplies only <see cref="WriteRecordAsync"/>.
/// </summary>
/// <typeparam name="TRecord">The record type the loader consumes.</typeparam>
/// <typeparam name="TProgress">The progress-report type, typically <see cref="MultiStreamProgress"/> or a type derived from it.</typeparam>
/// <remarks>
/// Destinations are identified by reference: return the same <see cref="NamedStreamDestination"/>
/// instance for every record that belongs in the same stream. In progress,
/// <see cref="CurrentSourceNumber"/> is the number of destinations opened so far,
/// <see cref="CurrentSourceName"/> the destination of the last record, and the total is -1 because
/// destinations are only known as records arrive.
/// </remarks>
public abstract class MultiStreamLoaderBase<TRecord, TProgress> : LoaderBase<TRecord, TProgress>
    where TRecord : notnull
    where TProgress : notnull
{
    private readonly Func<TRecord, NamedStreamDestination> _destinationSelector;
    private int _openedDestinationCount;
    private string? _currentDestinationName;



    /// <summary>
    /// Initializes a new instance of the <see cref="MultiStreamLoaderBase{TRecord, TProgress}"/> class.
    /// </summary>
    /// <param name="destinationSelector">Chooses the destination for each record. Must not return <see langword="null"/>.</param>
    /// <param name="options">The base-stage configuration; <see langword="null"/> keeps the defaults.</param>
    /// <exception cref="ArgumentNullException"><paramref name="destinationSelector"/> is <see langword="null"/>.</exception>
    protected MultiStreamLoaderBase(Func<TRecord, NamedStreamDestination> destinationSelector, LoaderOptions? options = null)
        : base(options)
    {
        _destinationSelector = destinationSelector ?? throw new ArgumentNullException(nameof(destinationSelector));
    }



    /// <summary>
    /// The number of destinations opened so far in this run.
    /// </summary>
    protected int CurrentSourceNumber => Volatile.Read(ref _openedDestinationCount);



    /// <summary>
    /// The name of the destination the last record was written to, or <see langword="null"/>.
    /// </summary>
    protected string? CurrentSourceName => Volatile.Read(ref _currentDestinationName);



    /// <summary>
    /// Writes one record to its destination.
    /// </summary>
    /// <param name="record">The record to write.</param>
    /// <param name="destination">The destination the selector chose for it.</param>
    /// <param name="token">The run's cancellation token.</param>
    /// <returns>A task that completes when the record has been written.</returns>
    protected abstract Task WriteRecordAsync(TRecord record, NamedStreamDestination destination, CancellationToken token);



    /// <summary>
    /// Called once per destination, before its first record is written. Override to write a header.
    /// The base implementation does nothing.
    /// </summary>
    /// <param name="destination">The destination being opened.</param>
    /// <param name="token">The run's cancellation token.</param>
    /// <returns>A task that completes when the destination is ready for records.</returns>
    protected virtual Task OnDestinationOpenedAsync(NamedStreamDestination destination, CancellationToken token) =>
        Task.CompletedTask;



    /// <summary>
    /// Called once per opened destination, in the order they were opened, after the input ends
    /// successfully. Override to write a footer. Not called when the load fails. The base implementation
    /// does nothing.
    /// </summary>
    /// <param name="destination">The destination being closed.</param>
    /// <param name="token">The run's cancellation token.</param>
    /// <returns>A task that completes when the destination is complete.</returns>
    protected virtual Task OnDestinationClosingAsync(NamedStreamDestination destination, CancellationToken token) =>
        Task.CompletedTask;



    /// <summary>
    /// Builds a <see cref="MultiStreamProgress"/> snapshot of the counters, timing and current
    /// destination. A <see cref="LoaderBase{TDestination, TProgress}.CreateProgressReport"/> override
    /// returns it as is, or copies its values into a derived progress type.
    /// </summary>
    /// <returns>The current snapshot.</returns>
    protected MultiStreamProgress CreateMultiStreamProgress() =>
        new
        (
            CurrentItemCount,
            StartedAt,
            Elapsed,
            CurrentSourceNumber,
            totalSourceCount: -1,
            CurrentSourceName,
            currentFailedSourceCount: 0
        );



    /// <inheritdoc/>
    protected sealed override async Task LoadWorkerAsync(IAsyncEnumerable<TRecord> items, CancellationToken token)
    {
        Volatile.Write(ref _openedDestinationCount, 0);
        Volatile.Write(ref _currentDestinationName, null);

        var opened = new List<NamedStreamDestination>();
        var known = new HashSet<NamedStreamDestination>();
        try
        {
            await foreach (var record in items.WithCancellation(token).ConfigureAwait(false))
            {
                if (CurrentSkippedItemCount < SkipItemCount)
                {
                    IncrementCurrentSkippedItemCount();
                    continue;
                }

                if (CurrentItemCount >= MaximumItemCount)
                {
                    break;
                }

                var destination = _destinationSelector(record)
                    ?? throw new InvalidOperationException("The destination selector returned null.");

                if (known.Add(destination))
                {
                    opened.Add(destination);
                    _ = Interlocked.Increment(ref _openedDestinationCount);
                    await OnDestinationOpenedAsync(destination, token).ConfigureAwait(false);
                }

                Volatile.Write(ref _currentDestinationName, destination.Name);
                await WriteRecordAsync(record, destination, token).ConfigureAwait(false);
                IncrementCurrentItemCount();
            }

            foreach (var destination in opened)
            {
                await OnDestinationClosingAsync(destination, token).ConfigureAwait(false);
#if NET8_0_OR_GREATER
                await destination.Writer.FlushAsync(token).ConfigureAwait(false);
#else
                await destination.Writer.FlushAsync().ConfigureAwait(false);
#endif
            }
        }
        finally
        {
            // Reverse order of opening, like nested using blocks. After the success-path FlushAsync there
            // is nothing left to write; after a failure, disposal releases the stream regardless.
            for (var i = opened.Count - 1; i >= 0; i--)
            {
#if NETCOREAPP3_0_OR_GREATER
                await opened[i].Writer.DisposeAsync().ConfigureAwait(false);
#else
                opened[i].Writer.Dispose();
#endif
            }
        }
    }
}
