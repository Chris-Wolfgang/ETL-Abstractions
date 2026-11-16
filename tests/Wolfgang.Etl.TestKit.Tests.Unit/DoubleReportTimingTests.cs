using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

/// <summary>
/// Verifies that the doubles surface the base class's timing instrumentation (Abstractions
/// 0.14.0) in the <see cref="Report"/> they build — <see cref="Report.StartedAt"/> and
/// <see cref="Report.Elapsed"/>, which in turn drive <see cref="Report.ItemsPerSecond"/>. The
/// <c>CreateProgressReport</c> override is byte-identical across all six doubles, so
/// <see cref="TestExtractor{T}"/> stands in as the representative.
/// </summary>
public class DoubleReportTimingTests
{
    [Fact]
    public void CreateProgressReport_before_any_run_has_no_StartedAt()
    {
        var sut = new ExposedTestExtractor<int>(new List<int> { 1, 2, 3 });

        var report = sut.GetProgressReport();

        Assert.Null(report.StartedAt);
    }

    [Fact]
    public async Task CreateProgressReport_after_a_run_surfaces_StartedAt_and_Elapsed()
    {
        var clock = new ManualTimeSource();
        var sut = new ExposedTestExtractor<int>(new List<int> { 1, 2, 3 });
        sut.WithTimeSource(clock);

        await DrainAdvancingAfterFirstItem(sut, clock, TimeSpan.FromSeconds(10));
        var report = sut.GetProgressReport();

        Assert.NotNull(report.StartedAt);
        Assert.Equal(TimeSpan.FromSeconds(10), report.Elapsed);
        Assert.Equal(3, report.CurrentItemCount);
    }

    [Fact]
    public async Task CreateProgressReport_after_a_run_computes_the_throughput_from_the_elapsed_time()
    {
        var clock = new ManualTimeSource();
        var sut = new ExposedTestExtractor<int>(Enumerable.Range(0, 50).ToList());
        sut.WithTimeSource(clock);

        await DrainAdvancingAfterFirstItem(sut, clock, TimeSpan.FromSeconds(10));
        var report = sut.GetProgressReport();

        // 50 items over the 10 s the fake clock advanced.
        Assert.Equal(5d, report.ItemsPerSecond);
    }

    [Fact]
    public async Task CreateProgressReport_for_a_collection_source_surfaces_TotalItemCount()
    {
        var sut = new ExposedTestExtractor<int>(new List<int> { 1, 2, 3, 4, 5 });

        await sut.ExtractAsync().ToListAsync();
        var report = sut.GetProgressReport();

        Assert.Equal(5, report.TotalItemCount);
        Assert.Equal(100d, report.PercentComplete!.Value);
    }

    [Fact]
    public void CreateProgressReport_for_an_enumerator_source_has_no_TotalItemCount()
    {
        var sut = new ExposedTestExtractor<int>(Enumerable.Range(1, 3).GetEnumerator());

        var report = sut.GetProgressReport();

        Assert.Null(report.TotalItemCount);
    }

    // The start of the run is captured when the first item is processed, so advancing the fake
    // clock after that item gives the run an exact, known elapsed time.
    private static async Task DrainAdvancingAfterFirstItem(TestExtractor<int> sut, ManualTimeSource clock, TimeSpan elapsed)
    {
        await using var enumerator = sut.ExtractAsync().GetAsyncEnumerator();
        await enumerator.MoveNextAsync();
        clock.Advance(elapsed);

        while (await enumerator.MoveNextAsync())
        {
        }
    }



    private sealed class ExposedTestExtractor<T> : TestExtractor<T>
        where T : notnull
    {
        public ExposedTestExtractor(IEnumerable<T> items)
            : base(items)
        {
        }

        public ExposedTestExtractor(IEnumerator<T> enumerator)
            : base(enumerator)
        {
        }

        public Report GetProgressReport() => CreateProgressReport();
    }
}
