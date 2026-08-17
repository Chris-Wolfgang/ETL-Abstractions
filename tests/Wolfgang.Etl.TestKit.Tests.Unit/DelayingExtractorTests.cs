using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            () => new DelayingExtractor<int>(new[] { 1 }, null!)
        );
    }



    [Fact]
    public void Constructor_when_delay_is_negative_throws_ArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>
        (
            () => new DelayingExtractor<int>(new[] { 1 }, TimeSpan.FromMilliseconds(-1))
        );

        Assert.Contains("must not be negative", ex.Message);
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



    // ------------------------------------------------------------------
    // Mutation-hardening (#346)
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_with_no_items_and_a_pre_cancelled_token_still_throws()
    {
        // The pre-loop ThrowIfCancellationRequested is the only cancellation check that
        // runs when the source is empty (the in-loop check never executes), so an empty
        // source with an already-cancelled token must still throw.
        var sut = new DelayingExtractor<int>(Array.Empty<int>(), TimeSpan.Zero);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            async () =>
            {
                await foreach (var _ in sut.ExtractAsync(cts.Token))
                {
                }
            }
        );
    }



    [Fact]
    public async Task ExtractAsync_advances_the_delay_index_across_skipped_items()
    {
        // The zero-based index handed to the delay selector must keep advancing through
        // skipped items, so a yielded item is delayed for its true position — not 0.
        var seen = new List<int>();
        var sut = new DelayingExtractor<int>
        (
            new[] { 1, 2, 3, 4 },
            i => { seen.Add(i); return TimeSpan.Zero; }
        )
        { SkipItemCount = 2 };

        await foreach (var _ in sut.ExtractAsync())
        {
        }

        // Items 0 and 1 are skipped (the selector is only consulted for yielded items);
        // the two yielded items are queried at their real indices 2 and 3.
        Assert.Equal(new[] { 2, 3 }, seen);
    }



    [Fact]
    public async Task ExtractAsync_advances_the_delay_index_for_each_yielded_item()
    {
        var seen = new List<int>();
        var sut = new DelayingExtractor<int>
        (
            new[] { 10, 20, 30 },
            i => { seen.Add(i); return TimeSpan.Zero; }
        );

        await foreach (var _ in sut.ExtractAsync())
        {
        }

        Assert.Equal(new[] { 0, 1, 2 }, seen);
    }



    [Fact]
    public async Task ExtractAsync_actually_waits_the_delay_before_yielding()
    {
        // Dropping the Task.Delay would make extraction effectively instantaneous.
        var sut = new DelayingExtractor<int>(new[] { 1, 2, 3 }, TimeSpan.FromMilliseconds(30));

        var sw = Stopwatch.StartNew();
        await foreach (var _ in sut.ExtractAsync())
        {
        }
        sw.Stop();

        // 3 items x 30 ms = ~90 ms nominal; a 45 ms floor is far above the no-delay case
        // (~0 ms) yet loose enough to stay reliable on shared CI runners.
        Assert.True
        (
            sw.ElapsedMilliseconds >= 45,
            $"Expected the extractor to actually delay; elapsed only {sw.ElapsedMilliseconds} ms."
        );
    }
}
