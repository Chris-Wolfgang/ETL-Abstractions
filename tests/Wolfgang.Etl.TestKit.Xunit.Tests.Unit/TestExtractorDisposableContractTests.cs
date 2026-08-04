using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Verifies that <see cref="TestExtractor{T}"/> satisfies the
/// <see cref="DisposableStageContractTests{TSut}"/> contract (0.14 disposability + 0.17
/// use-after-dispose guard), and serves as a reference example for a custom extractor.
/// </summary>
public class TestExtractorDisposableContractTests
    : DisposableStageContractTests<TestExtractor<int>>
{
    /// <inheritdoc/>
    protected override TestExtractor<int> CreateSut() =>
        new TestExtractor<int>(new List<int> { 1, 2, 3 });

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
            await foreach (var _ in sut.ExtractAsync().ConfigureAwait(false))
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
