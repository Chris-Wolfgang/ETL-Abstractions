using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

/// <summary>
/// Behavioural coverage for <see cref="ExtractorBase{TSource, TProgress}"/> beyond the
/// TestKit contract tests — per-run counter reset, <c>StartedAt</c>/<c>Elapsed</c> timing,
/// the progress-path final report, and timer disposal (mutation-testing hardening, #205).
/// </summary>
public sealed class ExtractorBaseCoverageTests
{
    private static async Task Drain(IAsyncEnumerable<int> source)
    {
        await foreach (var item in source)
        {
            _ = item;
        }
    }


    [Fact]
    public async Task CurrentItemCount_resets_between_runs()
    {
        var extractor = new TimedExtractor(count: 5);

        await Drain(extractor.ExtractAsync());
        Assert.Equal(5, extractor.CurrentItemCount);

        await Drain(extractor.ExtractAsync());
        Assert.Equal(5, extractor.CurrentItemCount); // reset per run, not 10
    }


    [Fact]
    public async Task CurrentItemCount_resets_between_runs_on_the_progress_path()
    {
        var extractor = new TimedExtractor(count: 5);
        var progress = new SynchronousProgress<EtlProgress>(_ => { });

        await Drain(extractor.ExtractAsync(progress));
        await Drain(extractor.ExtractAsync(progress));

        Assert.Equal(5, extractor.CurrentItemCount);
    }


    [Fact]
    public async Task StartedAt_and_Elapsed_are_set_after_producing_items()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-5);
        var extractor = new TimedExtractor(count: 8, perItemDelayMs: 2);

        await Drain(extractor.ExtractAsync());

        Assert.NotNull(extractor.StartedAtPublic);
        Assert.True(
            extractor.StartedAtPublic > before,
            "StartedAt should be a real capture of the run start, not default(DateTimeOffset)");
        Assert.True(
            extractor.StartedAtPublic <= DateTimeOffset.UtcNow,
            "StartedAt should not be in the future");
        Assert.True(extractor.ElapsedPublic > TimeSpan.Zero, "Elapsed should be positive after doing work");
        Assert.True(extractor.ElapsedPublic < TimeSpan.FromMinutes(1), "Elapsed should be a sane duration");
    }


    [Fact]
    public async Task CurrentSkippedItemCount_resets_between_runs()
    {
        var extractor = new TimedExtractor(count: 5) { SkipItemCount = 2 };

        await Drain(extractor.ExtractAsync());
        Assert.Equal(2, extractor.CurrentSkippedItemCount);

        await Drain(extractor.ExtractAsync());
        Assert.Equal(2, extractor.CurrentSkippedItemCount); // reset per run, not 4
    }


    [Fact]
    public async Task StartedAt_is_set_when_every_item_is_skipped()
    {
        var extractor = new TimedExtractor(count: 4) { SkipItemCount = 4 };

        await Drain(extractor.ExtractAsync());

        Assert.Equal(0, extractor.CurrentItemCount);
        Assert.Equal(4, extractor.CurrentSkippedItemCount);
        Assert.NotNull(extractor.StartedAtPublic); // the skip path must also mark the run as started
    }


    [Fact]
    public void Dispose_delegates_to_Dispose_bool()
    {
        var extractor = new DisposeRecordingExtractor();

        extractor.Dispose();

        Assert.Equal(1, extractor.DisposeBoolCalls);
        Assert.True(extractor.LastDisposing);
    }


    [Fact]
    public async Task DisposeAsync_delegates_to_Dispose_bool()
    {
        var extractor = new DisposeRecordingExtractor();

        await extractor.DisposeAsync();

        Assert.Equal(1, extractor.DisposeBoolCalls);
        Assert.True(extractor.LastDisposing);
    }


    [Fact]
    public async Task StartedAt_is_null_and_Elapsed_is_Zero_when_nothing_is_produced()
    {
        var extractor = new TimedExtractor(count: 0);

        await Drain(extractor.ExtractAsync());

        Assert.Null(extractor.StartedAtPublic);
        Assert.Equal(TimeSpan.Zero, extractor.ElapsedPublic);
    }


    [Fact]
    public async Task StartedAt_resets_to_null_when_a_later_run_produces_nothing()
    {
        var extractor = new TimedExtractor(count: 5);

        await Drain(extractor.ExtractAsync());
        Assert.NotNull(extractor.StartedAtPublic);

        extractor.Count = 0;
        await Drain(extractor.ExtractAsync());

        Assert.Null(extractor.StartedAtPublic); // the run-start timestamp must be reset each run
        Assert.Equal(TimeSpan.Zero, extractor.ElapsedPublic);
    }


    [Fact]
    public async Task Progress_path_reports_the_final_extracted_count()
    {
        var reports = new List<int>();
        var extractor = new TimedExtractor(count: 7);
        var progress = new SynchronousProgress<EtlProgress>(p => reports.Add(p.CurrentItemCount));

        await Drain(extractor.ExtractAsync(progress));

        Assert.NotEmpty(reports);
        Assert.Equal(7, reports[^1]);
    }


    [Fact]
    public async Task Progress_path_disposes_the_timer()
    {
        var timer = new RecordingTimer();
        var extractor = new TimedExtractor(count: 4, timer);
        var progress = new SynchronousProgress<EtlProgress>(_ => { });

        await Drain(extractor.ExtractAsync(progress));

        Assert.True(timer.Disposed, "the progress timer should be disposed after the run");
    }


    [Fact]
    public async Task A_timer_tick_reports_the_running_count()
    {
        var reports = new List<int>();
        var timer = new RecordingTimer();
        var extractor = new TimedExtractor(count: 6, timer);
        var progress = new SynchronousProgress<EtlProgress>(p => reports.Add(p.CurrentItemCount));

        await Drain(extractor.ExtractAsync(progress));
        timer.RaiseElapsed();

        Assert.Contains(6, reports);
    }


    [ExcludeFromCodeCoverage]
    private sealed class TimedExtractor : ExtractorBase<int, EtlProgress>
    {
        private readonly int _perItemDelayMs;
        private readonly IProgressTimer? _timer;
        private bool _wired;

        public TimedExtractor(int count, int perItemDelayMs = 0)
        {
            Count = count;
            _perItemDelayMs = perItemDelayMs;
        }

        public TimedExtractor(int count, IProgressTimer timer)
        {
            Count = count;
            _timer = timer;
        }

        public int Count { get; set; }

        public TimeSpan ElapsedPublic => Elapsed;

        public DateTimeOffset? StartedAtPublic => StartedAt;

        protected override async IAsyncEnumerable<int> ExtractWorkerAsync(
            [EnumeratorCancellation] CancellationToken token)
        {
            var skipped = 0;

            for (var i = 0; i < Count; i++)
            {
                token.ThrowIfCancellationRequested();

                if (skipped < SkipItemCount)
                {
                    skipped++;
                    IncrementCurrentSkippedItemCount();
                    continue;
                }

                IncrementCurrentItemCount();
                yield return i;

                if (_perItemDelayMs > 0)
                {
                    await Task.Delay(_perItemDelayMs, token);
                }
                else
                {
                    await Task.Yield();
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
    private sealed class DisposeRecordingExtractor : ExtractorBase<int, EtlProgress>
    {
        public int DisposeBoolCalls { get; private set; }

        public bool LastDisposing { get; private set; }

#pragma warning disable CS1998 // async iterator with no yielded items is intentional
        protected override async IAsyncEnumerable<int> ExtractWorkerAsync(
            [EnumeratorCancellation] CancellationToken token)
        {
            yield break;
        }
#pragma warning restore CS1998

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
