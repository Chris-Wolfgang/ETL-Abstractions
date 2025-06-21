using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wolfgang.Etl.Abstractions
{
    public interface ILoadAsync<in T>
    {

        /// <summary>
        /// Loads the data asynchronously.
        /// </summary>
        /// <param name="source">The source data to load.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LoadAsync(IAsyncEnumerable<T> source);
    }
}
