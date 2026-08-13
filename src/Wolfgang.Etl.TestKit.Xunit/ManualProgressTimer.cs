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
/// </remarks>
/// <example>
/// <code>
/// // In your concrete contract test class, override CreateSutWithTimer to return
/// // your SUT wired to the supplied timer (see ExtractorBaseContractTests).
///
/// // The base class uses it like this in the progress callback test:
/// var timer = new ManualProgressTimer();
/// var sut = CreateSutWithTimer(timer);
/// TProgress? captured = null;
/// var progress = new SynchronousProgress&lt;TProgress&gt;(r => captured = r);
///
/// // Begin enumeration and pull the first item so the pipeline is mid-flight,
/// // then fire the timer deterministically before draining the rest. This
/// // avoids the race where a fast synchronous source completes (and the
/// // timer's Elapsed handler is unsubscribed in the worker's finally) before
/// // Fire() is reached.
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
