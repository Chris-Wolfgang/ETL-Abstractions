using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Exercises <see cref="CancellationContractTests{TSut}"/> by driving a <see cref="DelayingExtractor{T}"/>,
/// confirming the base's prompt-cancellation contract holds against a genuinely latent source.
/// </summary>
public sealed class DelayingExtractorCancellationContractTests
    : CancellationContractTests<DelayingExtractor<int>>
{
    private static readonly TimeSpan PerItemDelay = TimeSpan.FromMilliseconds(5);



    protected override async Task<CancellationOutcome> RunAndCancelMidStreamAsync(int itemCount, int cancelAfter)
    {
        var sut = new DelayingExtractor<int>(Enumerable.Range(0, itemCount).ToArray(), PerItemDelay);
        using var cts = new CancellationTokenSource();

        return await DrainAsync
        (
            sut.ExtractAsync(cts.Token),
            processed =>
            {
                if (processed == cancelAfter)
                {
#pragma warning disable CA1849, VSTHRD103 // sync Cancel() — CancelAsync is net8+ only
                    cts.Cancel();
#pragma warning restore CA1849, VSTHRD103
                }
            }
        ).ConfigureAwait(false);
    }



    // An already-cancelled token makes the extractor throw before yielding any item (the contract
    // asserts zero items are processed), so there is no per-item callback.
    protected override async Task<CancellationOutcome> RunWithPreCancelledTokenAsync(int itemCount)
    {
        var sut   = new DelayingExtractor<int>(Enumerable.Range(0, itemCount).ToArray(), PerItemDelay);
        var token = new CancellationToken(canceled: true);

        return await DrainAsync(sut.ExtractAsync(token), onItem: null).ConfigureAwait(false);
    }



    [Fact]
    public async Task RunAndCancelMidStreamAsync_when_cancelAfter_is_never_reached_drains_every_item_without_cancelling()
    {
        var outcome = await RunAndCancelMidStreamAsync(itemCount: 2, cancelAfter: 3);

        Assert.False(outcome.Canceled);
        Assert.Equal(2, outcome.ProcessedItemCount);
    }



    // Counts each item before calling onItem with the running count; the run is cancelled when the
    // enumeration throws OperationCanceledException (or a derived type).
    private static async Task<CancellationOutcome> DrainAsync(IAsyncEnumerable<int> items, Action<int>? onItem)
    {
        var processed = 0;
        var canceled  = false;

        try
        {
            await foreach (var _ in items.ConfigureAwait(false))
            {
                processed++;
                onItem?.Invoke(processed);
            }
        }
        catch (OperationCanceledException)
        {
            canceled = true;
        }

        return new CancellationOutcome(canceled, processed);
    }
}
