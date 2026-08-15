using System.Runtime.CompilerServices;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

/// <summary>
/// Covers the #348 <c>WorkerResilience</c> seam on the three base stages: the default is a no-op
/// passthrough, an assigned wrapper is consulted by the base <c>WrapWorkerExecution</c>, the wrapper
/// receives a re-invocable worker factory (the retry primitive), and a null assignment throws.
/// </summary>
public class WorkerResilienceTests
{
    // ---------- Extractor ----------

    [Fact]
    public async Task Extractor_default_WorkerResilience_is_a_passthrough_Async()
    {
        var sut = new CountingExtractor();

        var items = await Drain(sut.ExtractAsync(CancellationToken.None));

        Assert.Equal(new[] { 1, 2, 3 }, items);
        Assert.Equal(1, sut.WorkerInvocations);       // worker ran exactly once
    }


    [Fact]
    public async Task Extractor_assigned_WorkerResilience_wraps_the_worker_Async()
    {
        var wrapped = false;
        var sut = new CountingExtractor
        {
            WorkerResilience = (factory, token) =>
            {
                wrapped = true;
                return factory(token);
            },
        };

        await Drain(sut.ExtractAsync(CancellationToken.None));

        Assert.True(wrapped);
    }


    [Fact]
    public async Task Extractor_WorkerResilience_receives_a_reinvocable_factory_Async()
    {
        Func<CancellationToken, IAsyncEnumerable<int>>? captured = null;
        var sut = new CountingExtractor
        {
            WorkerResilience = (factory, token) =>
            {
                captured = factory;
                return factory(token);
            },
        };

        await Drain(sut.ExtractAsync(CancellationToken.None));
        var again = await Drain(captured!(CancellationToken.None));   // re-invoke -> a fresh stream (the retry primitive)

        Assert.Equal(new[] { 1, 2, 3 }, again);
        Assert.Equal(2, sut.WorkerInvocations);
    }


    [Fact]
    public void Extractor_WorkerResilience_assigned_null_throws()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new CountingExtractor { WorkerResilience = null! });
        Assert.Equal("value", ex.ParamName);
    }


    // ---------- Loader ----------

    [Fact]
    public async Task Loader_assigned_WorkerResilience_wraps_the_worker_Async()
    {
        var wrapped = false;
        var sut = new CountingLoader
        {
            WorkerResilience = (factory, token) =>
            {
                wrapped = true;
                return factory(token);
            },
        };

        await sut.LoadAsync(AsyncSource(1, 2, 3), CancellationToken.None);

        Assert.True(wrapped);
        Assert.Equal(new[] { 1, 2, 3 }, sut.Loaded);
    }


    [Fact]
    public void Loader_WorkerResilience_assigned_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CountingLoader { WorkerResilience = null! });
    }


    // ---------- Transformer ----------

    [Fact]
    public async Task Transformer_assigned_WorkerResilience_wraps_the_worker_Async()
    {
        var wrapped = false;
        var sut = new CountingTransformer
        {
            WorkerResilience = (factory, token) =>
            {
                wrapped = true;
                return factory(token);
            },
        };

        var items = await Drain(sut.TransformAsync(AsyncSource(1, 2, 3), CancellationToken.None));

        Assert.True(wrapped);
        Assert.Equal(new[] { 10, 20, 30 }, items);
    }


    [Fact]
    public void Transformer_WorkerResilience_assigned_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CountingTransformer { WorkerResilience = null! });
    }


    // ---------- helpers / doubles ----------

    private static async IAsyncEnumerable<int> AsyncSource(params int[] items)
    {
        foreach (var item in items)
        {
            await Task.Yield();
            yield return item;
        }
    }


    private static async Task<List<int>> Drain(IAsyncEnumerable<int> source)
    {
        var result = new List<int>();
        await foreach (var item in source.ConfigureAwait(false))
        {
            result.Add(item);
        }

        return result;
    }


    private sealed class CountingExtractor : ExtractorBase<int>
    {
        public int WorkerInvocations { get; private set; }

        protected override async IAsyncEnumerable<int> ExtractWorkerAsync([EnumeratorCancellation] CancellationToken token)
        {
            WorkerInvocations++;
            foreach (var i in new[] { 1, 2, 3 })
            {
                await Task.Yield();
                IncrementCurrentItemCount();
                yield return i;
            }
        }
    }


    private sealed class CountingLoader : LoaderBase<int>
    {
        public List<int> Loaded { get; } = new();

        protected override async Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token)
        {
            await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
            {
                Loaded.Add(item);
                IncrementCurrentItemCount();
            }
        }
    }


    private sealed class CountingTransformer : TransformerBase<int, int>
    {
        protected override async IAsyncEnumerable<int> TransformWorkerAsync(
            IAsyncEnumerable<int> items, [EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
            {
                IncrementCurrentItemCount();
                yield return item * 10;
            }
        }
    }
}
