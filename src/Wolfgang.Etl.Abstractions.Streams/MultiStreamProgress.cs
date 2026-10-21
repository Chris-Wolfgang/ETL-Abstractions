using System;

namespace Wolfgang.Etl.Abstractions.Streams;

/// <summary>
/// Progress for a multi-stream extractor or loader: the base <see cref="Report"/> counters and timing,
/// plus which stream is current. Concrete libraries derive from it to add their own per-record fields.
/// </summary>
/// <remarks>
/// All values are constructor parameters with get-only properties, so the type has the same shape on
/// every target framework (see the remarks on <see cref="Report"/> about <c>init</c> accessors).
/// </remarks>
public record MultiStreamProgress : Report
{
    /// <summary>
    /// Initializes a new <see cref="MultiStreamProgress"/>.
    /// </summary>
    /// <param name="currentItemCount">The number of items processed so far. Must be 0 or more.</param>
    /// <param name="startedAt">The UTC time processing started, or <see langword="null"/>.</param>
    /// <param name="elapsed">The time elapsed since processing started.</param>
    /// <param name="currentSourceNumber">The 1-based number of the current stream, or 0 before the first one.</param>
    /// <param name="totalSourceCount">The total number of streams, or -1 when it is not known up front.</param>
    /// <param name="currentSourceName">The current stream's name, or <see langword="null"/> when it has none.</param>
    /// <param name="currentFailedSourceCount">The number of streams that failed and were skipped so far.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="currentItemCount"/>, <paramref name="currentSourceNumber"/> or
    /// <paramref name="currentFailedSourceCount"/> is negative, or <paramref name="totalSourceCount"/> is
    /// less than -1.
    /// </exception>
    public MultiStreamProgress
    (
        int currentItemCount,
        DateTimeOffset? startedAt,
        TimeSpan elapsed,
        int currentSourceNumber,
        int totalSourceCount,
        string? currentSourceName,
        int currentFailedSourceCount
    )
        : base(currentItemCount, startedAt, elapsed)
    {
        if (currentSourceNumber < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentSourceNumber), currentSourceNumber, "The current source number cannot be negative.");
        }

        if (totalSourceCount < -1)
        {
            throw new ArgumentOutOfRangeException(nameof(totalSourceCount), totalSourceCount, "The total source count must be -1 (unknown) or more.");
        }

        if (currentFailedSourceCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentFailedSourceCount), currentFailedSourceCount, "The failed source count cannot be negative.");
        }

        CurrentSourceNumber = currentSourceNumber;
        TotalSourceCount = totalSourceCount;
        CurrentSourceName = currentSourceName;
        CurrentFailedSourceCount = currentFailedSourceCount;
    }



    /// <summary>
    /// The 1-based number of the stream being processed, or 0 before the first one is opened.
    /// </summary>
    public int CurrentSourceNumber { get; }



    /// <summary>
    /// The total number of streams, or -1 when the input could not be counted up front (for example a
    /// lazy <c>Directory.EnumerateFiles</c> projection). Consumers must tolerate -1.
    /// </summary>
    public int TotalSourceCount { get; }



    /// <summary>
    /// The current stream's name, or <see langword="null"/> when the caller supplied none.
    /// </summary>
    public string? CurrentSourceName { get; }



    /// <summary>
    /// The number of streams that failed and were skipped under <see cref="MultiStreamErrorAction.Continue"/>.
    /// </summary>
    public int CurrentFailedSourceCount { get; }
}
