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
    // Held for the whole of a tick and while Dispose sets the flag, so Dispose returns only once no
    // tick is mid-report: a stage's final report can then never be followed by a stale periodic one.
    private readonly object _tickGate = new();
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
    /// Invoked on each timer tick. A tick that the thread pool had already queued can still arrive
    /// after <see cref="Dispose"/>; the <see cref="_disposed"/> check, made under
    /// <see cref="_tickGate"/>, drops it.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private void OnTick(TimerCallback callback, object? state)
    {
        lock (_tickGate)
        {
            if (_disposed)
            {
                return;
            }

            // A tick runs on a thread-pool thread with nothing above it: an exception that escaped
            // here would end the process rather than fail the run. Stop ticking instead. A fault
            // that persists (a report that cannot be built, a sink that always throws) surfaces to
            // the caller through the stage's final report, which runs on the run's own flow.
            try
            {
                callback(state);
                Elapsed?.Invoke();
            }
#pragma warning disable CA1031 // Do not catch general exception types — intentional: nothing may escape a timer callback.
            catch (Exception)
#pragma warning restore CA1031
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
    }



    /// <inheritdoc/>
    public void Start(int intervalMilliseconds)
    {
        if (Volatile.Read(ref _disposed))
        {
            return;
        }
        _timer.Change(intervalMilliseconds, intervalMilliseconds);
    }



    /// <inheritdoc/>
    public void StopTimer()
    {
        if (Volatile.Read(ref _disposed))
        {
            return;
        }
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
    }



    /// <inheritdoc/>
    public void Dispose()
    {
        // Waits for a tick that is already reporting (the lock is re-entrant, so a tick that disposes
        // its own timer does not deadlock). Callbacks therefore must not block on the thread that
        // disposes the timer.
        lock (_tickGate)
        {
            if (_disposed)
            {
                return;
            }

            Volatile.Write(ref _disposed, true);
            Elapsed = null;
        }

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
