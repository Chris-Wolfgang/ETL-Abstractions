using System;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Provides a point-in-time snapshot of the progress of an ETL process — how many
/// items have been processed, how long it has been running, and (when the total is
/// known) how far along it is and how much longer it is expected to take.
/// </summary>
/// <remarks>
/// <para>
/// This class can be used as a base class for other progress reports and expanded
/// with additional information specific to a particular extractor, transformer, or loader.
/// </para>
/// <para>
/// All values are <em>snapshot</em> values captured at the moment the report is
/// constructed. <see cref="Elapsed"/> does not advance after construction, mirroring
/// <see cref="CurrentItemCount"/>. The throughput and completion estimates
/// (<see cref="ItemsPerSecond"/>, <see cref="PercentComplete"/>,
/// <see cref="EstimatedRemaining"/>) are derived from those snapshot values, so a
/// given report is internally consistent.
/// </para>
/// </remarks>
public record Report
{
    /// <summary>
    /// Constructs a new instance of the <see cref="Report"/> class with the specified current item count.
    /// </summary>
    /// <param name="currentItemCount">The number of items processed so far. Must be greater than or equal to 0.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="currentItemCount"/> is less than 0.</exception>
    public Report(int currentItemCount)
    {
        if (currentItemCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentItemCount), "Current item count cannot be less than 0.");
        }

        CurrentItemCount = currentItemCount;
    }



    /// <summary>
    /// The number of items that have been processed so far in the ETL process.
    /// </summary>
    public int CurrentItemCount { get; }



    /// <summary>
    /// The wall-clock time (UTC) at which processing started, or <c>null</c> if it
    /// has not started yet (no items processed) or the producer does not track it.
    /// </summary>
    public DateTimeOffset? StartedAt { get; init; }



    /// <summary>
    /// The wall-clock time that had elapsed since processing started, captured at the
    /// moment this report was constructed. <see cref="TimeSpan.Zero"/> when timing is
    /// not tracked or processing has not started.
    /// </summary>
    public TimeSpan Elapsed { get; init; }



    /// <summary>
    /// The total number of items expected to be processed, when known (for example a
    /// file line count or a SQL <c>COUNT(*)</c>). <c>null</c> for unknown-size or
    /// infinite sources. Must be greater than or equal to 0 when set.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The specified value is less than 0.</exception>
    public int? TotalItemCount
    {
        get;
        init
        {
            if (value is < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Total item count cannot be less than 0.");
            }

            field = value;
        }
    }



    /// <summary>
    /// The processing throughput, in items per second, derived from
    /// <see cref="CurrentItemCount"/> and <see cref="Elapsed"/>. Returns 0 until at
    /// least some time has elapsed.
    /// </summary>
    public double ItemsPerSecond =>
        Elapsed.TotalSeconds > 0
            ? CurrentItemCount / Elapsed.TotalSeconds
            : 0d;



    /// <summary>
    /// The fraction of work completed, as a percentage in the range [0, 100], when
    /// <see cref="TotalItemCount"/> is known; otherwise <c>null</c>. Clamped to 100
    /// if <see cref="CurrentItemCount"/> exceeds <see cref="TotalItemCount"/>.
    /// </summary>
    public double? PercentComplete
    {
        get
        {
            if (TotalItemCount is not { } total)
            {
                return null;
            }

            return total == 0
                ? 100d
                : Math.Min(100d, 100d * CurrentItemCount / total);
        }
    }



    /// <summary>
    /// The estimated time remaining until completion, when both
    /// <see cref="TotalItemCount"/> is known and throughput can be measured;
    /// otherwise <c>null</c>. Returns <see cref="TimeSpan.Zero"/> once the current
    /// count has reached the total.
    /// </summary>
    public TimeSpan? EstimatedRemaining
    {
        get
        {
            if (TotalItemCount is not { } total)
            {
                return null;
            }

            var remaining = total - CurrentItemCount;
            if (remaining <= 0)
            {
                return TimeSpan.Zero;
            }

            var rate = ItemsPerSecond;
            if (rate <= 0)
            {
                return null;
            }

            // Guard against TimeSpan.FromSeconds overflowing for a pathologically low
            // rate (e.g. a single item after a very long elapsed time); clamp to TimeSpan.MaxValue.
            var seconds = remaining / rate;
            // Stryker disable once Equality: equivalent mutant — >= versus > differs only when
            // seconds exactly equals TimeSpan.MaxValue.TotalSeconds. That is a computed double from
            // (int remaining / positive rate); no clean test input lands on that exact boundary, and
            // both branches clamp identically for every reachable value.
            return seconds >= TimeSpan.MaxValue.TotalSeconds
                ? TimeSpan.MaxValue
                : TimeSpan.FromSeconds(seconds);
        }
    }
}
