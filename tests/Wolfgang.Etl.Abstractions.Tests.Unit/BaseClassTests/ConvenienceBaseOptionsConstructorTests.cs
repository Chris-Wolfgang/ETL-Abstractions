using System.Runtime.CompilerServices;

namespace Wolfgang.Etl.Abstractions.Tests.Unit;

/// <summary>
/// Verifies that the single-progress-type convenience bases — <see cref="ExtractorBase{TSource}"/>,
/// <see cref="LoaderBase{TDestination}"/> and <see cref="TransformerBase{TSource, TDestination}"/> —
/// forward an options record to their two- and three-type-parameter bases. Constructors are not
/// inherited, so without their own forwarding constructors a stage built on a convenience base could
/// not be configured through an options record at all.
/// </summary>
public class ConvenienceBaseOptionsConstructorTests
{
    private static readonly Func<ItemErrorContext, ItemErrorAction> SkipPolicy = _ => ItemErrorAction.Skip;

    private sealed class ConvenienceExtractor : ExtractorBase<int>
    {
        public ConvenienceExtractor()
        {
        }

        public ConvenienceExtractor(ExtractorOptions? options) : base(options)
        {
        }

        protected override async IAsyncEnumerable<int> ExtractWorkerAsync([EnumeratorCancellation] CancellationToken token)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            yield break;
        }
    }

    private sealed class ConvenienceLoader : LoaderBase<int>
    {
        public ConvenienceLoader()
        {
        }

        public ConvenienceLoader(LoaderOptions? options) : base(options)
        {
        }

        protected override Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token) => Task.CompletedTask;
    }

    private sealed class ConvenienceTransformer : TransformerBase<int, int>
    {
        public ConvenienceTransformer()
        {
        }

        public ConvenienceTransformer(TransformerOptions? options) : base(options)
        {
        }

        protected override async IAsyncEnumerable<int> TransformWorkerAsync(IAsyncEnumerable<int> items, [EnumeratorCancellation] CancellationToken token)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            yield break;
        }
    }



    [Fact]
    public void ExtractorBase_convenience_when_constructed_with_options_forwards_every_member()
    {
        var options = new ExtractorOptions { ReportingInterval = 25, MaximumItemCount = 500, SkipItemCount = 3, ErrorPolicy = SkipPolicy };

        using var sut = new ConvenienceExtractor(options);

        Assert.Equal(25, sut.ReportingInterval);
        Assert.Equal(500, sut.MaximumItemCount);
        Assert.Equal(3, sut.SkipItemCount);
        Assert.Same(SkipPolicy, sut.ErrorPolicy);
    }



    [Fact]
    public void ExtractorBase_convenience_when_constructed_parameterless_matches_null_options()
    {
        using var parameterless = new ConvenienceExtractor();
        using var nullOptions   = new ConvenienceExtractor(null);

        Assert.Equal(nullOptions.ReportingInterval, parameterless.ReportingInterval);
        Assert.Equal(nullOptions.MaximumItemCount, parameterless.MaximumItemCount);
        Assert.Equal(nullOptions.SkipItemCount, parameterless.SkipItemCount);
        Assert.Same(nullOptions.ErrorPolicy, parameterless.ErrorPolicy);
    }



    [Fact]
    public void LoaderBase_convenience_when_constructed_with_options_forwards_every_member()
    {
        var options = new LoaderOptions { ReportingInterval = 25, MaximumItemCount = 500, SkipItemCount = 3, ErrorPolicy = SkipPolicy };

        using var sut = new ConvenienceLoader(options);

        Assert.Equal(25, sut.ReportingInterval);
        Assert.Equal(500, sut.MaximumItemCount);
        Assert.Equal(3, sut.SkipItemCount);
        Assert.Same(SkipPolicy, sut.ErrorPolicy);
    }



    [Fact]
    public void LoaderBase_convenience_when_constructed_parameterless_matches_null_options()
    {
        using var parameterless = new ConvenienceLoader();
        using var nullOptions   = new ConvenienceLoader(null);

        Assert.Equal(nullOptions.ReportingInterval, parameterless.ReportingInterval);
        Assert.Equal(nullOptions.MaximumItemCount, parameterless.MaximumItemCount);
        Assert.Equal(nullOptions.SkipItemCount, parameterless.SkipItemCount);
        Assert.Same(nullOptions.ErrorPolicy, parameterless.ErrorPolicy);
    }



    [Fact]
    public void TransformerBase_convenience_when_constructed_with_options_forwards_every_member()
    {
        var options = new TransformerOptions { ReportingInterval = 25, MaximumItemCount = 500, SkipItemCount = 3, ErrorPolicy = SkipPolicy };

        using var sut = new ConvenienceTransformer(options);

        Assert.Equal(25, sut.ReportingInterval);
        Assert.Equal(500, sut.MaximumItemCount);
        Assert.Equal(3, sut.SkipItemCount);
        Assert.Same(SkipPolicy, sut.ErrorPolicy);
    }



    [Fact]
    public void TransformerBase_convenience_when_constructed_parameterless_matches_null_options()
    {
        using var parameterless = new ConvenienceTransformer();
        using var nullOptions   = new ConvenienceTransformer(null);

        Assert.Equal(nullOptions.ReportingInterval, parameterless.ReportingInterval);
        Assert.Equal(nullOptions.MaximumItemCount, parameterless.MaximumItemCount);
        Assert.Equal(nullOptions.SkipItemCount, parameterless.SkipItemCount);
        Assert.Same(nullOptions.ErrorPolicy, parameterless.ErrorPolicy);
    }
}
