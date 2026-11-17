using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.TestKit;
using Xunit;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

/// <summary>
/// The final progress report is sent while a run unwinds. When the run failed, an exception from
/// building or delivering that report must not replace the run's own exception; when the run did
/// not fail, the report's exception still reaches the caller.
/// </summary>
public sealed class FinalReportFailureTests
{
    private static readonly int[] Items = { 1, 2, 3 };



    [Fact]
    public async Task ExtractAsync_when_worker_and_final_report_both_throw_surfaces_the_worker_exception()
    {
        var extractor = new FaultyExtractor<int>(Items)
            .ThrowAt(1, new InvalidDataException("worker"));

        var thrown = await Assert.ThrowsAsync<InvalidDataException>
        (
            () => DrainAsync(extractor.ExtractAsync(new ThrowingProgress<Report>()))
        );

        Assert.Equal("worker", thrown.Message);
    }



    [Fact]
    public async Task ExtractAsync_when_only_the_final_report_throws_surfaces_the_report_exception()
    {
        var extractor = new FaultyExtractor<int>(Items);

        await Assert.ThrowsAsync<ProgressSinkException>
        (
            () => DrainAsync(extractor.ExtractAsync(new ThrowingProgress<Report>()))
        );
    }



    [Fact]
    public async Task ExtractAsync_when_worker_cleanup_and_final_report_both_throw_surfaces_the_cleanup_exception()
    {
        var extractor = new ThrowsOnCleanupExtractor();

        var thrown = await Assert.ThrowsAsync<InvalidDataException>
        (
            () => TakeOneAsync(extractor.ExtractAsync(new ThrowingProgress<Report>()))
        );

        Assert.Equal("cleanup", thrown.Message);
    }



    [Fact]
    public async Task TransformAsync_when_worker_and_final_report_both_throw_surfaces_the_worker_exception()
    {
        var transformer = new FaultyTransformer<int>()
            .ThrowAt(1, new InvalidDataException("worker"));

        var thrown = await Assert.ThrowsAsync<InvalidDataException>
        (
            () => DrainAsync(transformer.TransformAsync(Items.ToAsyncEnumerable(), new ThrowingProgress<Report>()))
        );

        Assert.Equal("worker", thrown.Message);
    }



    [Fact]
    public async Task TransformAsync_when_only_the_final_report_throws_surfaces_the_report_exception()
    {
        var transformer = new FaultyTransformer<int>();

        await Assert.ThrowsAsync<ProgressSinkException>
        (
            () => DrainAsync(transformer.TransformAsync(Items.ToAsyncEnumerable(), new ThrowingProgress<Report>()))
        );
    }



    [Fact]
    public async Task TransformAsync_when_worker_cleanup_and_final_report_both_throw_surfaces_the_cleanup_exception()
    {
        var transformer = new ThrowsOnCleanupTransformer();

        var thrown = await Assert.ThrowsAsync<InvalidDataException>
        (
            () => TakeOneAsync(transformer.TransformAsync(Items.ToAsyncEnumerable(), new ThrowingProgress<Report>()))
        );

        Assert.Equal("cleanup", thrown.Message);
    }



    [Fact]
    public async Task LoadAsync_when_worker_and_final_report_both_throw_surfaces_the_worker_exception()
    {
        var loader = new FaultyLoader<int>(collectItems: false)
            .ThrowAt(1, new InvalidDataException("worker"));

        var thrown = await Assert.ThrowsAsync<InvalidDataException>
        (
            () => loader.LoadAsync(Items.ToAsyncEnumerable(), new ThrowingProgress<Report>())
        );

        Assert.Equal("worker", thrown.Message);
    }



    [Fact]
    public async Task LoadAsync_when_only_the_final_report_throws_surfaces_the_report_exception()
    {
        var loader = new FaultyLoader<int>(collectItems: false);

        await Assert.ThrowsAsync<ProgressSinkException>
        (
            () => loader.LoadAsync(Items.ToAsyncEnumerable(), new ThrowingProgress<Report>())
        );
    }



