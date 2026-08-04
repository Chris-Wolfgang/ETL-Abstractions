using System;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// An <see cref="IProgress{T}"/> implementation that invokes its callback synchronously
/// on the calling thread, rather than posting to a synchronisation context.
/// </summary>
/// <typeparam name="T">The type of progress value.</typeparam>
/// <remarks>
/// <para>
/// The standard <see cref="Progress{T}"/> class captures the current
/// <see cref="System.Threading.SynchronizationContext"/> and posts callbacks to it
/// asynchronously. In unit tests running without a synchronisation context this means
/// callbacks are dispatched to the thread pool and may not have fired by the time an
/// assertion runs.
/// </para>
/// <para>
/// <see cref="SynchronousProgress{T}"/> calls the handler immediately and on the same
/// thread that calls <see cref="Report"/>, making progress assertions in tests fully
/// deterministic without requiring delays or manual synchronisation.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// TProgress? lastReport = null;
/// var progress = new SynchronousProgress&lt;TProgress&gt;(r => lastReport = r);
///
/// await sut.ExtractAsync(progress).ToListAsync();
///
/// Assert.NotNull(lastReport);
/// </code>
/// </example>
public sealed class SynchronousProgress<T> : IProgress<T>
{
    private readonly Action<T> _handler;



    /// <summary>
    /// Initialises a new <see cref="SynchronousProgress{T}"/> that invokes
    /// <paramref name="handler"/> synchronously on each <see cref="Report"/> call.
    /// </summary>
    /// <param name="handler">The callback to invoke with each progress value.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public SynchronousProgress(Action<T> handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }



    /// <summary>
    /// Invokes the progress handler synchronously with <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The progress value to report.</param>
    public void Report(T value) => _handler(value);
}
