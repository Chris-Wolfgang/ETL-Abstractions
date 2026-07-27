using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// The default <see cref="IProgressTimer"/> implementation that wraps
/// <see cref="System.Threading.Timer"/> (via <see cref="ITimerCore"/>) to drive progress callbacks on
/// a background thread-pool thread at a regular interval.
/// </summary>
/// <remarks>
/// This class is used internally by the ETL base classes. In production code it is created
/// automatically by <c>ExtractorBase.CreateProgressTimer</c>, <c>TransformerBase.CreateProgressTimer</c>,
/// and <c>LoaderBase.CreateProgressTimer</c>. In unit tests, inject a fake <see cref="ITimerCore"/> via
/// the test constructor so the Start / StopTimer / Dispose contract can be verified deterministically.
/// </remarks>
internal sealed class SystemProgressTimer : IProgressTimer
{
    private readonly ITimerCore _timer;
    private bool _disposed;



    /// <inheritdoc/>
    public event Action? Elapsed;



    /// <summary>
    /// Initialises a new <see cref="SystemProgressTimer"/> backed by a real
    /// <see cref="System.Threading.Timer"/>, wiring <paramref name="callback"/> to fire on each tick.
    /// </summary>
    internal SystemProgressTimer(TimerCallback callback, object? state)
        : this(callback, state, onTick => new SystemTimerCore(onTick))
    {
    }



    /// <summary>
    /// Test seam: initialises a new <see cref="SystemProgressTimer"/> whose underlying timer is produced
    /// by <paramref name="coreFactory"/> (the factory receives the per-tick callback to invoke).
    /// </summary>
    internal SystemProgressTimer(TimerCallback callback, object? state, Func<TimerCallback, ITimerCore> coreFactory)
    {
        // The core is created stopped; Start() arms it.
        _timer = coreFactory(_ => OnTick(callback, state));
    }



    /// <summary>
    /// Invoked on each timer tick. The <see cref="_disposed"/> guard is a defensive
    /// race-condition safety net — a queued tick may fire just after
    /// <see cref="Dispose"/> sets the flag.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private void OnTick(TimerCallback callback, object? state)
    {
        if (_disposed)
        {
            return;
        }
        callback(state);
        Elapsed?.Invoke();
    }



    /// <inheritdoc/>
    public void Start(int intervalMilliseconds)
    {
        if (_disposed)
        {
            return;
        }
        _timer.Change(intervalMilliseconds, intervalMilliseconds);
    }



    /// <inheritdoc/>
    public void StopTimer()
    {
        if (_disposed)
        {
            return;
        }
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
    }



    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        Elapsed = null;
        _timer.Dispose();
    }



    /// <summary>The production <see cref="ITimerCore"/> — a thin wrapper over <see cref="Timer"/>.</summary>
    private sealed class SystemTimerCore : ITimerCore
    {
        private readonly Timer _timer;

        internal SystemTimerCore(TimerCallback onTick)
        {
#pragma warning disable MA0042 // Timer does not implement IAsyncDisposable
            _timer = new Timer(onTick, state: null, Timeout.Infinite, Timeout.Infinite);
#pragma warning restore MA0042
        }

        public void Change(int dueTime, int period) => _timer.Change(dueTime, period);

        [ExcludeFromCodeCoverage] // thin BCL delegation
        public void Dispose()
        {
#pragma warning disable CA1849, VSTHRD103 // Timer.Dispose() is correct here
            _timer.Dispose();
#pragma warning restore CA1849, VSTHRD103
        }
    }
}
