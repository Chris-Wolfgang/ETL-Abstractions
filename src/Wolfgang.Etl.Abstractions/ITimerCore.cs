using System;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// The minimal periodic-timer primitive that <see cref="SystemProgressTimer"/> drives. Production
/// code uses a wrapper over <see cref="System.Threading.Timer"/>; tests inject a deterministic fake
/// (one whose ticks fire on demand and that records <see cref="Change"/> / <see cref="IDisposable.Dispose"/>
/// calls), which makes the timer's Start / StopTimer / Dispose contract observable without relying on
/// real wall-clock ticks.
/// </summary>
internal interface ITimerCore : IDisposable
{
    /// <summary>
    /// Arms or disarms the timer. <see cref="System.Threading.Timeout.Infinite"/> for both arguments
    /// disarms it; a positive period arms it to fire repeatedly at that interval.
    /// </summary>
    void Change(int dueTime, int period);
}
