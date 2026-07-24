using System.Diagnostics.CodeAnalysis;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

/// <summary>
/// Behavioural coverage for <see cref="LoaderBase{TDestination, TProgress}"/> beyond the
/// TestKit contract tests — per-run counter reset, <c>StartedAt</c>/<c>Elapsed</c> timing,
/// the progress-path final report, and timer disposal (mutation-testing hardening, #205).
/// </summary>
public sealed class LoaderBaseCoverageTests
{
    private static async IAsyncEnumerable<int> Items(int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return i;
            await Task.Yield();
        }
    }


    [Fact]
    public async Task CurrentItemCount_resets_between_runs()
    {
        var loader = new TimedLoader();

        await loader.LoadAsync(Items(5));
        Assert.Equal(5, loader.CurrentItemCount);

        await loader.LoadAsync(Items(3));
        Assert.Equal(3, loader.CurrentItemCount); // reset per run, not 8
    }


    [Fact]
    public async Task CurrentItemCount_resets_between_runs_on_the_progress_path()
    {
        var loader = new TimedLoader();
        var progress = new SynchronousProgress<EtlProgress>(_ => { });

        await loader.LoadAsync(Items(5), progress);
        await loader.LoadAsync(Items(3), progress);

        Assert.Equal(3, loader.CurrentItemCount);
    }


    [Fact]
    public async Task StartedAt_and_Elapsed_are_set_after_processing_items()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-5);
        var loader = new TimedLoader(perItemDelayMs: 2);

        await loader.LoadAsync(Items(8));

        Assert.NotNull(loader.StartedAtPublic);
        Assert.True(
            loader.StartedAtPublic > before,
            "StartedAt should be a real capture of the run start, not default(DateTimeOffset)");
        Assert.True(
            loader.StartedAtPublic <= DateTimeOffset.UtcNow,
            "StartedAt should not be in the future");
        Assert.True(loader.ElapsedPublic > TimeSpan.Zero, "Elapsed should be positive after doing work");
        Assert.True(loader.ElapsedPublic < TimeSpan.FromMinutes(1), "Elapsed should be a sane duration");
    }


    [Fact]
    public async Task CurrentSkippedItemCount_resets_between_runs()
    {
        var loader = new TimedLoader { SkipItemCount = 2 };

        await loader.LoadAsync(Items(5));
        Assert.Equal(2, loader.CurrentSkippedItemCount);

        await loader.LoadAsync(Items(5));
        Assert.Equal(2, loader.CurrentSkippedItemCount); // reset per run, not 4
    }


    [Fact]
    public async Task StartedAt_is_set_when_every_item_is_skipped()
    {
        var loader = new TimedLoader { SkipItemCount = 4 };

        await loader.LoadAsync(Items(4));

        Assert.Equal(0, loader.CurrentItemCount);
        Assert.Equal(4, loader.CurrentSkippedItemCount);
        Assert.NotNull(loader.StartedAtPublic); // the skip path must also mark the run as started
    }


    [Fact]
    public void Dispose_delegates_to_Dispose_bool()
    {
        var loader = new DisposeRecordingLoader();

        loader.Dispose();

        Assert.Equal(1, loader.DisposeBoolCalls); // Dispose() must route through Dispose(bool)
        Assert.True(loader.LastDisposing);
    }


    [Fact]
    public async Task DisposeAsync_delegates_to_Dispose_bool()
    {
        var loader = new DisposeRecordingLoader();

        await loader.DisposeAsync();

        Assert.Equal(1, loader.DisposeBoolCalls); // DisposeAsync() must route through Dispose(bool)
        Assert.True(loader.LastDisposing);
    }


    [Fact]
    public async Task StartedAt_is_null_and_Elapsed_is_Zero_when_nothing_is_processed()
    {
        var loader = new TimedLoader();

        await loader.LoadAsync(Items(0));

        Assert.Null(loader.StartedAtPublic);
        Assert.Equal(TimeSpan.Zero, loader.ElapsedPublic);
    }


    [Fact]
    public async Task StartedAt_resets_to_null_when_a_later_run_processes_nothing()
    {
        var loader = new TimedLoader();

        await loader.LoadAsync(Items(5));
        Assert.NotNull(loader.StartedAtPublic);

        await loader.LoadAsync(Items(0));

        Assert.Null(loader.StartedAtPublic); // the run-start timestamp must be reset each run
        Assert.Equal(TimeSpan.Zero, loader.ElapsedPublic);
    }


    [Fact]
    public async Task Progress_path_reports_the_final_loaded_count()
    {
        var reports = new List<int>();
        var loader = new TimedLoader();
        var progress = new SynchronousProgress<EtlProgress>(p => reports.Add(p.CurrentItemCount));

        await loader.LoadAsync(Items(7), progress);

        Assert.NotEmpty(reports);
        Assert.Equal(7, reports[^1]); // the finally-block report reflects the final count
    }


    [Fact]
    public async Task Progress_path_disposes_the_timer()
    {
        var timer = new RecordingTimer();
        var loader = new TimedLoader(timer);
        var progress = new SynchronousProgress<EtlProgress>(_ => { });

        await loader.LoadAsync(Items(4), progress);

        Assert.True(timer.Disposed, "the progress timer should be disposed after the run");
    }


    [Fact]
    public async Task A_timer_tick_reports_the_running_count()
    {
        var reports = new List<int>();
        var timer = new RecordingTimer();
        var loader = new TimedLoader(timer);
        var progress = new SynchronousProgress<EtlProgress>(p => reports.Add(p.CurrentItemCount));

        await loader.LoadAsync(Items(6), progress);
        timer.RaiseElapsed(); // simulate a scheduled tick

        Assert.Contains(6, reports);
    }


    [Fact]
    public async Task Skipped_items_increment_the_skipped_counter()
    {
        var loader = new TimedLoader { SkipItemCount = 2 };

        await loader.LoadAsync(Items(5));

        Assert.Equal(2, loader.CurrentSkippedItemCount);
        Assert.Equal(3, loader.CurrentItemCount);
    }


    [ExcludeFromCodeCoverage]
    private sealed class TimedLoader : LoaderBase<int, EtlProgress>
    {
        private readonly int _perItemDelayMs;
        private readonly IProgressTimer? _timer;
        private bool _wired;

        public TimedLoader(int perItemDelayMs = 0) => _perItemDelayMs = perItemDelayMs;

        public TimedLoader(IProgressTimer timer) => _timer = timer;

        public TimeSpan ElapsedPublic => Elapsed;

        public DateTimeOffset? StartedAtPublic => StartedAt;

        protected override async Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token)
        {
            var skipped = 0;

            await foreach (var item in items.WithCancellation(token))
            {
                _ = item;

                if (skipped < SkipItemCount)
                {
                    skipped++;
                    IncrementCurrentSkippedItemCount();
                    continue;
                }

                IncrementCurrentItemCount();

                if (_perItemDelayMs > 0)
                {
                    await Task.Delay(_perItemDelayMs, token);
                }
            }
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);

        protected override IProgressTimer CreateProgressTimer(IProgress<EtlProgress> progress)
        {
            if (_timer is null)
            {
                return base.CreateProgressTimer(progress);
            }

            if (!_wired)
            {
                _wired = true;
                _timer.Elapsed += () => progress.Report(CreateProgressReport());
            }

            return _timer;
        }
    }


    [ExcludeFromCodeCoverage]
    private sealed class DisposeRecordingLoader : LoaderBase<int, EtlProgress>
    {
        public int DisposeBoolCalls { get; private set; }

        public bool LastDisposing { get; private set; }

        protected override Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token)
            => Task.CompletedTask;

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);

        protected override void Dispose(bool disposing)
        {
            DisposeBoolCalls++;
            LastDisposing = disposing;
            base.Dispose(disposing);
        }
    }


    [ExcludeFromCodeCoverage]
    private sealed class RecordingTimer : IProgressTimer
    {
        public bool Disposed { get; private set; }

        public event Action? Elapsed;

        public void Start(int intervalMilliseconds)
        {
        }

        public void StopTimer()
        {
        }

        public void RaiseElapsed() => Elapsed?.Invoke();

        public void Dispose() => Disposed = true;
    }
}
