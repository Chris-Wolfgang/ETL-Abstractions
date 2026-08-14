using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Exercises <see cref="RetryContractTests{TSut}"/> by driving a <see cref="RetryingExtractor{T}"/>,
/// confirming the retry contract holds against the reference retry double.
/// </summary>
public sealed class RetryingExtractorRetryContractTests
    : RetryContractTests<RetryingExtractor<int>>
{
    protected override Task<RetryOutcome> RunWithTransientFaultAsync(int failFirstAttempts, int maxAttempts) =>
        DriveAsync(new RetryingExtractor<int>(Enumerable.Range(0, 5).ToArray(), failFirstAttempts, maxAttempts));



    protected override Task<RetryOutcome> RunWithPermanentFaultAsync(int maxAttempts) =>
        DriveAsync(new RetryingExtractor<int>(Enumerable.Range(0, 5).ToArray(), failFirstAttempts: maxAttempts, maxAttempts: maxAttempts));



    private static async Task<RetryOutcome> DriveAsync(RetryingExtractor<int> sut)
    {
        var itemCount = 0;
        var succeeded = false;

        try
        {
            await foreach (var _ in sut.ExtractAsync(CancellationToken.None).ConfigureAwait(false))
            {
                itemCount++;
            }

            succeeded = true;
        }
        catch (InvalidOperationException)
        {
            succeeded = false;
        }

        return new RetryOutcome(succeeded, sut.AttemptCount, itemCount);
    }
}
