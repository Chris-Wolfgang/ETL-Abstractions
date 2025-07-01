using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Wolfgang.Etl.Abstractions
{
    /// <summary>
    /// Defines an asynchronous loader interface for loading data of type T.
    /// A class implementing this interface is intended to be the last step in an
    /// ETL (Extract, Transform, Load) process.
    /// </summary>
    /// <typeparam name="TDestination">Represents a single item to be sent to the destination of the ETL</typeparam>
    /// <remarks>
    /// The loader is the last step in the ETL process and is responsible for saving
    /// data to the destination after processing. The loader is responsible for handling
    /// any extractions that may occur during the loading process, including but not
    /// limited to communication errors, authentication errors, permission errors. The
    /// loader should be resilient and capable of retrying operations in case of
    /// transient failures. The loaders should be resilient and capable of retrying
    /// operations in case of transient failures. The loaded data is provided as an
    /// asynchronous stream of type TDestination, allowing for efficient writing of
    /// the data to the destination. Ideally, the loader should NOT do any transformation
    /// of the data; its sole responsibility is to save the data to the destination.
    /// However, occasionally, some minimal transformation may be necessary to
    /// ensure the data is in a suitable format for further processing. For example, if the
    /// data is being written to a CSV file, the same library that writes the CSV file
    /// may handle the conversion of objects of type T to strings (rows). In this case,
    /// the pragmatic approach is to allow the loader to perform this minimal transformation.
    /// </remarks>
    public interface ILoadWithCancellationAsync<in TDestination> :
        ILoadAsync<TDestination>
        where TDestination : notnull
    {
        /// <summary>
        /// Loads the data asynchronously.
        /// </summary>
        /// <param name="items">The items to be loaded to the destination.</param>
        /// <param name="token">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// The extractor should be able to handle cancellation requests gracefully.
        /// If the caller doesn't plan on cancelling the extraction, they can pass CancellationToken.None.
        /// </remarks>
        Task LoadAsync(IAsyncEnumerable<TDestination> items, CancellationToken token);
    }
}