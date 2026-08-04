using System;
using System.Linq;
using System.Threading.Tasks;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Verifies that <see cref="TestTransformer{T}"/> satisfies the
/// <see cref="DisposableStageContractTests{TSut}"/> contract (0.14 disposability + 0.17
/// use-after-dispose guard), and serves as a reference example for a custom transformer.
/// </summary>
public class TestTransformerDisposableContractTests
    : DisposableStageContractTests<TestTransformer<int>>
{
    /// <inheritdoc/>
    protected override TestTransformer<int> CreateSut() =>
        new TestTransformer<int>();

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
            await foreach (var _ in sut.TransformAsync(Enumerable.Range(1, 3).ToAsyncEnumerable()).ConfigureAwait(false))
            {
            }

            return false;
        }
        catch (ObjectDisposedException)
        {
            return true;
        }
    }
}
