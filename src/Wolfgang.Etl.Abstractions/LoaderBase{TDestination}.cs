namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// A convenience <see cref="LoaderBase{TDestination, TProgress}"/> that reports progress with the
/// built-in <see cref="Report"/> type and supplies a default <see cref="CreateProgressReport"/>, so a
/// derived loader only has to implement <c>LoadWorkerAsync</c>. Use this instead of the
/// two-type-parameter base when you don't need a custom progress-report type — it removes the
/// progress-record and <c>CreateProgressReport</c> boilerplate. Override
/// <see cref="CreateProgressReport"/> if you want to enrich the report (for example set a known total).
/// </summary>
/// <typeparam name="TDestination">The type of the object being loaded.</typeparam>
public abstract class LoaderBase<TDestination> : LoaderBase<TDestination, Report>
    where TDestination : notnull
{
    /// <summary>
    /// Builds a <see cref="Report"/> snapshot from the current item count and timing. Override to add
    /// more detail (for example a known <see cref="Report.TotalItemCount"/>).
    /// </summary>
    /// <returns>A <see cref="Report"/> for the current run.</returns>
    protected override Report CreateProgressReport() => new(CurrentItemCount, StartedAt, Elapsed);
}
