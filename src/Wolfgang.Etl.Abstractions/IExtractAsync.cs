using System.Collections.Generic;
using System.Threading;

namespace Wolfgang.Etl.Abstractions
{
    public interface IExtractAsync<out T>
    {
        IAsyncEnumerable<T> ExtractAsync();
    }
}
