using System;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// The start of a stage run: the monotonic timestamp <c>Elapsed</c> is measured from and the
/// wall-clock time surfaced as <c>StartedAt</c>. Immutable, so the base classes publish both values
/// together with a single reference exchange and a reader can never observe one without the other.
/// </summary>
internal sealed class RunStart
{
    internal RunStart(long timestamp, DateTimeOffset utc)
    {
        Timestamp = timestamp;
        Utc = utc;
    }



    internal long Timestamp { get; }



    internal DateTimeOffset Utc { get; }
}
