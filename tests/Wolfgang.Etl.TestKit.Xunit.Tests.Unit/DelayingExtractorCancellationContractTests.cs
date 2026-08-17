using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Exercises <see cref="CancellationContractTests{TSut}"/> by driving a <see cref="DelayingExtractor{T}"/>,
/// confirming the base's prompt-cancellation contract holds against a genuinely latent source.
/// </summary>
public sealed class DelayingExtractorCancellationContractTests
    : CancellationContractTests<DelayingExtractor<int>>
{
    private static readonly TimeSpan PerItemDelay = TimeSpan.FromMilliseconds(5);



    // The await-foreach normal-completion path is unreachable: this run always cancels mid-stream
    // (the contract asserts the stream never drains), so the loop only ever exits via the thrown
    // OperationCanceledException — never by the enumerator completing.
    [ExcludeFromCodeCoverage]
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



    // The loop body and normal-completion path are unreachable: an already-cancelled token makes the
    // extractor throw before yielding any item (the contract asserts zero items are processed), so
    // execution goes straight to the catch.
    [ExcludeFromCodeCoverage]
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
