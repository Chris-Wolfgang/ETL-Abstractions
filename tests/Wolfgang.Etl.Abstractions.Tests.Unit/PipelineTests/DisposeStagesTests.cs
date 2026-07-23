using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests;

/// <summary>
/// Covers the opt-in <see cref="IPipeline.DisposeStagesOnCompletion"/> behavior: stages that
/// implement <see cref="IDisposable"/> / <see cref="IAsyncDisposable"/> are disposed after the run,
/// the default leaves them alone, the run exception wins over disposal errors, and disposal failures
/// aggregate.
/// </summary>
public class DisposeStagesTests
{
    private sealed class TrackingExtractor : IExtractAsync<int>, IDisposable
    {
        private readonly int _count;

        public TrackingExtractor(int count) => _count = count;

        public bool Disposed { get; private set; }

        public async IAsyncEnumerable<int> ExtractAsync()
        {
            for (var i = 0; i < _count; i++)
            {
                yield return i;
            }

            await Task.CompletedTask;
        }

        public void Dispose() => Disposed = true;
    }


    private sealed class TrackingLoader : ILoadAsync<int>, IDisposable
    {
        private readonly bool _throwOnLoad;

        public TrackingLoader(bool throwOnLoad = false) => _throwOnLoad = throwOnLoad;

        public bool Disposed { get; private set; }

        public async Task LoadAsync(IAsyncEnumerable<int> items)
        {
            await foreach (var _ in items)
            {
                if (_throwOnLoad)
                {
                    throw new InvalidOperationException("load failed");
                }
            }
        }

        public void Dispose() => Disposed = true;
    }


    // Implements both IDisposable and IAsyncDisposable to prove the async path is preferred.
    private sealed class DualDisposableLoader : ILoadAsync<int>, IDisposable, IAsyncDisposable
    {
        public bool DisposeCalled { get; private set; }

        public bool DisposeAsyncCalled { get; private set; }

        public async Task LoadAsync(IAsyncEnumerable<int> items)
        {
            await foreach (var _ in items)
            {
            }
        }

        public void Dispose() => DisposeCalled = true;

        public ValueTask DisposeAsync()
        {
            DisposeAsyncCalled = true;
            return default;
        }
    }


    private sealed class ThrowingDisposableLoader : ILoadAsync<int>, IDisposable
    {
        public async Task LoadAsync(IAsyncEnumerable<int> items)
        {
            await foreach (var _ in items)
            {
            }
        }

        public void Dispose() => throw new InvalidOperationException("dispose failed");
    }


    private sealed class ThrowingDisposableExtractor : IExtractAsync<int>, IDisposable
    {
        private readonly int _count;

        public ThrowingDisposableExtractor(int count) => _count = count;

        public async IAsyncEnumerable<int> ExtractAsync()
        {
            for (var i = 0; i < _count; i++)
            {
                yield return i;
            }

            await Task.CompletedTask;
        }

        public void Dispose() => throw new InvalidOperationException("extractor dispose failed");
    }


    // Not disposable — must be skipped without error.
    private sealed class PlainTransformer : ITransformAsync<int, int>
    {
        public async IAsyncEnumerable<int> TransformAsync(IAsyncEnumerable<int> items)
        {
            await foreach (var item in items)
            {
                yield return item;
            }
        }
    }


    [Fact]
    public async Task DisposeStagesOnCompletion_disposes_disposable_stages_after_a_successful_run()
    {
        var extractor = new TrackingExtractor(3);
        var loader = new TrackingLoader();

        await Pipeline
            .Extract(extractor)
            .Load(loader)
            .DisposeStagesOnCompletion()
            .RunAsync();

        Assert.True(extractor.Disposed);
        Assert.True(loader.Disposed);
    }


    [Fact]
    public async Task Without_DisposeStagesOnCompletion_stages_are_not_disposed()
    {
        var extractor = new TrackingExtractor(3);
        var loader = new TrackingLoader();

        await Pipeline
            .Extract(extractor)
            .Load(loader)
            .RunAsync();

        Assert.False(extractor.Disposed);
        Assert.False(loader.Disposed);
    }


    [Fact]
    public async Task DisposeStagesOnCompletion_disposes_stages_on_the_load_progress_path()
    {
        // A load-progress loader routes the run through PipelineWithLoadProgress, whose
        // dispose branch is separate from the bare path — exercise it explicitly.
        var extractor = new TrackingExtractor(3);

        await Pipeline
            .Extract(extractor)
            .Load(new TestDoubles.ProgressOnlyLoader<int, string>("p"))
            .DisposeStagesOnCompletion()
            .RunAsync();

        Assert.True(extractor.Disposed);
    }


    [Fact]
    public async Task Load_progress_pipeline_without_DisposeStagesOnCompletion_leaves_stages_undisposed()
    {
        var extractor = new TrackingExtractor(3);

        await Pipeline
            .Extract(extractor)
            .Load(new TestDoubles.ProgressOnlyLoader<int, string>("p"))
            .RunAsync();

        Assert.False(extractor.Disposed);
    }


