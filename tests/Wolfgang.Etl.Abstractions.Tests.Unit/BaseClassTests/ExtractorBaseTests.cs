using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;
using Xunit.Abstractions;
using OperationCanceledException = System.OperationCanceledException;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests
{
    public class ExtractorBaseTests(ITestOutputHelper testOutputHelper)
    {

        [Fact]
        public async Task ExtractorBase_works_with_specified_versions_of_dotnet()
        {
            var sut = new FibonacciExtractorFromExtractorBase();

            Assert.Equal(10, await sut.ExtractAsync().Take(10).CountAsync());
        }




        [Fact]
        public async Task ExtractAsync_returns_expected_results()
        {
            var expectedResults = new[] { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55 };

            var sut = new FibonacciExtractorFromExtractorBase();

            var actualResults = await sut.ExtractAsync().ToListAsync();

            Assert.Equal(expectedResults, actualResults);
        }



        [Fact]
        public async Task ExtractWithCancellationAsync_returns_expected_results()
        {
            var expectedResults = new[] { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55 };

            var sut = new FibonacciExtractorFromExtractorBase();

            var actualResults = await sut.ExtractAsync(CancellationToken.None).ToListAsync();

            Assert.Equal(expectedResults, actualResults);
        }



        [Fact]
        public async Task ExtractWithCancellationAsync_throws_exception_when_cancellation_is_requested()
        {
            var sut = new FibonacciExtractorFromExtractorBase(5);

            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(5));

            await Assert.ThrowsAsync<TaskCanceledException>(async () =>  await sut.ExtractAsync(cts.Token).ToListAsync(CancellationToken.None));
        }



        [Fact]
        public async Task ExtractWithProgressAsync_when_passed_null_progress_throws_ArgumentNullException()
        {
            var sut = new FibonacciExtractorFromExtractorBase();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.ExtractAsync(null!).ToListAsync());
        }



        [Fact]
        public async Task ExtractWithProgressAsync_returns_expected_results()
        {
            var expectedResults = new[] { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55 };

            var sut = new FibonacciExtractorFromExtractorBase();

            var progress = new Progress<EtlProgress>(_ => { });

            var actualResults = await sut.ExtractAsync(progress).ToListAsync();

            Assert.Equal(expectedResults, actualResults);
        }




        [Fact(Skip = "Need to investigate why this test fails most of the time but occasionally passes")]
        public async Task ExtractWithProgressAsync_reports_progress_expected_results()
        {
            var sut = new FibonacciExtractorFromExtractorBase(250)
            {
                ReportingInterval = 100
            };

            var progressReportCount = 0;
            var progress = new Progress<EtlProgress>(_ =>
            {
                testOutputHelper.WriteLine("Progress reported.");
                Interlocked.Increment(ref progressReportCount);
            });

            await sut.ExtractAsync(progress).ToListAsync();
            Assert.True(progressReportCount > 0, $"Value was expected to be greater than 0 but was {progressReportCount}");
        }

        

        [Fact]
        public async Task ExtractWithProgressAsync_throws_exception_when_cancellation_is_requested()
        {
            var sut = new FibonacciExtractorFromExtractorBase(5);

            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(5));

            await Assert.ThrowsAsync<TaskCanceledException>(async () => await sut.ExtractAsync(cts.Token).ToListAsync(CancellationToken.None));
        }



        [Fact]
        public async Task ExtractWithProgressAndCancellationAsync_throws_exception_when_cancellation_is_requested()
        {
            var sut = new FibonacciExtractorFromExtractorBase(5);

            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(5));
            var progress = new Progress<EtlProgress>(_ => { });

            await Assert.ThrowsAsync<TaskCanceledException>(async () => await sut.ExtractAsync(progress, cts.Token).ToListAsync(CancellationToken.None));
        }



        [Fact]
        public async Task ExtractWithProgressAndCancellationAsync_when_passed_null_progress_throws_ArgumentNullException()
        {
            var sut = new FibonacciExtractorFromExtractorBase();

            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(5));

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.ExtractAsync(null!, cts.Token).ToListAsync(CancellationToken.None));
        }
        


        [Fact]
        public async Task ExtractWithProgressAndCancellationAsync_returns_expected_results()
        {
            var expectedResults = new[] { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55 };

            var sut = new FibonacciExtractorFromExtractorBase();

            var progress = new Progress<EtlProgress>(_ => { });

            var actualResults = await sut.ExtractAsync(progress, CancellationToken.None).ToListAsync();

            Assert.Equal(expectedResults, actualResults);
        }



        [Fact(Skip = "Need to investigate why this test fails most of the time but occasionally passes")]
        public async Task ExtractWithProgressAndCancellationAsync_reports_progress_expected_results()
        {
            var sut = new FibonacciExtractorFromExtractorBase(250)
            {
                ReportingInterval = 100
            };

            var progressReportCount = 0;
            var progress = new Progress<EtlProgress>(_ =>
            {
                testOutputHelper.WriteLine("Progress reported.");
                Interlocked.Increment(ref progressReportCount);
            });

            await sut.ExtractAsync(progress, CancellationToken.None).ToListAsync();

            Assert.True(progressReportCount > 0, $"Value was expected to be greater than 0 but was {progressReportCount}");
        }



        [Fact]
        public void CurrentItemCount_when_assigned_a_value_less_than_0_throws_ArgumentOutOfRangeException()
        {
            
            var sut = new FibonacciExtractorFromExtractorBase();

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.TestSettingCurrentItemCount( -1));
        }



        [Fact]
        public void CurrentItemCount_when_assigned_a_valid_value_stores_the_value()
        {

            var sut = new FibonacciExtractorFromExtractorBase();

            sut.TestSettingCurrentItemCount(10);

            Assert.Equal(10, sut.CurrentItemCount);
        }



        [Fact]
        public void ReportingInterval_when_assigned_a_value_less_than_0_throws_ArgumentOutOfRangeException()
        {

            var sut = new FibonacciExtractorFromExtractorBase();
            Assert.Throws<ArgumentOutOfRangeException>(() => sut.ReportingInterval = -1);
        }


        [Fact]
        public void ReportingInterval_when_assigned_a_valid_value_stores_the_value()
        {

            var sut = new FibonacciExtractorFromExtractorBase()
            {
                ReportingInterval = 10
            };
            Assert.Equal(10, sut.ReportingInterval);
        }



        [Fact]
        public void MaximumItemCount_when_assigned_a_value_less_than_1_throws_ArgumentOutOfRangeException()
        {

            var sut = new FibonacciExtractorFromExtractorBase();
            Assert.Throws<ArgumentOutOfRangeException>(() => sut.MaximumItemCount =0);
        }


        [Fact]
        public void MaximumItemCount_when_assigned_a_valid_value_stores_the_value()
        {

            var sut = new FibonacciExtractorFromExtractorBase()
            {
                MaximumItemCount = 10
            };
            Assert.Equal(10, sut.MaximumItemCount);
        }

        
        [Fact]
        public void SkipItemCount_when_assigned_a_value_less_than_0_throws_ArgumentOutOfRangeException()
        {

            var sut = new FibonacciExtractorFromExtractorBase();
            Assert.Throws<ArgumentOutOfRangeException>(() => sut.SkipItemCount = -1);
        }


        [Fact]
        public void SkipItemCount_when_assigned_a_valid_value_stores_the_value()
        {

            var sut = new FibonacciExtractorFromExtractorBase()
            {
                SkipItemCount = 10
            };
            Assert.Equal(10, sut.SkipItemCount);
        }





    }




    [ExcludeFromCodeCoverage]
    internal class FibonacciExtractorFromExtractorBase(int delay) : ExtractorBase<int, EtlProgress>
    {
        public FibonacciExtractorFromExtractorBase() : this(0) { }


        protected override async IAsyncEnumerable<int> ExtractWorkerAsync
        (
            [EnumeratorCancellation] CancellationToken token
        )
        {
            var stopwatch = Stopwatch.StartNew();

            var current = 1;
            var previous = 0;
            for (var x = 0; x < 10; ++x)
            {
                await Task.Delay(delay, token);  // Simulate asynchronous operation
                if (token.IsCancellationRequested)
                {
                    throw new TaskCanceledException("Extraction was cancelled.");
                }
                yield return current;
                ++CurrentItemCount;

                var temp = current;
                current += previous;
                previous = temp;
            }

            stopwatch.Stop();
        }



        protected override EtlProgress CreateProgressReport()
        {
            return new EtlProgress(CurrentItemCount);
        }



        /// <summary>
        /// Used for testing purposes to set the CurrentItemCount property.
        /// </summary>
        /// <param name="value"></param>
        public void TestSettingCurrentItemCount(int value)
        {
            CurrentItemCount = value;
        }
    }
}
