using System.Collections.Generic;
using System.Linq;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

// Kept on the obsolete two-parameter base on purpose: it still ships, and its default
// GetCurrentItemCount (a cast to the Report-progress stage) needs a test that runs it.
public class TestLoaderIdempotencyTests
#pragma warning disable CS0618 // Type or member is obsolete
    : IdempotentLoaderContractTests<TestLoader<int>, int>
#pragma warning restore CS0618
{
    protected override TestLoader<int> CreateSut(int itemCount) =>
        new TestLoader<int>(collectItems: true);

    protected override IReadOnlyList<int> CreateSourceItems() =>
        Enumerable.Range(1, 5).ToList();

    protected override IReadOnlyList<int>? TryGetLoadedItems(TestLoader<int> sut) =>
        sut?.GetCollectedItems();
}
