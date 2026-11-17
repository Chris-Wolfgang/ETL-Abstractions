using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;
using Xunit;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

public class StageRunnerTests
{
    [Fact]
    public async Task ExtractAsync_when_the_run_outlasts_the_reporting_interval_collects_only_the_final_report()
    {
        // A 1 ms interval against a run of at least 50 ms: on a real timer this run collects
        // interval reports as well, and how many depends on the machine.
        using var sut = new SlowExtractor(new ExtractorOptions { ReportingInterval = 1 });

        var run = await StageRunner.ExtractAsync(sut);

        Assert.Equal(new[] { 1 }, run.Items);
        Assert.Equal(1, Assert.Single(run.Reports).CurrentItemCount);
    }



    [Fact]
    public async Task ExtractAsync_when_the_test_installed_a_timer_core_keeps_it()
    {
        using var sut = new SlowExtractor(new ExtractorOptions { ReportingInterval = 1 });
        var installed = 0;
        sut.TimerCoreFactory = _ =>
        {
            installed++;
            return new IdleTimerCore();
        };

        await StageRunner.ExtractAsync(sut);

        Assert.Equal(1, installed);
    }



    private sealed class SlowExtractor : ExtractorBase<int, EtlProgress>
    {
        public SlowExtractor(ExtractorOptions options)
            : base(options)
        {
        }



        protected override async IAsyncEnumerable<int> ExtractWorkerAsync([EnumeratorCancellation] CancellationToken token)
        {
            await Task.Delay(50, token);
            IncrementCurrentItemCount();
            yield return 1;
        }



        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }
}
