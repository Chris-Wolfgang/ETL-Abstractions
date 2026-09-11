using System.ComponentModel;

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
    /// Initializes a new instance of the <see cref="TransformerBase{TSource, TDestination}"/> class with the documented
    /// default configuration.
    /// </summary>
    /// <remarks>
    /// Retained for binary compatibility with derived assemblies compiled before the
    /// options-record constructor existed — constructors are not inherited, so this convenience
    /// base needs its own. Hidden from IntelliSense and scheduled for removal once every package
    /// in the family has rebuilt; new code should pass an <see cref="TransformerOptions"/> record, or omit
    /// the argument to take the defaults. See ADR-0009.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected TransformerBase()
    {
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="TransformerBase{TSource, TDestination}"/> class with the supplied
    /// configuration, forwarding it to <see cref="TransformerBase{TSource, TDestination, TProgress}"/>.
    /// </summary>
    /// <param name="options">
    /// Construction-time configuration. When <see langword="null"/> — or omitted — the
    /// documented defaults apply.
    /// </param>
    protected TransformerBase(TransformerOptions? options = null) : base(options)
    {
    }



    /// <summary>
    /// Builds a <see cref="Report"/> snapshot from the current item count and timing. Override to add
    /// more detail (for example a known <see cref="Report.TotalItemCount"/>).
    /// </summary>
    /// <returns>A <see cref="Report"/> for the current run.</returns>
    protected override Report CreateProgressReport() => new(CurrentItemCount, StartedAt, Elapsed);
}
