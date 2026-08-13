using System;
using System.Threading;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit;

/// <summary>
/// A manually-driven progress timer for tests: instead of a real <see cref="System.Threading.Timer"/>
/// firing on an interval, the stage's progress callback fires only when <see cref="Tick"/> is called,
/// making progress-callback assertions deterministic. Attach it to a stage with
/// <see cref="ProgressTimerExtensions.WithManualProgressTimer{TSource, TProgress}(ExtractorBase{TSource, TProgress}, ManualProgressTimerCore)"/>
/// (and the loader / transformer overloads).
/// </summary>
/// <remarks>
/// This drives the internal timer-core seam on the base classes (the same seam the base's own
/// <c>CreateProgressTimer</c> uses), reachable here because <c>Wolfgang.Etl.TestKit</c> is an
/// internals-visible friend of <c>Wolfgang.Etl.Abstractions</c> — so a component needs <b>no</b>
/// per-type timer-injection plumbing to be timer-testable. Attach it before the run starts; the stage
/// builds its progress timer when the run begins.
/// <example><code>
/// var timer = new ManualProgressTimerCore();
/// var extractor = new TestExtractor&lt;int&gt;(new[] { 1, 2, 3 }).WithManualProgressTimer(timer);
///
/// await using var e = extractor.ExtractAsync(progress).GetAsyncEnumerator();
/// await e.MoveNextAsync();
/// timer.Tick();            // fires the progress callback exactly once
/// </code></example>
/// </remarks>
public sealed class ManualProgressTimerCore
{
    private TimerCallback? _onTick;

    // Consumed by ProgressTimerExtensions to set the base's internal TimerCoreFactory. The base calls
    // this with the per-tick callback when it builds its progress timer; we capture the callback so
    // Tick() can invoke it on demand and return a no-op core (interval timing is irrelevant here).
    internal Func<TimerCallback, ITimerCore> CoreFactory =>
        onTick =>
        {
            _onTick = onTick;
            return new Core();
        };



    /// <summary>
    /// Fires a single timer tick, synchronously invoking the stage's progress callback exactly once.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The stage's progress timer has not been built yet — begin the run (start enumeration, or invoke
    /// the loader) before calling <see cref="Tick"/>.
    /// </exception>
    public void Tick()
    {
        var onTick = _onTick
            ?? throw new InvalidOperationException
            (
                "The progress timer has not started. Begin the run (start enumeration / invoke the " +
                "loader) before calling Tick()."
            );

        onTick(state: null);
    }



    // A do-nothing ITimerCore: a manual timer never schedules real callbacks, so Change is a no-op.
    private sealed class Core : ITimerCore
    {
        public void Change(int dueTime, int period)
        {
        }



        public void Dispose()
        {
        }
    }
}
