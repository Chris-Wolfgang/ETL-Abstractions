namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Implemented by ETL stages that support a <em>dry run</em> — a mode in which the
/// full pipeline is exercised but the external side effect that mutates a destination
/// or source is skipped.
/// </summary>
/// <remarks>
/// A dry run runs the stage exactly as a real run would — enumerating the source,
/// evaluating <c>SkipItemCount</c> and <c>MaximumItemCount</c>, incrementing the
/// progress counters, firing the progress-timer callback and logging as usual — but
/// it does <b>not</b> perform the side effect that mutates external state. This is
/// useful for validating a newly built pipeline (source feed, mapping, batching,
/// throttling) against production data without writing, and for estimating how long
/// a real run would take.
/// <para>
/// The interface is opt-in: a stage advertises it <em>only</em> when it genuinely
/// honours <see cref="IsDryRun"/>. It is therefore the responsibility of the
/// implementer to gate its side effect on <see cref="IsDryRun"/>; the ETL base
/// classes deliberately do not implement this interface because the side effect
/// lives inside the derived stage and the base class has no way to skip it.
/// </para>
/// <para>
/// Dry run applies to any stage with an external side effect — a loader skips its
/// write, and an extractor that mutates its source as it reads (for example
/// committing a message-queue offset, deleting a message on receipt, marking mail
/// read or moving a processed file) skips that acknowledgement. A pure transformer
/// has no external side effect and would not normally implement this interface.
/// </para>
/// </remarks>
public interface ISupportDryRun
{
    /// <summary>
    /// Gets a value indicating whether the stage runs in dry-run mode.
    /// </summary>
    /// <value>
    /// When <see langword="true"/>, the stage runs the full pipeline but skips the
    /// external side effect that mutates the destination or source. Defaults to
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// The property is read-only by contract: dry-run mode is configuration, fixed when the stage
    /// is constructed, and must not change while a pipeline is enumerating. Per ADR-0009 an
    /// implementer exposes it as <c>{ get; }</c>, assigned once in its constructor from the
    /// <c>IsDryRun</c> member of its options record — the same place every other setting lives.
    /// <para>
    /// An implementer may still declare a settable property to satisfy this member; widening a
    /// read-only interface member is legal. Doing so reintroduces the mid-run mutation this
    /// contract exists to prevent, so prefer a get-only property fed from the options record.
    /// </para>
    /// </remarks>
    bool IsDryRun { get; }
}
