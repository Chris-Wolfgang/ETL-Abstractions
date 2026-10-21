using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;
using Xunit;

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
    public void StopTimer_prevents_further_callbacks()
    {
        var callbackCount = 0;
        ArmableTimerCore? core = null;
        using var timer = new SystemProgressTimer(_ => callbackCount++, state: null, onTick => core = new ArmableTimerCore(onTick));

        timer.Start(50);
        core!.FireIfArmed();
        timer.StopTimer();
        core.FireIfArmed();
        core.FireIfArmed();

        // Only the tick before StopTimer reached the callback: StopTimer disarmed the core, so the
        // later period boundaries never fire (#736: deterministic, no wall-clock waits).
        Assert.Equal(1, callbackCount);
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

        var sut = new CapturingExtractor(
            onTimerCreated: t =>
            {
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
    public void Callback_does_not_fire_after_dispose()
    {
        var callbackCount = 0;
        var elapsedCount = 0;
        ArmableTimerCore? core = null;
        var timer = new SystemProgressTimer(_ => callbackCount++, state: null, onTick => core = new ArmableTimerCore(onTick));
        timer.Elapsed += () => elapsedCount++;

        timer.Start(20);
        core!.FireIfArmed();
        timer.Dispose();

        // A tick the thread pool had already queued when Dispose ran still reaches OnTick; the
        // disposed guard must swallow it.
        core.FireRegardless();

        Assert.Equal(1, callbackCount);
        Assert.Equal(1, elapsedCount);
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
        private readonly int _workerDelayMs;

        public CapturingExtractor(
            Action<IProgressTimer> onTimerCreated,
            int intervalMs = 100,
            int workerDelayMs = 0)
            : base(new ExtractorOptions { ReportingInterval = intervalMs })
        {
            _onTimerCreated = onTimerCreated;
            _workerDelayMs = workerDelayMs > 0 ? workerDelayMs : intervalMs * 2;
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



    /// <summary>
    /// A deterministic <see cref="ITimerCore"/>: it fires only when the test says so, and
    /// <see cref="FireIfArmed"/> models a real timer by firing only while armed by <c>Change</c>.
    /// </summary>
    private sealed class ArmableTimerCore : ITimerCore
    {
        private readonly TimerCallback _onTick;
        private bool _armed;

        public ArmableTimerCore(TimerCallback onTick) => _onTick = onTick;

        public void Change(int dueTime, int period) => _armed = dueTime != Timeout.Infinite;

        public void Dispose() => _armed = false;

        public void FireIfArmed()
        {
            if (_armed)
            {
                _onTick(null);
            }
        }

        // A callback already dispatched before the timer was stopped or disposed.
        public void FireRegardless() => _onTick(null);
    }
}
