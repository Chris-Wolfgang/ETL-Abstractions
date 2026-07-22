using System.Globalization;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.Globalization;

/// <summary>
/// Globalization / CultureInfo invariance (#215). The default PR matrix runs only
/// <c>en-US</c>; the Turkish dotted-I, German decimal-comma, Chinese collation, and
/// Arabic digit-shaping bug classes surface only under non-<c>en-US</c> cultures.
///
/// <para>
/// Allowlist of intentionally culture-sensitive public members: <b>none</b>. The
/// base classes (<see cref="ExtractorBase{TSource, TProgress}"/>,
/// <see cref="LoaderBase{TDestination, TProgress}"/>,
/// <see cref="TransformerBase{TSource, TDestination, TProgress}"/>) orchestrate
/// <see cref="System.Collections.Generic.IAsyncEnumerable{T}"/> flows and integer
/// counters; <see cref="Report"/> exposes numeric metrics computed by
/// culture-invariant arithmetic. Field-level parsing/formatting is the format
/// packages' responsibility, not this one's. Every public member is therefore
/// culture-invariant by contract, and these tests assert it under six hostile
/// cultures. (Exception <em>message</em> text may format numbers via the current
/// culture — that is UI text, not behaviour, and is out of scope.)
/// </para>
/// </summary>
public sealed class CultureInvarianceTests
{
    public static IEnumerable<object[]> HostileCultures() =>
        new[] { "en-US", "tr-TR", "de-DE", "zh-CN", "ar-SA", "ja-JP" }
            .Select(culture => new object[] { culture });


    // Sanity guard: proves the scope actually swaps the culture (so the invariance
    // theories below are meaningful and not silently running under en-US). tr-TR
    // upper-cases 'i' to the dotted 'İ'; en-US to plain 'I'.
    [Theory]
    [InlineData("tr-TR", "İ")]
    [InlineData("en-US", "I")]
    public void CultureScope_swaps_the_current_culture(string culture, string expectedUpperI)
    {
        using var scope = new CultureScope(culture);

        Assert.Equal(culture, CultureInfo.CurrentCulture.Name);
        Assert.Equal(culture, CultureInfo.CurrentUICulture.Name);
        Assert.Equal(expectedUpperI, "i".ToUpper(CultureInfo.CurrentCulture));
    }


    [Theory]
    [MemberData(nameof(HostileCultures))]
    public void Report_metrics_are_culture_invariant(string culture)
    {
        using var scope = new CultureScope(culture);

        var report = new Report(50)
        {
            TotalItemCount = 100,
            Elapsed = TimeSpan.FromSeconds(10),
        };

        // Fixed results in every culture: 50/10 = 5/s, 50/100 = 50%, 50 left / 5 = 10s.
        Assert.Equal(5d, report.ItemsPerSecond);
        Assert.Equal(50d, report.PercentComplete);
        Assert.Equal(TimeSpan.FromSeconds(10), report.EstimatedRemaining);
    }


    [Theory]
    [MemberData(nameof(HostileCultures))]
    public async Task Pipeline_output_is_culture_invariant(string culture)
    {
        var loader = new CollectingLoader();

        using (new CultureScope(culture))
        {
            await Pipeline
                .Extract(new RangeExtractor(5))
                .Transform(new DoublingTransformer())
                .Load(loader)
                .RunAsync();
        }

        // Culture-neutral stages: any variance would be the orchestration injecting culture.
        Assert.Equal(new[] { 2, 4, 6, 8, 10 }, loader.Items);
    }


    // Sets CurrentCulture + CurrentUICulture for the scope's lifetime and restores both
    // on dispose, so a hostile culture can't leak into the next test (xunit reuses
    // threads). CurrentCulture flows across await, so it holds for the async pipeline run.
    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _culture;
        private readonly CultureInfo _uiCulture;

        public CultureScope(string culture)
        {
            _culture = CultureInfo.CurrentCulture;
            _uiCulture = CultureInfo.CurrentUICulture;

            var target = CultureInfo.GetCultureInfo(culture);
            CultureInfo.CurrentCulture = target;
            CultureInfo.CurrentUICulture = target;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _culture;
            CultureInfo.CurrentUICulture = _uiCulture;
        }
    }


    private sealed class RangeExtractor : IExtractAsync<int>
    {
        private readonly int _count;

        public RangeExtractor(int count) => _count = count;

        public async IAsyncEnumerable<int> ExtractAsync()
        {
            for (var i = 1; i <= _count; i++)
            {
                yield return i;
                await Task.Yield();
            }
        }
    }


    private sealed class DoublingTransformer : ITransformAsync<int, int>
    {
        public async IAsyncEnumerable<int> TransformAsync(IAsyncEnumerable<int> items)
        {
            await foreach (var item in items)
            {
                yield return item * 2;
            }
        }
    }


    private sealed class CollectingLoader : ILoadAsync<int>
    {
        private readonly List<int> _items = new();

        public IReadOnlyList<int> Items => _items;

        public async Task LoadAsync(IAsyncEnumerable<int> items)
        {
            await foreach (var item in items)
            {
                _items.Add(item);
            }
        }
    }
}
