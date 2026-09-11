using System;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Construction-time configuration for a <see cref="LoaderBase{TDestination, TProgress}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Per ADR-0009, stage configuration is supplied as a record passed to the constructor rather
/// than assigned to properties after construction, so that it is settled before a run
/// rather than mutated during one. Derived loaders inherit this record to add their own settings, giving a caller
/// a single object to configure the whole stage.
/// </para>
/// <para>
/// <c>WorkerResilience</c> is deliberately absent — see the corresponding note on
/// <see cref="ExtractorOptions"/>.
/// </para>
/// </remarks>
public record LoaderOptions
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
    /// The maximum number of items to load. Defaults to <see cref="int.MaxValue"/>.
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
    /// The number of items to skip before loading. Defaults to <c>0</c>.
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
