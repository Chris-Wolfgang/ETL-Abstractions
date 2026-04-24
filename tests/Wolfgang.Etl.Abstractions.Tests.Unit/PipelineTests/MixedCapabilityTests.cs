using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;
using Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests.TestDoubles;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests;

/// <summary>
/// Tests that mix capability combinations in a single chain. The purpose is to exercise every
/// combination of stage-type transitions — <c>IExtractStage.Transform(bare/cancel/progress/full)</c>
/// and <c>ITransformStage.Transform(bare/cancel/progress/full)</c> and the four <c>Load</c>
/// overloads from both stage types. These transitions are internal plumbing but they are
/// how real pipelines compose stages of varied capabilities.
/// </summary>
public class MixedCapabilityTests
{
    // ---------------------------------------------------------------
    // IExtractStage.Transform -> each of 4 transformer capabilities
    // ---------------------------------------------------------------

    [Fact]
    public async Task Bare_extract_then_each_transformer_kind_then_bare_load()
    {
        var extractor = new BareExtractor<int>(new[] { 1, 2, 3 });
        var t1 = new BareTransformer<int, int>(x => x + 1);
        var t2 = new CancelOnlyTransformer<int, int>(x => x + 10);
        var t3 = new ProgressOnlyTransformer<int, int, string>(x => x + 100, "t3");
        var t4 = new FullTransformer<int, int, string>(x => x + 1000, "t4");
        var loader = new BareLoader<int>();

        await Pipeline
            .Extract(extractor)
            .Transform(t1)
            .Transform(t2)
            .Transform(t3)
            .Transform(t4)
            .Load(loader)
            .RunAsync();

        // 1 -> 2 -> 12 -> 112 -> 1112
        // 2 -> 3 -> 13 -> 113 -> 1113
        // 3 -> 4 -> 14 -> 114 -> 1114
        Assert.Equal(new[] { 1112, 1113, 1114 }, loader.Loaded);
    }


    // ---------------------------------------------------------------
    // ITransformStage.Load — all 4 loader capability overloads
    // ---------------------------------------------------------------

    [Fact]
    public async Task Transform_then_Load_with_bare_loader()
    {
        var extractor = new BareExtractor<int>(new[] { 1, 2 });
        var transformer = new BareTransformer<int, int>(x => x * 10);
        var loader = new BareLoader<int>();

        await Pipeline
            .Extract(extractor)
            .Transform(transformer)
            .Load(loader)
            .RunAsync();

        Assert.Equal(new[] { 10, 20 }, loader.Loaded);
    }


    [Fact]
    public async Task Transform_then_Load_with_cancel_only_loader()
    {
        var extractor = new BareExtractor<int>(new[] { 1, 2 });
        var transformer = new BareTransformer<int, int>(x => x * 10);
        var loader = new CancelOnlyLoader<int>();

        await Pipeline
            .Extract(extractor)
            .Transform(transformer)
            .Load(loader)
            .RunAsync();

        Assert.Equal(new[] { 10, 20 }, loader.Loaded);
        Assert.True(loader.TokenOverloadWasCalled);
    }


    [Fact]
    public async Task Transform_then_Load_with_progress_only_loader_and_progress()
    {
        var reports = new List<string>();
        var progress = new SynchronousProgress<string>(reports.Add);
        var extractor = new BareExtractor<int>(new[] { 1, 2 });
        var transformer = new BareTransformer<int, int>(x => x * 10);
        var loader = new ProgressOnlyLoader<int, string>("l");

        await Pipeline
            .Extract(extractor)
            .Transform(transformer)
            .Load(loader).WithProgress(progress)
            .RunAsync();

        Assert.Equal(new[] { 10, 20 }, loader.Loaded);
        Assert.True(loader.ProgressOverloadWasCalled);
        Assert.Equal(2, reports.Count);
    }


    [Fact]
    public async Task Transform_then_Load_with_full_loader_and_progress()
    {
        var reports = new List<string>();
        var progress = new SynchronousProgress<string>(reports.Add);
        var extractor = new BareExtractor<int>(new[] { 1, 2 });
        var transformer = new BareTransformer<int, int>(x => x * 10);
        var loader = new FullLoader<int, string>("l");

        await Pipeline
            .Extract(extractor)
            .Transform(transformer)
            .Load(loader).WithProgress(progress)
            .RunAsync();

        Assert.Equal(new[] { 10, 20 }, loader.Loaded);
        Assert.True(loader.FullOverloadWasCalled);
    }


