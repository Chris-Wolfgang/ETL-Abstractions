using System;
using System.Threading;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// The default <see cref="IProgressTimer"/> implementation that wraps
/// <see cref="System.Threading.Timer"/> to drive progress callbacks on a
/// background thread-pool thread at a regular interval.
/// </summary>
/// <remarks>
/// This class is used internally by the ETL base classes. In production code
/// it is created automatically by
/// <c>ExtractorBase.CreateProgressTimer</c>,
/// <c>TransformerBase.CreateProgressTimer</c>, and
/// <c>LoaderBase.CreateProgressTimer</c>.
/// In unit tests, override <c>CreateProgressTimer</c> to return a
/// <c>ManualProgressTimer</c> instead.
/// </remarks>
internal sealed class SystemProgressTimer : IProgressTimer
{
    private readonly Timer _timer;
    private bool _disposed;



    /// <inheritdoc/>
    public event Action? Elapsed;



    /// <summary>
    /// Initialises a new <see cref="SystemProgressTimer"/> and immediately
    /// wires the supplied <paramref name="callback"/> to fire on each tick.
    /// </summary>
    internal SystemProgressTimer(
        TimerCallback callback,
        object? state,
        int intervalMilliseconds)
    {
        // Timer is created stopped (Timeout.Infinite) — Start() arms it.
#pragma warning disable MA0042 // Timer does not implement IAsyncDisposable
        _timer = new Timer(
            _ =>
            {
                callback(state);
                Elapsed?.Invoke();
            },
            state: null,
            Timeout.Infinite,
            Timeout.Infinite);
#pragma warning restore MA0042
    }



    /// <inheritdoc/>
    public void Start(int intervalMilliseconds)
    {
        if (_disposed) return;
        _timer.Change(intervalMilliseconds, intervalMilliseconds);
    }



    /// <inheritdoc/>
    public void StopTimer()
    {
        if (_disposed) return;
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
    }



    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Elapsed = null;
#pragma warning disable CA1849, VSTHRD103 // Timer.Dispose() is correct here
        _timer.Dispose();
#pragma warning restore CA1849, VSTHRD103
    }
}
