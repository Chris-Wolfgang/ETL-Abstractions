using System.Collections.Generic;
using System.Linq;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

public class TestExtractorWithCancellationAsyncContractTests
    : ExtractWithCancellationAsyncContractTests<TestExtractor<int>, int>
{
    protected override TestExtractor<int> CreateSut(int itemCount) =>
        new TestExtractor<int>(Enumerable.Range(1, itemCount).ToList());

    protected override IReadOnlyList<int> CreateExpectedItems() =>
        Enumerable.Range(1, 5).ToList();
}
