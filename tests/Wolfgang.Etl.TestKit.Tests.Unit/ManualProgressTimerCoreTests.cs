using System;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

public class ManualProgressTimerCoreTests
{
    [Fact]
    public void Tick_before_the_run_starts_throws_InvalidOperationException()
    {
        var timer = new ManualProgressTimerCore();

        Assert.Throws<InvalidOperationException>
        (
            () => timer.Tick()
        );
    }



    [Fact]
    public async Task Tick_after_the_run_starts_invokes_the_progress_callback_Async()
    {
        var timer = new ManualProgressTimerCore();
        var sut = new TestExtractor<int>(new[] { 1, 2, 3 });
        sut.WithManualProgressTimer(timer);

        Report? captured = null;
        var progress = new SyncProgress<Report>(r => captured = r);

        await using var enumerator = sut.ExtractAsync(progress).GetAsyncEnumerator();
        await enumerator.MoveNextAsync().ConfigureAwait(false);   // starts the run; builds the progress timer
        timer.Tick();                                            // fires the callback exactly once

        Assert.NotNull(captured);
    }



    [Fact]
    public void WithManualProgressTimer_when_extractor_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => ((ExtractorBase<int, Report>)null!).WithManualProgressTimer(new ManualProgressTimerCore())
        );
    }



    [Fact]
    public void WithManualProgressTimer_when_extractor_timer_is_null_throws_ArgumentNullException()
    {
        var sut = new TestExtractor<int>(new[] { 1 });

        Assert.Throws<ArgumentNullException>
        (
            () => sut.WithManualProgressTimer(null!)
        );
    }



    [Fact]
    public void WithManualProgressTimer_when_loader_timer_is_null_throws_ArgumentNullException()
    {
        var sut = new TestLoader<int>(collectItems: false);

        Assert.Throws<ArgumentNullException>
        (
            () => sut.WithManualProgressTimer(null!)
        );
    }



    [Fact]
    public void WithManualProgressTimer_when_transformer_timer_is_null_throws_ArgumentNullException()
    {
        var sut = new TestTransformer<int>();

        Assert.Throws<ArgumentNullException>
        (
            () => sut.WithManualProgressTimer(null!)
        );
    }



    // A synchronous IProgress<T> so a Tick's callback is observed inline (System.Progress<T> posts async).
    private sealed class SyncProgress<T> : IProgress<T>
    {
        private readonly Action<T> _callback;

        public SyncProgress(Action<T> callback) => _callback = callback;

        public void Report(T value) => _callback(value);
    }
}
