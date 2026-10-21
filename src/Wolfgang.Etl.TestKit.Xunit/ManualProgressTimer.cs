using System;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// A controllable implementation of <see cref="IProgressTimer"/> for use in unit tests.
/// Fires <see cref="IProgressTimer.Elapsed"/> synchronously on demand via <see cref="Fire"/>,
/// rather than on a background thread-pool thread after a real time interval.
/// </summary>
/// <remarks>
/// <para>
/// Inject a <see cref="ManualProgressTimer"/> via the <see langword="protected"/>
/// constructor of your derived extractor, transformer, or loader to make progress
/// callback assertions fully deterministic — no delays, no races, no flaky tests.
/// </para>
/// <para>
/// <see cref="Start"/> and <see cref="StopTimer"/> are no-ops; the timer only fires when
/// <see cref="Fire"/> is called explicitly.
/// </para>
/// <para>
/// The contract-test base classes no longer use this type: they drive the base stage's own timer
/// through <c>ManualProgressTimerCore</c> and <c>WithManualProgressTimer</c> from
/// <c>Wolfgang.Etl.TestKit</c>, which needs no injection constructor. Use
/// <see cref="ManualProgressTimer"/> only for a component that already returns an injected
/// <see cref="IProgressTimer"/> from its <c>CreateProgressTimer</c> override.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // MyExtractor has an internal test constructor that stores the timer and returns it from
/// // its CreateProgressTimer override, subscribing Elapsed to report progress.
/// var timer = new ManualProgressTimer();
/// var sut = new MyExtractor(source, timer);
/// Report? captured = null;
/// var progress = new SynchronousProgress&lt;Report&gt;(r =&gt; captured = r);
///
/// // Pull the first item so the run is mid-flight, then fire the timer deterministically
/// // before draining the rest. Firing after a fast source completes would find the
/// // Elapsed handler already unsubscribed by the worker's finally block.
/// await using var enumerator = sut.ExtractAsync(progress).GetAsyncEnumerator();
/// await enumerator.MoveNextAsync();
/// timer.Fire();
/// while (await enumerator.MoveNextAsync()) { }
///
/// Assert.NotNull(captured);
/// </code>
/// </example>
public sealed class ManualProgressTimer : IProgressTimer
{
    private bool _disposed;



    /// <inheritdoc/>
    public event Action? Elapsed;



    /// <summary>
    /// Fires the <see cref="Elapsed"/> event synchronously on the calling thread.
    /// </summary>
    /// <remarks>
    /// Call this from your test to simulate a timer tick at exactly the right moment,
    /// making progress callback assertions fully deterministic.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    /// The timer has been disposed.
    /// </exception>
    public void Fire()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ManualProgressTimer));
        }

        Elapsed?.Invoke();
    }



    /// <summary>
    /// No-op. <see cref="ManualProgressTimer"/> does not start a real timer;
    /// use <see cref="Fire"/> to trigger <see cref="Elapsed"/> explicitly.
    /// </summary>
    /// <param name="intervalMilliseconds">Ignored.</param>
    public void Start(int intervalMilliseconds) { }



    /// <summary>
    /// No-op. <see cref="ManualProgressTimer"/> has no real timer to stop.
    /// </summary>
    public void StopTimer() { }



    /// <summary>
    /// Releases all resources used by the <see cref="ManualProgressTimer"/>.
    /// After disposal, calling <see cref="Fire"/> will throw
    /// <see cref="ObjectDisposedException"/>.
    /// </summary>
    public void Dispose()
    {
        _disposed = true;
        Elapsed = null;
    }
}