    // ---------------------------------------------------------------
    // ExtractStageWithProgress delegation — Transform/Load without WithProgress called first
    //
    // These verify the intermediate-overload paths on ExtractStageWithProgress that delegate
    // to a fresh ExtractStage using the no-progress source.
    // ---------------------------------------------------------------

    [Fact]
    public async Task Full_extract_without_WithProgress_Transform_bare_then_Load_bare()
    {
        var extractor = new FullExtractor<int, string>(new[] { 1, 2 }, "e");
        var transformer = new BareTransformer<int, int>(x => x + 100);
        var loader = new BareLoader<int>();

        await Pipeline
            .Extract(extractor)
            .Transform(transformer)
            .Load(loader)
            .RunAsync();

        Assert.Equal(new[] { 101, 102 }, loader.Loaded);
    }


    [Fact]
    public async Task Full_extract_without_WithProgress_Transform_progress_then_Load_bare()
    {
        var reports = new List<string>();
        var progress = new SynchronousProgress<string>(reports.Add);
        var extractor = new FullExtractor<int, string>(new[] { 1, 2 }, "e");
        var transformer = new ProgressOnlyTransformer<int, int, string>(x => x + 100, "t");
        var loader = new BareLoader<int>();

        await Pipeline
            .Extract(extractor)
            .Transform(transformer).WithProgress(progress)
            .Load(loader)
            .RunAsync();

        Assert.Equal(new[] { 101, 102 }, loader.Loaded);
        Assert.True(transformer.ProgressOverloadWasCalled);
    }


    [Fact]
    public async Task Full_extract_without_WithProgress_Transform_full_then_Load_bare()
    {
        var reports = new List<string>();
        var progress = new SynchronousProgress<string>(reports.Add);
        var extractor = new FullExtractor<int, string>(new[] { 1, 2 }, "e");
        var transformer = new FullTransformer<int, int, string>(x => x + 100, "t");
        var loader = new BareLoader<int>();

        await Pipeline
            .Extract(extractor)
            .Transform(transformer).WithProgress(progress)
            .Load(loader)
            .RunAsync();

        Assert.Equal(new[] { 101, 102 }, loader.Loaded);
        Assert.True(transformer.FullOverloadWasCalled);
    }


    [Fact]
    public async Task Full_extract_without_WithProgress_Load_progress()
    {
        var reports = new List<string>();
        var progress = new SynchronousProgress<string>(reports.Add);
        var extractor = new FullExtractor<int, string>(new[] { 1, 2 }, "e");
        var loader = new ProgressOnlyLoader<int, string>("l");

        await Pipeline
            .Extract(extractor)
            .Load(loader).WithProgress(progress)
            .RunAsync();

        Assert.Equal(new[] { 1, 2 }, loader.Loaded);
        Assert.True(loader.ProgressOverloadWasCalled);
    }


    [Fact]
    public async Task Full_extract_without_WithProgress_Load_full()
    {
        var reports = new List<string>();
        var progress = new SynchronousProgress<string>(reports.Add);
        var extractor = new FullExtractor<int, string>(new[] { 1, 2 }, "e");
        var loader = new FullLoader<int, string>("l");

        await Pipeline
            .Extract(extractor)
            .Load(loader).WithProgress(progress)
            .RunAsync();

        Assert.Equal(new[] { 1, 2 }, loader.Loaded);
        Assert.True(loader.FullOverloadWasCalled);
    }


    // ---------------------------------------------------------------
    // TransformStageWithProgress delegation — Transform/Load without WithProgress called first
    // ---------------------------------------------------------------

    [Fact]
    public async Task Full_transform_without_WithProgress_then_Transform_bare_then_Load()
    {
        var extractor = new BareExtractor<int>(new[] { 1, 2 });
        var t1 = new FullTransformer<int, int, string>(x => x + 10, "t1");
        var t2 = new BareTransformer<int, int>(x => x + 100);
        var loader = new BareLoader<int>();

        await Pipeline
            .Extract(extractor)
            .Transform(t1)
            .Transform(t2)
            .Load(loader)
            .RunAsync();

        Assert.Equal(new[] { 111, 112 }, loader.Loaded);
    }


