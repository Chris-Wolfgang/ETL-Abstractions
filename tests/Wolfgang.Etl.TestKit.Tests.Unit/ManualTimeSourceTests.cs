using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

public class ManualTimeSourceTests
{
    [Fact]
    public void Advance_when_delta_is_negative_throws_ArgumentOutOfRangeException()
    {
        var clock = new ManualTimeSource();

        Assert.Throws<ArgumentOutOfRangeException>
        (
            () => clock.Advance(TimeSpan.FromSeconds(-1))
        );
    }



    [Fact]
    public void WithTimeSource_when_timeSource_is_null_throws_ArgumentNullException()
    {
        var extractor = new TestExtractor<int>(new[] { 1 });

        Assert.Throws<ArgumentNullException>
        (
            () => extractor.WithTimeSource(null!)
        );
    }



    [Fact]
    public async Task Report_Elapsed_reflects_the_advanced_time_deterministically()
    {
        var clock = new ManualTimeSource();
        var sut   = new ExposedExtractor<int>(Enumerable.Range(0, 50).ToArray());
        sut.WithTimeSource(clock);

        await sut.ExtractAsync(CancellationToken.None).ToListAsync().ConfigureAwait(false);
        clock.Advance(TimeSpan.FromSeconds(10));

        var report = sut.GetProgressReport();

        Assert.Equal(TimeSpan.FromSeconds(10), report.Elapsed);
    }



    [Fact]
    public async Task Report_StartedAt_is_reported_once_a_run_has_begun()
    {
        var clock = new ManualTimeSource();
        var sut   = new ExposedExtractor<int>(new[] { 1, 2, 3 });
        sut.WithTimeSource(clock);

        await sut.ExtractAsync(CancellationToken.None).ToListAsync().ConfigureAwait(false);

        var report = sut.GetProgressReport();

        Assert.NotNull(report.StartedAt);
    }



    [Fact]
    public async Task Report_ItemsPerSecond_computes_deterministically()
    {
        var clock = new ManualTimeSource();
        var sut   = new ExposedExtractor<int>(Enumerable.Range(0, 50).ToArray());
        sut.WithTimeSource(clock);

        await sut.ExtractAsync(CancellationToken.None).ToListAsync().ConfigureAwait(false);
        clock.Advance(TimeSpan.FromSeconds(10));

        var report = sut.GetProgressReport();

        // 50 items over 10 deterministic seconds.
        Assert.Equal(5.0, report.ItemsPerSecond);
    }



    private sealed class ExposedExtractor<T> : TestExtractor<T>
        where T : notnull
    {
        public ExposedExtractor(IEnumerable<T> items)
            : base(items)
        {
        }



        public Report GetProgressReport() => CreateProgressReport();
    }
}
