namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Implemented by an ETL stage that counts items its error policy discarded, so a pipeline can read
/// that count uniformly regardless of the stage's concrete type. <see cref="ExtractorBase{TSource, TProgress}"/>,
/// <see cref="LoaderBase{TDestination, TProgress}"/>, and <see cref="TransformerBase{TSource, TDestination, TProgress}"/>
/// all implement it, and <c>EtlPipeline</c> sums <see cref="CurrentErrorItemCount"/> across every stage
/// that reports it into <see cref="EtlPipelineProgress.ErrorItemCount"/>.
/// </summary>
public interface IReportsItemErrors
{
    /// <summary>
    /// The number of items this stage's error policy has discarded (<c>OnItemError</c> returned
    /// <see cref="ItemErrorAction.Skip"/>) so far in the current run.
    /// </summary>
    int CurrentErrorItemCount { get; }
}
