using System;



namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Represents a timer that raises <see cref="Elapsed"/> at a regular interval,
/// used by extractor, transformer, and loader base classes to drive progress callbacks.
/// </summary>
/// <remarks>
/// The default production implementation typically uses <see cref="System.Threading.Timer"/>
/// internally. In unit tests, a custom implementation can be injected to fire the
/// <see cref="Elapsed"/> event on demand, making progress callback assertions
/// deterministic.
/// </remarks>
public interface IProgressTimer : IDisposable
{
    /// <summary>
    /// Raised at each timer interval. Subscribers receive progress notifications
    /// and should call <c>CreateProgressReport()</c> then <c>Report()</c>.
    /// </summary>
#pragma warning disable MA0046 // Elapsed intentionally uses Action (not EventHandler) — changing the delegate signature would be a breaking change across all downstream ETL libraries
    event Action? Elapsed;
#pragma warning restore MA0046

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
