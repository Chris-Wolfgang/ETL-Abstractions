using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

/// <summary>
/// Covers the #94 retry seam: the <c>WrapWorkerExecution</c> hook on each base class. The default
/// implementation is a no-op (behaviour is unchanged), and an override receives a <em>re-invocable</em>
/// worker factory — calling it again produces a fresh worker run, which is what lets a resilience
/// strategy retry a transient failure. The seam is exercised on both the no-progress and the
/// with-progress worker paths of all three base classes.
/// </summary>
public class RetrySeamTests
{
    // ---------- Extractor ----------

    [Fact]
    public async Task Extractor_default_seam_yields_all_items_and_runs_the_worker_once()
    {
        var sut = new SeamExtractor(new[] { 1, 2, 3 });

        var items = await Drain(sut.ExtractAsync(CancellationToken.None));

        Assert.Equal(new[] { 1, 2, 3 }, items);
        Assert.Equal(1, sut.WorkerStartCount);   // default no-op invokes the factory exactly once
        Assert.Equal(1, sut.SeamCallCount);
    }


    [Fact]
    public async Task Extractor_override_re_invokes_the_factory_producing_a_fresh_run_each_time()
    {
        var sut = new SeamExtractor(new[] { 1, 2, 3 }) { WorkerRuns = 3 };

        var items = await Drain(sut.ExtractAsync(CancellationToken.None));

        Assert.Equal(new[] { 1, 2, 3 }, items);  // only the final run is yielded
        Assert.Equal(3, sut.WorkerStartCount);    // factory re-invoked -> worker restarted 3 times
        Assert.Equal(1, sut.SeamCallCount);       // seam wraps the run once
    }


    [Fact]
    public async Task Extractor_seam_wraps_the_with_progress_path_too()
    {
        var sut = new SeamExtractor(new[] { 7, 8 }) { WorkerRuns = 2 };
        var progress = new SynchronousProgress<EtlProgress>(_ => { });

        var items = await Drain(sut.ExtractAsync(progress, CancellationToken.None));

        Assert.Equal(new[] { 7, 8 }, items);
        Assert.Equal(2, sut.WorkerStartCount);
        Assert.Equal(1, sut.SeamCallCount);
    }


    [Fact]
    public async Task Extractor_seam_can_retry_a_transient_failure()
    {
        // Worker throws on its first start, succeeds on the second; the retrying seam recovers.
        var sut = new RetryingExtractor(new[] { 5, 6 }, failuresBeforeSuccess: 1);

        var items = await Drain(sut.ExtractAsync(CancellationToken.None));

        Assert.Equal(new[] { 5, 6 }, items);
        Assert.Equal(2, sut.WorkerStartCount);
    }


    [Fact]
    public void Extractor_seam_throws_when_the_factory_is_null()
    {
        var sut = new SeamExtractor(Array.Empty<int>());

        Assert.Throws<ArgumentNullException>(() => sut.InvokeSeamWithNull());
    }


    // ---------- Loader ----------

    [Fact]
    public async Task Loader_default_seam_loads_all_items_and_runs_the_worker_once()
    {
        var sut = new SeamLoader();

        await sut.LoadAsync(AsyncSource(1, 2, 3), CancellationToken.None);

        Assert.Equal(new[] { 1, 2, 3 }, sut.Loaded);
        Assert.Equal(1, sut.WorkerStartCount);
        Assert.Equal(1, sut.SeamCallCount);
    }


    [Fact]
    public async Task Loader_override_re_invokes_the_factory_producing_a_fresh_run_each_time()
    {
        var sut = new SeamLoader { WorkerRuns = 3 };

        await sut.LoadAsync(AsyncSource(1, 2), CancellationToken.None);

        Assert.Equal(3, sut.WorkerStartCount);
        Assert.Equal(1, sut.SeamCallCount);
    }


    [Fact]
    public async Task Loader_seam_wraps_the_with_progress_path_too()
    {
        var sut = new SeamLoader { WorkerRuns = 2 };
        var progress = new SynchronousProgress<EtlProgress>(_ => { });

        await sut.LoadAsync(AsyncSource(9), progress, CancellationToken.None);

        Assert.Equal(2, sut.WorkerStartCount);
        Assert.Equal(1, sut.SeamCallCount);
    }


