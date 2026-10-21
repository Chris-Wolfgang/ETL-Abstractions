namespace Wolfgang.Etl.Abstractions.Streams;

/// <summary>
/// What a <see cref="MultiStreamExtractorBase{TRecord, TProgress}"/> does when reading one source
/// throws.
/// </summary>
public enum MultiStreamErrorAction
{
    /// <summary>
    /// Rethrow the failure and end the run. The default.
    /// </summary>
    Stop,

    /// <summary>
    /// Skip the rest of the failed source and continue with the next one. The failure is counted in
    /// <see cref="MultiStreamProgress.CurrentFailedSourceCount"/> and passed to the extractor's
    /// <c>SourceErrorHandler</c>, so it is never silent. Records already read from the failed source
    /// have been yielded and stay yielded.
    /// </summary>
    Continue,
}
