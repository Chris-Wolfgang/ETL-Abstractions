using System.Collections.Generic;
using System.Linq;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Verifies that <see cref="TestExtractor{T}"/> satisfies the full
/// <see cref="ExtractorBaseContractTests{TSut, TItem, TProgress}"/> contract.
/// </summary>
/// <remarks>
/// This class also serves as a reference example of how to wire up
/// <see cref="ExtractorBaseContractTests{TSut, TItem, TProgress}"/> for your own extractor.
/// </remarks>
public class TestExtractorContractTests
    : ExtractorBaseContractTests<TestExtractor<int>, int, Report>
{
    /// <inheritdoc/>
    protected override TestExtractor<int> CreateSut(int itemCount) =>
        new TestExtractor<int>(Enumerable.Range(1, itemCount).ToList());

    /// <inheritdoc/>
    protected override IReadOnlyList<int> CreateExpectedItems() =>
        Enumerable.Range(1, 5).ToList();

    /// <inheritdoc/>
    protected override TestExtractor<int> CreateSutOverSource(IEnumerable<int> source) =>
        new TestExtractor<int>(source);
}
