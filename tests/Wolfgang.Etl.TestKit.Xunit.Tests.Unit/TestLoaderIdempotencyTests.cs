using System.Collections.Generic;
using System.Linq;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

public class TestLoaderIdempotencyTests
    : IdempotentLoaderContractTests<TestLoader<int>, int>
{
    protected override TestLoader<int> CreateSut(int itemCount) =>
        new TestLoader<int>(collectItems: true);

    protected override IReadOnlyList<int> CreateSourceItems() =>
        Enumerable.Range(1, 5).ToList();

    protected override IReadOnlyList<int>? TryGetLoadedItems(TestLoader<int> sut) =>
        sut?.GetCollectedItems();
}
