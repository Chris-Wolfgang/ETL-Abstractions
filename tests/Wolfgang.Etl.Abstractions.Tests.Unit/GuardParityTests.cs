using System.Globalization;
using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit;

/// <summary>
/// The argument guards must throw the same exception on every target framework: on .NET 6.0+ /
/// .NET 8.0+ the runtime's <c>ThrowIfNull</c> / <c>ThrowIfLessThan</c>, elsewhere the internal
/// polyfills. These tests pin <c>ParamName</c>, <c>ActualValue</c> and the message format so a
/// divergence on any target slot fails.
/// </summary>
public class GuardParityTests
{
    private sealed class GuardExtractor : ExtractorBase<int, EtlProgress>
    {
        protected override async IAsyncEnumerable<int> ExtractWorkerAsync([EnumeratorCancellation] CancellationToken token)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            yield break;
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }

    private sealed class GuardLoader : LoaderBase<int, EtlProgress>
    {
        protected override Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token) => Task.CompletedTask;

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }

    private sealed class GuardTransformer : TransformerBase<int, int, EtlProgress>
    {
        protected override async IAsyncEnumerable<int> TransformWorkerAsync(IAsyncEnumerable<int> items, [EnumeratorCancellation] CancellationToken token)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            yield break;
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }

    private static async IAsyncEnumerable<int> EmptyAsync()
    {
        await Task.CompletedTask.ConfigureAwait(false);
        yield break;
    }



    public static TheoryData<Func<object>, int, int> OutOfRangeGuards => new()
    {
        { () => new ExtractorOptions { ReportingInterval = 0 }, 0, 1 },
        { () => new ExtractorOptions { MaximumItemCount = 0 }, 0, 1 },
        { () => new ExtractorOptions { SkipItemCount = -1 }, -1, 0 },
        { () => new LoaderOptions { ReportingInterval = 0 }, 0, 1 },
        { () => new LoaderOptions { MaximumItemCount = 0 }, 0, 1 },
        { () => new LoaderOptions { SkipItemCount = -1 }, -1, 0 },
        { () => new TransformerOptions { ReportingInterval = 0 }, 0, 1 },
        { () => new TransformerOptions { MaximumItemCount = 0 }, 0, 1 },
        { () => new TransformerOptions { SkipItemCount = -1 }, -1, 0 },
    };



    [Theory]
    [MemberData(nameof(OutOfRangeGuards))]
    public void Range_guard_when_value_is_below_the_floor_throws_with_ParamName_ActualValue_and_the_runtime_message(Func<object> construct, int actual, int floor)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(construct);

        Assert.Equal("value", ex.ParamName);
        Assert.Equal(actual, ex.ActualValue);
        Assert.StartsWith
        (
            string.Format(CultureInfo.InvariantCulture, "value ('{0}') must be greater than or equal to '{1}'.", actual, floor),
            ex.Message,
            StringComparison.Ordinal
        );
    }



    [Fact]
    public void GuardExtractor_ReportingInterval_setter_when_value_is_below_the_floor_throws_with_ParamName_ActualValue_and_the_runtime_message()
    {
        using var sut = new GuardExtractor();

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => sut.ReportingInterval = 0);

        Assert.Equal("value", ex.ParamName);
        Assert.Equal(0, ex.ActualValue);
        Assert.StartsWith("value ('0') must be greater than or equal to '1'.", ex.Message, StringComparison.Ordinal);
    }



    [Fact]
    public void GuardLoader_MaximumItemCount_setter_when_value_is_below_the_floor_throws_with_ParamName_ActualValue_and_the_runtime_message()
    {
        using var sut = new GuardLoader();

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => sut.MaximumItemCount = 0);

        Assert.Equal("value", ex.ParamName);
        Assert.Equal(0, ex.ActualValue);
        Assert.StartsWith("value ('0') must be greater than or equal to '1'.", ex.Message, StringComparison.Ordinal);
    }



    [Fact]
    public void GuardTransformer_SkipItemCount_setter_when_value_is_below_the_floor_throws_with_ParamName_ActualValue_and_the_runtime_message()
    {
        using var sut = new GuardTransformer();

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => sut.SkipItemCount = -1);

        Assert.Equal("value", ex.ParamName);
        Assert.Equal(-1, ex.ActualValue);
        Assert.StartsWith("value ('-1') must be greater than or equal to '0'.", ex.Message, StringComparison.Ordinal);
    }



    [Fact]
    public void ExtractAsync_when_progress_is_null_throws_ArgumentNullException_naming_progress()
    {
        using var sut = new GuardExtractor();

        var ex = Assert.Throws<ArgumentNullException>(() => sut.ExtractAsync((IProgress<EtlProgress>)null!));

        Assert.Equal("progress", ex.ParamName);
    }



    [Fact]
    public async Task LoadAsync_when_items_is_null_throws_ArgumentNullException_naming_items()
    {
        using var sut = new GuardLoader();

        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => sut.LoadAsync((IAsyncEnumerable<int>)null!));

        Assert.Equal("items", ex.ParamName);
    }



    [Fact]
    public async Task LoadAsync_when_progress_is_null_throws_ArgumentNullException_naming_progress()
    {
        using var sut = new GuardLoader();

        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => sut.LoadAsync(EmptyAsync(), (IProgress<EtlProgress>)null!));

        Assert.Equal("progress", ex.ParamName);
    }



    [Fact]
    public void TransformAsync_when_items_is_null_throws_ArgumentNullException_naming_items()
    {
        using var sut = new GuardTransformer();

        var ex = Assert.Throws<ArgumentNullException>(() => sut.TransformAsync((IAsyncEnumerable<int>)null!));

        Assert.Equal("items", ex.ParamName);
    }
}
