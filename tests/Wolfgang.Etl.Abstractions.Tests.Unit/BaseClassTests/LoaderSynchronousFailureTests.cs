using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;
using Xunit;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

/// <summary>
/// A worker that throws before its first await must fail the same way from every
/// <c>LoadAsync</c> overload: through the returned task, never synchronously.
/// </summary>
public sealed class LoaderSynchronousFailureTests
{
    // Never enumerated: the worker throws before it touches the source.
    private static IAsyncEnumerable<int> NoItems() => Array.Empty<int>().ToAsyncEnumerable();



    [Fact]
    public async Task LoadAsync_when_worker_throws_synchronously_returns_a_faulted_task()
    {
        var loader = new ThrowingLoader();

        var task = loader.LoadAsync(NoItems());

        Assert.True(task.IsFaulted);
        await Assert.ThrowsAsync<InvalidOperationException>(() => task);
    }



    [Fact]
    public async Task LoadAsync_with_token_when_worker_throws_synchronously_returns_a_faulted_task()
    {
        var loader = new ThrowingLoader();

        var task = loader.LoadAsync(NoItems(), CancellationToken.None);

        Assert.True(task.IsFaulted);
        await Assert.ThrowsAsync<InvalidOperationException>(() => task);
    }



    [Fact]
    public async Task LoadAsync_with_progress_when_worker_throws_synchronously_returns_a_faulted_task()
    {
        var loader = new ThrowingLoader();
        var progress = new SynchronousProgress<EtlProgress>(_ => { });

        var task = loader.LoadAsync(NoItems(), progress);

        Assert.True(task.IsFaulted);
        await Assert.ThrowsAsync<InvalidOperationException>(() => task);
    }



    private sealed class ThrowingLoader : LoaderBase<int, EtlProgress>
    {
        protected override Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token)
        {
            throw new InvalidOperationException("worker failed before its first await");
        }



        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }
}