    [Fact]
    public async Task DisposeStagesOnCompletion_prefers_DisposeAsync_over_Dispose()
    {
        var loader = new DualDisposableLoader();

        await Pipeline
            .Extract(new TrackingExtractor(2))
            .Load(loader)
            .DisposeStagesOnCompletion()
            .RunAsync();

        Assert.True(loader.DisposeAsyncCalled);
        Assert.False(loader.DisposeCalled);
    }


    [Fact]
    public async Task DisposeStagesOnCompletion_skips_non_disposable_stages()
    {
        var extractor = new TrackingExtractor(2);
        var loader = new TrackingLoader();

        // PlainTransformer implements neither interface; it must simply be skipped.
        await Pipeline
            .Extract(extractor)
            .Transform(new PlainTransformer())
            .Load(loader)
            .DisposeStagesOnCompletion()
            .RunAsync();

        Assert.True(extractor.Disposed);
        Assert.True(loader.Disposed);
    }


    [Fact]
    public async Task DisposeStagesOnCompletion_disposes_stages_even_when_the_run_throws()
    {
        var extractor = new TrackingExtractor(3);
        var loader = new TrackingLoader(throwOnLoad: true);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => Pipeline
            .Extract(extractor)
            .Load(loader)
            .DisposeStagesOnCompletion()
            .RunAsync());

        Assert.Equal("load failed", ex.Message);
        Assert.True(extractor.Disposed);
        Assert.True(loader.Disposed);
    }


    [Fact]
    public async Task DisposeStagesOnCompletion_aggregates_disposal_exceptions()
    {
        var ex = await Assert.ThrowsAsync<AggregateException>(() => Pipeline
            .Extract(new TrackingExtractor(2))
            .Load(new ThrowingDisposableLoader())
            .DisposeStagesOnCompletion()
            .RunAsync());

        Assert.Contains(ex.InnerExceptions, e => e is InvalidOperationException && string.Equals(e.Message, "dispose failed", StringComparison.Ordinal));
        Assert.StartsWith("One or more pipeline stages threw while being disposed.", ex.Message);
    }


    [Fact]
    public async Task DisposeStagesOnCompletion_aggregates_every_stage_disposal_failure()
    {
        // Two stages throw on disposal — both errors must accumulate into the aggregate,
        // proving the error list is appended to (errors ??= ...) rather than reset per stage.
        var ex = await Assert.ThrowsAsync<AggregateException>(() => Pipeline
            .Extract(new ThrowingDisposableExtractor(2))
            .Load(new ThrowingDisposableLoader())
            .DisposeStagesOnCompletion()
            .RunAsync());

        Assert.Equal(2, ex.InnerExceptions.Count);
        Assert.Contains(ex.InnerExceptions, e => string.Equals(e.Message, "extractor dispose failed", StringComparison.Ordinal));
        Assert.Contains(ex.InnerExceptions, e => string.Equals(e.Message, "dispose failed", StringComparison.Ordinal));
    }


    // Records the order in which stages were disposed by appending to a shared list.
    private sealed class OrderRecordingExtractor : IExtractAsync<int>, IDisposable
    {
        private readonly List<string> _log;

        public OrderRecordingExtractor(List<string> log) => _log = log;

        public async IAsyncEnumerable<int> ExtractAsync()
        {
            yield return 0;
            await Task.CompletedTask;
        }

        public void Dispose() => _log.Add(nameof(OrderRecordingExtractor));
    }


    private sealed class OrderRecordingTransformer : ITransformAsync<int, int>, IDisposable
    {
        private readonly List<string> _log;

        public OrderRecordingTransformer(List<string> log) => _log = log;

        public async IAsyncEnumerable<int> TransformAsync(IAsyncEnumerable<int> items)
        {
            await foreach (var item in items)
            {
                yield return item;
            }
        }

        public void Dispose() => _log.Add(nameof(OrderRecordingTransformer));
    }


    private sealed class OrderRecordingLoader : ILoadAsync<int>, IDisposable
    {
        private readonly List<string> _log;

        public OrderRecordingLoader(List<string> log) => _log = log;

        public async Task LoadAsync(IAsyncEnumerable<int> items)
        {
            await foreach (var _ in items)
            {
            }
        }

        public void Dispose() => _log.Add(nameof(OrderRecordingLoader));
    }


    [Fact]
    public async Task DisposeStagesOnCompletion_disposes_in_reverse_construction_order()
    {
        // Locks in the LIFO contract documented on IPipeline.DisposeStagesOnCompletion —
        // matching nested `using`/`await using` and DI-container scope disposal, so
        // downstream stages get torn down before the upstream stages they may reference.
        var log = new List<string>();

        await Pipeline
            .Extract(new OrderRecordingExtractor(log))
            .Transform(new OrderRecordingTransformer(log))
            .Load(new OrderRecordingLoader(log))
            .DisposeStagesOnCompletion()
            .RunAsync();

        Assert.Equal
        (
            new[]
            {
                nameof(OrderRecordingLoader),
                nameof(OrderRecordingTransformer),
                nameof(OrderRecordingExtractor),
            },
            log
        );
    }
}
