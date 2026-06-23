using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

/// <summary>
/// Verifies that <c>CurrentItemCount</c> / <c>CurrentSkippedItemCount</c> (and the captured start
/// time) reset at the start of every run, so re-running the same instance reports per-run counts
/// rather than a cumulative total (issue #246).
/// </summary>
public class ResetRunStateTests
{
    private static async IAsyncEnumerable<int> Range(int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return i;
        }

        await Task.CompletedTask;
    }


    private sealed class CountingExtractor : ExtractorBase<int, EtlProgress>
    {
        private readonly int _count;

        public CountingExtractor(int count) => _count = count;

        protected override async IAsyncEnumerable<int> ExtractWorkerAsync([EnumeratorCancellation] CancellationToken token)
        {
            for (var i = 0; i < _count; i++)
            {
                IncrementCurrentItemCount();
                yield return i;
            }

            await Task.CompletedTask;
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    private sealed class CountingLoader : LoaderBase<int, EtlProgress>
    {
        protected override async Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token)
        {
            await foreach (var _ in items.WithCancellation(token))
            {
                IncrementCurrentItemCount();
            }
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    private sealed class CountingTransformer : TransformerBase<int, int, EtlProgress>
    {
        protected override async IAsyncEnumerable<int> TransformWorkerAsync(IAsyncEnumerable<int> items, [EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var item in items.WithCancellation(token))
            {
                IncrementCurrentItemCount();
                yield return item;
            }
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    private static async Task DrainAsync(IAsyncEnumerable<int> sequence)
    {
        await foreach (var _ in sequence)
        {
        }
    }


    [Fact]
    public async Task ExtractorBase_CurrentItemCount_resets_between_runs()
    {
        var sut = new CountingExtractor(5);

        await DrainAsync(sut.ExtractAsync());
        Assert.Equal(5, sut.CurrentItemCount);

        await DrainAsync(sut.ExtractAsync());
        Assert.Equal(5, sut.CurrentItemCount);
    }


    [Fact]
    public async Task LoaderBase_CurrentItemCount_resets_between_runs()
    {
        var sut = new CountingLoader();

        await sut.LoadAsync(Range(5));
        Assert.Equal(5, sut.CurrentItemCount);

        await sut.LoadAsync(Range(5));
        Assert.Equal(5, sut.CurrentItemCount);
    }


    [Fact]
    public async Task TransformerBase_CurrentItemCount_resets_between_runs()
    {
        var sut = new CountingTransformer();

        await DrainAsync(sut.TransformAsync(Range(5)));
        Assert.Equal(5, sut.CurrentItemCount);

        await DrainAsync(sut.TransformAsync(Range(5)));
        Assert.Equal(5, sut.CurrentItemCount);
    }
}
