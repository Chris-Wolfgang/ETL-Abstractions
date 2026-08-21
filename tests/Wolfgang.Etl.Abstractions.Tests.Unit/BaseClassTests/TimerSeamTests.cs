using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

/// <summary>
/// Deterministic tests for the <see cref="SystemProgressTimer"/> Start / StopTimer / Dispose contract
/// and the base classes' <c>CreateProgressTimer</c> factory, driven through the <see cref="ITimerCore"/>
/// seam (#328). A fake core records <c>Change</c>/<c>Dispose</c> calls, so the timer behaviour is
/// verified without relying on real wall-clock ticks (which made the pre-seam tests flaky and left
/// mutation survivors).
/// </summary>
public class TimerSeamTests
{
    // ---- SystemProgressTimer: Start / StopTimer ----

    [Fact]
    public void Start_arms_the_core_with_the_interval()
    {
        var (timer, core) = NewTimer();

        timer.Start(250);

        Assert.Equal(1, core.ChangeCount);
        Assert.Equal(250, core.LastDueTime);
        Assert.Equal(250, core.LastPeriod);
    }


    [Fact]
    public void StopTimer_disarms_the_core()
    {
        var (timer, core) = NewTimer();

        timer.Start(250);
        timer.StopTimer();

        Assert.Equal(2, core.ChangeCount);
        Assert.Equal(Timeout.Infinite, core.LastDueTime);
        Assert.Equal(Timeout.Infinite, core.LastPeriod);
    }


    // ---- SystemProgressTimer: Dispose ----

    [Fact]
    public void Dispose_disposes_the_core()
    {
        var (timer, core) = NewTimer();

        timer.Dispose();

        Assert.Equal(1, core.DisposeCount);
    }


    [Fact]
    public void Dispose_is_idempotent()
    {
        var (timer, core) = NewTimer();

        timer.Dispose();
        timer.Dispose();

        Assert.Equal(1, core.DisposeCount); // the second Dispose short-circuits
    }


    // ---- SystemProgressTimer: operations after Dispose are no-ops ----

    [Fact]
    public void Start_after_Dispose_does_not_touch_the_core()
    {
        var (timer, core) = NewTimer();

        timer.Dispose();
        timer.Start(250);

        Assert.Equal(0, core.ChangeCount); // Start short-circuited; the disposed timer is not re-armed
    }


    [Fact]
    public void StopTimer_after_Dispose_does_not_touch_the_core()
    {
        var (timer, core) = NewTimer();

        timer.Dispose();
        timer.StopTimer();

        Assert.Equal(0, core.ChangeCount); // StopTimer short-circuited
    }


    // ---- A tick drives the callback + Elapsed event ----

    [Fact]
    public void A_tick_invokes_the_callback_and_raises_Elapsed()
    {
        var callbackFired = 0;
        var elapsedFired = 0;
        FakeTimerCore? core = null;
        using var timer = new SystemProgressTimer(_ => callbackFired++, state: null, onTick => core = new FakeTimerCore(onTick));
        timer.Elapsed += () => elapsedFired++;
        timer.Start(100);

        core!.Tick();

        Assert.Equal(1, callbackFired);
        Assert.Equal(1, elapsedFired);
    }


    // ---- CreateProgressTimer factory starts the timer (base classes) ----

    [Fact]
    public void LoaderBase_CreateProgressTimer_returns_a_started_timer()
    {
        FakeTimerCore? core = null;
        using var loader = new TimerSeamLoader { ReportingInterval = 321, TimerCoreFactory = onTick => core = new FakeTimerCore(onTick) };

        using var timer = loader.CallCreateProgressTimer();

        Assert.NotNull(core);
        Assert.Equal(321, core.LastDueTime); // CreateProgressTimer called Start(ReportingInterval)
        Assert.Equal(321, core.LastPeriod);
    }


    [Fact]
    public void TransformerBase_CreateProgressTimer_returns_a_started_timer()
    {
        FakeTimerCore? core = null;
        using var transformer = new TimerSeamTransformer { ReportingInterval = 654, TimerCoreFactory = onTick => core = new FakeTimerCore(onTick) };

        using var timer = transformer.CallCreateProgressTimer();

        Assert.NotNull(core);
        Assert.Equal(654, core.LastDueTime);
        Assert.Equal(654, core.LastPeriod);
    }


    [Fact]
    public void ExtractorBase_CreateProgressTimer_returns_a_started_timer()
    {
        FakeTimerCore? core = null;
        using var extractor = new TimerSeamExtractor { ReportingInterval = 111, TimerCoreFactory = onTick => core = new FakeTimerCore(onTick) };

        using var timer = extractor.CallCreateProgressTimer();

        Assert.NotNull(core);
        Assert.Equal(111, core.LastDueTime);
        Assert.Equal(111, core.LastPeriod);
    }


    // ---- helpers / doubles ----

    private static (SystemProgressTimer timer, FakeTimerCore core) NewTimer()
    {
        FakeTimerCore? core = null;
        var timer = new SystemProgressTimer(_ => { }, state: null, onTick => core = new FakeTimerCore(onTick));
        return (timer, core!);
    }


    // A deterministic ITimerCore that records Change/Dispose calls and fires ticks on demand.
    [ExcludeFromCodeCoverage]
    private sealed class FakeTimerCore : ITimerCore
    {
        private readonly TimerCallback _onTick;

        public int ChangeCount;
        public int LastDueTime = int.MinValue;
        public int LastPeriod = int.MinValue;
        public int DisposeCount;

        public FakeTimerCore(TimerCallback onTick) => _onTick = onTick;

        public void Change(int dueTime, int period)
        {
            ChangeCount++;
            LastDueTime = dueTime;
            LastPeriod = period;
        }

        public void Dispose() => DisposeCount++;

        public void Tick() => _onTick(null);
    }


    [ExcludeFromCodeCoverage]
    private sealed class TimerSeamLoader : LoaderBase<int, EtlProgress>
    {
        public IProgressTimer CallCreateProgressTimer() =>
            CreateProgressTimer(new SynchronousProgress<EtlProgress>(_ => { }));

        protected override Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token) => Task.CompletedTask;

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    [ExcludeFromCodeCoverage]
    private sealed class TimerSeamTransformer : TransformerBase<int, int, EtlProgress>
    {
        public IProgressTimer CallCreateProgressTimer() =>
            CreateProgressTimer(new SynchronousProgress<EtlProgress>(_ => { }));

        protected override async IAsyncEnumerable<int> TransformWorkerAsync(
            IAsyncEnumerable<int> items, [EnumeratorCancellation] CancellationToken token)
        {
            await Task.CompletedTask;
            yield break;
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    [ExcludeFromCodeCoverage]
    private sealed class TimerSeamExtractor : ExtractorBase<int, EtlProgress>
    {
        public IProgressTimer CallCreateProgressTimer() =>
            CreateProgressTimer(new SynchronousProgress<EtlProgress>(_ => { }));

        protected override async IAsyncEnumerable<int> ExtractWorkerAsync([EnumeratorCancellation] CancellationToken token)
        {
            await Task.CompletedTask;
            yield break;
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }
}
