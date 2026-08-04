using System;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit;

/// <summary>
/// A controllable time source for tests: time stands still until <see cref="Advance"/> is called,
/// making a stage's <see cref="Report"/> timing metrics — <see cref="Report.Elapsed"/>,
/// <see cref="Report.ItemsPerSecond"/>, <see cref="Report.PercentComplete"/>, and
/// <see cref="Report.EstimatedRemaining"/> — deterministic rather than wall-clock-dependent.
/// </summary>
/// <remarks>
/// <para>
/// Attach it to an extractor / loader / transformer with
/// <see cref="TimeSourceExtensions.WithTimeSource{TSource, TProgress}(ExtractorBase{TSource, TProgress}, ManualTimeSource)"/>
/// (and its loader / transformer overloads) <em>before</em> the run starts, then call
/// <see cref="Advance"/> to move the clock forward by a known amount. The stage reads its start
/// timestamp when the run begins, so a subsequent <c>Advance(TimeSpan.FromSeconds(10))</c> makes
/// <see cref="Report.Elapsed"/> report exactly ten seconds.
/// </para>
/// <para>
/// This works because <c>Wolfgang.Etl.TestKit</c> is an internals-visible friend of
/// <c>Wolfgang.Etl.Abstractions</c>, so it can supply the internal time-source seam the base classes
/// read. The default epoch is a fixed constant so <see cref="Report.StartedAt"/> is reproducible too.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var clock = new ManualTimeSource();
/// var extractor = new TestExtractor&lt;int&gt;(Enumerable.Range(0, 50).ToArray()).WithTimeSource(clock);
/// await foreach (var _ in extractor.ExtractAsync()) { }   // start timestamp captured from the frozen clock
/// clock.Advance(TimeSpan.FromSeconds(10));
/// // A report built now has Elapsed == 10s and ItemsPerSecond == 5.
/// </code>
/// </example>
public sealed class ManualTimeSource : ITimeSource
{
    private static readonly DateTimeOffset DefaultEpoch = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

    // The base classes treat a captured start timestamp of 0 as the "run has not started" sentinel,
    // so the source must never report 0 — begin one second in and only ever move forward.
    private const long InitialTimestamp = TimeSpan.TicksPerSecond;

    private DateTimeOffset _utcNow;
    private long _timestamp;



    /// <summary>Initializes a new <see cref="ManualTimeSource"/> at a fixed default epoch.</summary>
    public ManualTimeSource()
        : this(DefaultEpoch)
    {
    }



    /// <summary>Initializes a new <see cref="ManualTimeSource"/> at <paramref name="start"/>.</summary>
    /// <param name="start">The initial wall-clock value reported by the source.</param>
    public ManualTimeSource(DateTimeOffset start)
    {
        _utcNow    = start;
        _timestamp = InitialTimestamp;
    }



    /// <summary>Moves the clock forward by <paramref name="delta"/>.</summary>
    /// <param name="delta">The non-negative amount of time to advance.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="delta"/> is negative.</exception>
    public void Advance(TimeSpan delta)
    {
        if (delta < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delta), delta, "Cannot advance time by a negative amount.");
        }

        _utcNow    += delta;
        _timestamp += delta.Ticks;
    }



    // ITimeSource is internal to Abstractions (visible here via InternalsVisibleTo). Implement it
    // explicitly so these members stay off ManualTimeSource's public surface — callers only need
    // the constructors and Advance. The timestamp frequency is one tick per 100 ns
    // (TimeSpan.TicksPerSecond), so Advance can add TimeSpan.Ticks directly.

    DateTimeOffset ITimeSource.UtcNow => _utcNow;



    long ITimeSource.GetTimestamp() => _timestamp;



    long ITimeSource.TimestampFrequency => TimeSpan.TicksPerSecond;
}