    [Fact]
    public async Task RunAsync_when_a_stage_and_the_final_snapshot_both_throw_surfaces_the_stage_exception()
    {
        var loader = new FaultyLoader<int>(collectItems: false)
            .ThrowAt(1, new InvalidDataException("worker"));

        var thrown = await Assert.ThrowsAsync<InvalidDataException>
        (
            () => EtlPipeline
                .Create()
                .From(new TestExtractor<int>(Items))
                .To(loader)
                .RunAsync(new ThrowOnFinalSnapshot(Items.Length))
        );

        Assert.Equal("worker", thrown.Message);
    }



    [Fact]
    public async Task RunAsync_when_only_the_final_snapshot_throws_surfaces_the_snapshot_exception()
    {
        await Assert.ThrowsAsync<ProgressSinkException>
        (
            () => EtlPipeline
                .Create()
                .From(new TestExtractor<int>(Items))
                .To(new TestLoader<int>(collectItems: false))
                .RunAsync(new ThrowOnFinalSnapshot(Items.Length))
        );
    }



    private static async Task DrainAsync<T>(IAsyncEnumerable<T> source)
    {
        await foreach (var _ in source)
        {
            // Enumerated only to run the stage to its end.
        }
    }



    // Abandons the sequence after its first item, so the stage's worker is disposed mid-run.
    private static async Task TakeOneAsync<T>(IAsyncEnumerable<T> source)
    {
        await foreach (var _ in source)
        {
            break;
        }
    }



    private sealed class ProgressSinkException : Exception
    {
        public ProgressSinkException() : base("progress sink")
        {
        }
    }



    private sealed class ThrowingProgress<T> : IProgress<T>
    {
        public void Report(T value) => throw new ProgressSinkException();
    }



    // In-flight pipeline snapshots are throttled and are delivered while the run is still going, where
    // a throwing sink is the stage's failure. Only the final snapshot is the subject here, so this
    // sink throws once the run has stopped: it is called from the sink's finally block.
    private sealed class ThrowOnFinalSnapshot : IProgress<EtlPipelineProgress>
    {
        private readonly int _itemCount;
        private int _reports;

        public ThrowOnFinalSnapshot(int itemCount) => _itemCount = itemCount;

        public void Report(EtlPipelineProgress value)
        {
            // The first record always produces one in-flight snapshot; later ones are at least
            // 100 ms apart, which this three-item run never reaches. The second call is the final one.
            _reports++;
            if (_reports > 1 || value.ExtractedItemCount >= _itemCount)
            {
                throw new ProgressSinkException();
            }
        }
    }



    private sealed class ThrowsOnCleanupExtractor : ExtractorBase<int, Report>
    {
        protected override async IAsyncEnumerable<int> ExtractWorkerAsync([EnumeratorCancellation] CancellationToken token)
        {
            try
            {
                foreach (var item in Items)
                {
                    await Task.Yield();
                    IncrementCurrentItemCount();
                    yield return item;
                }
            }
            finally
            {
                throw new InvalidDataException("cleanup");
            }
        }



        protected override Report CreateProgressReport() => new(CurrentItemCount);
    }



    private sealed class ThrowsOnCleanupTransformer : TransformerBase<int, int, Report>
    {
        protected override async IAsyncEnumerable<int> TransformWorkerAsync
        (
            IAsyncEnumerable<int> items,
            [EnumeratorCancellation] CancellationToken token
        )
        {
            try
            {
                await foreach (var item in items.WithCancellation(token))
                {
                    IncrementCurrentItemCount();
                    yield return item;
                }
            }
            finally
            {
                throw new InvalidDataException("cleanup");
            }
        }



        protected override Report CreateProgressReport() => new(CurrentItemCount);
    }
}
