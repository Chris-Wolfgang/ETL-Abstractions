namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// A convenience <see cref="TransformerBase{TSource, TDestination, TProgress}"/> that reports progress
/// with the built-in <see cref="Report"/> type and supplies a default <see cref="CreateProgressReport"/>,
/// so a derived transformer only has to implement <c>TransformWorkerAsync</c>. Use this instead of the
/// three-type-parameter base when you don't need a custom progress-report type — it removes the
/// progress-record and <c>CreateProgressReport</c> boilerplate. Override
/// <see cref="CreateProgressReport"/> if you want to enrich the report (for example set a known total).
/// </summary>
/// <typeparam name="TSource">The type of the source object.</typeparam>
/// <typeparam name="TDestination">The type of the destination object.</typeparam>
public abstract class TransformerBase<TSource, TDestination> : TransformerBase<TSource, TDestination, Report>
    where TSource : notnull
    where TDestination : notnull
{
    /// <summary>
    /// Builds a <see cref="Report"/> snapshot from the current item count and timing. Override to add
    /// more detail (for example a known <see cref="Report.TotalItemCount"/>).
    /// </summary>
    /// <returns>A <see cref="Report"/> for the current run.</returns>
    protected override Report CreateProgressReport() => new(CurrentItemCount, StartedAt, Elapsed);
}
