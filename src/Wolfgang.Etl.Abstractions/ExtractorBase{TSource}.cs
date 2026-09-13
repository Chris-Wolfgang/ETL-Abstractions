using System.ComponentModel;

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
    /// Initializes a new instance of the <see cref="ExtractorBase{TSource}"/> class with the documented
    /// default configuration.
    /// </summary>
    /// <remarks>
    /// Retained for binary compatibility with derived assemblies compiled before the
    /// options-record constructor existed — constructors are not inherited, so this convenience
    /// base needs its own. Hidden from IntelliSense and scheduled for removal once every package
    /// in the family has rebuilt; new code should pass an <see cref="ExtractorOptions"/> record, or omit
    /// the argument to take the defaults. See ADR-0009.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected ExtractorBase()
    {
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="ExtractorBase{TSource}"/> class with the supplied
    /// configuration, forwarding it to <see cref="ExtractorBase{TSource, TProgress}"/>.
    /// </summary>
    /// <param name="options">
    /// Construction-time configuration. When <see langword="null"/> — or omitted — the
    /// documented defaults apply.
    /// </param>
    protected ExtractorBase(ExtractorOptions? options = null) : base(options)
    {
    }



    /// <summary>
    /// Builds a <see cref="Report"/> snapshot from the current item count and timing. Override to add
    /// more detail (for example a known <see cref="Report.TotalItemCount"/>).
    /// </summary>
    /// <returns>A <see cref="Report"/> for the current run.</returns>
    protected override Report CreateProgressReport() => new(CurrentItemCount, StartedAt, Elapsed);
}
