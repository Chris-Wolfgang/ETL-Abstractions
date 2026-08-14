using System;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Verifies that <see cref="FaultyExtractor{T}"/> satisfies the
/// <see cref="ErrorHandlingContractTests{TSut}"/> contract, and serves as a reference example
/// of wiring the error-hook contract for a custom extractor.
/// </summary>
public class FaultyExtractorErrorHandlingContractTests
    : ErrorHandlingContractTests<FaultyExtractor<int>>
{
    /// <inheritdoc/>
    protected override async Task<ErrorHandlingOutcome> RunSingleFaultScenarioAsync(ItemErrorAction policy)
    {
        // Five items with exactly one failing item (index 2) and two survivors after it.
        var sut = new FaultyExtractor<int>(Enumerable.Range(1, 5).ToList())
            .ThrowAt(2, new FormatException("bad row"))
            .HandleErrorsWith(_ => policy);

        var aborted = false;

        try
        {
            await foreach (var _ in sut.ExtractAsync().ConfigureAwait(false))
            {
            }
        }
        catch (FormatException)
        {
            aborted = true;
        }

        return new ErrorHandlingOutcome
        (
            aborted,
            sut.CurrentItemCount,
            sut.CurrentErrorItemCount,
            sut.CurrentSkippedItemCount
        );
    }
}
