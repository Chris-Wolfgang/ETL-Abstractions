using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

/// <summary>
/// Tests for the Abstractions 0.18.0 error-hook support on <see cref="FaultyTransformer{T}"/>.
/// </summary>
public class FaultyTransformerErrorHookTests
{
    private static IAsyncEnumerable<int> SourceAsync() =>
        Enumerable.Range(1, 5).ToAsyncEnumerable();

    [Fact]
    public async Task TransformAsync_when_no_error_policy_configured_propagates_the_fault()
    {
        var boom = new IOException("boom");
        var sut = new FaultyTransformer<int>().ThrowAt(2, boom);

        var thrown = await Assert.ThrowsAsync<IOException>
        (
            async () => await sut.TransformAsync(SourceAsync()).ToListAsync()
        );

        Assert.Same(boom, thrown);
        Assert.Equal(0, sut.CurrentErrorItemCount);
    }

    [Fact]
    public async Task TransformAsync_when_SkipErrors_discards_the_failed_item_and_continues()
    {
        var sut = new FaultyTransformer<int>()
            .ThrowAt(2, new FormatException("bad row"))
            .SkipErrors();

        var results = await sut.TransformAsync(SourceAsync()).ToListAsync();

        Assert.Equal(new[] { 1, 2, 4, 5 }, results);
    }

    [Fact]
    public async Task TransformAsync_when_SkipErrors_counts_the_failure_as_an_error_not_a_skip()
    {
        var sut = new FaultyTransformer<int>()
            .ThrowAt(2, new FormatException("bad row"))
            .SkipErrors();

        await sut.TransformAsync(SourceAsync()).ToListAsync();

        Assert.Equal(1, sut.CurrentErrorItemCount);
        Assert.Equal(0, sut.CurrentSkippedItemCount);
        Assert.Equal(4, sut.CurrentItemCount);
    }

    [Fact]
    public async Task TransformAsync_when_SkipErrors_records_the_failure_in_CapturedErrors()
    {
        var boom = new FormatException("bad row");
        var sut = new FaultyTransformer<int>()
            .ThrowAt(2, boom)
            .SkipErrors();

        await sut.TransformAsync(SourceAsync()).ToListAsync();

        var captured = Assert.Single(sut.CapturedErrors);
        Assert.Equal(3, captured.ItemNumber);
        Assert.Same(boom, captured.Exception);
    }

    [Fact]
    public async Task TransformAsync_when_HandleErrorsWith_returns_Abort_propagates_the_fault()
    {
        var boom = new IOException("fatal");
        var sut = new FaultyTransformer<int>()
            .ThrowAt(2, boom)
            .HandleErrorsWith(_ => ItemErrorAction.Abort);

        var thrown = await Assert.ThrowsAsync<IOException>
        (
            async () => await sut.TransformAsync(SourceAsync()).ToListAsync()
        );

        Assert.Same(boom, thrown);
    }

    [Fact]
    public void HandleErrorsWith_when_policy_is_null_throws_ArgumentNullException()
    {
        var sut = new FaultyTransformer<int>();

        Assert.Throws<ArgumentNullException>(() => sut.HandleErrorsWith(null!));
    }
}
