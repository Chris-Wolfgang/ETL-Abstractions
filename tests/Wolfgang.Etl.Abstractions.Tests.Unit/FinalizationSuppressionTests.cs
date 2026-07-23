using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit;

/// <summary>
/// Verifies that disposing a base ETL component calls <see cref="GC.SuppressFinalize"/> so a derived
/// type that owns unmanaged resources via a finalizer is not finalized after an explicit dispose.
/// The base types ship without a finalizer, so this is invisible unless a derived type declares one —
/// which is exactly what these doubles do. A mutation that drops the <c>SuppressFinalize</c> call lets
/// the finalizer run and trips the assertion.
/// </summary>
public sealed class FinalizationSuppressionTests
{
    [Fact]
    public void LoaderBase_Dispose_suppresses_finalization()
    {
        var finalizerRan = new StrongBox<bool>(false);
        CreateAndDisposeLoader(finalizerRan);

        ForceFinalization();

        Assert.False(finalizerRan.Value, "Dispose() must call GC.SuppressFinalize so the finalizer never runs");
    }


    [Fact]
    public async Task LoaderBase_DisposeAsync_suppresses_finalization()
    {
        var finalizerRan = new StrongBox<bool>(false);
        await CreateAndDisposeAsyncLoader(finalizerRan);

        ForceFinalization();

        Assert.False(finalizerRan.Value, "DisposeAsync() must call GC.SuppressFinalize so the finalizer never runs");
    }


    [Fact]
    public void ExtractorBase_Dispose_suppresses_finalization()
    {
        var finalizerRan = new StrongBox<bool>(false);
        CreateAndDisposeExtractor(finalizerRan);

        ForceFinalization();

        Assert.False(finalizerRan.Value, "Dispose() must call GC.SuppressFinalize so the finalizer never runs");
    }


    [Fact]
    public async Task ExtractorBase_DisposeAsync_suppresses_finalization()
    {
        var finalizerRan = new StrongBox<bool>(false);
        await CreateAndDisposeAsyncExtractor(finalizerRan);

        ForceFinalization();

        Assert.False(finalizerRan.Value, "DisposeAsync() must call GC.SuppressFinalize so the finalizer never runs");
    }


    [Fact]
    public void TransformerBase_Dispose_suppresses_finalization()
    {
        var finalizerRan = new StrongBox<bool>(false);
        CreateAndDisposeTransformer(finalizerRan);

        ForceFinalization();

        Assert.False(finalizerRan.Value, "Dispose() must call GC.SuppressFinalize so the finalizer never runs");
    }


    [Fact]
    public async Task TransformerBase_DisposeAsync_suppresses_finalization()
    {
        var finalizerRan = new StrongBox<bool>(false);
        await CreateAndDisposeAsyncTransformer(finalizerRan);

        ForceFinalization();

        Assert.False(finalizerRan.Value, "DisposeAsync() must call GC.SuppressFinalize so the finalizer never runs");
    }


    // Separate non-inlined methods so the instance is unreachable (and therefore collectable) once
    // they return — the disposed instance must not be rooted by the calling test frame.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateAndDisposeLoader(StrongBox<bool> finalizerRan)
    {
        var loader = new FinalizableLoader(finalizerRan);
        loader.Dispose();
    }


    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task CreateAndDisposeAsyncLoader(StrongBox<bool> finalizerRan)
    {
        var loader = new FinalizableLoader(finalizerRan);
        await loader.DisposeAsync();
    }


    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateAndDisposeExtractor(StrongBox<bool> finalizerRan)
    {
        var extractor = new FinalizableExtractor(finalizerRan);
        extractor.Dispose();
    }


    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task CreateAndDisposeAsyncExtractor(StrongBox<bool> finalizerRan)
    {
        var extractor = new FinalizableExtractor(finalizerRan);
        await extractor.DisposeAsync();
    }


    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateAndDisposeTransformer(StrongBox<bool> finalizerRan)
    {
        var transformer = new FinalizableTransformer(finalizerRan);
        transformer.Dispose();
    }


    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task CreateAndDisposeAsyncTransformer(StrongBox<bool> finalizerRan)
    {
        var transformer = new FinalizableTransformer(finalizerRan);
        await transformer.DisposeAsync();
    }


    private static void ForceFinalization()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }


    [ExcludeFromCodeCoverage]
    private sealed class FinalizableLoader : LoaderBase<int, EtlProgress>
    {
        private readonly StrongBox<bool> _finalizerRan;

        public FinalizableLoader(StrongBox<bool> finalizerRan) => _finalizerRan = finalizerRan;

        ~FinalizableLoader() => _finalizerRan.Value = true;

        protected override Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token)
            => Task.CompletedTask;

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    [ExcludeFromCodeCoverage]
    private sealed class FinalizableExtractor : ExtractorBase<int, EtlProgress>
    {
        private readonly StrongBox<bool> _finalizerRan;

        public FinalizableExtractor(StrongBox<bool> finalizerRan) => _finalizerRan = finalizerRan;

        ~FinalizableExtractor() => _finalizerRan.Value = true;

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
    private sealed class FinalizableTransformer : TransformerBase<int, int, EtlProgress>
    {
        private readonly StrongBox<bool> _finalizerRan;

        public FinalizableTransformer(StrongBox<bool> finalizerRan) => _finalizerRan = finalizerRan;

        ~FinalizableTransformer() => _finalizerRan.Value = true;

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
