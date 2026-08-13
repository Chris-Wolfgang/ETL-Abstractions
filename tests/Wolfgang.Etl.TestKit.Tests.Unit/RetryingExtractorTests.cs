using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

public class RetryingExtractorTests
{
    // ------------------------------------------------------------------
    // Constructor — argument validation
    // ------------------------------------------------------------------

    [Fact]
    public void Constructor_when_items_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new RetryingExtractor<int>(null!, failFirstAttempts: 0, maxAttempts: 1)
        );
    }



    [Fact]
    public void Constructor_when_failFirstAttempts_is_negative_throws_ArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>
        (
            () => new RetryingExtractor<int>(new[] { 1 }, failFirstAttempts: -1, maxAttempts: 1)
        );
    }



    [Fact]
    public void Constructor_when_maxAttempts_is_less_than_one_throws_ArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>
        (
            () => new RetryingExtractor<int>(new[] { 1 }, failFirstAttempts: 0, maxAttempts: 0)
        );
    }



    // ------------------------------------------------------------------
    // Retry behaviour
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_when_no_fault_configured_succeeds_on_the_first_attempt()
    {
        var sut = new RetryingExtractor<int>(new[] { 1, 2, 3 }, failFirstAttempts: 0, maxAttempts: 5);

        var items = await sut.ExtractAsync(CancellationToken.None).ToListAsync().ConfigureAwait(false);

        Assert.Equal(new[] { 1, 2, 3 }, items);
        Assert.Equal(1, sut.AttemptCount);
    }



    [Fact]
    public async Task ExtractAsync_when_transient_fault_clears_within_budget_succeeds_after_retries()
    {
        var sut = new RetryingExtractor<int>(new[] { 1, 2, 3 }, failFirstAttempts: 2, maxAttempts: 5);

        var items = await sut.ExtractAsync(CancellationToken.None).ToListAsync().ConfigureAwait(false);

        Assert.Equal(new[] { 1, 2, 3 }, items);
        Assert.Equal(3, sut.AttemptCount);
    }



    [Fact]
    public async Task ExtractAsync_when_fault_never_clears_throws_after_max_attempts()
    {
        var sut = new RetryingExtractor<int>(new[] { 1, 2, 3 }, failFirstAttempts: 5, maxAttempts: 3);

        await Assert.ThrowsAsync<InvalidOperationException>
        (
            async () =>
            {
                await foreach (var _ in sut.ExtractAsync(CancellationToken.None).ConfigureAwait(false))
                {
                }
            }
        ).ConfigureAwait(false);

        Assert.Equal(3, sut.AttemptCount);
    }
}
