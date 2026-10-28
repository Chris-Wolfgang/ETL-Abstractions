using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;
using Xunit;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.EtlPipelineTests;

/// <summary>
/// Per-stage typed progress on <see cref="EtlPipeline"/> (#382): the extractor, transformer and loader
/// overloads that take an <see cref="IProgress{T}"/> of the stage's own progress type.
/// </summary>
public class EtlPipelineStageProgressTests
{
    [Fact]
    public async Task From_with_progress_reports_the_extractors_own_progress()
    {
        var reports = new List<EtlProgress>();
        var loader = new CollectingLoader<int>();

        await EtlPipeline
            .Create()
            .From(new SeededExtractor<int>(new[] { 1, 2, 3 }), new SynchronousProgress<EtlProgress>(reports.Add))
            .To(loader)
            .RunAsync();

        Assert.Equal(new[] { 1, 2, 3 }, loader.Loaded);
        Assert.Equal(3, reports[reports.Count - 1].CurrentItemCount);
    }



    [Fact]
    public async Task Through_with_progress_transforms_and_reports_the_transformers_own_progress()
    {
        var reports = new List<EtlProgress>();
        var loader = new CollectingLoader<int>();

        await EtlPipeline
            .Create()
            .From(new SeededExtractor<int>(new[] { 1, 2, 3 }))
            .Through(new DoublingTransformer(), new SynchronousProgress<EtlProgress>(reports.Add))
            .To(loader)
            .RunAsync();

        Assert.Equal(new[] { 2, 4, 6 }, loader.Loaded);
        Assert.Equal(3, reports[reports.Count - 1].CurrentItemCount);
    }



    [Fact]
    public async Task To_with_progress_reports_the_loaders_own_progress_and_the_pipeline_aggregate()
    {
        var loaderReports = new List<EtlProgress>();
        var pipelineReports = new List<EtlPipelineProgress>();
        var loader = new CollectingLoader<int>();

        await EtlPipeline
            .Create()
            .From(new SeededExtractor<int>(new[] { 1, 2 }))
            .To(loader, new SynchronousProgress<EtlProgress>(loaderReports.Add))
            .RunAsync(new SynchronousProgress<EtlPipelineProgress>(pipelineReports.Add));

        Assert.Equal(new[] { 1, 2 }, loader.Loaded);
        Assert.Equal(2, loaderReports[loaderReports.Count - 1].CurrentItemCount);
        Assert.Equal(2, pipelineReports[pipelineReports.Count - 1].LoadedItemCount);
    }



    [Fact]
    public async Task Through_and_To_with_progress_adapt_a_pipeline_that_EtlPipeline_did_not_build()
    {
        var transformReports = new List<EtlProgress>();
        var loadReports = new List<EtlProgress>();
        var loader = new CollectingLoader<int>();
        var foreign = new ForeignPipeline(new[] { 5, 6 });

        await foreign
            .Through(new DoublingTransformer(), new SynchronousProgress<EtlProgress>(transformReports.Add))
            .To(loader, new SynchronousProgress<EtlProgress>(loadReports.Add))
            .RunAsync();

        Assert.Equal(new[] { 10, 12 }, loader.Loaded);
        Assert.Equal(2, transformReports[transformReports.Count - 1].CurrentItemCount);
        Assert.Equal(2, loadReports[loadReports.Count - 1].CurrentItemCount);
    }



    [Fact]
    public void From_with_progress_when_extractor_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>
        (
            () => EtlPipeline.Create().From((ExtractorBase<int, EtlProgress>)null!, new SynchronousProgress<EtlProgress>(_ => { }))
        );

