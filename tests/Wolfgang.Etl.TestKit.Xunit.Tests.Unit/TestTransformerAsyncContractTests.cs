using System.Collections.Generic;
using System.Linq;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

public class TestTransformerAsyncContractTests
    : TransformAsyncContractTests<TestTransformer<int>, int>
{
    protected override TestTransformer<int> CreateSut(int itemCount) =>
        new TestTransformer<int>();

    protected override IReadOnlyList<int> CreateExpectedItems() =>
        Enumerable.Range(1, 5).ToList();
}
