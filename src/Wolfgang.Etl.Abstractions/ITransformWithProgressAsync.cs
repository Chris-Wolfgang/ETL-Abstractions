using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Wolfgang.Etl.Abstractions
{
    /// <summary>
    /// Defines an asynchronous transformer interface for transforming data of type T to TResult.
    /// A class implementing this interface is intended to be the second step in an
    /// ETL (Extract, Transform, Load) process.
    /// </summary>
    /// <typeparam name="TSource">Represents a single item from the source of the ETL</typeparam>
    /// <typeparam name="TDestination">Represents a single item to be sent to the destination of the ETL</typeparam>
    /// <typeparam name="TProgress">The value of the updated progress</typeparam>
    /// <remarks>
    /// The transformer is responsible for transforming data from source type to the destination type.
    /// The transformation may involve converting data types, filtering, aggregating, and mapping data.
    /// The transformer should be resilient and capable of handling any exceptions that may occur. The
    /// data is provided as an asynchronous stream of type TSource, and returned as an asynchronous
    /// stream of type TDestination allowing for efficient processing by the loader component.
    /// Ideally, the transformer should do all the transformation of the data, including not limited to
    /// deserializing a string of JSON from web service call and binding it to a specific type T or
    /// serializing an object of type T to a string of JSON to before passing it to the loader to.
    /// </remarks>
    public interface ITransformWithProgressAsync<in TSource, out TDestination, out TProgress> :
        ITransformAsync<TSource, TDestination>
        where TSource : notnull where TDestination : notnull where TProgress : notnull
    {
        /// <summary>
        /// Asynchronously transforms data of type TSource to TDestination.
        /// </summary>
        /// <param name="items">Asynchronous list of TSource </param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>Asynchronous&lt;T&gt;</returns>
        /// <exception cref="ArgumentNullException">The value of items or progress is null</exception>
        IAsyncEnumerable<TDestination>TransformAsync
            (
                [NotNull] IAsyncEnumerable<TSource> items, 
                [NotNull] IProgress<TProgress> progress
            );
    }
}
