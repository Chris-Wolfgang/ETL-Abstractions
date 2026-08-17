using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

/// <summary>
/// Verifies the timing instrumentation that <see cref="ExtractorBase{TSource, TProgress}"/>
/// (and its loader/transformer siblings) capture automatically on the first processed item.
/// </summary>
public class BaseClassTimingTests
{
    // Minimal extractor that exposes the protected StartedAt / Elapsed timing
    // signals so the base-class instrumentation can be asserted directly.
    private sealed class TimedExtractor : ExtractorBase<int, EtlProgress>
    {
        private readonly int _count;



        public TimedExtractor(int count)
        {
            _count = count;
        }



        public DateTimeOffset? StartedAtForTest => StartedAt;

        public TimeSpan ElapsedForTest => Elapsed;



        protected override async IAsyncEnumerable<int> ExtractWorkerAsync
        (
            [EnumeratorCancellation] CancellationToken token
        )
        {
            for (var i = 0; i < _count; i++)
            {
                IncrementCurrentItemCount();
                yield return i;
            }

            await Task.CompletedTask;
        }



        protected override EtlProgress CreateProgressReport()
        {
            return new EtlProgress(CurrentItemCount);
        }
    }



    [Fact]
    public void StartedAt_is_null_before_any_item_is_processed()
    {
        var sut = new TimedExtractor(3);

        Assert.Null(sut.StartedAtForTest);
        Assert.Equal(TimeSpan.Zero, sut.ElapsedForTest);
    }



    [Fact]
    public async Task StartedAt_is_captured_once_extraction_has_produced_items()
    {
        var sut = new TimedExtractor(5);

        await foreach (var _ in sut.ExtractAsync(new Progress<EtlProgress>()))
        {
        }

        Assert.NotNull(sut.StartedAtForTest);
        Assert.True(sut.ElapsedForTest >= TimeSpan.Zero);
    }



    private static async IAsyncEnumerable<int> Range(int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return i;
        }

        await Task.CompletedTask;
    }


    private sealed class TimedLoader : LoaderBase<int, EtlProgress>
    {
        public DateTimeOffset? StartedAtForTest => StartedAt;

        public TimeSpan ElapsedForTest => Elapsed;

        protected override async Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token)
        {
            await foreach (var _ in items.WithCancellation(token))
            {
                IncrementCurrentItemCount();
            }
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    private sealed class TimedTransformer : TransformerBase<int, int, EtlProgress>
    {
        public DateTimeOffset? StartedAtForTest => StartedAt;

        public TimeSpan ElapsedForTest => Elapsed;

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


    [Fact]
    public void LoaderBase_StartedAt_is_null_before_any_item_is_processed()
    {
        var sut = new TimedLoader();

        Assert.Null(sut.StartedAtForTest);
        Assert.Equal(TimeSpan.Zero, sut.ElapsedForTest);
    }


    [Fact]
    public async Task LoaderBase_StartedAt_is_captured_once_loading_has_processed_items()
    {
        var sut = new TimedLoader();

        await sut.LoadAsync(Range(5), new Progress<EtlProgress>());

        Assert.NotNull(sut.StartedAtForTest);
        Assert.True(sut.ElapsedForTest >= TimeSpan.Zero);
    }


    [Fact]
    public void TransformerBase_StartedAt_is_null_before_any_item_is_processed()
    {
        var sut = new TimedTransformer();

        Assert.Null(sut.StartedAtForTest);
        Assert.Equal(TimeSpan.Zero, sut.ElapsedForTest);
    }


    [Fact]
    public async Task TransformerBase_StartedAt_is_captured_once_transformation_has_processed_items()
    {
        var sut = new TimedTransformer();

        await foreach (var _ in sut.TransformAsync(Range(5), new Progress<EtlProgress>()))
        {
        }

        Assert.NotNull(sut.StartedAtForTest);
        Assert.True(sut.ElapsedForTest >= TimeSpan.Zero);
    }
}
