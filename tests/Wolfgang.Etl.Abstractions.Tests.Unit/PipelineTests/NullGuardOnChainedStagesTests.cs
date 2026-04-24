using System;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests.TestDoubles;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests;

/// <summary>
/// Null-argument guards on the mid-chain <see cref="ITransformStage{TSource}"/> overloads.
/// <see cref="PipelineBehaviorTests"/> covers the guards on the initial
/// <see cref="IExtractStage{TSource}"/> overloads; these cover the guards that fire one stage
/// later, after at least one <c>Transform</c> has already been appended.
/// </summary>
public class NullGuardOnChainedStagesTests
{
    private static ITransformStage<int> MidChainStage() => Pipeline
        .Extract(new BareExtractor<int>(new[] { 1 }))
        .Transform(new BareTransformer<int, int>(x => x));


    [Fact]
    public void TransformStage_Transform_bare_null_throws()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => MidChainStage().Transform((ITransformAsync<int, int>)null!)
        );
    }


    [Fact]
    public void TransformStage_Transform_cancel_null_throws()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => MidChainStage().Transform((ITransformWithCancellationAsync<int, int>)null!)
        );
    }


    [Fact]
    public void TransformStage_Transform_progress_null_throws()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => MidChainStage().Transform((ITransformWithProgressAsync<int, int, string>)null!)
        );
    }


    [Fact]
    public void TransformStage_Transform_full_null_throws()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => MidChainStage().Transform((ITransformWithProgressAndCancellationAsync<int, int, string>)null!)
        );
    }


    [Fact]
    public void TransformStage_Load_bare_null_throws()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => MidChainStage().Load((ILoadAsync<int>)null!)
        );
    }


    [Fact]
    public void TransformStage_Load_cancel_null_throws()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => MidChainStage().Load((ILoadWithCancellationAsync<int>)null!)
        );
    }


    [Fact]
    public void TransformStage_Load_progress_null_throws()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => MidChainStage().Load((ILoadWithProgressAsync<int, string>)null!)
        );
    }


    [Fact]
    public void TransformStage_Load_full_null_throws()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => MidChainStage().Load((ILoadWithProgressAndCancellationAsync<int, string>)null!)
        );
    }
}
