using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit;

/// <summary>
/// Verifies that the library never marshals a continuation back onto the caller's
/// <see cref="SynchronizationContext"/> — i.e. every internal await uses
/// <c>ConfigureAwait(false)</c>. This is the behaviour that keeps the library safe from the classic
/// sync-over-async deadlock and from needless context-hopping for WPF / WinForms / legacy-ASP.NET
/// consumers, and it is invisible to a normal headless test (which has no context), so it needs an
/// explicit context installed to exercise it.
///
/// <para>Each test installs a counting context, starts the operation while it is current (so the
/// awaits that suspend on that synchronous stack capture it under a <c>ConfigureAwait(true)</c>
/// mutation), then awaits the result off-context and asserts nothing was posted back. The test
/// doubles are all context-agnostic (their own awaits use <c>ConfigureAwait(false)</c>), so the only
/// thing that can post to the context is the library code under test.</para>
/// </summary>
public sealed class ConfigureAwaitContextTests
{
    [Fact]
    public Task EtlPipeline_streaming_run_does_not_capture_the_context()
        => AssertDoesNotCaptureContext(() => EtlPipeline
            .Create()
            .From(SuspendingSource(3))
            .To(new ContextAgnosticLoader())
            .RunAsync());


    [Fact]
    public Task DisposingOwned_streaming_run_does_not_capture_the_context()
        => AssertDoesNotCaptureContext(() => EtlPipeline
            .Create()
            .From(SuspendingSource(3))
            .To(new ContextAgnosticLoader())
            .DisposingOwned(new SuspendingAsyncDisposable())
            .RunAsync());


    [Fact]
    public Task DisposingOwned_resource_cleanup_does_not_capture_the_context()
        // Synchronous run keeps execution on the context thread so the suspending owned-resource
        // disposal is the await that would capture it.
        => AssertDoesNotCaptureContext(() => EtlPipeline
            .Create()
            .From(SynchronousSource(2))
            .To(new ContextAgnosticLoader())
            .DisposingOwned(new SuspendingAsyncDisposable())
            .RunAsync());


    [Fact]
    public Task LoaderBase_progress_run_does_not_capture_the_context()
        => AssertDoesNotCaptureContext(() => new ContextAgnosticLoader()
            .LoadAsync(SuspendingSource(3), new NoOpProgress<EtlProgress>()));


    [Fact]
    public Task Fluent_pipeline_streaming_run_does_not_capture_the_context()
        => AssertDoesNotCaptureContext(() => Pipeline
            .Extract(new SuspendingExtractor(3))
            .Load(new ContextAgnosticListLoader())
            .DisposeStagesOnCompletion()
            .RunAsync());


    [Fact]
    public Task Fluent_pipeline_stage_disposal_does_not_capture_the_context()
        // Synchronous run + a suspending IAsyncDisposable stage → the disposal await is the one that
        // would capture the context.
        => AssertDoesNotCaptureContext(() => Pipeline
            .Extract(new SuspendingAsyncDisposableExtractor(2))
            .Load(new ContextAgnosticListLoader())
            .DisposeStagesOnCompletion()
            .RunAsync());


    // Starts the operation while a counting context is current on this thread, restores the previous
    // context, then awaits off-context and asserts the library posted nothing back. A ConfigureAwait
    // flip to (true) on any await that suspended while the context was current posts a continuation
    // to it and trips the assertion.
    private static async Task AssertDoesNotCaptureContext(Func<Task> start)
    {
        var context = new CountingSynchronizationContext();
        var previous = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(context);

        Task run;
        try
        {
            run = start();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previous);
        }

        await run.ConfigureAwait(false);

        Assert.Equal(0, context.Posts);
    }


    // Suspends between items (so the library's awaits configure real continuations) but never
    // captures the ambient context itself.
    private static async IAsyncEnumerable<int> SuspendingSource(int count)
    {
        for (var i = 0; i < count; i++)
        {
            await Task.Delay(1).ConfigureAwait(false);
            yield return i;
        }
    }


    // Completes synchronously — keeps the run on the context thread so a suspending disposal await
    // is the one under test.
    private static async IAsyncEnumerable<int> SynchronousSource(int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return i;
        }

        await Task.CompletedTask;
    }


    [ExcludeFromCodeCoverage]
    private sealed class CountingSynchronizationContext : SynchronizationContext
    {
        private int _posts;

        public int Posts => Volatile.Read(ref _posts);

        public override void Post(SendOrPostCallback d, object? state)
        {
            Interlocked.Increment(ref _posts);
            ThreadPool.QueueUserWorkItem(_ => d(state));
        }

        public override void Send(SendOrPostCallback d, object? state)
        {
            Interlocked.Increment(ref _posts);
            d(state);
        }
    }


    [ExcludeFromCodeCoverage]
    private sealed class NoOpProgress<T> : IProgress<T>
    {
        public void Report(T value)
        {
        }
    }


    [ExcludeFromCodeCoverage]
    private sealed class ContextAgnosticLoader : LoaderBase<int, EtlProgress>
    {
        protected override async Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token)
        {
            await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
            {
                _ = item;
                IncrementCurrentItemCount();
            }
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    [ExcludeFromCodeCoverage]
    private sealed class ContextAgnosticListLoader : ILoadAsync<int>
    {
        public async Task LoadAsync(IAsyncEnumerable<int> items)
        {
            await foreach (var item in items.ConfigureAwait(false))
            {
                _ = item;
            }
        }
    }


    [ExcludeFromCodeCoverage]
    private sealed class SuspendingExtractor : IExtractAsync<int>
    {
        private readonly int _count;

        public SuspendingExtractor(int count) => _count = count;

        public async IAsyncEnumerable<int> ExtractAsync()
        {
            for (var i = 0; i < _count; i++)
            {
                await Task.Delay(1).ConfigureAwait(false);
                yield return i;
            }
        }
    }


    [ExcludeFromCodeCoverage]
    private sealed class SuspendingAsyncDisposableExtractor : IExtractAsync<int>, IAsyncDisposable
    {
        private readonly int _count;

        public SuspendingAsyncDisposableExtractor(int count) => _count = count;

        public async IAsyncEnumerable<int> ExtractAsync()
        {
            for (var i = 0; i < _count; i++)
            {
                yield return i;
            }

            await Task.CompletedTask;
        }

        public async ValueTask DisposeAsync() => await Task.Delay(1).ConfigureAwait(false);
    }


    [ExcludeFromCodeCoverage]
    private sealed class SuspendingAsyncDisposable : IAsyncDisposable
    {
        public async ValueTask DisposeAsync() => await Task.Delay(1).ConfigureAwait(false);
    }
}
