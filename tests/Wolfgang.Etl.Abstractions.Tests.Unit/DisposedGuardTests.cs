using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit;

/// <summary>
/// Covers the dispose contract added to the base classes: once disposed, the public entry points
/// throw <see cref="ObjectDisposedException"/>, and <c>Dispose</c> is idempotent. This gives the
/// <c>Dispose(bool)</c> idempotency guard (<c>if (_disposed) …; _disposed = true;</c>) an observable
/// effect — a mutation that negates the guard, drops its block, or skips the assignment leaves the
/// instance "not disposed" and the throw never happens.
/// </summary>
public sealed class DisposedGuardTests
{
    private static async IAsyncEnumerable<int> Empty()
    {
        await Task.CompletedTask;
        yield break;
    }


    private static readonly IProgress<EtlProgress> Progress = new NoOpProgress();


    [Fact]
    public void LoaderBase_every_LoadAsync_overload_throws_after_Dispose()
    {
        var loader = new NoOpLoader();
        loader.Dispose();

        Assert.Throws<ObjectDisposedException>(() => { _ = loader.LoadAsync(Empty()); });
        Assert.Throws<ObjectDisposedException>(() => { _ = loader.LoadAsync(Empty(), CancellationToken.None); });
        Assert.Throws<ObjectDisposedException>(() => { _ = loader.LoadAsync(Empty(), Progress); });
        Assert.Throws<ObjectDisposedException>(() => { _ = loader.LoadAsync(Empty(), Progress, CancellationToken.None); });
    }


    [Fact]
    public async Task LoaderBase_LoadAsync_throws_after_DisposeAsync()
    {
        var loader = new NoOpLoader();
        await loader.DisposeAsync();

        Assert.Throws<ObjectDisposedException>(() => { _ = loader.LoadAsync(Empty()); });
    }


    [Fact]
    public void ExtractorBase_every_ExtractAsync_overload_throws_after_Dispose()
    {
        var extractor = new NoOpExtractor();
        extractor.Dispose();

        Assert.Throws<ObjectDisposedException>(() => extractor.ExtractAsync());
        Assert.Throws<ObjectDisposedException>(() => extractor.ExtractAsync(CancellationToken.None));
        Assert.Throws<ObjectDisposedException>(() => extractor.ExtractAsync(Progress));
        Assert.Throws<ObjectDisposedException>(() => extractor.ExtractAsync(Progress, CancellationToken.None));
    }


    [Fact]
    public async Task ExtractorBase_ExtractAsync_throws_after_DisposeAsync()
    {
        var extractor = new NoOpExtractor();
        await extractor.DisposeAsync();

        Assert.Throws<ObjectDisposedException>(() => extractor.ExtractAsync());
    }


    [Fact]
    public void TransformerBase_every_TransformAsync_overload_throws_after_Dispose()
    {
        var transformer = new NoOpTransformer();
        transformer.Dispose();

        Assert.Throws<ObjectDisposedException>(() => transformer.TransformAsync(Empty()));
        Assert.Throws<ObjectDisposedException>(() => transformer.TransformAsync(Empty(), CancellationToken.None));
        Assert.Throws<ObjectDisposedException>(() => transformer.TransformAsync(Empty(), Progress));
        Assert.Throws<ObjectDisposedException>(() => transformer.TransformAsync(Empty(), Progress, CancellationToken.None));
    }


    [Fact]
    public async Task TransformerBase_TransformAsync_throws_after_DisposeAsync()
    {
        var transformer = new NoOpTransformer();
        await transformer.DisposeAsync();

        Assert.Throws<ObjectDisposedException>(() => transformer.TransformAsync(Empty()));
    }


    [Fact]
    public void Dispose_is_idempotent()
    {
        var loader = new NoOpLoader();

        loader.Dispose();
        var second = Record.Exception(() => loader.Dispose());

        Assert.Null(second);
    }


    [Fact]
    public void A_live_component_does_not_throw()
    {
        // Guards against a mutant that makes ThrowIfDisposed always throw (or the guard being
        // inverted): a freshly-constructed, undisposed loader must accept a call.
        var loader = new NoOpLoader();

        var exception = Record.Exception(() => { _ = loader.LoadAsync(Empty()); });

        Assert.Null(exception);
    }


    [ExcludeFromCodeCoverage]
    private sealed class NoOpProgress : IProgress<EtlProgress>
    {
        public void Report(EtlProgress value)
        {
        }
    }


    [ExcludeFromCodeCoverage]
    private sealed class NoOpLoader : LoaderBase<int, EtlProgress>
    {
        protected override Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token)
            => Task.CompletedTask;

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    [ExcludeFromCodeCoverage]
    private sealed class NoOpExtractor : ExtractorBase<int, EtlProgress>
    {
#pragma warning disable CS1998 // async iterator with no yielded items is intentional
        protected override async IAsyncEnumerable<int> ExtractWorkerAsync(
            [EnumeratorCancellation] CancellationToken token)
        {
            yield break;
        }
#pragma warning restore CS1998

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    [ExcludeFromCodeCoverage]
    private sealed class NoOpTransformer : TransformerBase<int, int, EtlProgress>
    {
#pragma warning disable CS1998 // async iterator with no yielded items is intentional
        protected override async IAsyncEnumerable<int> TransformWorkerAsync(
            IAsyncEnumerable<int> items,
            [EnumeratorCancellation] CancellationToken token)
        {
            yield break;
        }
#pragma warning restore CS1998

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }
}
