using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Verifies that <see cref="TestExtractor{T}"/>'s enumeration hot path satisfies the
/// <see cref="AllocationBudgetContractTests{TSut}"/> contract, and serves as the reference
/// wiring for a custom extractor (note the <c>[Collection("Allocation")]</c> serialization).
/// </summary>
[Collection("Allocation")]
public sealed class TestExtractorAllocationBudgetContractTests
    : AllocationBudgetContractTests<TestExtractor<int>>
{
    // The doubles' hot path is allocation-free; use the kit's proven-stable per-item budget —
    // the process-wide counter picks up background-allocation noise on shared CI runners, so a
    // strict 0 flakes (see the internal AllocationRegressionTests, which uses the same value).
    protected override double MaxBytesPerItem => 8.0;

    protected override TestExtractor<int> CreateSut(int itemCount) =>
        new TestExtractor<int>(new int[itemCount]);

    protected override async Task ExerciseHotPathAsync(CancellationToken cancellationToken)
    {
        await foreach (var _ in Sut.ExtractAsync(cancellationToken).ConfigureAwait(false))
        {
        }
    }
}
