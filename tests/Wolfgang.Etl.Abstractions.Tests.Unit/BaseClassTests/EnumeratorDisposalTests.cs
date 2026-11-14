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
    private readonly List<EtlProgress> _reports = new();


    private sealed class EndlessExtractor : ExtractorBase<int, EtlProgress>
    {
        public bool WorkerCleanedUp { get; private set; }

        protected override async IAsyncEnumerable<int> ExtractWorkerAsync([EnumeratorCancellation] CancellationToken token)
        {
            try
            {
                var next = 0;
                while (true)
                {
                    await Task.Yield();
                    yield return next++;
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
            await foreach (var _ in items.WithCancellation(token))
            {
                IncrementCurrentItemCount();
            }

            try
            {
                var next = 0;
                while (true)
                {
                    await Task.Yield();
                    yield return next++;
                }
            }
            finally
            {
                WorkerCleanedUp = true;
            }
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    private static async IAsyncEnumerable<int> OneItemSource()
    {
        await Task.Yield();
        yield return 1;
    }


    private static async Task TakeOneAndAbandonAsync(IAsyncEnumerable<int> stream)
    {
        var enumerator = stream.GetAsyncEnumerator();
        // Two items: the second MoveNextAsync resumes the worker past its first yield before the run is abandoned.
        Assert.True(await enumerator.MoveNextAsync());
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

        await TakeOneAndAbandonAsync(sut.ExtractAsync(new SynchronousProgress<EtlProgress>(_reports.Add)));

        Assert.True(sut.WorkerCleanedUp);
    }


    [Fact]
    public async Task TransformAsync_when_abandoned_early_disposes_the_worker_enumerator()
    {
        var sut = new EndlessTransformer();

        await TakeOneAndAbandonAsync(sut.TransformAsync(OneItemSource()));

        Assert.True(sut.WorkerCleanedUp);
    }


    [Fact]
    public async Task TransformAsync_with_progress_when_abandoned_early_disposes_the_worker_enumerator()
    {
        var sut = new EndlessTransformer();

        await TakeOneAndAbandonAsync(sut.TransformAsync(OneItemSource(), new SynchronousProgress<EtlProgress>(_reports.Add)));

        Assert.True(sut.WorkerCleanedUp);
    }
}
