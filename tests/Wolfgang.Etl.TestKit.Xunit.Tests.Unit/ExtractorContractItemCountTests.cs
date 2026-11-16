using System.Collections.Generic;
using System.Linq;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Runs the extractor contract with the smallest allowed expected sequence (three items), proving
/// the contract does not depend on a particular item count.
/// </summary>
public class ThreeItemExtractorContractTests
    : ExtractorBaseContractTests<TestExtractor<int>, int, Report>
{
    /// <inheritdoc/>
    protected override TestExtractor<int> CreateSut(int itemCount, int maximumItemCount, int skipItemCount, int reportingInterval) =>
        new TestExtractor<int>(Enumerable.Range(1, itemCount).ToList(), new ExtractorOptions { MaximumItemCount = maximumItemCount, SkipItemCount = skipItemCount, ReportingInterval = reportingInterval });

    /// <inheritdoc/>
    protected override IReadOnlyList<int> CreateExpectedItems() =>
        Enumerable.Range(1, 3).ToList();

    /// <inheritdoc/>
    protected override TestExtractor<int> CreateSutOverSource(IEnumerable<int> source, int maximumItemCount = int.MaxValue) =>
        new TestExtractor<int>(source, new ExtractorOptions { MaximumItemCount = maximumItemCount });
}



/// <summary>
/// Runs the extractor contract with a longer expected sequence (seven items).
/// </summary>
public class SevenItemExtractorContractTests
    : ExtractorBaseContractTests<TestExtractor<int>, int, Report>
{
    /// <inheritdoc/>
    protected override TestExtractor<int> CreateSut(int itemCount, int maximumItemCount, int skipItemCount, int reportingInterval) =>
        new TestExtractor<int>(Enumerable.Range(1, itemCount).ToList(), new ExtractorOptions { MaximumItemCount = maximumItemCount, SkipItemCount = skipItemCount, ReportingInterval = reportingInterval });

    /// <inheritdoc/>
    protected override IReadOnlyList<int> CreateExpectedItems() =>
        Enumerable.Range(1, 7).ToList();

    /// <inheritdoc/>
    protected override TestExtractor<int> CreateSutOverSource(IEnumerable<int> source, int maximumItemCount = int.MaxValue) =>
        new TestExtractor<int>(source, new ExtractorOptions { MaximumItemCount = maximumItemCount });
}
