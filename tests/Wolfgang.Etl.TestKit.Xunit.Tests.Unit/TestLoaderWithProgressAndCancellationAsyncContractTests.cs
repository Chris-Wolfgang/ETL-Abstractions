using System.Collections.Generic;
using System.Linq;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

public class TestLoaderWithProgressAndCancellationAsyncContractTests
    : LoadWithProgressAndCancellationAsyncContractTests<TestLoader<int>, int, Report>
{
    protected override TestLoader<int> CreateSut(int itemCount) =>
        new TestLoader<int>(collectItems: false);

    protected override IReadOnlyList<int> CreateSourceItems() =>
        Enumerable.Range(1, 5).ToList();
}
