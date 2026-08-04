using System;
using System.Linq;
using System.Threading.Tasks;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Verifies that <see cref="TestLoader{T}"/> satisfies the
/// <see cref="DisposableStageContractTests{TSut}"/> contract (0.14 disposability + 0.17
/// use-after-dispose guard), and serves as a reference example for a custom loader.
/// </summary>
public class TestLoaderDisposableContractTests
    : DisposableStageContractTests<TestLoader<int>>
{
    /// <inheritdoc/>
    protected override TestLoader<int> CreateSut() =>
        new TestLoader<int>(collectItems: false);

    /// <inheritdoc/>
    protected override async Task<bool> InvokeReportsObjectDisposedAsync(bool disposeFirst, bool useAsyncDispose)
    {
        var sut = CreateSut();

        if (disposeFirst)
        {
            if (useAsyncDispose)
            {
                await sut.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                sut.Dispose();
            }
        }

        try
        {
            await sut.LoadAsync(Enumerable.Range(1, 3).ToAsyncEnumerable()).ConfigureAwait(false);

            return false;
        }
        catch (ObjectDisposedException)
        {
            return true;
        }
    }
}
