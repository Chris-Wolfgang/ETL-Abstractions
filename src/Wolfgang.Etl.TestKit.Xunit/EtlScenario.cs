namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Entry point for building a fluent end-to-end ETL scenario over the <c>Wolfgang.Etl.TestKit</c>
/// doubles and asserting its final state in a single expression. See <see cref="EtlScenario{T}"/>.
/// </summary>
/// <example>
/// <code>
/// await EtlScenario
///     .From(1, 2, 3, 4)
///     .WithExtractorFault(index: 2, new FormatException("bad row"))
///     .RunAndAssertAsync(expectedLoaded: new[] { 1, 2, 4 }, expectedErrors: 1);
/// </code>
/// </example>
public static class EtlScenario
{
    /// <summary>Starts a scenario whose source streams <paramref name="items"/>.</summary>
    /// <typeparam name="T">The item type flowing through the pipeline.</typeparam>
    /// <param name="items">The items the source produces.</param>
    /// <returns>A fluent <see cref="EtlScenario{T}"/> builder.</returns>
    public static EtlScenario<T> From<T>(params T[] items)
        where T : notnull =>
        new EtlScenario<T>(items);
}
