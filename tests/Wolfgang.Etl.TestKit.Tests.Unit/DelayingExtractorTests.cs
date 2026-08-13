using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

public class DelayingExtractorTests
{
    // ------------------------------------------------------------------
    // Constructor — argument validation
    // ------------------------------------------------------------------

    [Fact]
    public void Constructor_when_items_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new DelayingExtractor<int>(null!, TimeSpan.Zero)
        );
    }



    [Fact]
    public void Constructor_when_delaySelector_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new DelayingExtractor<int>(new[] { 1 }, (Func<int, TimeSpan>)null!)
        );
    }



    [Fact]
    public void Constructor_when_delay_is_negative_throws_ArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>
        (
            () => new DelayingExtractor<int>(new[] { 1 }, TimeSpan.FromMilliseconds(-1))
        );
    }



    // ------------------------------------------------------------------
    // Extraction behaviour
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_yields_all_items_in_order()
    {
        var sut = new DelayingExtractor<int>(new[] { 1, 2, 3 }, TimeSpan.Zero);

        var actual = await sut.ExtractAsync(CancellationToken.None).ToListAsync();

        Assert.Equal(new[] { 1, 2, 3 }, actual);
    }



    [Fact]
    public async Task ExtractAsync_when_MaximumItemCount_is_set_stops_at_the_limit()
    {
        var sut = new DelayingExtractor<int>(new[] { 1, 2, 3, 4, 5 }, TimeSpan.Zero) { MaximumItemCount = 2 };

        var actual = await sut.ExtractAsync(CancellationToken.None).ToListAsync();

        Assert.Equal(new[] { 1, 2 }, actual);
    }



    [Fact]
    public async Task ExtractAsync_when_SkipItemCount_is_set_skips_the_first_N_items()
    {
        var sut = new DelayingExtractor<int>(new[] { 1, 2, 3, 4 }, TimeSpan.Zero) { SkipItemCount = 2 };

        var actual = await sut.ExtractAsync(CancellationToken.None).ToListAsync();

        Assert.Equal(new[] { 3, 4 }, actual);
    }



    [Fact]
    public async Task ExtractAsync_per_index_delay_selector_yields_all_items()
    {
        var sut = new DelayingExtractor<int>(new[] { 10, 20, 30 }, i => TimeSpan.FromMilliseconds(i));

        var actual = await sut.ExtractAsync(CancellationToken.None).ToListAsync();

        Assert.Equal(new[] { 10, 20, 30 }, actual);
    }



    // ------------------------------------------------------------------
    // Cancellation
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_with_already_cancelled_token_throws_and_yields_nothing()
    {
        var sut   = new DelayingExtractor<int>(Enumerable.Range(0, 100).ToArray(), TimeSpan.FromMilliseconds(5));
        var token = new CancellationToken(canceled: true);

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            async () =>
            {
                await foreach (var _ in sut.ExtractAsync(token))
                {
                }
            }
        );

        Assert.Equal(0, sut.CurrentItemCount);
    }



    [Fact]
    public async Task ExtractAsync_cancelling_mid_stream_stops_promptly()
    {
        var sut = new DelayingExtractor<int>(Enumerable.Range(0, 100).ToArray(), TimeSpan.FromMilliseconds(5));
        using var cts = new CancellationTokenSource();

        var processed = 0;

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            async () =>
            {
                await foreach (var _ in sut.ExtractAsync(cts.Token))
                {
                    processed++;
                    if (processed == 3)
                    {
#pragma warning disable CA1849, VSTHRD103 // sync Cancel() — CancelAsync is net8+ only
                        cts.Cancel();
#pragma warning restore CA1849, VSTHRD103
                    }
                }
            }
        );

        // Stopped near the cancel point rather than draining all 100.
        Assert.True(processed <= 4, $"Expected a prompt stop (≤ 4), but processed {processed}.");
    }
}
