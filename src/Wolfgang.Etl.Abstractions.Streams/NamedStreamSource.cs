using System;
using System.IO;

namespace Wolfgang.Etl.Abstractions.Streams;

/// <summary>
/// One input stream for a <see cref="MultiStreamExtractorBase{TRecord, TProgress}"/>: the
/// <see cref="StreamReader"/> to read, plus an optional human-readable <see cref="Name"/> (a file path,
/// blob key or URL) that the extractor surfaces in its progress as
/// <see cref="MultiStreamProgress.CurrentSourceName"/>.
/// </summary>
/// <remarks>
/// The extractor takes ownership of <see cref="Reader"/>: it disposes the reader once that source's
/// records have been read, whether the source succeeded or failed.
/// </remarks>
public sealed class NamedStreamSource
{
    /// <summary>
    /// Initializes a new <see cref="NamedStreamSource"/>.
    /// </summary>
    /// <param name="reader">The reader for this source. The extractor disposes it after use.</param>
    /// <param name="name">An optional name for progress reporting and error handling; <see langword="null"/> when none.</param>
    /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <see langword="null"/>.</exception>
    public NamedStreamSource(StreamReader reader, string? name = null)
    {
        Reader = reader ?? throw new ArgumentNullException(nameof(reader));
        Name = name;
    }



    /// <summary>
    /// The reader for this source.
    /// </summary>
    public StreamReader Reader { get; }



    /// <summary>
    /// The source's human-readable name, or <see langword="null"/> when the caller supplied none.
    /// </summary>
    public string? Name { get; }
}