    [Fact]
    public void Loader_seam_throws_when_the_factory_is_null()
    {
        var sut = new SeamLoader();

        Assert.Throws<ArgumentNullException>(() => { _ = sut.InvokeSeamWithNull(); });
    }


    // ---------- Transformer ----------

    [Fact]
    public async Task Transformer_default_seam_yields_all_items_and_runs_the_worker_once()
    {
        var sut = new SeamTransformer();

        var items = await Drain(sut.TransformAsync(AsyncSource(1, 2, 3), CancellationToken.None));

        Assert.Equal(new[] { 10, 20, 30 }, items);
        Assert.Equal(1, sut.WorkerStartCount);
        Assert.Equal(1, sut.SeamCallCount);
    }


    [Fact]
    public async Task Transformer_override_re_invokes_the_factory_producing_a_fresh_run_each_time()
    {
        var sut = new SeamTransformer { WorkerRuns = 3 };

        var items = await Drain(sut.TransformAsync(AsyncSource(4), CancellationToken.None));

        Assert.Equal(new[] { 40 }, items);   // only the final run is yielded
        Assert.Equal(3, sut.WorkerStartCount);
        Assert.Equal(1, sut.SeamCallCount);
    }


    [Fact]
    public async Task Transformer_seam_wraps_the_with_progress_path_too()
    {
        var sut = new SeamTransformer { WorkerRuns = 2 };
        var progress = new SynchronousProgress<EtlProgress>(_ => { });

        var items = await Drain(sut.TransformAsync(AsyncSource(5), progress, CancellationToken.None));

        Assert.Equal(new[] { 50 }, items);
        Assert.Equal(2, sut.WorkerStartCount);
        Assert.Equal(1, sut.SeamCallCount);
    }


    [Fact]
    public void Transformer_seam_throws_when_the_factory_is_null()
    {
        var sut = new SeamTransformer();

        Assert.Throws<ArgumentNullException>(() => sut.InvokeSeamWithNull());
    }


    // ---------- helpers ----------

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


    // ---------- doubles ----------

    // Extractor whose overridden seam invokes the factory (WorkerRuns - 1) discarded times then a
    // final real run, proving the factory produces a fresh worker run on each call. WorkerRuns == 1
    // (the default) leaves the seam a pass-through, exercising the base default no-op.
    [ExcludeFromCodeCoverage]
    private sealed class SeamExtractor : ExtractorBase<int, EtlProgress>
    {
        private readonly int[] _items;
        private int _workerStarts;

        public SeamExtractor(int[] items) => _items = items;

        public int WorkerRuns { get; init; } = 1;

        public int WorkerStartCount => Volatile.Read(ref _workerStarts);

        public int SeamCallCount { get; private set; }

        public IAsyncEnumerable<int> InvokeSeamWithNull() => base.WrapWorkerExecution(null!, CancellationToken.None);

        protected override async IAsyncEnumerable<int> ExtractWorkerAsync([EnumeratorCancellation] CancellationToken token)
        {
            Interlocked.Increment(ref _workerStarts);
            foreach (var item in _items)
            {
                await Task.Yield();
                IncrementCurrentItemCount();
                yield return item;
            }
        }

