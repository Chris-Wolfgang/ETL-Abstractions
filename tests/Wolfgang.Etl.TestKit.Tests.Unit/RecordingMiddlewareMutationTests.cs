using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

/// <summary>
/// Mutation-hardening tests (#346) for <see cref="RecordingMiddleware{T}"/>: they pin the
/// cancellation check at the top of <see cref="RecordingMiddleware{T}.OnItemAsync"/>.
/// </summary>
public class RecordingMiddlewareMutationTests
{
    [Fact]
    public async Task OnItemAsync_when_token_is_already_cancelled_throws_and_records_nothing()
    {
        // The guard runs before the item is recorded, so a cancelled token must throw and leave
        // Observed empty. Dropping the guard would record the item and return a result instead.
        var middleware = new RecordingMiddleware<int>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>
        (
            async () => await middleware.OnItemAsync(42, cts.Token)
        );

        Assert.Empty(middleware.Observed);
    }
}
