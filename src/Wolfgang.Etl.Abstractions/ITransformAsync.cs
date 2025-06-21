using System.Collections.Generic;

namespace Wolfgang.Etl.Abstractions
{
    public interface ITransformAsync<in T, out TResult>
    {
        IAsyncEnumerable<TResult> TransformAsync(IAsyncEnumerable<T> source);
    }
}
