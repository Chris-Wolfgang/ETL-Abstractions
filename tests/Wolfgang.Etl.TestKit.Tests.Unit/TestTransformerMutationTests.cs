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
/// Targeted mutation-hardening tests for <see cref="TestTransformer{T}"/> (#346).
/// Each test kills a specific surviving mutant; the survivor's source line is noted
/// in a comment so the mapping stays traceable.
/// </summary>
public class TestTransformerMutationTests
{
    // ------------------------------------------------------------------
    // Timer-injection wiring seam
    // ------------------------------------------------------------------

    [Fact]
    public void CreateProgressTimer_with_injected_timer_returns_that_timer()
    {
        // L86: `if (_progressTimer is null)`. With an injected (non-null) timer the original skips
        // the base and returns the injected instance; the `is not null` mutant would delegate to the
        // base and hand back a different, base-built timer.
        using var timer = new ManualProgressTimer();
        var progress = new SynchronousProgress<Report>(_ => { });
        var sut = new ExposedTimerTransformer(timer);

        var result = sut.CallCreateProgressTimer(progress);

        Assert.Same(timer, result);
    }



    [Fact]
    public void CreateProgressTimer_called_twice_wires_Elapsed_only_once()
    {
        // Kills three survivors on the wiring seam:
        //   L91 `!_progressTimerWired` — remove `!` => never wires => 0 reports.
        //   L93 `_progressTimerWired = true;` — flip/remove => re-wires => 2 reports.
        //   L95 `_progressTimer.Elapsed += _elapsedHandler;` — remove => 0 reports.
        using var timer = new ManualProgressTimer();
        var reportCount = 0;
        var progress = new SynchronousProgress<Report>(_ => reportCount++);
        var sut = new ExposedTimerTransformer(timer);

        sut.CallCreateProgressTimer(progress);
        sut.CallCreateProgressTimer(progress);
        timer.Fire();

        Assert.Equal(1, reportCount);
    }



    [Fact]
    public void Dispose_when_not_disposing_leaves_the_Elapsed_subscription_intact()
    {
        // L113: the Dispose guard requires disposing plus progressTimer-not-null plus elapsedHandler-not-null, all three together.
        // On the finalizer path (disposing == false) the original must not touch the caller-owned
        // timer, so a subsequent Fire() still reports. Either `&&`->`||` mutant unsubscribes here.
        using var timer = new ManualProgressTimer();
        var reportCount = 0;
        var progress = new SynchronousProgress<Report>(_ => reportCount++);
        var sut = new ExposedTimerTransformer(timer);

        sut.CallCreateProgressTimer(progress); // wires the Elapsed handler
        sut.CallDispose(false);                // finalizer path — must NOT unsubscribe
        timer.Fire();

        Assert.Equal(1, reportCount);
    }



    [Fact]
    public void TransformAsync_after_Dispose_throws_ObjectDisposedException()
    {
        // L119: `base.Dispose(disposing);`. Removing it leaves the base `_disposed` flag unset,
        // so the use-after-dispose guard on TransformAsync would no longer fire.
        var sut = new TestTransformer<int>();
        var source = new TestExtractor<int>(new List<int> { 1 }).ExtractAsync();

        sut.Dispose();

        Assert.Throws<ObjectDisposedException>(() => sut.TransformAsync(source));
    }



    // ------------------------------------------------------------------
    // Worker statements
    // ------------------------------------------------------------------

    [Fact]
    public async Task TransformAsync_with_empty_source_and_a_pre_cancelled_token_still_throws()
    {
        // L137: the pre-loop `token.ThrowIfCancellationRequested()`. The source is empty AND ignores
        // cancellation, so the transformer's own pre-loop check is the ONLY thing that can throw —
        // removing it would let the run complete without observing the cancel.
        var transformer = new TestTransformer<int>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            async () =>
            {
                await foreach (var _ in transformer.TransformAsync(UncancellableSource(0), cts.Token))
                {
                }
            }
        );
    }



    [Fact]
    public async Task TransformAsync_cancelling_mid_stream_throws_via_the_in_loop_check()
    {
        // L143: the in-loop `token.ThrowIfCancellationRequested()`. The source deliberately ignores
        // cancellation, so the transformer's own in-loop check is the ONLY thing that can throw —
        // removing it would let the transformer drain every item without observing the cancel.
        var transformer = new TestTransformer<int>();
        using var cts = new CancellationTokenSource();
        var seen = 0;

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            async () =>
            {
                await foreach (var _ in transformer.TransformAsync(UncancellableSource(100), cts.Token))
                {
                    seen++;
                    if (seen == 3)
                    {
#pragma warning disable CA1849, VSTHRD103 // sync Cancel() — CancelAsync is net8+ only
                        cts.Cancel();
#pragma warning restore CA1849, VSTHRD103
                    }
                }
            }
        );

        Assert.True(seen <= 4, $"Expected a prompt in-loop stop (<= 4), but processed {seen}.");
    }



    [Fact]
    public async Task TransformAsync_stops_the_injected_timer_when_the_run_completes()
    {
        // L162: `_progressTimer?.StopTimer();` in the worker's finally. Running without a progress
        // sink means the base never touches the injected timer, so this line is the only StopTimer
        // caller — removing it leaves the recording timer untouched.
        var timer = new RecordingProgressTimer();
        var sut = new ExposedTimerTransformer(timer);
        var source = new TestExtractor<int>(new List<int> { 1, 2, 3 }).ExtractAsync();

        await sut.TransformAsync(source).ToListAsync();

        Assert.True
        (
            timer.StopTimerCallCount >= 1,
            "Expected the worker's finally to stop the injected timer."
        );
    }



    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    // A source that ignores its cancellation token entirely, so only the transformer's own
    // in-loop cancellation check (L143) can observe a mid-stream cancel.
    private static async IAsyncEnumerable<int> UncancellableSource(int count)
    {
        for (var i = 0; i < count; i++)
        {
            await Task.Yield();
            yield return i;
        }
    }



    private sealed class ExposedTimerTransformer : TestTransformer<int>
    {
        public ExposedTimerTransformer(IProgressTimer timer) : base(timer) { }

        public IProgressTimer CallCreateProgressTimer(IProgress<Report> progress) =>
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
