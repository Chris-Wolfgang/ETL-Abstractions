using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

public class RetryingExtractorBranchesTests
{
    [Fact]
    public async Task ExtractAsync_honours_SkipItemCount_and_MaximumItemCount()
    {
        var sut = new RetryingExtractor<int>(new[] { 1, 2, 3, 4, 5 }, failFirstAttempts: 0, maxAttempts: 3)
        {
            SkipItemCount    = 1,
            MaximumItemCount = 2,
        };

        var items = await sut.ExtractAsync(CancellationToken.None).ToListAsync().ConfigureAwait(false);

        Assert.Equal(new[] { 2, 3 }, items);
    }



    [Fact]
    public async Task ExtractAsync_with_already_cancelled_token_throws_and_does_not_retry()
    {
        var sut = new RetryingExtractor<int>(Enumerable.Range(0, 100).ToArray(), failFirstAttempts: 0, maxAttempts: 5);
        var token = new CancellationToken(canceled: true);

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            async () =>
            {
                await foreach (var _ in sut.ExtractAsync(token).ConfigureAwait(false))
                {
                }
            }
        ).ConfigureAwait(false);

        // Cancellation is not a retryable fault — only the single attempt ran.
        Assert.Equal(1, sut.AttemptCount);
    }
}
