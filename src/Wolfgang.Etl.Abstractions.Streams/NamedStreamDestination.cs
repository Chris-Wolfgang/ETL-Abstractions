using System;
using System.IO;

namespace Wolfgang.Etl.Abstractions.Streams;

/// <summary>
/// One output stream for a <see cref="MultiStreamLoaderBase{TRecord, TProgress}"/>: the
/// <see cref="StreamWriter"/> to write, plus an optional human-readable <see cref="Name"/> that the
/// loader surfaces in its progress as <see cref="MultiStreamProgress.CurrentSourceName"/>.
/// </summary>
/// <remarks>
/// The loader takes ownership of <see cref="Writer"/>: it flushes and disposes every destination it
/// opened when the load ends. A destination is identified by reference, so return the same instance
/// from the destination selector for every record that belongs in the same stream.
/// </remarks>
public sealed class NamedStreamDestination
{
    /// <summary>
    /// Initializes a new <see cref="NamedStreamDestination"/>.
    /// </summary>
    /// <param name="writer">The writer for this destination. The loader flushes and disposes it.</param>
    /// <param name="name">An optional name for progress reporting; <see langword="null"/> when none.</param>
    /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <see langword="null"/>.</exception>
    public NamedStreamDestination(StreamWriter writer, string? name = null)
    {
        Writer = writer ?? throw new ArgumentNullException(nameof(writer));
        Name = name;
    }



    /// <summary>
    /// The writer for this destination.
    /// </summary>
    public StreamWriter Writer { get; }



    /// <summary>
    /// The destination's human-readable name, or <see langword="null"/> when the caller supplied none.
    /// </summary>
    public string? Name { get; }
}
