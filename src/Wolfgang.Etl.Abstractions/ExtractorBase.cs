﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using JetBrains.Annotations;


namespace Wolfgang.Etl.Abstractions
{
    public abstract class ExtractorBase<TSource, TProgress> 
        : IExtractWithProgressAndCancellationAsync<TSource, TProgress>
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
        /// The current number of items extracted so far.
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
        /// The maximum number of items to extract. Once the extractor has reached this limit,
        /// it should stop extracting and signal the end of the sequence.
        /// </summary>
        /// <remarks>
        /// This is useful for partially extracting data from a source, especially when the source is large
        /// or infinite or during development.
        /// </remarks>
        /// <exception cref="ArgumentException">The specified value is less than 1</exception>
        /// <example>
        /// <code>
        ///     var count = 0;
        ///     using (var reader = new StreamReader(filePath))
        ///     {
        ///         while (!reader.EndOfStream)
        ///         {
        ///             yield return await reader.ReadLineAsync();
        ///             count++;
        ///             if (count >= MaximumItemCount)
        ///             {
        ///                 Console.WriteLine("Maximum item count reached. Stopping extraction.");
        ///                 break; // Stop extracting if the maximum item count is reached
        ///             }
        ///         }
        ///     }
        /// </code>
        /// </example>
        [UsedImplicitly]
        [Range(0, int.MaxValue, ErrorMessage = "Current item count cannot be less than 1.")]
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
        /// The number of items to skip before extracting.
        /// The extractor should skip the specified number of items before starting to yield results.
        /// </summary>
        /// <remarks>
        /// This is useful for partially extracting data from a source during development, or to skip
        /// items that were already processed or are not relevant for the current extraction.
        /// </remarks>
        /// <exception cref="ArgumentException">The specified value is less than 0</exception>
        /// <example>
        /// <code>
        ///     using (var reader = new StreamReader(filePath))
        ///     {
        ///         // Skip the specified number of items before starting to yield results
        ///
        ///         var skipCount = 0;
        ///         while (!reader.EndOfStream && skipCount &lt; SkipItemCount)
        ///         {
        ///             await reader.ReadLineAsync();
        ///             skipCount++;
        ///         }
        ///
        /// 
        ///         // Now start yielding results
        /// 
        ///         var count++;
        ///         while (!reader.EndOfStream)
        ///         {
        ///             yield return await reader.ReadLineAsync();
        ///             count++;
        ///        }
        ///     }
        /// </code>
        /// </example>
        [UsedImplicitly]
        [Range(0, int.MaxValue, ErrorMessage = "Current item count cannot be less than 0.")]
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
        /// Asynchronously extracts data of type TSource from a source.
        /// </summary>
        /// <returns>
        /// IAsyncEnumerable&lt;T&gt;
        /// The result may be an empty sequence if no data is available or if the extraction fails.
        /// </returns>
        public IAsyncEnumerable<TSource> ExtractAsync()
        {
            return ExtractWorkerAsync(CancellationToken.None);
        }



        /// <summary>
        /// Asynchronously extracts data of type TSource from a source.
        /// </summary>
        /// <param name="token">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>
        /// IAsyncEnumerable&lt;T&gt;
        /// The result may be an empty sequence if no data is available or if the extraction fails.
        /// </returns>
        /// <remarks>
        /// The extractor should be able to handle cancellation requests gracefully.
        /// If the caller doesn't plan on cancelling the extraction, CancellationToken.None should be passed in.
        /// </remarks>
        public IAsyncEnumerable<TSource> ExtractAsync(CancellationToken token)
        {
            return ExtractWorkerAsync(token);
        }



        /// <summary>
        /// Asynchronously extracts data of type TSource from a source.
        /// </summary>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>
        /// IAsyncEnumerable&lt;T&gt;
        /// The result may be an empty sequence if no data is available or if the extraction fails.
        /// </returns>
        /// <exception cref="ArgumentNullException">The value of progress is null</exception>
        public IAsyncEnumerable<TSource> ExtractAsync(IProgress<TProgress> progress)
        {
            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress), "Progress cannot be null.");
            }

            using var timer = new Timer
            (
                _ => progress.Report(CreateProgressReport()),
                null,
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(ReportingInterval)
            );

            return ExtractWorkerAsync(CancellationToken.None);
        }



        /// <summary>
        /// Asynchronously extracts data of type TSource from a source.
        /// </summary>
        /// <param name="progress">A provider for progress updates.</param>
        /// <param name="token">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>
        /// IAsyncEnumerable&lt;T&gt;
        /// The result may be an empty sequence if no data is available or if the extraction fails.
        /// </returns>
        /// <remarks>
        /// The extractor should be able to handle cancellation requests gracefully.
        /// If the caller doesn't plan on cancelling the extraction, CancellationToken.None should be passed in.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The value of progress is null</exception>
        public IAsyncEnumerable<TSource> ExtractAsync(IProgress<TProgress> progress, CancellationToken token)
        {
            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress), "Progress cannot be null.");
            }

            using var timer = new Timer
            (
                _ => progress.Report(CreateProgressReport()),
                null,
                TimeSpan.Zero,
                
                TimeSpan.FromMilliseconds(ReportingInterval)
            );
            
            
            return ExtractWorkerAsync(token);
        }



        /// <summary>
        /// This method is the core implementation of the extraction logic and should be
        /// overridden by derived classes.
        /// </summary>
        /// <param name="token">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>
        /// IAsyncEnumerable&lt;T&gt;
        /// The result may be an empty sequence if no data is available or if the extraction fails.
        /// </returns>
        protected abstract IAsyncEnumerable<TSource> ExtractWorkerAsync(CancellationToken token);



        /// <summary>
        /// Creates a progress report of type TProgress. This gives the derived class the opportunity to
        /// implement a custom progress report that is specific to the extraction process.
        /// </summary>
        /// <returns>Progress of type TProgress</returns>
        protected abstract TProgress CreateProgressReport();
    }
}
