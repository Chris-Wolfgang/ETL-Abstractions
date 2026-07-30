namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// A convenience <see cref="ExtractorBase{TSource, TProgress}"/> that reports progress with the
/// built-in <see cref="Report"/> type and supplies a default <see cref="CreateProgressReport"/>, so a
/// derived extractor only has to implement <c>ExtractWorkerAsync</c>. Use this instead of the
/// two-type-parameter base when you don't need a custom progress-report type — it removes the
/// progress-record and <c>CreateProgressReport</c> boilerplate. Override
/// <see cref="CreateProgressReport"/> if you want to enrich the report (for example set a known total).
/// </summary>
/// <typeparam name="TSource">The type of the object being extracted.</typeparam>
public abstract class ExtractorBase<TSource> : ExtractorBase<TSource, Report>
    where TSource : notnull
{
    /// <summary>
    /// Builds a <see cref="Report"/> snapshot from the current item count and timing. Override to add
    /// more detail (for example a known <see cref="Report.TotalItemCount"/>).
    /// </summary>
    /// <returns>A <see cref="Report"/> for the current run.</returns>
    protected override Report CreateProgressReport() => new(CurrentItemCount, StartedAt, Elapsed);
}
