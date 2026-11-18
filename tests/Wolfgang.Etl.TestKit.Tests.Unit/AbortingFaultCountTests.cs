using System;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

/// <summary>
/// An aborting fault leaves the same counters whichever way the abort was configured: the
/// fail-fast default and an error policy that returns <see cref="ItemErrorAction.Abort"/> both
/// leave the failing item out of <c>CurrentItemCount</c> and out of <c>CurrentErrorItemCount</c>.
/// </summary>
public class AbortingFaultCountTests
{
    private static readonly int[] Items = { 10, 20, 30, 40, 50 };



    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExtractAsync_when_a_fault_aborts_the_run_counts_only_the_items_before_it(bool abortThroughPolicy)
    {
        var sut = new FaultyExtractor<int>(Items).ThrowAt(2, new InvalidOperationException("boom"));
        if (abortThroughPolicy)
        {
            sut.HandleErrorsWith(_ => ItemErrorAction.Abort);
        }

        await Assert.ThrowsAsync<InvalidOperationException>
        (
            async () => await sut.ExtractAsync().ToListAsync()
        );

        Assert.Equal(2, sut.CurrentItemCount);
        Assert.Equal(0, sut.CurrentErrorItemCount);
    }



    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LoadAsync_when_a_fault_aborts_the_run_counts_only_the_items_before_it(bool abortThroughPolicy)
    {
        var sut = new FaultyLoader<int>(collectItems: true).ThrowAt(2, new InvalidOperationException("boom"));
        if (abortThroughPolicy)
        {
            sut.HandleErrorsWith(_ => ItemErrorAction.Abort);
        }

        await Assert.ThrowsAsync<InvalidOperationException>
        (
            async () => await sut.LoadAsync(Items.ToAsyncEnumerable())
        );

        Assert.Equal(2, sut.CurrentItemCount);
        Assert.Equal(0, sut.CurrentErrorItemCount);
        Assert.Equal(new[] { 10, 20 }, sut.GetCollectedItems());
    }



    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TransformAsync_when_a_fault_aborts_the_run_counts_only_the_items_before_it(bool abortThroughPolicy)
    {
        var sut = new FaultyTransformer<int>().ThrowAt(2, new InvalidOperationException("boom"));
        if (abortThroughPolicy)
        {
            sut.HandleErrorsWith(_ => ItemErrorAction.Abort);
        }

        await Assert.ThrowsAsync<InvalidOperationException>
        (
            async () => await sut.TransformAsync(Items.ToAsyncEnumerable()).ToListAsync()
        );

        Assert.Equal(2, sut.CurrentItemCount);
        Assert.Equal(0, sut.CurrentErrorItemCount);
    }
}
