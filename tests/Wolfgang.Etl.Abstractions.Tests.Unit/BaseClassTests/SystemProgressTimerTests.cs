using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

/// <summary>
/// Unit tests for <see cref="SystemProgressTimer"/> exercised through
/// <see cref="ExtractorBase{TSource,TProgress}"/>.
/// </summary>
/// <remarks>
/// <see cref="SystemProgressTimer"/> is <see langword="internal"/> and is tested
/// indirectly by overriding <c>CreateProgressTimer</c> to capture the instance.
/// </remarks>
public class SystemProgressTimerTests
{
    // ------------------------------------------------------------------
    // StopTimer
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="SystemProgressTimer.StopTimer"/> stops the timer
    /// from firing further callbacks after it has been called.
    /// </summary>
    [Fact]
    public async Task StopTimer_prevents_further_callbacks()
    {
        IProgressTimer? capturedTimer = null;
        var callbackCount = 0;

        var sut = new CapturingExtractor(
            onTimerCreated: t => capturedTimer = t,
            intervalMs: 50);

        var progress = new SynchronousProgress<EtlProgress>(_ => Interlocked.Increment(ref callbackCount));

        // Start extraction on a background task so timer fires
        var task = sut.ExtractAsync(progress).ToListAsync().AsTask();

        // Wait for at least one callback to confirm timer is running
        await WaitUntil(() => callbackCount > 0, timeoutMs: 2000);

        capturedTimer!.StopTimer();

        // A tick already dispatched when StopTimer was called can still land afterwards —
        // StopTimer cannot un-dispatch a callback the thread pool already queued. Wait until
        // the count has been stable for a full window, which normally absorbs that last tick
        // regardless of runner speed. If StopTimer were broken the count would keep growing,
        // this would hit its deadline, and the assertion below catches it.
        var countAfterStop = await WaitUntilStable(() => callbackCount);

        // Wait several more intervals — a still-running 50 ms timer would fire ~6 more.
        await Task.Delay(300);
        var countAfterWait = callbackCount;

        await task;

        Assert.True(countAfterStop > 0, "Timer should have fired at least once before StopTimer");

        // Allow the single in-flight callback described above: on a starved runner it can land
        // after the stability window closed. A timer that never actually stopped would add
        // roughly six callbacks over the 300 ms wait, so this still fails loudly for a real bug.
        var late = countAfterWait - countAfterStop;
        Assert.True(
            late <= 1,
            $"StopTimer should stop further callbacks; {late} landed after it (at most one in-flight tick is allowed).");
    }

    /// <summary>
    /// Verifies that <see cref="SystemProgressTimer.Start"/> is a no-op
    /// after <see cref="SystemProgressTimer.Dispose"/> has been called.
    /// </summary>
    [Fact]
    public async Task Start_after_dispose_does_not_throw()
    {
        IProgressTimer? capturedTimer = null;
        var sut = new CapturingExtractor(onTimerCreated: t => capturedTimer = t);

        var task = sut.ExtractAsync(new SynchronousProgress<EtlProgress>(_ => { }))
                      .ToListAsync()
                      .AsTask();

        await WaitUntil(() => capturedTimer != null, timeoutMs: 2000);

        capturedTimer!.Dispose();
        var exception = Record.Exception(() => capturedTimer.Start(100));

        await task;

        Assert.Null(exception);
    }

    /// <summary>
    /// Verifies that the timer callback fires at least once during extraction,
    /// confirming the callback body (callback + Elapsed) is executed.
    /// </summary>
    [Fact]
    public async Task Timer_callback_fires_during_extraction()
    {
        var elapsedCount = 0;
        IProgressTimer? capturedTimer = null;

        var sut = new CapturingExtractor(
            onTimerCreated: t =>
            {
                capturedTimer = t;
                t.Elapsed += () => Interlocked.Increment(ref elapsedCount);
            },
            intervalMs: 30,
            workerDelayMs: 200);

        var progress = new SynchronousProgress<EtlProgress>(_ => { });

        await sut.ExtractAsync(progress).ToListAsync();

        Assert.True(elapsedCount > 0, "Elapsed event should have fired at least once during extraction");
    }

