using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.Polly.Tests.Unit;

public class PollyResilienceTests
{
    private static ResiliencePipeline Retry(int maxRetryAttempts) =>
        new ResiliencePipelineBuilder()
            .AddRetry
            (
                new RetryStrategyOptions
                {
                    MaxRetryAttempts = maxRetryAttempts,
                    Delay = TimeSpan.Zero,
                    ShouldHandle = new PredicateBuilder().Handle<IOException>(),
                }
            )
            .Build();



    private static async Task<List<int>> DrainAsync(IAsyncEnumerable<int> source)
    {
        var result = new List<int>();
        await foreach (var item in source)
        {
            result.Add(item);
        }

        return result;
    }



    private static async IAsyncEnumerable<int> AsAsync(IEnumerable<int> items)
    {
        foreach (var item in items)
        {
            await Task.Yield();
            yield return item;
        }
    }



    [Fact]
    public void ForStream_when_pipeline_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => PollyResilience.ForStream<int>(null!));

        Assert.Equal("pipeline", ex.ParamName);
    }



    [Fact]
    public void ForLoader_when_pipeline_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => PollyResilience.ForLoader(null!));

        Assert.Equal("pipeline", ex.ParamName);
    }



    [Fact]
    public async Task ForStream_retries_a_failed_attempt_and_yields_each_item_once()
    {
        var sut = new FlakyExtractor(new[] { 1, 2, 3 }, failingAttempts: 1)
        {
            WorkerResilience = PollyResilience.ForStream<int>(Retry(2)),
        };

        var items = await DrainAsync(sut.ExtractAsync());

        Assert.Equal(new[] { 1, 2, 3 }, items);
        Assert.Equal(2, sut.Attempts);
    }



    [Fact]
    public async Task ForStream_when_every_attempt_fails_rethrows_the_last_failure()
    {
        var sut = new FlakyExtractor(new[] { 1 }, failingAttempts: 5)
        {
            WorkerResilience = PollyResilience.ForStream<int>(Retry(2)),
        };

        await Assert.ThrowsAsync<IOException>(() => DrainAsync(sut.ExtractAsync()));

        Assert.Equal(3, sut.Attempts);
    }



    [Fact]
    public async Task ForStream_with_an_empty_pipeline_passes_the_items_through()
    {
        var sut = new FlakyExtractor(new[] { 4, 5 }, failingAttempts: 0)
        {
            WorkerResilience = PollyResilience.ForStream<int>(ResiliencePipeline.Empty),
        };

        var items = await DrainAsync(sut.ExtractAsync());

        Assert.Equal(new[] { 4, 5 }, items);
        Assert.Equal(1, sut.Attempts);
    }



    [Fact]
    public async Task ForStream_forwards_the_run_cancellation_token_to_the_worker()
    {
        using var cts = new CancellationTokenSource();
        var sut = new FlakyExtractor(new[] { 1 }, failingAttempts: 0)
        {
            WorkerResilience = PollyResilience.ForStream<int>(ResiliencePipeline.Empty),
        };
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => DrainAsync(sut.ExtractAsync(cts.Token)));
    }



    [Fact]
    public async Task ForLoader_retries_a_failed_load()
    {
        var sut = new FlakyLoader(failingAttempts: 1)
        {
            WorkerResilience = PollyResilience.ForLoader(Retry(2)),
        };

        await sut.LoadAsync(AsAsync(new[] { 7, 8 }));

        Assert.Equal(2, sut.Attempts);
        Assert.Equal(new[] { 7, 8 }, sut.Loaded);
    }



    [Fact]
    public async Task ForLoader_when_every_attempt_fails_rethrows_the_last_failure()
    {
        var sut = new FlakyLoader(failingAttempts: 5)
        {
            WorkerResilience = PollyResilience.ForLoader(Retry(1)),
        };

        await Assert.ThrowsAsync<IOException>(() => sut.LoadAsync(AsAsync(new[] { 1 })));

        Assert.Equal(2, sut.Attempts);
    }



    // Yields its items; the first failingAttempts attempts throw IOException instead of the last item.
    private sealed class FlakyExtractor : ExtractorBase<int, Report>
    {
        private readonly int[] _items;
        private readonly int _failingAttempts;


        public FlakyExtractor(int[] items, int failingAttempts)
        {
            _items = items;
            _failingAttempts = failingAttempts;
        }


        public int Attempts { get; private set; }


        protected override async IAsyncEnumerable<int> ExtractWorkerAsync([EnumeratorCancellation] CancellationToken token)
        {
            Attempts++;
            var attempt = Attempts;
            for (var i = 0; i < _items.Length; i++)
            {
                await Task.Yield();
                token.ThrowIfCancellationRequested();
                if (attempt <= _failingAttempts && i == _items.Length - 1)
                {
                    throw new IOException("transient");
                }

                IncrementCurrentItemCount();
                yield return _items[i];
            }
        }


        protected override Report CreateProgressReport() => new(CurrentItemCount);
    }



    // Collects what it loads; the first failingAttempts attempts throw IOException after one item.
    private sealed class FlakyLoader : LoaderBase<int, Report>
    {
        private readonly int _failingAttempts;


        public FlakyLoader(int failingAttempts) => _failingAttempts = failingAttempts;


        public int Attempts { get; private set; }


        public List<int> Loaded { get; } = new();


        protected override async Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token)
        {
            Attempts++;
            Loaded.Clear();
            await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
            {
                Loaded.Add(item);
                if (Attempts <= _failingAttempts)
                {
                    throw new IOException("transient");
                }
            }
        }


        protected override Report CreateProgressReport() => new(CurrentItemCount);
    }
}
