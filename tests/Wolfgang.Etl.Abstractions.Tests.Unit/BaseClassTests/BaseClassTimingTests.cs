using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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



        public System.DateTimeOffset? StartedAtForTest => StartedAt;

        public System.TimeSpan ElapsedForTest => Elapsed;



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
        Assert.Equal(System.TimeSpan.Zero, sut.ElapsedForTest);
    }



    [Fact]
    public async Task StartedAt_is_captured_once_extraction_has_produced_items()
    {
        var sut = new TimedExtractor(5);

        await foreach (var _ in sut.ExtractAsync())
        {
        }

        Assert.NotNull(sut.StartedAtForTest);
        Assert.True(sut.ElapsedForTest >= System.TimeSpan.Zero);
    }
}
