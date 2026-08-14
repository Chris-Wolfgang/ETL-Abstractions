using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

public class TimeSourceExtensionsTests
{
    // ------------------------------------------------------------------
    // Null-argument guards (all three overloads)
    // ------------------------------------------------------------------

    [Fact]
    public void WithTimeSource_when_extractor_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => ((ExtractorBase<int, Report>)null!).WithTimeSource(new ManualTimeSource())
        );
    }



    [Fact]
    public void WithTimeSource_when_loader_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => ((LoaderBase<int, Report>)null!).WithTimeSource(new ManualTimeSource())
        );
    }



    [Fact]
    public void WithTimeSource_when_transformer_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => ((TransformerBase<int, int, Report>)null!).WithTimeSource(new ManualTimeSource())
        );
    }



    [Fact]
    public void WithTimeSource_loader_overload_when_timeSource_is_null_throws_ArgumentNullException()
    {
        var loader = new TestLoader<int>(collectItems: false);

        Assert.Throws<ArgumentNullException>(() => loader.WithTimeSource(null!));
    }



    [Fact]
    public void WithTimeSource_transformer_overload_when_timeSource_is_null_throws_ArgumentNullException()
    {
        var transformer = new TestTransformer<int>();

        Assert.Throws<ArgumentNullException>(() => transformer.WithTimeSource(null!));
    }



    // ------------------------------------------------------------------
    // Loader / transformer overloads drive deterministic timing
    // ------------------------------------------------------------------

    [Fact]
    public async Task WithTimeSource_loader_overload_drives_deterministic_Report_timing()
    {
        var clock  = new ManualTimeSource();
        var loader = new ExposedLoader<int>();
        var same   = loader.WithTimeSource(clock);
        Assert.Same(loader, same);

        await loader.LoadAsync(new[] { 1, 2, 3, 4, 5 }.ToAsyncEnumerable(), CancellationToken.None);
        clock.Advance(TimeSpan.FromSeconds(5));

        Assert.Equal(TimeSpan.FromSeconds(5), loader.GetProgressReport().Elapsed);
    }



    [Fact]
    public async Task WithTimeSource_transformer_overload_drives_deterministic_Report_timing()
    {
        var clock       = new ManualTimeSource();
        var transformer = new ExposedTransformer<int>();
        var same        = transformer.WithTimeSource(clock);
        Assert.Same(transformer, same);

        await transformer.TransformAsync(new[] { 1, 2, 3 }.ToAsyncEnumerable(), CancellationToken.None).ToListAsync();
        clock.Advance(TimeSpan.FromSeconds(2));

        Assert.Equal(TimeSpan.FromSeconds(2), transformer.GetProgressReport().Elapsed);
    }



    private sealed class ExposedLoader<T> : TestLoader<T>
        where T : notnull
    {
        public ExposedLoader()
            : base(collectItems: false)
        {
        }



        public Report GetProgressReport() => CreateProgressReport();
    }



    private sealed class ExposedTransformer<T> : TestTransformer<T>
        where T : notnull
    {
        public Report GetProgressReport() => CreateProgressReport();
    }
}
