using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using JetBrains.Annotations;


namespace Wolfgang.Etl.Abstractions
{
    public abstract class TransformerBase<TSource, TDestination, TProgress> 
        : ITransformWithProgressAndCancellationAsync<TSource, TDestination, TProgress>
    {

        private int _reportingInterval = 1_000;
        private int _maximumItemCount = int.MaxValue;
        private int _skipItemCount;
        private int _currentItemCount;


        /// <summary>
        /// The number of milliseconds between progress updates.
        /// </summary>
        /// <exception cref="ArgumentException">Value cannot be less than 1</exception>
        public int ReportingInterval
        {
            get => _reportingInterval;
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Reporting interval must be greater than 0.");
                }
                _reportingInterval = value;
            }
        }


        /// <summary>
        /// The current number of items transformed so far.
        /// </summary>
        /// <remarks>
        /// It is the responsibility of the derived class to keep this value up to date as the
        /// base class will have no way of knowing the correct value
        /// </remarks>
        [UsedImplicitly]
        [Range(0, int.MaxValue, ErrorMessage = "Current item count cannot be less than 0.")]
        public int CurrentItemCount
        {
            get => _currentItemCount;
            protected set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _currentItemCount = value;
            }
        }



        /// <summary>
        /// The maximum number of items to transform. Once the transformer has reached this limit,
        /// it should stop transforming and signal the end of the sequence.
        /// </summary>
        /// <remarks>
        /// This is useful for transforming a subset of data, especially when the source is large
        /// or infinite or during development.
        /// </remarks>
        /// <exception cref="ArgumentException">The specified value is less than 1</exception>
        /// <example>
        /// <code>
        ///     foreach (var item in items.Skip(SkipItemCount).Take(MaxItemCount))
        ///     {
        ///         // Transformer each item and return it
        ///     }
        /// </code>
        /// </example>
        [UsedImplicitly]
        public int MaximumItemCount
        {
            get => _maximumItemCount;
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Maximum item count cannot be less than 1.");
                }
                _maximumItemCount = value;
            }
        }



        /// <summary>
        /// The number of items to skip before transforming.
        /// The transformer should skip the specified number of items before starting to yield results.
        /// </summary>
        /// <remarks>
        /// This is useful for transforming a subset of data, especially when the source is large
        /// or infinite or during development.
        /// </remarks>
        /// <exception cref="ArgumentException">The specified value is less than 0</exception>
        /// <example>
        /// <code>
        ///     foreach (var item in items.Skip(SkipItemCount).Take(MaxItemCount))
        ///     {
        ///         // Transformer each item and return it
        ///     }
        /// </code>
        /// </example>
        [UsedImplicitly]
        public int SkipItemCount
        {
            get => _skipItemCount;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Skip item count cannot be less than 0.");
                }
                _skipItemCount = value;
            }
        }



        /// <summary>
        /// Asynchronously transforms data of type TSource to TDestination
        /// </summary>
        /// <param name="items">IAsyncEnumerable&lt;TSource&gt; - A list of 0 or more items to be transformed</param>
        /// <returns>
        /// IAsyncEnumerable&lt;T&gt;
        /// The result may be an empty sequence if no data is available or if the transformation fails.
        /// </returns>
        public IAsyncEnumerable<TDestination> TransformAsync
        (
            [NotNull] IAsyncEnumerable<TSource> items
        )
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            return TransformWorkerAsync(items, CancellationToken.None);
        }



        /// <summary>
        /// Asynchronously transforms data of type TSource to TDestination
        /// </summary>
        /// <param name="items">IAsyncEnumerable&lt;TSource&gt; - A list of 0 or more items to be transformed</param>
        /// <param name="token">A CancellationToken to observe while waiting for the task to complete</param>
        /// <returns>
        /// IAsyncEnumerable&lt;TDestination&gt; - A list of 0 or more transformed items
        /// </returns>
        /// <remarks>
        /// </remarks>
        public IAsyncEnumerable<TDestination> TransformAsync
        (
            [NotNull] IAsyncEnumerable<TSource> items, 
            CancellationToken token
        )
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            return TransformWorkerAsync(items, token);
        }



        /// <summary>
        /// Asynchronously transforms data of type TSource to TDestination
        /// </summary>
        /// <param name="items">IAsyncEnumerable&lt;TSource&gt; - A list of 0 or more items to be transformed</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>IAsyncEnumerable&lt;T&gt; The result may be an empty sequence if no data is available or if the transformation fails.
        /// </returns> 
        /// <exception cref="ArgumentNullException">The value of progress is null</exception>
        public IAsyncEnumerable<TDestination> TransformAsync
        (
            [NotNull] IAsyncEnumerable<TSource> items,
            [NotNull] IProgress<TProgress> progress
        )
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            using var timer = new Timer
            (
                _ => progress.Report(CreateProgressReport()),
                null,
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(ReportingInterval)
            );

            return TransformWorkerAsync(items, CancellationToken.None);
        }



        /// <summary>
        /// Asynchronously transforms data of type TSource to TDestination
        /// </summary>
        /// <param name="items">IAsyncEnumerable&lt;TSource&gt; - A list of 0 or more items to be transformed</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <param name="token">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>
        /// IAsyncEnumerable&lt;T&gt; The result may be an empty sequence if no data is available or if the transformation fails.
        /// </returns> 
        /// <remarks>
        /// The transformer should be able to handle cancellation requests gracefully.
        /// If the caller doesn't plan on cancelling the transformation, they can pass CancellationToken.None.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The value of progress is null</exception>
        public IAsyncEnumerable<TDestination> TransformAsync
        (
            IAsyncEnumerable<TSource> items, 
            IProgress<TProgress> progress, 
            CancellationToken token
        )
        {
            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            using var timer = new Timer
            (
                _ => progress.Report(CreateProgressReport()),
                null,
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(ReportingInterval)
            );

            
            return TransformWorkerAsync(items, token);
        }



        protected abstract IAsyncEnumerable<TDestination> TransformWorkerAsync(IAsyncEnumerable<TSource>items,  CancellationToken token);



        protected abstract TProgress CreateProgressReport();
    }
}
