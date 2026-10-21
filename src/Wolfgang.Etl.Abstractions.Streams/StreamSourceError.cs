using System;

namespace Wolfgang.Etl.Abstractions.Streams;

/// <summary>
/// Describes a source that failed while a <see cref="MultiStreamExtractorBase{TRecord, TProgress}"/>
/// was reading it. Passed to the extractor's <c>SourceErrorHandler</c>.
/// </summary>
public sealed class StreamSourceError
{
    /// <summary>
    /// Initializes a new <see cref="StreamSourceError"/>.
    /// </summary>
    /// <param name="name">The failed source's name, or <see langword="null"/> when it had none.</param>
    /// <param name="sourceIndex">The zero-based position of the failed source in the input sequence.</param>
    /// <param name="exception">The exception reading the source threw.</param>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="sourceIndex"/> is negative.</exception>
    public StreamSourceError(string? name, int sourceIndex, Exception exception)
    {
        if (sourceIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceIndex), sourceIndex, "The source index cannot be negative.");
        }

        Name = name;
        SourceIndex = sourceIndex;
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }



    /// <summary>
    /// The failed source's name, or <see langword="null"/> when it had none.
    /// </summary>
    public string? Name { get; }



    /// <summary>
    /// The zero-based position of the failed source in the input sequence.
    /// </summary>
    public int SourceIndex { get; }



    /// <summary>
    /// The exception reading the source threw.
    /// </summary>
    public Exception Exception { get; }
}
