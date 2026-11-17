using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            () => extractor.ExtractAsync(new ThrowingProgress<Report>()).ToListAsync().AsTask()
        );

        Assert.Equal("worker", thrown.Message);
    }



    [Fact]
    public async Task ExtractAsync_when_only_the_final_report_throws_surfaces_the_report_exception()
    {
        var extractor = new FaultyExtractor<int>(Items);

        await Assert.ThrowsAsync<NotSupportedException>
        (
            () => extractor.ExtractAsync(new ThrowingProgress<Report>()).ToListAsync().AsTask()
        );
    }



    [Fact]
    public async Task ExtractAsync_when_worker_cleanup_and_final_report_both_throw_surfaces_the_cleanup_exception()
    {
        var extractor = new ThrowsOnCleanupExtractor();

        var thrown = await Assert.ThrowsAsync<InvalidDataException>
        (
            () => extractor.ExtractAsync(new ThrowingProgress<Report>()).FirstAsync().AsTask()
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
            () => transformer.TransformAsync(Items.ToAsyncEnumerable(), new ThrowingProgress<Report>()).ToListAsync().AsTask()
        );

        Assert.Equal("worker", thrown.Message);
    }



    [Fact]
    public async Task TransformAsync_when_only_the_final_report_throws_surfaces_the_report_exception()
    {
        var transformer = new FaultyTransformer<int>();

        await Assert.ThrowsAsync<NotSupportedException>
        (
            () => transformer.TransformAsync(Items.ToAsyncEnumerable(), new ThrowingProgress<Report>()).ToListAsync().AsTask()
        );
    }



    [Fact]
    public async Task TransformAsync_when_worker_cleanup_and_final_report_both_throw_surfaces_the_cleanup_exception()
    {
        var transformer = new TestTransformer<int>();

        var thrown = await Assert.ThrowsAsync<InvalidDataException>
        (
            () => transformer.TransformAsync(new ThrowsOnDisposeSequence(), new ThrowingProgress<Report>()).FirstAsync().AsTask()
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

        await Assert.ThrowsAsync<NotSupportedException>
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
        await Assert.ThrowsAsync<NotSupportedException>
        (
            () => EtlPipeline
                .Create()
                .From(new TestExtractor<int>(Items))
                .To(new TestLoader<int>(collectItems: false))
                .RunAsync(new ThrowOnFinalSnapshot(Items.Length))
        );
    }



    private sealed class ThrowingProgress<T> : IProgress<T>
    {
        public void Report(T value) => throw new NotSupportedException("progress sink");
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
                throw new NotSupportedException("progress sink");
            }
        }
    }



    // A worker stream that never ends and fails when it is disposed, as a worker whose cleanup
    // throws does. Hand-written rather than an iterator so that every line of it runs.
    private sealed class ThrowsOnDisposeSequence : IAsyncEnumerable<int>, IAsyncEnumerator<int>
    {
        public int Current => 1;

        public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = default) => this;

        public ValueTask<bool> MoveNextAsync() => new(true);

        public ValueTask DisposeAsync() => throw new InvalidDataException("cleanup");
    }



    private sealed class ThrowsOnCleanupExtractor : ExtractorBase<int, Report>
    {
        protected override IAsyncEnumerable<int> ExtractWorkerAsync(CancellationToken token) =>
            new ThrowsOnDisposeSequence();

        protected override Report CreateProgressReport() => new(CurrentItemCount);
    }
}
