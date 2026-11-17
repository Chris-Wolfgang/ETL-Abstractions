using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

/// <summary>
/// <c>CapturedErrors</c> on the Faulty doubles is per run, like the stage's counters: a reused
/// instance reports the errors of its latest run, not of every run it has made.
/// </summary>
public sealed class CapturedErrorsPerRunTests
{
    private static readonly int[] Items = { 1, 2, 3 };



    [Fact]
    public async Task FaultyExtractor_when_run_twice_reports_only_the_latest_run_in_CapturedErrors()
    {
        var sut = new FaultyExtractor<int>(Items)
            .ThrowAt(1, new FormatException("bad row"))
            .SkipErrors();

        await sut.ExtractAsync().ToListAsync();
        await sut.ExtractAsync().ToListAsync();

        Assert.Equal(1, sut.CurrentErrorItemCount);
        Assert.Single(sut.CapturedErrors);
    }



    [Fact]
    public async Task FaultyTransformer_when_run_twice_reports_only_the_latest_run_in_CapturedErrors()
    {
        var sut = new FaultyTransformer<int>()
            .ThrowAt(1, new FormatException("bad row"))
            .SkipErrors();

        await sut.TransformAsync(Items.ToAsyncEnumerable()).ToListAsync();
        await sut.TransformAsync(Items.ToAsyncEnumerable()).ToListAsync();

        Assert.Equal(1, sut.CurrentErrorItemCount);
        Assert.Single(sut.CapturedErrors);
    }



    [Fact]
    public async Task FaultyLoader_when_run_twice_reports_only_the_latest_run_in_CapturedErrors()
    {
        var sut = new FaultyLoader<int>(collectItems: true)
            .ThrowAt(1, new FormatException("bad row"))
            .SkipErrors();

        await sut.LoadAsync(Items.ToAsyncEnumerable());
        await sut.LoadAsync(Items.ToAsyncEnumerable());

        Assert.Equal(1, sut.CurrentErrorItemCount);
        Assert.Single(sut.CapturedErrors);
    }
}
