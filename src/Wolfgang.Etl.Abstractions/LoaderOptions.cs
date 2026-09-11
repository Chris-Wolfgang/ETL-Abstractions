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
    private const int DefaultReportingInterval = 1_000;
    private const int DefaultMaximumItemCount  = int.MaxValue;
    private const int DefaultSkipItemCount     = 0;



    /// <summary>
    /// Initializes a new instance, taking the documented default for any argument omitted.
    /// </summary>
    /// <remarks>
    /// Equivalent to an object initializer, and provided for one reason: an <c>init</c> accessor
    /// carries an <c>IsExternalInit</c> modifier whose identity differs between this package's
    /// <c>netstandard2.0</c> and modern assemblies, so a caller compiled against the former but
    /// running against the latter fails with <see cref="MissingMethodException"/> on
    /// <c>new LoaderOptions { … }</c>. Constructor parameters carry no such modifier. Prefer this
    /// constructor in code that ships only a <c>netstandard2.0</c> asset.
    /// </remarks>
    /// <param name="reportingInterval">The number of items between progress reports. Defaults to <c>1000</c>.</param>
    /// <param name="maximumItemCount">The maximum number of items to be loaded. Defaults to <see cref="int.MaxValue"/>.</param>
    /// <param name="skipItemCount">The number of items to skip first. Defaults to <c>0</c>.</param>
    /// <param name="errorPolicy">The item-error policy, or <see langword="null"/> for fail-fast (<see cref="DefaultItemErrorPolicy.Abort"/>).</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="reportingInterval"/> or <paramref name="maximumItemCount"/> is less than 1, or <paramref name="skipItemCount"/> is less than 0.</exception>
    public LoaderOptions
    (
        int reportingInterval = DefaultReportingInterval,
        int maximumItemCount = DefaultMaximumItemCount,
        int skipItemCount = DefaultSkipItemCount,
        Func<ItemErrorContext, ItemErrorAction>? errorPolicy = null
    )
    {
        ReportingInterval = reportingInterval;
        MaximumItemCount  = maximumItemCount;
        SkipItemCount     = skipItemCount;
        ErrorPolicy       = errorPolicy ?? DefaultItemErrorPolicy.Abort;
    }



    /// <summary>
    /// The number of items between progress reports. Defaults to <c>1000</c>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is less than 1.</exception>
    public int ReportingInterval
    {
        get;
        init
        {
#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
#else
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Reporting interval must be greater than 0.");
            }
#endif
            field = value;
        }
    } = DefaultReportingInterval;



    /// <summary>
    /// The maximum number of items to load. Defaults to <see cref="int.MaxValue"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is less than 1.</exception>
    public int MaximumItemCount
    {
        get;
        init
        {
#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
#else
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Maximum item count cannot be less than 1.");
            }
#endif
            field = value;
        }
    } = DefaultMaximumItemCount;



    /// <summary>
    /// The number of items to skip before loading. Defaults to <c>0</c>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is less than 0.</exception>
    public int SkipItemCount
    {
        get;
        init
        {
#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 0);
#else
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Skip item count cannot be less than 0.");
            }
#endif
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
