using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit.Xunit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

/// <summary>
/// Targeted mutation-hardening tests for <see cref="TestExtractor{T}"/> (#346).
/// Each test kills a specific surviving mutant; the survivor's source line is noted
/// in a comment so the mapping stays traceable.
/// </summary>
public class TestExtractorMutationTests
{
    // ------------------------------------------------------------------
    // count == 0 boundary guards — kills `count < 0` -> `count <= 0`
    // (Func<T>,count L205; Func<int,T>,count L269; +timer L340; +timer L416)
    // ------------------------------------------------------------------

    [Fact]
    public async Task Constructor_func_count_when_count_is_zero_constructs_and_yields_no_items()
    {
        // L205: `count < 0`. A zero count is valid (yields nothing); the `<=` mutant would throw.
        var sut = new TestExtractor<int>(() => 1, 0);

        var results = await sut.ExtractAsync().ToListAsync();

        Assert.Empty(results);
    }



    [Fact]
    public async Task Constructor_indexed_factory_count_when_count_is_zero_constructs_and_yields_no_items()
    {
        // L269: `count < 0` on the indexed-factory + count constructor.
        var sut = new TestExtractor<int>(i => i, 0);

        var results = await sut.ExtractAsync().ToListAsync();

        Assert.Empty(results);
    }



    [Fact]
    public async Task Constructor_func_count_timer_when_count_is_zero_constructs_and_yields_no_items()
    {
        // L340: `count < 0` on the protected Func<T> + count + timer constructor.
        using var timer = new ManualProgressTimer();
        var sut = new ExposedTimerExtractor(() => 1, 0, timer);

        var results = await sut.ExtractAsync().ToListAsync();

        Assert.Empty(results);
    }



    [Fact]
    public async Task Constructor_indexed_factory_count_timer_when_count_is_zero_constructs_and_yields_no_items()
    {
        // L416: `count < 0` on the protected Func<int,T> + count + timer constructor.
        using var timer = new ManualProgressTimer();
        var sut = new ExposedTimerExtractor(i => i, 0, timer);

        var results = await sut.ExtractAsync().ToListAsync();

        Assert.Empty(results);
    }



    // ------------------------------------------------------------------
    // Timer-injection wiring seam
    // ------------------------------------------------------------------

    [Fact]
    public void CreateProgressTimer_called_twice_wires_Elapsed_only_once()
    {
        // Kills three survivors on the wiring seam:
        //   L483 `!_progressTimerWired`  — remove `!` => never wires => 0 reports.
        //   L485 `_progressTimerWired = true;` — flip/remove => re-wires => 2 reports.
        //   L487 `_progressTimer.Elapsed += _elapsedHandler;` — remove => 0 reports.
        // The guarded original wires exactly one handler, so one Fire() => exactly one report.
        using var timer = new ManualProgressTimer();
        var reportCount = 0;
        var progress = new SynchronousProgress<Report>(_ => reportCount++);
        var sut = new ExposedTimerExtractor(new List<int> { 1, 2, 3 }, timer);

        sut.CallCreateProgressTimer(progress);
        sut.CallCreateProgressTimer(progress);
        timer.Fire();

        Assert.Equal(1, reportCount);
    }



    [Fact]
    public void Dispose_when_not_disposing_leaves_the_Elapsed_subscription_intact()
    {
        // L505: the Dispose guard requires disposing plus progressTimer-not-null plus elapsedHandler-not-null, all three together.
        // On the finalizer path (disposing == false) the original leaves the caller-owned timer
        // untouched, so a subsequent Fire() still reports. Either `&&`->`||` mutant would
        // unsubscribe here, producing zero reports.
        using var timer = new ManualProgressTimer();
        var reportCount = 0;
        var progress = new SynchronousProgress<Report>(_ => reportCount++);
        var sut = new ExposedTimerExtractor(new List<int> { 1, 2, 3 }, timer);

        sut.CallCreateProgressTimer(progress); // wires the Elapsed handler
        sut.CallDispose(false);                // finalizer path — must NOT unsubscribe
        timer.Fire();

        Assert.Equal(1, reportCount);
    }



    [Fact]
    public void ExtractAsync_after_Dispose_throws_ObjectDisposedException()
    {
        // L511: `base.Dispose(disposing);`. Removing it leaves the base `_disposed` flag unset,
        // so the use-after-dispose guard on ExtractAsync would no longer fire.
        var sut = new TestExtractor<int>(new List<int> { 1, 2, 3 });

        sut.Dispose();

        Assert.Throws<ObjectDisposedException>(() => sut.ExtractAsync());
    }



    // ------------------------------------------------------------------
    // Worker statements
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_with_empty_source_and_a_pre_cancelled_token_still_throws()
    {
        // L533: the pre-loop `token.ThrowIfCancellationRequested()`. With an empty source the
        // in-loop check never runs, so removing the pre-loop statement would let the run complete.
        var sut = new TestExtractor<int>(new List<int>());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            async () =>
            {
                await foreach (var _ in sut.ExtractAsync(cts.Token))
                {
                }
            }
        );
    }



    [Fact]
    public async Task ExtractAsync_stops_the_injected_timer_when_the_run_completes()
    {
        // L569: `_progressTimer?.StopTimer();` in the worker's finally. Running without a progress
        // sink means the base never creates or touches the injected timer, so the only StopTimer
        // call comes from this line — removing it leaves the recording timer untouched.
        var timer = new RecordingProgressTimer();
        var sut = new ExposedTimerExtractor(new List<int> { 1, 2, 3 }, timer);

        await sut.ExtractAsync().ToListAsync();

        Assert.True
        (
            timer.StopTimerCallCount >= 1,
            "Expected the worker's finally to stop the injected timer."
        );
    }



    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private sealed class ExposedTimerExtractor : TestExtractor<int>
    {
        public ExposedTimerExtractor(IEnumerable<int> items, IProgressTimer timer)
            : base(items, timer) { }

        public ExposedTimerExtractor(Func<int> factory, int count, IProgressTimer timer)
            : base(factory, count, timer) { }

        public ExposedTimerExtractor(Func<int, int> factory, int count, IProgressTimer timer)
            : base(factory, count, timer) { }

        public void CallCreateProgressTimer(IProgress<Report> progress) =>
            CreateProgressTimer(progress);

        public void CallDispose(bool disposing) => Dispose(disposing);
    }



    // A minimal IProgressTimer test double that records StopTimer invocations.
    private sealed class RecordingProgressTimer : IProgressTimer
    {
        public int StopTimerCallCount { get; private set; }

#pragma warning disable CS0067 // Elapsed is mandated by IProgressTimer but never raised by this recording double.
        public event Action? Elapsed;
#pragma warning restore CS0067

        public void Start(int intervalMilliseconds) { }

        public void StopTimer() => StopTimerCallCount++;

        public void Dispose() { }
    }
}
