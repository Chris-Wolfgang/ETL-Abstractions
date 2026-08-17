using System.Collections.Generic;
using System.Linq;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Verifies that <see cref="TestLoader{T}"/> satisfies the full
/// <see cref="LoaderBaseContractTests{TSut, TItem, TProgress}"/> contract.
/// </summary>
/// <remarks>
/// This class also serves as a reference example of how to wire up
/// <see cref="LoaderBaseContractTests{TSut, TItem, TProgress}"/> for your own loader.
/// </remarks>
public class TestLoaderContractTests
    : LoaderBaseContractTests<TestLoader<int>, int, Report>
{
    /// <inheritdoc/>
    protected override TestLoader<int> CreateSut(int itemCount) =>
        new TestLoader<int>(collectItems: false);

    /// <inheritdoc/>
    protected override IReadOnlyList<int> CreateSourceItems() =>
        Enumerable.Range(1, 5).ToList();
}