        Assert.Equal("extractor", ex.ParamName);
    }



    [Fact]
    public void From_with_progress_when_progress_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>
        (
            () => EtlPipeline.Create().From(new SeededExtractor<int>(new[] { 1 }), (IProgress<EtlProgress>)null!)
        );

        Assert.Equal("progress", ex.ParamName);
    }



    [Fact]
    public void Through_with_progress_when_pipeline_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>
        (
            () => ((IEtlPipeline<int>)null!).Through(new DoublingTransformer(), new SynchronousProgress<EtlProgress>(_ => { }))
        );

        Assert.Equal("pipeline", ex.ParamName);
    }



    [Fact]
    public void Through_with_progress_when_transformer_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>
        (
            () => Source().Through((TransformerBase<int, int, EtlProgress>)null!, new SynchronousProgress<EtlProgress>(_ => { }))
        );

        Assert.Equal("transformer", ex.ParamName);
    }



    [Fact]
    public void Through_with_progress_when_progress_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>
        (
            () => Source().Through(new DoublingTransformer(), (IProgress<EtlProgress>)null!)
        );

        Assert.Equal("progress", ex.ParamName);
    }



    [Fact]
    public void To_with_progress_when_pipeline_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>
        (
            () => ((IEtlPipeline<int>)null!).To(new CollectingLoader<int>(), new SynchronousProgress<EtlProgress>(_ => { }))
        );

        Assert.Equal("pipeline", ex.ParamName);
    }



    [Fact]
    public void To_with_progress_when_loader_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>
        (
            () => Source().To((LoaderBase<int, EtlProgress>)null!, new SynchronousProgress<EtlProgress>(_ => { }))
        );

        Assert.Equal("loader", ex.ParamName);
    }



    [Fact]
    public void To_with_progress_when_progress_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>
        (
            () => Source().To(new CollectingLoader<int>(), (IProgress<EtlProgress>)null!)
        );

        Assert.Equal("progress", ex.ParamName);
    }



    private static IEtlPipeline<int> Source() =>
        EtlPipeline
            .Create()
            .From(new SeededExtractor<int>(new[] { 1 }));



    // A TransformerBase double that doubles each item and counts it.
    private sealed class DoublingTransformer : TransformerBase<int, int, EtlProgress>
    {
        protected override async IAsyncEnumerable<int> TransformWorkerAsync
        (
            IAsyncEnumerable<int> items,
            [EnumeratorCancellation] CancellationToken token
        )
        {
            await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
            {
                IncrementCurrentItemCount();
                yield return item * 2;
            }
        }


        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }



    // An IEtlPipeline that EtlPipeline did not build: the typed-progress overloads must adapt it
    // through AsAsyncEnumerable. Only that member is part of the scenario under test.
    private sealed class ForeignPipeline : IEtlPipeline<int>
    {
        private readonly int[] _items;


        public ForeignPipeline(int[] items) => _items = items;


        public async IAsyncEnumerable<int> AsAsyncEnumerable([EnumeratorCancellation] CancellationToken token = default)
        {
            foreach (var item in _items)
            {
                await Task.Yield();
                token.ThrowIfCancellationRequested();
                yield return item;
            }
        }


        [ExcludeFromCodeCoverage]
        public IEtlPipeline<TOut> Through<TOut>(ITransformAsync<int, TOut> transformer)
            where TOut : notnull => throw new NotSupportedException();


        [ExcludeFromCodeCoverage]
        public IEtlPipeline<TOut> Through<TOut>(ITransformWithCancellationAsync<int, TOut> transformer)
            where TOut : notnull => throw new NotSupportedException();


        [ExcludeFromCodeCoverage]
        public IEtlPipeline<TOut> Through<TOut>(Func<IAsyncEnumerable<int>, IAsyncEnumerable<TOut>> stage)
            where TOut : notnull => throw new NotSupportedException();


        [ExcludeFromCodeCoverage]
        public IEtlPipeline<TOut> Through<TOut>(Func<IAsyncEnumerable<int>, CancellationToken, IAsyncEnumerable<TOut>> stage)
            where TOut : notnull => throw new NotSupportedException();


        [ExcludeFromCodeCoverage]
        public IEtlPipelineSink To<TProgress>(LoaderBase<int, TProgress> loader)
            where TProgress : notnull => throw new NotSupportedException();
    }
}