        protected override async IAsyncEnumerable<int> WrapWorkerExecution(
            Func<CancellationToken, IAsyncEnumerable<int>> workerFactory,
            [EnumeratorCancellation] CancellationToken token)
        {
            SeamCallCount++;
            if (WorkerRuns == 1)
            {
                await foreach (var item in base.WrapWorkerExecution(workerFactory, token).WithCancellation(token))
                {
                    yield return item;
                }

                yield break;
            }

            for (var run = 1; run < WorkerRuns; run++)
            {
                await foreach (var _ in workerFactory(token).WithCancellation(token))
                {
                    // discard – simulates a failed attempt being retried from scratch
                }
            }

            await foreach (var item in workerFactory(token).WithCancellation(token))
            {
                yield return item;
            }
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    // Extractor whose seam retries a genuine transient failure by re-invoking the factory.
    [ExcludeFromCodeCoverage]
    private sealed class RetryingExtractor : ExtractorBase<int, EtlProgress>
    {
        private readonly int[] _items;
        private readonly int _failuresBeforeSuccess;
        private int _workerStarts;

        public RetryingExtractor(int[] items, int failuresBeforeSuccess)
        {
            _items = items;
            _failuresBeforeSuccess = failuresBeforeSuccess;
        }

        public int WorkerStartCount => Volatile.Read(ref _workerStarts);

        protected override async IAsyncEnumerable<int> ExtractWorkerAsync([EnumeratorCancellation] CancellationToken token)
        {
            var attempt = Interlocked.Increment(ref _workerStarts);
            await Task.Yield();
            if (attempt <= _failuresBeforeSuccess)
            {
                throw new InvalidOperationException("transient");
            }

            foreach (var item in _items)
            {
                yield return item;
            }
        }

        protected override async IAsyncEnumerable<int> WrapWorkerExecution(
            Func<CancellationToken, IAsyncEnumerable<int>> workerFactory,
            [EnumeratorCancellation] CancellationToken token)
        {
            for (var attempt = 1; ; attempt++)
            {
                var buffer = new List<int>();
                var enumerator = workerFactory(token).GetAsyncEnumerator(token);
                var failed = false;
                try
                {
                    while (true)
                    {
                        try
                        {
                            if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                            {
                                break;
                            }
                        }
                        catch (InvalidOperationException) when (attempt <= _failuresBeforeSuccess)
                        {
                            failed = true;
                            break;
                        }

                        buffer.Add(enumerator.Current);
                    }
                }
                finally
                {
                    await enumerator.DisposeAsync().ConfigureAwait(false);
                }

                if (failed)
                {
                    continue;
                }

                foreach (var item in buffer)
                {
                    yield return item;
                }

                yield break;
            }
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    [ExcludeFromCodeCoverage]
    private sealed class SeamLoader : LoaderBase<int, EtlProgress>
    {
        private int _workerStarts;

        public List<int> Loaded { get; } = new();

        public int WorkerRuns { get; init; } = 1;

        public int WorkerStartCount => Volatile.Read(ref _workerStarts);

        public int SeamCallCount { get; private set; }

        public Task InvokeSeamWithNull() => base.WrapWorkerExecution(null!, CancellationToken.None);

        protected override async Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token)
        {
            Interlocked.Increment(ref _workerStarts);
            Loaded.Clear();
            await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
            {
                Loaded.Add(item);
                IncrementCurrentItemCount();
            }
        }

        protected override async Task WrapWorkerExecution(
            Func<CancellationToken, Task> workerFactory,
            CancellationToken token)
        {
            SeamCallCount++;
            if (WorkerRuns == 1)
            {
                await base.WrapWorkerExecution(workerFactory, token).ConfigureAwait(false);
                return;
            }

            for (var run = 1; run <= WorkerRuns; run++)
            {
                await workerFactory(token).ConfigureAwait(false);
            }
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    [ExcludeFromCodeCoverage]
    private sealed class SeamTransformer : TransformerBase<int, int, EtlProgress>
    {
        private int _workerStarts;

        public int WorkerRuns { get; init; } = 1;

        public int WorkerStartCount => Volatile.Read(ref _workerStarts);

        public int SeamCallCount { get; private set; }

        public IAsyncEnumerable<int> InvokeSeamWithNull() => base.WrapWorkerExecution(null!, CancellationToken.None);

        protected override async IAsyncEnumerable<int> TransformWorkerAsync(
            IAsyncEnumerable<int> items, [EnumeratorCancellation] CancellationToken token)
        {
            Interlocked.Increment(ref _workerStarts);
            await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
            {
                IncrementCurrentItemCount();
                yield return item * 10;
            }
        }

        protected override async IAsyncEnumerable<int> WrapWorkerExecution(
            Func<CancellationToken, IAsyncEnumerable<int>> workerFactory,
            [EnumeratorCancellation] CancellationToken token)
        {
            SeamCallCount++;
            if (WorkerRuns == 1)
            {
                await foreach (var item in base.WrapWorkerExecution(workerFactory, token).WithCancellation(token))
                {
                    yield return item;
                }

                yield break;
            }

            for (var run = 1; run < WorkerRuns; run++)
            {
                await foreach (var _ in workerFactory(token).WithCancellation(token))
                {
                    // discard
                }
            }

            await foreach (var item in workerFactory(token).WithCancellation(token))
            {
                yield return item;
            }
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }
}
