using System;
using System.Diagnostics;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// The default <see cref="ITimeSource"/>: reads the real system clock via
/// <see cref="DateTimeOffset.UtcNow"/> and <see cref="Stopwatch"/>. A shared stateless singleton —
/// the base classes use it whenever no test time source has been injected.
/// </summary>
internal sealed class SystemTimeSource : ITimeSource
{
    internal static readonly SystemTimeSource Instance = new();


    private SystemTimeSource()
    {
    }


    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;


    public long GetTimestamp() => Stopwatch.GetTimestamp();


    public long TimestampFrequency => Stopwatch.Frequency;
}
