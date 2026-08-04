using System.Collections.Generic;
using System.Linq;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

public class TestTransformerWithProgressAndCancellationAsyncContractTests
    : TransformWithProgressAndCancellationAsyncContractTests<TestTransformer<int>, int, Report>
{
    protected override TestTransformer<int> CreateSut(int itemCount) =>
        new TestTransformer<int>();

    protected override IReadOnlyList<int> CreateExpectedItems() =>
        Enumerable.Range(1, 5).ToList();
}
