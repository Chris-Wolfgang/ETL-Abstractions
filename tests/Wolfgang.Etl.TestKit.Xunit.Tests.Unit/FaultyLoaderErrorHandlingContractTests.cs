using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Verifies that <see cref="FaultyLoader{T}"/> satisfies the
/// <see cref="ErrorHandlingContractTests{TSut}"/> contract, and serves as a reference example
/// of wiring the error-hook contract for a custom loader.
/// </summary>
public class FaultyLoaderErrorHandlingContractTests
    : ErrorHandlingContractTests<FaultyLoader<int>>
{
    /// <inheritdoc/>
    protected override async Task<ErrorHandlingOutcome> RunSingleFaultScenarioAsync(ItemErrorAction policy)
    {
        var sut = new FaultyLoader<int>(collectItems: false)
            .ThrowAt(2, new FormatException("bad row"))
            .HandleErrorsWith(_ => policy);

        var aborted = false;

        try
        {
            await sut.LoadAsync(Enumerable.Range(1, 5).ToAsyncEnumerable()).ConfigureAwait(false);
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
