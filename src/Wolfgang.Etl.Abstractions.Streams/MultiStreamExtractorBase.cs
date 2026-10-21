using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Wolfgang.Etl.Abstractions.Streams;

/// <summary>
/// Base class for an extractor that reads the same record format from several streams in sequence,
/// for example every CSV file in a folder. The base class owns the orchestration: it walks the
/// sources in order, disposes each reader once its records have been read, applies the
/// <see cref="SourceErrorAction"/> policy when a source fails, applies <c>SkipItemCount</c> and
/// <c>MaximumItemCount</c> across the combined stream, counts every record, and tracks which source is
/// current for progress. A concrete library supplies only <see cref="ExtractSourceAsync"/>: "read the
/// records of one stream", usually by delegating to its single-stream extractor.
/// </summary>
/// <typeparam name="TRecord">The record type every source produces.</typeparam>
/// <typeparam name="TProgress">The progress-report type, typically <see cref="MultiStreamProgress"/> or a type derived from it.</typeparam>
/// <remarks>
/// <para>
/// Records are counted by the base class, once per yielded record, so
/// <see cref="ExtractSourceAsync"/> must not call <c>IncrementCurrentItemCount</c> itself.
/// </para>
/// <para>
/// The extractor owns the readers. Each source's reader is disposed after that source has been read,
/// whether it succeeded or failed. Sources the run never reaches (because it was cancelled, stopped on
/// an error or hit <c>MaximumItemCount</c>) are not touched, so a lazily projected sequence never opens
/// them. Because readers are disposed, an instance reads its sources once; build a new instance, with
/// new readers, to read them again.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class CsvMultiStreamExtractor : MultiStreamExtractorBase&lt;CsvRow, MultiStreamProgress&gt;
/// {
///     public CsvMultiStreamExtractor(IEnumerable&lt;NamedStreamSource&gt; sources) : base(sources)
///     {
///     }
///
///     protected override IAsyncEnumerable&lt;CsvRow&gt; ExtractSourceAsync(StreamReader reader, CancellationToken token) =&gt;
///         new CsvExtractor(reader).ExtractAsync(token);
///
///     protected override MultiStreamProgress CreateProgressReport() =&gt; CreateMultiStreamProgress();
/// }
/// </code>
/// </example>
public abstract class MultiStreamExtractorBase<TRecord, TProgress> : ExtractorBase<TRecord, TProgress>
    where TRecord : notnull
    where TProgress : notnull
{
    private readonly IEnumerable<NamedStreamSource> _sources;
    private int _currentSourceNumber;
    private int _totalSourceCount = -1;
    private string? _currentSourceName;
    private int _currentFailedSourceCount;



    /// <summary>
    /// Initializes a new instance of the <see cref="MultiStreamExtractorBase{TRecord, TProgress}"/> class.
    /// </summary>
    /// <param name="sources">
    /// The sources to read, in order. A list or array gives <see cref="TotalSourceCount"/> up front; any
    /// other sequence is enumerated lazily and reports -1.
    /// </param>
    /// <param name="options">The base-stage configuration; <see langword="null"/> keeps the defaults.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sources"/> is <see langword="null"/>.</exception>
    protected MultiStreamExtractorBase(IEnumerable<NamedStreamSource> sources, ExtractorOptions? options = null)
        : base(options)
    {
        _sources = sources ?? throw new ArgumentNullException(nameof(sources));
    }



    /// <summary>
    /// What to do when reading a source throws. Defaults to <see cref="MultiStreamErrorAction.Stop"/>,
    /// which rethrows. <see cref="MultiStreamErrorAction.Continue"/> skips the rest of that source,
    /// counts it in <see cref="CurrentFailedSourceCount"/>, calls <see cref="SourceErrorHandler"/> and
    /// moves on to the next source. Cancellation is never treated as a source error.
    /// </summary>
    public MultiStreamErrorAction SourceErrorAction { get; init; } = MultiStreamErrorAction.Stop;



    /// <summary>
    /// Called with the details of each source that fails under
    /// <see cref="MultiStreamErrorAction.Continue"/>; <see langword="null"/> for none.
    /// </summary>
    public Action<StreamSourceError>? SourceErrorHandler { get; init; }



    /// <summary>
    /// The 1-based number of the source being read, or 0 before the first source is opened.
    /// </summary>
    protected int CurrentSourceNumber => Volatile.Read(ref _currentSourceNumber);



    /// <summary>
    /// The number of sources, when the input is a collection; otherwise -1.
    /// </summary>
    protected int TotalSourceCount => Volatile.Read(ref _totalSourceCount);



    /// <summary>
    /// The name of the source being read, or <see langword="null"/> when it has none.
    /// </summary>
    protected string? CurrentSourceName => Volatile.Read(ref _currentSourceName);



    /// <summary>
    /// The number of sources that failed and were skipped so far in this run.
    /// </summary>
    protected int CurrentFailedSourceCount => Volatile.Read(ref _currentFailedSourceCount);



    /// <summary>
    /// Reads the records of one source. The base class disposes <paramref name="reader"/> afterwards.
    /// </summary>
    /// <param name="reader">The reader for the source.</param>
    /// <param name="token">The run's cancellation token.</param>
    /// <returns>The source's records, in order.</returns>
    protected abstract IAsyncEnumerable<TRecord> ExtractSourceAsync(StreamReader reader, CancellationToken token);



    /// <summary>
    /// Builds a <see cref="MultiStreamProgress"/> snapshot of the counters, timing and current source.
    /// A <see cref="ExtractorBase{TSource, TProgress}.CreateProgressReport"/> override returns it as is, or
    /// copies its values into a derived progress type.
    /// </summary>
    /// <returns>The current snapshot.</returns>
    protected MultiStreamProgress CreateMultiStreamProgress() =>
        new
        (
            CurrentItemCount,
            StartedAt,
            Elapsed,
            CurrentSourceNumber,
            TotalSourceCount,
            CurrentSourceName,
            CurrentFailedSourceCount
        );



    /// <inheritdoc/>
    protected sealed override async IAsyncEnumerable<TRecord> ExtractWorkerAsync
    (
        [EnumeratorCancellation] CancellationToken token
    )
    {
        Volatile.Write(ref _currentSourceNumber, 0);
        Volatile.Write(ref _currentSourceName, null);
        Volatile.Write(ref _currentFailedSourceCount, 0);
        Volatile.Write(ref _totalSourceCount, CountOf(_sources));

        var index = -1;
        foreach (var source in _sources)
        {
            index++;
            token.ThrowIfCancellationRequested();

            if (CurrentItemCount >= MaximumItemCount)
            {
                yield break;
            }

            if (source is null)
            {
                throw new InvalidOperationException($"The source sequence contains a null {nameof(NamedStreamSource)} at index {index}.");
            }

            Volatile.Write(ref _currentSourceName, source.Name);
            Volatile.Write(ref _currentSourceNumber, index + 1);

            await foreach (var record in ReadSourceAsync(source, index, token).ConfigureAwait(false))
            {
                yield return record;
            }
        }
    }



    // Reads one source: applies the error policy, the skip budget and the maximum, counts every yielded
    // record, and disposes the reader whether the source succeeds or fails.
    private async IAsyncEnumerable<TRecord> ReadSourceAsync
    (
        NamedStreamSource source,
        int index,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        var reader = source.Reader;
        try
        {
            var records = ExtractSourceAsync(reader, token).GetAsyncEnumerator(token);
            try
            {
                while (CurrentItemCount < MaximumItemCount)
                {
                    TRecord record;
                    try
                    {
                        if (!await records.MoveNextAsync().ConfigureAwait(false))
                        {
                            break;
                        }

                        record = records.Current;
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException && SourceErrorAction == MultiStreamErrorAction.Continue)
                    {
                        _ = Interlocked.Increment(ref _currentFailedSourceCount);
                        SourceErrorHandler?.Invoke(new StreamSourceError(source.Name, index, ex));
                        break;
                    }

                    if (CurrentSkippedItemCount < SkipItemCount)
                    {
                        IncrementCurrentSkippedItemCount();
                        continue;
                    }

                    IncrementCurrentItemCount();
                    yield return record;
                }
            }
            finally
            {
                await records.DisposeAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            reader.Dispose();
        }
    }



    private static int CountOf(IEnumerable<NamedStreamSource> sources) =>
        sources switch
        {
            ICollection<NamedStreamSource> collection => collection.Count,
            IReadOnlyCollection<NamedStreamSource> readOnly => readOnly.Count,
            _ => -1,
        };
}
