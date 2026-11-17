using System.Collections.Generic;
using System.Linq;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

// Kept on the obsolete two-parameter base on purpose: it still ships, and its default
// GetCurrentItemCount (a cast to the Report-progress stage) needs a test that runs it.
public class TestTransformerIdempotencyTests
#pragma warning disable CS0618 // Type or member is obsolete
    : IdempotentTransformerContractTests<TestTransformer<int>, int>
#pragma warning restore CS0618
{
    protected override TestTransformer<int> CreateSut(int itemCount) =>
        new TestTransformer<int>();

    protected override IReadOnlyList<int> CreateExpectedItems() =>
        Enumerable.Range(1, 5).ToList();
}
