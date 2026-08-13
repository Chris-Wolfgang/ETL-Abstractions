using System.Collections.Generic;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Exercises <see cref="EtlPipelineContractTests{TItem, TProgress}"/> by composing a plain source
/// with a <see cref="TestLoader{T}"/> sink, confirming the contract base drives a real
/// <see cref="EtlPipeline"/> end-to-end.
/// </summary>
public class TestLoaderPipelineContractTests
    : EtlPipelineContractTests<int, Report>
{
    protected override IReadOnlyList<int> CreateSourceItems() =>
        new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };



    protected override LoaderBase<int, Report> CreateSink() =>
        new TestLoader<int>(collectItems: true);



    protected override IReadOnlyList<int>? GetLoadedItems() =>
        ((TestLoader<int>)Sink).GetCollectedItems();
}
