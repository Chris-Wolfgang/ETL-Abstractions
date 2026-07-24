using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

/// <summary>
/// Behavioural coverage for <see cref="TransformerBase{TSource, TDestination, TProgress}"/>
/// beyond the TestKit contract tests — per-run counter reset, <c>StartedAt</c>/<c>Elapsed</c>
/// timing, the progress-path final report, and timer disposal (mutation-testing hardening, #205).
/// </summary>
public sealed class TransformerBaseCoverageTests
{
    private static async IAsyncEnumerable<int> Items(int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return i;
            await Task.Yield();
        }
    }


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
        var transformer = new TimedTransformer();

        await Drain(transformer.TransformAsync(Items(5)));
        Assert.Equal(5, transformer.CurrentItemCount);

        await Drain(transformer.TransformAsync(Items(3)));
        Assert.Equal(3, transformer.CurrentItemCount); // reset per run, not 8
    }


    [Fact]
    public async Task CurrentItemCount_resets_between_runs_on_the_progress_path()
    {
        var transformer = new TimedTransformer();
        var progress = new SynchronousProgress<EtlProgress>(_ => { });

        await Drain(transformer.TransformAsync(Items(5), progress));
        await Drain(transformer.TransformAsync(Items(3), progress));

        Assert.Equal(3, transformer.CurrentItemCount);
    }


    [Fact]
    public async Task StartedAt_and_Elapsed_are_set_after_transforming_items()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-5);
        var transformer = new TimedTransformer(perItemDelayMs: 2);

        await Drain(transformer.TransformAsync(Items(8)));

        Assert.NotNull(transformer.StartedAtPublic);
        Assert.True(
            transformer.StartedAtPublic > before,
            "StartedAt should be a real capture of the run start, not default(DateTimeOffset)");
        Assert.True(
            transformer.StartedAtPublic <= DateTimeOffset.UtcNow,
            "StartedAt should not be in the future");
        Assert.True(transformer.ElapsedPublic > TimeSpan.Zero, "Elapsed should be positive after doing work");
        Assert.True(transformer.ElapsedPublic < TimeSpan.FromMinutes(1), "Elapsed should be a sane duration");
    }


    [Fact]
    public async Task CurrentSkippedItemCount_resets_between_runs()
    {
        var transformer = new TimedTransformer { SkipItemCount = 2 };

        await Drain(transformer.TransformAsync(Items(5)));
        Assert.Equal(2, transformer.CurrentSkippedItemCount);

        await Drain(transformer.TransformAsync(Items(5)));
        Assert.Equal(2, transformer.CurrentSkippedItemCount); // reset per run, not 4
    }


    [Fact]
    public async Task StartedAt_is_set_when_every_item_is_skipped()
    {
        var transformer = new TimedTransformer { SkipItemCount = 4 };

        await Drain(transformer.TransformAsync(Items(4)));

        Assert.Equal(0, transformer.CurrentItemCount);
        Assert.Equal(4, transformer.CurrentSkippedItemCount);
        Assert.NotNull(transformer.StartedAtPublic); // the skip path must also mark the run as started
    }


    [Fact]
    public void Dispose_delegates_to_Dispose_bool()
    {
        var transformer = new DisposeRecordingTransformer();

        transformer.Dispose();

        Assert.Equal(1, transformer.DisposeBoolCalls);
        Assert.True(transformer.LastDisposing);
    }


    [Fact]
    public async Task DisposeAsync_delegates_to_Dispose_bool()
    {
        var transformer = new DisposeRecordingTransformer();

        await transformer.DisposeAsync();

        Assert.Equal(1, transformer.DisposeBoolCalls);
        Assert.True(transformer.LastDisposing);
    }


    [Fact]
    public async Task StartedAt_is_null_and_Elapsed_is_Zero_when_nothing_is_transformed()
    {
        var transformer = new TimedTransformer();

        await Drain(transformer.TransformAsync(Items(0)));

        Assert.Null(transformer.StartedAtPublic);
        Assert.Equal(TimeSpan.Zero, transformer.ElapsedPublic);
    }


    [Fact]
    public async Task StartedAt_resets_to_null_when_a_later_run_transforms_nothing()
    {
        var transformer = new TimedTransformer();

        await Drain(transformer.TransformAsync(Items(5)));
        Assert.NotNull(transformer.StartedAtPublic);

        await Drain(transformer.TransformAsync(Items(0)));

        Assert.Null(transformer.StartedAtPublic); // the run-start timestamp must be reset each run
        Assert.Equal(TimeSpan.Zero, transformer.ElapsedPublic);
    }


    [Fact]
    public async Task Progress_path_reports_the_final_transformed_count()
    {
        var reports = new List<int>();
        var transformer = new TimedTransformer();
        var progress = new SynchronousProgress<EtlProgress>(p => reports.Add(p.CurrentItemCount));

        await Drain(transformer.TransformAsync(Items(7), progress));

        Assert.NotEmpty(reports);
        Assert.Equal(7, reports[^1]);
    }


    [Fact]
    public async Task Progress_path_disposes_the_timer()
    {
        var timer = new RecordingTimer();
        var transformer = new TimedTransformer(timer);
        var progress = new SynchronousProgress<EtlProgress>(_ => { });

        await Drain(transformer.TransformAsync(Items(4), progress));

        Assert.True(timer.Disposed, "the progress timer should be disposed after the run");
    }


    [Fact]
    public async Task A_timer_tick_reports_the_running_count()
    {
        var reports = new List<int>();
        var timer = new RecordingTimer();
        var transformer = new TimedTransformer(timer);
        var progress = new SynchronousProgress<EtlProgress>(p => reports.Add(p.CurrentItemCount));

        await Drain(transformer.TransformAsync(Items(6), progress));
        timer.RaiseElapsed();

        Assert.Contains(6, reports);
    }


    [ExcludeFromCodeCoverage]
    private sealed class TimedTransformer : TransformerBase<int, int, EtlProgress>
    {
        private readonly int _perItemDelayMs;
        private readonly IProgressTimer? _timer;
        private bool _wired;

        public TimedTransformer(int perItemDelayMs = 0) => _perItemDelayMs = perItemDelayMs;

        public TimedTransformer(IProgressTimer timer) => _timer = timer;

        public TimeSpan ElapsedPublic => Elapsed;

        public DateTimeOffset? StartedAtPublic => StartedAt;

        protected override async IAsyncEnumerable<int> TransformWorkerAsync(
            IAsyncEnumerable<int> items,
            [EnumeratorCancellation] CancellationToken token)
        {
            var skipped = 0;

            await foreach (var item in items.WithCancellation(token))
            {
                if (skipped < SkipItemCount)
                {
                    skipped++;
                    IncrementCurrentSkippedItemCount();
                    continue;
                }

                IncrementCurrentItemCount();
                yield return item;

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
    private sealed class DisposeRecordingTransformer : TransformerBase<int, int, EtlProgress>
    {
        public int DisposeBoolCalls { get; private set; }

        public bool LastDisposing { get; private set; }

#pragma warning disable CS1998 // async iterator with no yielded items is intentional
        protected override async IAsyncEnumerable<int> TransformWorkerAsync(
            IAsyncEnumerable<int> items,
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
