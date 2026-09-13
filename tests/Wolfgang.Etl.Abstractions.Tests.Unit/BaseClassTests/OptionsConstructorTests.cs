using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit;

/// <summary>
/// Verifies the options-record constructors added to the three base stages (ADR-0009): a
/// supplied record is applied to the base configuration, a <see langword="null"/> record and
/// the parameterless constructor both leave the documented defaults in place, and the
/// parameterless constructor — a shipped API member — still exists for derived types that
/// never pass options.
/// </summary>
public class OptionsConstructorTests
{
    private static readonly Func<ItemErrorContext, ItemErrorAction> SkipPolicy = _ => ItemErrorAction.Skip;

    // ------------------------------------------------------------------
    // Doubles: one per stage kind, each with both constructors
    // ------------------------------------------------------------------

    private sealed class OptionsExtractor : ExtractorBase<int, EtlProgress>
    {
        public OptionsExtractor()
        {
        }

        public OptionsExtractor(ExtractorOptions? options) : base(options)
        {
        }

        protected override async IAsyncEnumerable<int> ExtractWorkerAsync([EnumeratorCancellation] CancellationToken token)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            yield break;
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }

    private sealed class OptionsLoader : LoaderBase<int, EtlProgress>
    {
        public OptionsLoader()
        {
        }

        public OptionsLoader(LoaderOptions? options) : base(options)
        {
        }

        protected override Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token) => Task.CompletedTask;

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }

    private sealed class OptionsTransformer : TransformerBase<int, int, EtlProgress>
    {
        public OptionsTransformer()
        {
        }

        public OptionsTransformer(TransformerOptions? options) : base(options)
        {
        }

        protected override async IAsyncEnumerable<int> TransformWorkerAsync(IAsyncEnumerable<int> items, [EnumeratorCancellation] CancellationToken token)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            yield break;
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // ExtractorBase
    // ------------------------------------------------------------------

    [Fact]
    public void ExtractorBase_when_constructed_with_options_applies_every_member()
    {
        var options = new ExtractorOptions
        {
            ReportingInterval = 25,
            MaximumItemCount  = 500,
            SkipItemCount     = 3,
            ErrorPolicy       = SkipPolicy
        };

        using var sut = new OptionsExtractor(options);

        Assert.Equal(25, sut.ReportingInterval);
        Assert.Equal(500, sut.MaximumItemCount);
        Assert.Equal(3, sut.SkipItemCount);
        Assert.Same(SkipPolicy, sut.ErrorPolicy);
    }



    [Fact]
    public void ExtractorBase_when_constructed_with_null_options_keeps_the_defaults()
    {
        using var sut = new OptionsExtractor(null);

        Assert.Equal(1_000, sut.ReportingInterval);
        Assert.Equal(int.MaxValue, sut.MaximumItemCount);
        Assert.Equal(0, sut.SkipItemCount);
        Assert.Same(DefaultItemErrorPolicy.Abort, sut.ErrorPolicy);
    }



    [Fact]
    public void ExtractorBase_when_constructed_parameterless_matches_null_options()
    {
        using var parameterless = new OptionsExtractor();
        using var nullOptions   = new OptionsExtractor(null);

        Assert.Equal(nullOptions.ReportingInterval, parameterless.ReportingInterval);
        Assert.Equal(nullOptions.MaximumItemCount, parameterless.MaximumItemCount);
        Assert.Equal(nullOptions.SkipItemCount, parameterless.SkipItemCount);
        Assert.Same(nullOptions.ErrorPolicy, parameterless.ErrorPolicy);
    }



    // ------------------------------------------------------------------
    // LoaderBase
    // ------------------------------------------------------------------

    [Fact]
    public void LoaderBase_when_constructed_with_options_applies_every_member()
    {
        var options = new LoaderOptions
        {
            ReportingInterval = 25,
            MaximumItemCount  = 500,
            SkipItemCount     = 3,
            ErrorPolicy       = SkipPolicy
        };

        using var sut = new OptionsLoader(options);

        Assert.Equal(25, sut.ReportingInterval);
        Assert.Equal(500, sut.MaximumItemCount);
        Assert.Equal(3, sut.SkipItemCount);
        Assert.Same(SkipPolicy, sut.ErrorPolicy);
    }



    [Fact]
    public void LoaderBase_when_constructed_with_null_options_keeps_the_defaults()
    {
        using var sut = new OptionsLoader(null);

        Assert.Equal(1_000, sut.ReportingInterval);
        Assert.Equal(int.MaxValue, sut.MaximumItemCount);
        Assert.Equal(0, sut.SkipItemCount);
        Assert.Same(DefaultItemErrorPolicy.Abort, sut.ErrorPolicy);
    }



    [Fact]
    public void LoaderBase_when_constructed_parameterless_matches_null_options()
    {
        using var parameterless = new OptionsLoader();
        using var nullOptions   = new OptionsLoader(null);

        Assert.Equal(nullOptions.ReportingInterval, parameterless.ReportingInterval);
        Assert.Equal(nullOptions.MaximumItemCount, parameterless.MaximumItemCount);
        Assert.Equal(nullOptions.SkipItemCount, parameterless.SkipItemCount);
        Assert.Same(nullOptions.ErrorPolicy, parameterless.ErrorPolicy);
    }



    // ------------------------------------------------------------------
    // TransformerBase
    // ------------------------------------------------------------------

    [Fact]
    public void TransformerBase_when_constructed_with_options_applies_every_member()
    {
        var options = new TransformerOptions
        {
            ReportingInterval = 25,
            MaximumItemCount  = 500,
            SkipItemCount     = 3,
            ErrorPolicy       = SkipPolicy
        };

        using var sut = new OptionsTransformer(options);

        Assert.Equal(25, sut.ReportingInterval);
        Assert.Equal(500, sut.MaximumItemCount);
        Assert.Equal(3, sut.SkipItemCount);
        Assert.Same(SkipPolicy, sut.ErrorPolicy);
    }



    [Fact]
    public void TransformerBase_when_constructed_with_null_options_keeps_the_defaults()
    {
        using var sut = new OptionsTransformer(null);

        Assert.Equal(1_000, sut.ReportingInterval);
        Assert.Equal(int.MaxValue, sut.MaximumItemCount);
        Assert.Equal(0, sut.SkipItemCount);
        Assert.Same(DefaultItemErrorPolicy.Abort, sut.ErrorPolicy);
    }



    [Fact]
    public void TransformerBase_when_constructed_parameterless_matches_null_options()
    {
        using var parameterless = new OptionsTransformer();
        using var nullOptions   = new OptionsTransformer(null);

        Assert.Equal(nullOptions.ReportingInterval, parameterless.ReportingInterval);
        Assert.Equal(nullOptions.MaximumItemCount, parameterless.MaximumItemCount);
        Assert.Equal(nullOptions.SkipItemCount, parameterless.SkipItemCount);
        Assert.Same(nullOptions.ErrorPolicy, parameterless.ErrorPolicy);
    }
}
