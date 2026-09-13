using System;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Construction-time configuration for an <see cref="ExtractorBase{TSource, TProgress}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Per ADR-0009, stage configuration is supplied as a record passed to the constructor rather
/// than assigned to properties after construction, so that it is settled before a run
/// rather than mutated during one.
/// </para>
/// <para>
/// A derived extractor inherits this record to add its own settings — for example a
/// <c>CsvExtractorOptions : ExtractorOptions</c> adding a <c>Delimiter</c> — so that a caller
/// configures the whole stage, base members included, through a single object.
/// </para>
/// <example>
/// <code>
/// var options = new ExtractorOptions
/// {
///     MaximumItemCount = 100,
///     SkipItemCount    = 10
/// };
/// </code>
/// </example>
/// <para>
/// <c>WorkerResilience</c> is deliberately absent. Its type differs on every base stage
/// (it is generic over the stage's own type parameter), so including it here would force
/// every derived options record in the fleet to become generic for the sake of one delegate.
/// It is a resilience strategy rather than a setting — the same category as <c>ILogger</c> —
/// and remains an init-only property on the stage.
/// </para>
/// </remarks>
public record ExtractorOptions
{
    /// <summary>
    /// The number of items between progress reports. Defaults to <c>1000</c>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is less than 1.</exception>
    public int ReportingInterval
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            field = value;
        }
    } = 1_000;



    /// <summary>
    /// The maximum number of items to extract. Defaults to <see cref="int.MaxValue"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is less than 1.</exception>
    public int MaximumItemCount
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            field = value;
        }
    } = int.MaxValue;



    /// <summary>
    /// The number of items to skip before extracting. Defaults to <c>0</c>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is less than 0.</exception>
    public int SkipItemCount
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 0);
            field = value;
        }
    }



    /// <summary>
    /// The policy invoked when an item fails to process. Defaults to fail-fast
    /// (<see cref="DefaultItemErrorPolicy.Abort"/>).
    /// </summary>
    /// <exception cref="ArgumentNullException">The value is <see langword="null"/>.</exception>
    public Func<ItemErrorContext, ItemErrorAction> ErrorPolicy
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(value));
    } = DefaultItemErrorPolicy.Abort;
}
