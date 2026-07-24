using System;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Describes a single item that failed to process. It is passed to a stage's <c>OnItemError</c>
/// policy so the policy can record the failure (a "dead letter") and decide whether to
/// <see cref="ItemErrorAction.Skip"/> the item or <see cref="ItemErrorAction.Abort"/> the run.
/// </summary>
public sealed class ItemErrorContext
{
    /// <summary>
    /// Initialises a new <see cref="ItemErrorContext"/>.
    /// </summary>
    /// <param name="recordNumber">The 1-based ordinal of the failed item within the current run.</param>
    /// <param name="exception">The exception the item raised.</param>
    /// <param name="rawContent">
    /// An optional, lazily-evaluated accessor for the item's raw source text (a line, a fixed-width
    /// record, an XML element). It is a delegate rather than a value so a stage that would have to
    /// buffer or reconstruct the raw content pays that cost only if the policy actually reads it.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
    public ItemErrorContext(long recordNumber, Exception exception, Func<string?>? rawContent = null)
    {
        RecordNumber = recordNumber;
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        RawContent = rawContent;
    }

    /// <summary>
    /// The 1-based ordinal of the failed item within the current run.
    /// </summary>
    public long RecordNumber { get; }

    /// <summary>
    /// The exception the item raised.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// An optional, lazily-evaluated accessor for the item's raw source text, or
    /// <see langword="null"/> if the stage does not supply one. Invoke it to obtain the text.
    /// </summary>
    public Func<string?>? RawContent { get; }
}
