using System;



namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Represents a timer that raises <see cref="Elapsed"/> at a regular interval,
/// used by extractor, transformer, and loader base classes to drive progress callbacks.
/// </summary>
/// <remarks>
/// The default production implementation uses <see cref="System.Threading.Timer"/>
/// internally. Inject a <c>ManualProgressTimer</c> (from
/// <c>Wolfgang.Etl.TestKit.Xunit</c>) in unit tests to fire the event on demand,
/// making progress callback assertions fully deterministic.
/// </remarks>
public interface IProgressTimer : IDisposable
{
    /// <summary>
    /// Raised at each timer interval. Subscribers receive progress notifications
    /// and should call <c>CreateProgressReport()</c> then <c>Report()</c>.
    /// </summary>
    event Action? Elapsed;

    /// <summary>
    /// Starts the timer with the specified interval in milliseconds.
    /// </summary>
    /// <param name="intervalMilliseconds">
    /// The number of milliseconds between <see cref="Elapsed"/> events.
    /// </param>
    void Start(int intervalMilliseconds);

    /// <summary>
    /// Stops the timer. No further <see cref="Elapsed"/> events will be raised
    /// until <see cref="Start"/> is called again.
    /// </summary>
    void StopTimer();
}
