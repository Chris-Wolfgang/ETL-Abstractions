using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;



namespace Wolfgang.Etl.Abstractions
{
    public abstract class LoaderBase<TDestination, TProgress> 
        : ILoadWithProgressAndCancellationAsync<TDestination, TProgress>
    {

        private int _reportingInterval = 1_000;
        private int _maximumItemCount = int.MaxValue;
        private int _skipItemCount;
        private int _currentItemCount;
        private int _currentSkippedItemCount;


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
        /// The current number of items loaded so far.
        /// </summary>
        /// <remarks>
        /// It is the responsibility of the derived class to keep this value up to date as the
        /// base class will have no way of knowing the correct value
        /// </remarks>

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
        /// Gets the current number of records skipped
        /// </summary>
        public int CurrentSkippedItemCount
        {
            get => _currentSkippedItemCount;
            protected set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "value cannot be less than 0.");
                }
            }
        }




        /// <summary>
        /// The maximum number of items to load. Once the loader has reached this limit,
        /// it should stop loading items and exist as if it had reached the end of the list
        /// </summary>
        /// <remarks>
        /// This is useful for partially loading data from a source, especially when the source is large
        /// or infinite or during development.
        /// </remarks>
        /// <exception cref="ArgumentException">The specified value is less than 1</exception>
        /// <example>
        /// <code>
        ///     foreach (var item in items.Skip(SkipItemCount).Take(MaxItemCount))
        ///     {
        ///         // Process the item
        ///     }
        /// </code>
        /// </example>

        public int MaximumItemCount
        {
            get => _maximumItemCount;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Maximum item count cannot be less than 0.");
                }
                _maximumItemCount = value;
            }
        }



        /// <summary>
        /// The number of items skipped before loading.
        /// The loader should skip the specified number of items before starting to process the remaining items.
        /// </summary>
        /// <remarks>
        /// This is useful for skipping the beginning of the list during testing or because it may already be loaded
        /// </remarks>
        /// <exception cref="ArgumentException">The specified value is less than 0</exception>
        /// <example>
        /// <code>
        ///     foreach (var item in items.Skip(SkipItemCount).Take(MaxItemCount))
        ///     {
        ///         // Process the item
        ///     }
        /// </code>
        /// </example>

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
        /// Asynchronously loads data of type TDestination into the target destination.
        /// </summary>
        /// <param name="items">The items to be loaded to the destination.</param>
        /// <remarks>
        /// Items may be an empty sequence if no data is available or if the extraction fails.
        /// </remarks>
        /// <returns>Task</returns>
        /// <exception cref="ArgumentNullException">Argument items is null</exception>
        public virtual Task LoadAsync
        (
            IAsyncEnumerable<TDestination> items
        )
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            return LoadWorkerAsync(items, CancellationToken.None);
        }



        /// <summary>
        /// Asynchronously loads data of type TDestination into the target destination.
        /// </summary>
        /// <param name="items">The items to be loaded to the destination.</param>
        /// <param name="token">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <remarks>
        /// Items may be an empty sequence if no data is available or if the extraction fails.
        /// </remarks>
        /// <returns>Task</returns>
        /// <exception cref="ArgumentNullException">Argument items is null</exception>
        public virtual Task LoadAsync
        (
            IAsyncEnumerable<TDestination> items, 
            CancellationToken token
        )
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            return LoadWorkerAsync(items, CancellationToken.None);
        }



        /// <summary>
        /// Asynchronously loads data of type TDestination into the target destination.
        /// </summary>
        /// <param name="items">The items to be loaded to the destination.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <remarks>
        /// Items may be an empty sequence if no data is available or if the extraction fails.
        /// </remarks>
        /// <returns>Task</returns>
        /// <exception cref="ArgumentNullException">Argument items is null</exception>
        /// <exception cref="ArgumentNullException">Argument progress is null</exception>
        public virtual Task LoadAsync
        (
            IAsyncEnumerable<TDestination> items,
            IProgress<TProgress> progress
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

            return LoadWorkerAsync(items, CancellationToken.None);
        }



        /// <summary>
        /// Asynchronously loads data of type TDestination into the target destination.
        /// </summary>
        /// <param name="items">The items to be loaded to the destination.</param>
        /// <param name="token">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <remarks>
        /// Items may be an empty sequence if no data is available or if the extraction fails.
        /// </remarks>
        /// <returns>Task</returns>
        /// <exception cref="ArgumentNullException">Argument items is null</exception>
        /// <exception cref="ArgumentNullException">Argument progress is null</exception>
        public virtual Task LoadAsync
        (
            IAsyncEnumerable<TDestination> items,
            IProgress<TProgress> progress, 
            CancellationToken token
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

            
            return LoadWorkerAsync(items, token);
        }



        /// <summary>
        /// This method is the core implementation of the loading logic and should be
        /// overridden by derived classes.
        /// </summary>
        /// <param name="items">The items to be loaded to the destination.</param>
        /// <param name="token">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <remarks>
        /// Items may be an empty sequence if no data is available or if the extraction fails.
        /// </remarks>
        /// <returns>Task</returns>
        /// <exception cref="ArgumentNullException">Argument items is null</exception>
        protected abstract Task LoadWorkerAsync
        (
            IAsyncEnumerable<TDestination>items, 
            CancellationToken token
        );



        /// <summary>
        /// Creates a progress report of type TProgress. This gives the derived class the opportunity to
        /// implement a custom progress report that is specific to the extraction process.
        /// </summary>
        /// <returns>Progress of type TProgress</returns>
        protected abstract TProgress CreateProgressReport();



        /// <summary>
        /// Increments the CurrentItemCount in a thread safe manner.
        /// </summary>
        /// <remarks>
        /// Simply calling CurrentItemCount++ or CurrentItemCount += 1 is not
        /// thread safe. This method ensures that CurrentItemCount is incremented safely 
        /// </remarks>
        protected void IncrementCurrentItemCount()
        {
            Interlocked.Increment(ref _currentItemCount);
        }



        /// <summary>
        /// Increments the CurrentItemCount in a thread safe manner.
        /// </summary>
        /// <remarks>
        /// Simply calling CurrentItemCount++ or CurrentItemCount += 1 is not
        /// thread safe. This method ensures that CurrentItemCount is incremented safely 
        /// </remarks>
        protected void IncrementCurrentSkippedItemCount()
        {
            Interlocked.Increment(ref _currentSkippedItemCount);
        }

    }
}
