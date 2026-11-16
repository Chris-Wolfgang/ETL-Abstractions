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
    public void LoadAsync_when_worker_throws_synchronously_returns_a_faulted_task()
    {
        var loader = new ThrowingLoader();

        var task = loader.LoadAsync(NoItems());

        AssertFaultedWithTheWorkerFailure(task);
    }



    [Fact]
    public void LoadAsync_with_token_when_worker_throws_synchronously_returns_a_faulted_task()
    {
        var loader = new ThrowingLoader();

        var task = loader.LoadAsync(NoItems(), CancellationToken.None);

        AssertFaultedWithTheWorkerFailure(task);
    }



    [Fact]
    public void LoadAsync_with_progress_when_worker_throws_synchronously_returns_a_faulted_task()
    {
        var loader = new ThrowingLoader();
        var progress = new SynchronousProgress<EtlProgress>(_ => { });

        var task = loader.LoadAsync(NoItems(), progress);

        AssertFaultedWithTheWorkerFailure(task);
    }



    // The task is already complete, so the failure is read from it rather than awaited.
    private static void AssertFaultedWithTheWorkerFailure(Task task)
    {
        Assert.True(task.IsFaulted);

        var aggregate = Assert.IsType<AggregateException>(task.Exception);
        var failure = Assert.Single(aggregate.InnerExceptions);
        Assert.IsType<InvalidOperationException>(failure);
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
