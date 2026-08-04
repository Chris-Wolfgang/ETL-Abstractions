using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.TestKit;
using Wolfgang.Etl.TestKit.Xunit;

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

        var processed = 0;
        var canceled  = false;

        try
        {
            await foreach (var _ in sut.ExtractAsync(cts.Token).ConfigureAwait(false))
            {
                processed++;

                if (processed == cancelAfter)
                {
#pragma warning disable CA1849, VSTHRD103 // sync Cancel() — CancelAsync is net8+ only
                    cts.Cancel();
#pragma warning restore CA1849, VSTHRD103
                }
            }
        }
        catch (OperationCanceledException)
        {
            canceled = true;
        }

        return new CancellationOutcome(canceled, processed);
    }



    protected override async Task<CancellationOutcome> RunWithPreCancelledTokenAsync(int itemCount)
    {
        var sut   = new DelayingExtractor<int>(Enumerable.Range(0, itemCount).ToArray(), PerItemDelay);
        var token = new CancellationToken(canceled: true);

        var processed = 0;
        var canceled  = false;

        try
        {
            await foreach (var _ in sut.ExtractAsync(token).ConfigureAwait(false))
            {
                processed++;
            }
        }
        catch (OperationCanceledException)
        {
            canceled = true;
        }

        return new CancellationOutcome(canceled, processed);
    }
}
