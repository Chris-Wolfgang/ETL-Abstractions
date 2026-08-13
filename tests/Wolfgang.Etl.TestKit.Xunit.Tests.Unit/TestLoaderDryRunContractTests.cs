using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Verifies that <see cref="TestLoader{T}"/> satisfies the
/// <see cref="SupportsDryRunContractTests{TSut}"/> contract.
/// </summary>
/// <remarks>
/// This class also serves as a reference example of how to wire up
/// <see cref="SupportsDryRunContractTests{TSut}"/> for your own dry-run-aware stage.
/// The observable side effect for <see cref="TestLoader{T}"/> is collecting items into
/// its buffer; dry-run mode skips it, so <c>GetCollectedItems()</c> reports nothing.
/// </remarks>
public class TestLoaderDryRunContractTests
    : SupportsDryRunContractTests<TestLoader<int>>
{
    private static IAsyncEnumerable<int> SourceItemsAsync() =>
        Enumerable.Range(1, 5).ToAsyncEnumerable();

    /// <inheritdoc/>
    protected override TestLoader<int> CreateSut() =>
        new TestLoader<int>(collectItems: true);

    /// <inheritdoc/>
    protected override async Task<bool> RunAndReportSideEffectAsync(bool isDryRun)
    {
        var loader = new TestLoader<int>(collectItems: true) { IsDryRun = isDryRun };
        await loader.LoadAsync(SourceItemsAsync()).ConfigureAwait(false);
        return loader.GetCollectedItems()?.Count > 0;
    }
}
