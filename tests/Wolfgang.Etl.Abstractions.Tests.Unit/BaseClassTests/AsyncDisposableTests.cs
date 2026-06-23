using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

/// <summary>
/// Verifies the <see cref="IAsyncDisposable"/> / <see cref="IDisposable"/> support added to the
/// base classes (issue #92): the default base implementation is a safe no-op, and a derived
/// override of <c>Dispose(bool)</c> is invoked through both <c>Dispose()</c> and <c>DisposeAsync()</c>.
/// </summary>
public class AsyncDisposableTests
{
    private sealed class PlainExtractor : ExtractorBase<int, EtlProgress>
    {
        protected override async IAsyncEnumerable<int> ExtractWorkerAsync([EnumeratorCancellation] CancellationToken token)
        {
            await Task.CompletedTask;
            yield break;
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    private sealed class ResourceOwningExtractor : ExtractorBase<int, EtlProgress>
    {
        public int DisposeCount { get; private set; }

        protected override async IAsyncEnumerable<int> ExtractWorkerAsync([EnumeratorCancellation] CancellationToken token)
        {
            await Task.CompletedTask;
            yield break;
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);

        protected override void Dispose(bool disposing)
        {
            DisposeCount++;
            base.Dispose(disposing);
        }
    }


    [Fact]
    public void Base_classes_implement_the_disposal_interfaces()
    {
        var sut = new PlainExtractor();

        Assert.IsAssignableFrom<IDisposable>(sut);
        Assert.IsAssignableFrom<IAsyncDisposable>(sut);
    }


    [Fact]
    public async Task Default_DisposeAsync_is_a_safe_no_op()
    {
        var sut = new PlainExtractor();

        // Should not throw.
        await sut.DisposeAsync();
        sut.Dispose();
    }


    [Fact]
    public void Dispose_invokes_the_derived_override()
    {
        var sut = new ResourceOwningExtractor();

        sut.Dispose();

        Assert.Equal(1, sut.DisposeCount);
    }


    [Fact]
    public async Task DisposeAsync_invokes_the_derived_override()
    {
        var sut = new ResourceOwningExtractor();

        await sut.DisposeAsync();

        Assert.Equal(1, sut.DisposeCount);
    }


    [Fact]
    public async Task Await_using_disposes_a_resource_owning_extractor()
    {
        var sut = new ResourceOwningExtractor();

        await using (sut)
        {
            await foreach (var _ in sut.ExtractAsync())
            {
            }
        }

        Assert.Equal(1, sut.DisposeCount);
    }
}
