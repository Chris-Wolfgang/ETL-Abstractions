using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;
using Xunit;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

/// <summary>
/// Abandoning a run early must dispose the worker's enumerator, so the worker's own <c>finally</c> blocks
/// (resource cleanup) run. Pins the <c>DisposeAsync</c> calls in the base classes' manual enumeration loops (#817).
/// </summary>
public sealed class EnumeratorDisposalTests
{
    private sealed class EndlessExtractor : ExtractorBase<int, EtlProgress>
    {
        public bool WorkerCleanedUp { get; private set; }

        protected override async IAsyncEnumerable<int> ExtractWorkerAsync([EnumeratorCancellation] CancellationToken token)
        {
            try
            {
                for (var i = 0; ; i++)
                {
                    await Task.Yield();
                    yield return i;
                }
            }
            finally
            {
                WorkerCleanedUp = true;
            }
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    private sealed class EndlessTransformer : TransformerBase<int, int, EtlProgress>
    {
        public bool WorkerCleanedUp { get; private set; }

        protected override async IAsyncEnumerable<int> TransformWorkerAsync(IAsyncEnumerable<int> items, [EnumeratorCancellation] CancellationToken token)
        {
            try
            {
                for (var i = 0; ; i++)
                {
                    await Task.Yield();
                    yield return i;
                }
            }
            finally
            {
                WorkerCleanedUp = true;
            }
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    private static async IAsyncEnumerable<int> EmptySource()
    {
        await Task.CompletedTask;
        yield break;
    }


    private static async Task TakeOneAndAbandonAsync(IAsyncEnumerable<int> stream)
    {
        var enumerator = stream.GetAsyncEnumerator();
        Assert.True(await enumerator.MoveNextAsync());
        await enumerator.DisposeAsync();
    }


    [Fact]
    public async Task ExtractAsync_when_abandoned_early_disposes_the_worker_enumerator()
    {
        var sut = new EndlessExtractor();

        await TakeOneAndAbandonAsync(sut.ExtractAsync());

        Assert.True(sut.WorkerCleanedUp);
    }


    [Fact]
    public async Task ExtractAsync_with_progress_when_abandoned_early_disposes_the_worker_enumerator()
    {
        var sut = new EndlessExtractor();

        await TakeOneAndAbandonAsync(sut.ExtractAsync(new Progress<EtlProgress>(_ => { })));

        Assert.True(sut.WorkerCleanedUp);
    }


    [Fact]
    public async Task TransformAsync_when_abandoned_early_disposes_the_worker_enumerator()
    {
        var sut = new EndlessTransformer();

        await TakeOneAndAbandonAsync(sut.TransformAsync(EmptySource()));

        Assert.True(sut.WorkerCleanedUp);
    }


    [Fact]
    public async Task TransformAsync_with_progress_when_abandoned_early_disposes_the_worker_enumerator()
    {
        var sut = new EndlessTransformer();

        await TakeOneAndAbandonAsync(sut.TransformAsync(EmptySource(), new Progress<EtlProgress>(_ => { })));

        Assert.True(sut.WorkerCleanedUp);
    }
}
