using System;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;
using Wolfgang.Etl.TestKit;
using Xunit;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.EtlPipelineTests;

/// <summary>
/// <see cref="EtlPipelineProgress.LoadedItemCount"/> is the loader's own count of loaded records,
/// so it agrees with the loader's <c>CurrentItemCount</c> when the loader does not load every
/// record it is given.
/// </summary>
public sealed class LoadedItemCountTests
{
    private static readonly int[] TenItems = Enumerable.Range(1, 10).ToArray();



    [Fact]
    public async Task RunAsync_when_the_loader_skips_items_reports_only_the_loaded_ones()
    {
        var loader = new TestLoader<int>(collectItems: true, new LoaderOptions { SkipItemCount = 4 });

        var final = await RunAsync(loader);

        Assert.Equal(10, final.ExtractedItemCount);
        Assert.Equal(6, final.LoadedItemCount);
        Assert.Equal(loader.CurrentItemCount, final.LoadedItemCount);
    }



    [Fact]
    public async Task RunAsync_when_the_loader_stops_at_MaximumItemCount_reports_only_the_loaded_ones()
    {
        var loader = new TestLoader<int>(collectItems: true, new LoaderOptions { MaximumItemCount = 3 });

        var final = await RunAsync(loader);

        Assert.Equal(3, final.LoadedItemCount);
        Assert.Equal(loader.CurrentItemCount, final.LoadedItemCount);
    }



    [Fact]
    public async Task RunAsync_when_the_loader_discards_a_faulted_item_does_not_count_it_as_loaded()
    {
        var loader = new FaultyLoader<int>(collectItems: true)
            .ThrowAt(1, new FormatException("bad row"))
            .SkipErrors();

        var final = await RunAsync(loader);

        Assert.Equal(10, final.ExtractedItemCount);
        Assert.Equal(9, final.LoadedItemCount);
        Assert.Equal(1, final.ErrorItemCount);
    }



    private static async Task<EtlPipelineProgress> RunAsync(LoaderBase<int, Report> loader)
    {
        EtlPipelineProgress? final = null;

        await EtlPipeline
            .Create()
            .From(new TestExtractor<int>(TenItems))
            .To(loader)
            .RunAsync(new SynchronousProgress<EtlPipelineProgress>(p => final = p));

        return final!;
    }
}
