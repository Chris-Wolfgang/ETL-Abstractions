using System;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Internal seam over the wall-clock and monotonic timer the base classes read when they compute
/// their <see cref="Report"/> timing metrics (<c>StartedAt</c>, <c>Elapsed</c>, and the throughput
/// values derived from them). The default is the system clock; a fake can be injected in tests — via
/// <c>InternalsVisibleTo</c> — so those metrics become deterministic. Deliberately internal: no
/// public API surface.
/// </summary>
internal interface ITimeSource
{
    /// <summary>The current UTC wall-clock time (used to capture <c>StartedAt</c>).</summary>
    DateTimeOffset UtcNow { get; }

    /// <summary>A monotonic timestamp tick count (used to measure <c>Elapsed</c>).</summary>
    long GetTimestamp();

    /// <summary>The number of <see cref="GetTimestamp"/> ticks per second.</summary>
    long TimestampFrequency { get; }
}
