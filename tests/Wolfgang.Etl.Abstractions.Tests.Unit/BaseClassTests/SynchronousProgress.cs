namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;
/// <summary>
/// An <see cref="IProgress{T}"/> implementation that invokes the callback synchronously
/// and inline, guaranteeing it executes before control returns to the caller.
/// Use this in tests instead of <see cref="System.Progress{T}"/>, which posts the callback
/// to the synchronization context asynchronously and may never fire before a test assertion runs.
/// </summary>
internal class SynchronousProgress<T> : IProgress<T>
{
    private readonly Action<T> _callback;

    public SynchronousProgress(Action<T> callback)
    {
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    public void Report(T value) => _callback(value);
}
