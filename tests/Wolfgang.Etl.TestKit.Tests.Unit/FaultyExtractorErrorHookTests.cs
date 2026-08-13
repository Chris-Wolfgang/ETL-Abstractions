using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

/// <summary>
/// Tests for the Abstractions 0.18.0 error-hook support on <see cref="FaultyExtractor{T}"/>
/// (<c>SkipErrors</c>, <c>HandleErrorsWith</c>, <c>CapturedErrors</c>, and the
/// <c>OnItemError</c>/<c>HandleItemError</c> routing).
/// </summary>
public class FaultyExtractorErrorHookTests
{
    private static List<int> Items() => new List<int> { 1, 2, 3, 4, 5 };

    [Fact]
    public async Task ExtractAsync_when_no_error_policy_configured_propagates_the_fault()
    {
        var boom = new IOException("boom");
        var sut = new FaultyExtractor<int>(Items()).ThrowAt(2, boom);

        var thrown = await Assert.ThrowsAsync<IOException>
        (
            async () => await sut.ExtractAsync().ToListAsync()
        );

        Assert.Same(boom, thrown);
        Assert.Equal(0, sut.CurrentErrorItemCount);
    }

    [Fact]
    public async Task ExtractAsync_when_SkipErrors_discards_the_failed_item_and_continues()
    {
        var sut = new FaultyExtractor<int>(Items())
            .ThrowAt(2, new FormatException("bad row"))
            .SkipErrors();

        var results = await sut.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 1, 2, 4, 5 }, results);
    }

    [Fact]
    public async Task ExtractAsync_when_SkipErrors_counts_the_failure_as_an_error()
    {
        var sut = new FaultyExtractor<int>(Items())
            .ThrowAt(2, new FormatException("bad row"))
            .SkipErrors();

        await sut.ExtractAsync().ToListAsync();

        Assert.Equal(1, sut.CurrentErrorItemCount);
    }

    [Fact]
    public async Task ExtractAsync_when_SkipErrors_does_not_count_the_failure_as_an_intentional_skip()
    {
        var sut = new FaultyExtractor<int>(Items())
            .ThrowAt(2, new FormatException("bad row"))
            .SkipErrors();

        await sut.ExtractAsync().ToListAsync();

        Assert.Equal(0, sut.CurrentSkippedItemCount);
        Assert.Equal(4, sut.CurrentItemCount);
    }

    [Fact]
    public async Task ExtractAsync_when_SkipErrors_records_the_failure_in_CapturedErrors()
    {
        var boom = new FormatException("bad row");
        var sut = new FaultyExtractor<int>(Items())
            .ThrowAt(2, boom)
            .SkipErrors();

        await sut.ExtractAsync().ToListAsync();

        var captured = Assert.Single(sut.CapturedErrors);
        Assert.Equal(3, captured.ItemNumber);
        Assert.Same(boom, captured.Exception);
    }

    [Fact]
    public async Task ExtractAsync_when_HandleErrorsWith_returns_Abort_propagates_the_fault()
    {
        var boom = new IOException("fatal");
        var sut = new FaultyExtractor<int>(Items())
            .ThrowAt(2, boom)
            .HandleErrorsWith(_ => ItemErrorAction.Abort);

        var thrown = await Assert.ThrowsAsync<IOException>
        (
            async () => await sut.ExtractAsync().ToListAsync()
        );

        Assert.Same(boom, thrown);
        Assert.Equal(0, sut.CurrentErrorItemCount);
    }

    [Fact]
    public async Task ExtractAsync_when_HandleErrorsWith_decides_per_exception_type()
    {
        var sut = new FaultyExtractor<int>(Items())
            .ThrowAt(1, new FormatException("skip me"))
            .ThrowAt(3, new IOException("abort me"))
            .HandleErrorsWith(ctx => ctx.Exception is FormatException
                ? ItemErrorAction.Skip
                : ItemErrorAction.Abort);

        var results = new List<int>();

        await Assert.ThrowsAsync<IOException>
        (
            async () =>
            {
                await foreach (var item in sut.ExtractAsync())
                {
                    results.Add(item);
                }
            }
        );

        // Item at index 1 skipped (FormatException), then aborts at index 3 (IOException).
        Assert.Equal(new[] { 1, 3 }, results);
        Assert.Equal(1, sut.CurrentErrorItemCount);
    }

    [Fact]
    public void HandleErrorsWith_when_policy_is_null_throws_ArgumentNullException()
    {
        var sut = new FaultyExtractor<int>(Items());

        Assert.Throws<ArgumentNullException>(() => sut.HandleErrorsWith(null!));
    }

    [Fact]
    public void CapturedErrors_when_no_fault_fired_is_empty()
    {
        var sut = new FaultyExtractor<int>(Items()).SkipErrors();

        Assert.Empty(sut.CapturedErrors);
    }
}
