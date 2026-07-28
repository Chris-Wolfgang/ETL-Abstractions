namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Factory methods for creating <see cref="MiddlewareResult{T}"/> values from an
/// <see cref="IItemMiddleware{T}"/> implementation.
/// </summary>
public static class MiddlewareResult
{
    /// <summary>
    /// Keeps the item in the stream, optionally replacing it with a transformed value.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="item">The item to pass on.</param>
    /// <returns>A result that keeps <paramref name="item"/> flowing.</returns>
    public static MiddlewareResult<T> Continue<T>(T item) => new(item, skip: false);



    /// <summary>
    /// Drops the current item from the stream.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <returns>A result that discards the current item.</returns>
    public static MiddlewareResult<T> Drop<T>() => new(default!, skip: true);
}
