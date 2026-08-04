using System.Collections.Generic;
using System.Linq;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

public class TestLoaderWithCancellationAsyncContractTests
    : LoadWithCancellationAsyncContractTests<TestLoader<int>, int>
{
    protected override TestLoader<int> CreateSut(int itemCount) =>
        new TestLoader<int>(collectItems: false);

    protected override IReadOnlyList<int> CreateSourceItems() =>
        Enumerable.Range(1, 5).ToList();
}