    [Fact]
    public async Task Full_transform_without_WithProgress_then_Transform_cancel_then_Load()
    {
        var extractor = new BareExtractor<int>(new[] { 1, 2 });
        var t1 = new FullTransformer<int, int, string>(x => x + 10, "t1");
        var t2 = new CancelOnlyTransformer<int, int>(x => x + 100);
        var loader = new BareLoader<int>();

        await Pipeline
            .Extract(extractor)
            .Transform(t1)
            .Transform(t2)
            .Load(loader)
            .RunAsync();

        Assert.Equal(new[] { 111, 112 }, loader.Loaded);
    }


    [Fact]
    public async Task Full_transform_without_WithProgress_then_Transform_progress_then_Load()
    {
        var reports = new List<string>();
        var progress = new SynchronousProgress<string>(reports.Add);
        var extractor = new BareExtractor<int>(new[] { 1, 2 });
        var t1 = new FullTransformer<int, int, string>(x => x + 10, "t1");
        var t2 = new ProgressOnlyTransformer<int, int, string>(x => x + 100, "t2");
        var loader = new BareLoader<int>();

        await Pipeline
            .Extract(extractor)
            .Transform(t1)
            .Transform(t2).WithProgress(progress)
            .Load(loader)
            .RunAsync();

        Assert.Equal(new[] { 111, 112 }, loader.Loaded);
        Assert.True(t2.ProgressOverloadWasCalled);
    }


    [Fact]
    public async Task Full_transform_without_WithProgress_then_Transform_full_then_Load()
    {
        var reports = new List<string>();
        var progress = new SynchronousProgress<string>(reports.Add);
        var extractor = new BareExtractor<int>(new[] { 1, 2 });
        var t1 = new FullTransformer<int, int, string>(x => x + 10, "t1");
        var t2 = new FullTransformer<int, int, string>(x => x + 100, "t2");
        var loader = new BareLoader<int>();

        await Pipeline
            .Extract(extractor)
            .Transform(t1)
            .Transform(t2).WithProgress(progress)
            .Load(loader)
            .RunAsync();

        Assert.Equal(new[] { 111, 112 }, loader.Loaded);
        Assert.True(t2.FullOverloadWasCalled);
    }


    [Fact]
    public async Task Full_transform_without_WithProgress_then_Load_cancel_only()
    {
        var extractor = new BareExtractor<int>(new[] { 1, 2 });
        var t1 = new FullTransformer<int, int, string>(x => x + 10, "t1");
        var loader = new CancelOnlyLoader<int>();

        await Pipeline
            .Extract(extractor)
            .Transform(t1)
            .Load(loader)
            .RunAsync();

        Assert.Equal(new[] { 11, 12 }, loader.Loaded);
        Assert.True(loader.TokenOverloadWasCalled);
    }


    [Fact]
    public async Task Full_transform_without_WithProgress_then_Load_progress_only()
    {
        var reports = new List<string>();
        var progress = new SynchronousProgress<string>(reports.Add);
        var extractor = new BareExtractor<int>(new[] { 1, 2 });
        var t1 = new FullTransformer<int, int, string>(x => x + 10, "t1");
        var loader = new ProgressOnlyLoader<int, string>("l");

        await Pipeline
            .Extract(extractor)
            .Transform(t1)
            .Load(loader).WithProgress(progress)
            .RunAsync();

        Assert.Equal(new[] { 11, 12 }, loader.Loaded);
        Assert.True(loader.ProgressOverloadWasCalled);
    }


    [Fact]
    public async Task Full_transform_without_WithProgress_then_Load_full()
    {
        var reports = new List<string>();
        var progress = new SynchronousProgress<string>(reports.Add);
        var extractor = new BareExtractor<int>(new[] { 1, 2 });
        var t1 = new FullTransformer<int, int, string>(x => x + 10, "t1");
        var loader = new FullLoader<int, string>("l");

        await Pipeline
            .Extract(extractor)
            .Transform(t1)
            .Load(loader).WithProgress(progress)
            .RunAsync();

        Assert.Equal(new[] { 11, 12 }, loader.Loaded);
        Assert.True(loader.FullOverloadWasCalled);
    }
}
