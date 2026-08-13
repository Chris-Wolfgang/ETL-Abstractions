using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.TestKit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit.Globalization;

/// <summary>
/// Verifies the doubles are culture-invariant (#134): the same input must produce
/// the same output regardless of the ambient <see cref="CultureInfo.CurrentCulture"/>
/// / <see cref="CultureInfo.CurrentUICulture"/>. The suite runs the extract →
/// transform → load pipeline under a matrix of hostile cultures — <c>en-US</c>,
/// <c>tr-TR</c> (dotted-I), <c>de-DE</c> (decimal comma), <c>zh-CN</c>, <c>ar-SA</c>
/// (Hindi-Arabic digits, RTL), and <c>ja-JP</c> (full-width digits) — exercising the
/// number-separator, digit-shape, and case-folding bug classes.
/// </summary>
/// <remarks>
/// <b>Culture-sensitivity allowlist.</b> The doubles expose <em>no</em> public
/// method that is intentionally culture-sensitive: they move the caller's items
/// through the pipeline by reference/value without formatting or parsing them, and
/// the windowing / counting logic is pure integer arithmetic. The allowlist is
/// therefore empty and these tests assert the whole surface is invariant by
/// contract — if a future edit introduces culture-sensitive formatting (e.g. a
/// double that yields <c>value.ToString()</c> under the ambient culture), the
/// non-<c>en-US</c> rows fail.
/// </remarks>
public class CultureInvarianceTests
{
    /// <summary>
    /// Hostile cultures the pipeline runs under. <c>en-US</c> is the baseline;
    /// the rest each stress a distinct globalization bug class.
    /// </summary>
    public static readonly IEnumerable<object[]> HostileCultures = new[]
    {
        new object[] { "en-US" },
        new object[] { "tr-TR" },
        new object[] { "de-DE" },
        new object[] { "zh-CN" },
        new object[] { "ar-SA" },
        new object[] { "ja-JP" },
    };

    [Theory]
    [MemberData(nameof(HostileCultures))]
    public async Task Extractor_windowing_is_culture_invariant(string culture)
    {
        using var _ = new CultureSwapper(culture);

        var items = Enumerable.Range(0, 20).ToList();
        using var extractor = new TestExtractor<int>(items)
        {
            SkipItemCount = 5,
            MaximumItemCount = 7,
        };

        var results = new List<int>();
        await foreach (var item in extractor.ExtractAsync())
        {
            results.Add(item);
        }

        // The window is fixed integer arithmetic — identical under every culture.
        Assert.Equal(Enumerable.Range(5, 7), results);
    }

    [Theory]
    [MemberData(nameof(HostileCultures))]
    public async Task Full_pipeline_round_trips_decimals_identically_under_every_culture(string culture)
    {
        // decimal is the type most likely to expose a latent culture bug (comma vs
        // dot decimal separators); assert the doubles pass it through untouched.
        var items = new[] { 1.5m, 1000.25m, -3.14m, 0m, 999999.999m };

        List<decimal> collected;
        using (var _ = new CultureSwapper(culture))
        {
            using var transformer = new TestTransformer<decimal>();
            using var loader = new TestLoader<decimal>(collectItems: true);
            await loader.LoadAsync(transformer.TransformAsync(ToAsync(items)));
            collected = loader.GetCollectedItems()!.ToList();
        }

        Assert.Equal(items, collected);
    }

    private static async IAsyncEnumerable<decimal> ToAsync(IEnumerable<decimal> items)
    {
        foreach (var item in items)
        {
            yield return item;
        }

        await Task.CompletedTask;
    }
}