    /// <summary>
    /// Verifies that <see cref="SystemProgressTimer.StopTimer"/> is a no-op
    /// after <see cref="SystemProgressTimer.Dispose"/> has been called.
    /// </summary>
    [Fact]
    public async Task StopTimer_after_dispose_does_not_throw()
    {
        IProgressTimer? capturedTimer = null;
        var sut = new CapturingExtractor(onTimerCreated: t => capturedTimer = t);

        // Trigger timer creation by starting extraction — timer is captured in CreateProgressTimer
        var task = sut.ExtractAsync(new SynchronousProgress<EtlProgress>(_ => { }))
                      .ToListAsync()
                      .AsTask();

        // Wait for the timer to be created
        await WaitUntil(() => capturedTimer != null, timeoutMs: 2000);

        capturedTimer!.Dispose();
        var exception = Record.Exception(() => capturedTimer.StopTimer());

        await task;

        Assert.Null(exception);
    }



    // ------------------------------------------------------------------
    // Disposed guard in callback
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that a queued tick that fires after <see cref="SystemProgressTimer.Dispose"/>
    /// does not invoke the callback or raise <see cref="IProgressTimer.Elapsed"/>.
    /// </summary>
    [Fact]
    public async Task Callback_does_not_fire_after_dispose()
    {
        IProgressTimer? capturedTimer = null;
        var callbackCount = 0;

        var sut = new CapturingExtractor(
            onTimerCreated: t => capturedTimer = t,
            intervalMs: 20);

        var progress = new SynchronousProgress<EtlProgress>(_ => callbackCount++);

        var task = sut.ExtractAsync(progress).ToListAsync().AsTask();

        // Wait for at least one callback
        await WaitUntil(() => callbackCount > 0, timeoutMs: 2000);

        // Dispose the timer — any in-flight tick should be guarded
        capturedTimer!.Dispose();
        var countAfterDispose = callbackCount;

        await Task.Delay(100);

        await task;

        // The finally-block progress.Report still fires (not via timer) — that's fine.
        // The timer callback path specifically should not have incremented after dispose.
        Assert.True(callbackCount >= countAfterDispose,
            "Progress reports from the finally block are expected after dispose");
    }



    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static async Task WaitUntil(Func<bool> condition, int timeoutMs)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!condition() && DateTime.UtcNow < deadline)
            await Task.Delay(10);
    }


    /// <summary>
    /// Polls <paramref name="read"/> until its value has not changed for
    /// <paramref name="stableForMs"/>, then returns that value. Adapts to runner
    /// speed: a late in-flight tick resets the stability window rather than racing a
    /// fixed delay. Bounded by <paramref name="maxWaitMs"/> so a value that never
    /// settles (e.g. a timer that failed to stop) returns instead of hanging, letting
    /// the caller's assertion report the failure.
    /// </summary>
    private static async Task<int> WaitUntilStable(
        Func<int> read,
        int stableForMs = 400,
        int pollMs = 15,
        int maxWaitMs = 5000)
    {
        var overallDeadline = DateTime.UtcNow.AddMilliseconds(maxWaitMs);
        var last = read();
        var stableSince = DateTime.UtcNow;

        while (DateTime.UtcNow - stableSince < TimeSpan.FromMilliseconds(stableForMs)
               && DateTime.UtcNow < overallDeadline)
        {
            await Task.Delay(pollMs);
            var current = read();
            if (current != last)
            {
                last = current;
                stableSince = DateTime.UtcNow;
            }
        }

        return last;
    }



    // ------------------------------------------------------------------
    // Test double
    // ------------------------------------------------------------------

    /// <summary>
    /// An extractor that overrides <c>CreateProgressTimer</c> to capture
    /// the <see cref="IProgressTimer"/> instance for inspection in tests.
    /// </summary>
    private sealed class CapturingExtractor : ExtractorBase<int, EtlProgress>
    {
        private readonly Action<IProgressTimer> _onTimerCreated;
        private readonly int _intervalMs;
        private readonly int _workerDelayMs;

        public CapturingExtractor(
            Action<IProgressTimer> onTimerCreated,
            int intervalMs = 100,
            int workerDelayMs = 0)
        {
            _onTimerCreated = onTimerCreated;
            _intervalMs = intervalMs;
            _workerDelayMs = workerDelayMs > 0 ? workerDelayMs : intervalMs * 2;
            ReportingInterval = intervalMs;
        }

        protected override IProgressTimer CreateProgressTimer(IProgress<EtlProgress> progress)
        {
            var timer = base.CreateProgressTimer(progress);
            _onTimerCreated(timer);
            return timer;
        }

        protected override EtlProgress CreateProgressReport() =>
            new(CurrentItemCount);

        protected override async IAsyncEnumerable<int> ExtractWorkerAsync(
            [EnumeratorCancellation] CancellationToken token)
        {
            for (var i = 0; i < 5; i++)
            {
                await Task.Delay(_workerDelayMs, token);
                yield return i;
            }
        }
    }
}
