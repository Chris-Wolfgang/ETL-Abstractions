using System.Collections.Generic;
using System.Linq;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Verifies that <see cref="TestTransformer{T}"/> satisfies the full
/// <see cref="TransformerBaseContractTests{TSut, TItem, TProgress}"/> contract.
/// </summary>
/// <remarks>
/// This class also serves as a reference example of how to wire up
/// <see cref="TransformerBaseContractTests{TSut, TItem, TProgress}"/> for your own transformer.
/// </remarks>
public class TestTransformerContractTests
    : TransformerBaseContractTests<TestTransformer<int>, int, Report>
{
    /// <inheritdoc/>
    protected override TestTransformer<int> CreateSut(int itemCount) =>
        new TestTransformer<int>();

    /// <inheritdoc/>
    protected override IReadOnlyList<int> CreateExpectedItems() =>
        Enumerable.Range(1, 5).ToList();

    /// <inheritdoc/>
    protected override TestTransformer<int> CreateSutWithTimer(IProgressTimer timer) =>
        new TestTransformerWithTimer(timer);



    // Exposes the protected timer constructor of TestTransformer<T> for contract testing.
    private sealed class TestTransformerWithTimer : TestTransformer<int>
    {
        public TestTransformerWithTimer(IProgressTimer timer) : base(timer) { }
    }
}
