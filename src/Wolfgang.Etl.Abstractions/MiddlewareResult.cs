using System;
using System.Collections.Generic;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// The outcome of running a single item through an <see cref="IItemMiddleware{T}"/>: the
/// (possibly replaced) item to pass on, and whether the item should be dropped from the stream.
/// Create one with <see cref="MiddlewareResult.Continue{T}(T)"/> to keep an item flowing or
/// <see cref="MiddlewareResult.Drop{T}"/> to discard it.
/// </summary>
/// <typeparam name="T">The item type flowing through the pipeline.</typeparam>
public readonly struct MiddlewareResult<T> : IEquatable<MiddlewareResult<T>>
{
    internal MiddlewareResult(T item, bool skip)
    {
        Item = item;
        Skip = skip;
    }



    /// <summary>
    /// The item to pass on to the next middleware (or to the stream). Meaningful only when
    /// <see cref="Skip"/> is <see langword="false"/>.
    /// </summary>
    public T Item { get; }



    /// <summary>
    /// <see langword="true"/> to drop the item from the stream (later middleware is not run and the
    /// item is not yielded); <see langword="false"/> to keep it.
    /// </summary>
    public bool Skip { get; }



    /// <inheritdoc/>
    public bool Equals(MiddlewareResult<T> other) =>
        Skip == other.Skip && EqualityComparer<T>.Default.Equals(Item, other.Item);



    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is MiddlewareResult<T> other && Equals(other);



    /// <inheritdoc/>
    // Stryker disable once all: equivalent — any change to the hash formula still yields equal hash codes for equal values (the only GetHashCode contract), so no behavioural test can distinguish it.
    public override int GetHashCode() =>
        unchecked(((Skip ? 1 : 0) * 397) ^ (Item is null ? 0 : EqualityComparer<T>.Default.GetHashCode(Item)));



    /// <summary>Indicates whether two results are equal.</summary>
    public static bool operator ==(MiddlewareResult<T> left, MiddlewareResult<T> right) => left.Equals(right);



    /// <summary>Indicates whether two results are not equal.</summary>
    public static bool operator !=(MiddlewareResult<T> left, MiddlewareResult<T> right) => !left.Equals(right);
}
