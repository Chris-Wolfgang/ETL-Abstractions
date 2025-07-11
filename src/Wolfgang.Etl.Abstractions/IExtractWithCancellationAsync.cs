using System.Collections.Generic;
using System.Threading;

namespace Wolfgang.Etl.Abstractions
{
    /// <summary>
    /// Defines an asynchronous extractor interface for extracting data of type T. 
    /// A class implementing this interface is intended to be the first step in an
    /// ETL (Extract, Transform, Load) process. 
    /// </summary>
    /// <typeparam name="TSource">Represents a single item from the source of the ETL</typeparam>
    /// <remarks>
    /// The extractor is responsible for pulling data from a source, which could be a
    /// database, file, API, web service or any other data source.  It is also responsible for
    /// handling any exceptions that may occur during the extraction process, including
    /// but not limited to network issues, data format errors, or source unavailability.
    /// The extractor should be resilient and capable of retrying operations in case of
    /// transient failures. The extracted data is returned as an asynchronous stream of
    /// type T, allowing for efficient processing by the transformer and loader components
    /// of the ETL pipeline. Ideally, the extractor should NOT do any transformation of the
    /// data; its sole responsibility is to pull data from the source and provide it in its
    /// raw form to the next step in the ETL process. However, occasionally, some minimal
    /// transformation may be necessary to ensure the data is in a suitable format for further
    /// processing. For example, if the data is being extracted from a CSV file the same
    /// library that parses the CSV file may also bind the data to a specific type T. In this case
    /// the pragmatic approach is to allow the extractor to perform this minimal transformation
    /// </remarks>
    public interface IExtractWithCancellationAsync<out TSource> :
        IExtractAsync<TSource> 
        where TSource : notnull
    {
        /// <summary>
        /// Asynchronously extracts data of type TSource from a source.
        /// </summary>
        /// <param name="token">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>
        /// IAsyncEnumerable&lt;T&gt; - The result may be an empty sequence if no data is available or if the extraction fails.
        /// </returns>
        /// <remarks>
        /// The extractor should be able to handle cancellation requests gracefully.
        /// If the caller doesn't plan on cancelling the extraction, they can pass CancellationToken.None.
        /// </remarks>
        IAsyncEnumerable<TSource> ExtractAsync(CancellationToken token);
    }
}
