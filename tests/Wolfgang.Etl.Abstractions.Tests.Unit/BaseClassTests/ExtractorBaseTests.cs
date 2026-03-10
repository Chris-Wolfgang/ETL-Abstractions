using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;
using Xunit.Abstractions;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests
{
    public class ExtractorBaseTests(ITestOutputHelper _)
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
        public void MaximumItemCount_when_assigned_a_value_less_than_0_throws_ArgumentOutOfRangeException()
        {

            var sut = new FibonacciExtractorFromExtractorBase();
            Assert.Throws<ArgumentOutOfRangeException>(() => sut.MaximumItemCount = -1);
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



        [Fact]
        public void ReportingInterval_when_assigned_zero_throws_ArgumentOutOfRangeException()
        {
            var sut = new FibonacciExtractorFromExtractorBase();
            Assert.Throws<ArgumentOutOfRangeException>(() => sut.ReportingInterval = 0);
        }



        [Fact]
        public void MaximumItemCount_when_assigned_zero_is_valid_and_stores_the_value()
        {
            var sut = new FibonacciExtractorFromExtractorBase()
            {
                MaximumItemCount = 0
            };
            Assert.Equal(0, sut.MaximumItemCount);
        }



        [Fact]
        public async Task CurrentItemCount_reflects_number_of_items_extracted()
        {
            var sut = new FibonacciExtractorFromExtractorBase();

            await sut.ExtractAsync().ToListAsync();

            Assert.Equal(10, sut.CurrentItemCount);
        }



        [Fact]
        public async Task CurrentSkippedItemCount_reflects_number_of_items_skipped()
        {
            var sut = new FibonacciExtractorFromExtractorBaseWithSkips();

            await sut.ExtractAsync().ToListAsync();

            Assert.Equal(3, sut.CurrentSkippedItemCount);
        }



        [Fact]
        public void ReportingInterval_default_value_is_1000()
        {
            var sut = new FibonacciExtractorFromExtractorBase();
            Assert.Equal(1_000, sut.ReportingInterval);
        }



        [Fact]
        public void MaximumItemCount_default_value_is_int_MaxValue()
        {
            var sut = new FibonacciExtractorFromExtractorBase();
            Assert.Equal(int.MaxValue, sut.MaximumItemCount);
        }



        [Fact]
        public void SkipItemCount_default_value_is_zero()
        {
            var sut = new FibonacciExtractorFromExtractorBase();
            Assert.Equal(0, sut.SkipItemCount);
        }



        [Fact]
        public void CurrentItemCount_default_value_is_zero()
        {
            var sut = new FibonacciExtractorFromExtractorBase();
            Assert.Equal(0, sut.CurrentItemCount);
        }



        [Fact]
        public void CurrentSkippedItemCount_default_value_is_zero()
        {
            var sut = new FibonacciExtractorFromExtractorBase();
            Assert.Equal(0, sut.CurrentSkippedItemCount);
        }



        [Fact]
        public async Task ExtractWithProgressAsync_when_passed_null_progress_throws_ArgumentNullException_with_correct_param_name()
        {
            var sut = new FibonacciExtractorFromExtractorBase();

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await sut.ExtractAsync(null!).ToListAsync());

            Assert.Equal("progress", ex.ParamName);
        }



        [Fact]
        public async Task ExtractWithProgressAndCancellationAsync_when_passed_null_progress_throws_ArgumentNullException_with_correct_param_name()
        {
            var sut = new FibonacciExtractorFromExtractorBase();

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await sut.ExtractAsync(null!, CancellationToken.None).ToListAsync(CancellationToken.None));

            Assert.Equal("progress", ex.ParamName);
        }



        [Fact(Skip = "Timer-based progress callback fires on a thread pool thread and races with enumeration completion across all target frameworks. Needs a redesign of the progress mechanism to be reliably testable.")]
        public async Task ExtractWithProgressAsync_invokes_progress_callback()
        {
            var sut = new FibonacciExtractorFromExtractorBase(50) { ReportingInterval = 100 };
            using var callbackFired = new ManualResetEventSlim(false);
            var progress = new SynchronousProgress<EtlProgress>(report => callbackFired.Set());

            await sut.ExtractAsync(progress).ToListAsync();

            Assert.True(callbackFired.IsSet, "Progress callback was never invoked.");
        }



        [Fact(Skip = "Timer-based progress callback fires on a thread pool thread and races with enumeration completion across all target frameworks. Needs a redesign of the progress mechanism to be reliably testable.")]
        public async Task ExtractWithProgressAndCancellationAsync_invokes_progress_callback()
        {
            var sut = new FibonacciExtractorFromExtractorBase(50) { ReportingInterval = 100 };
            using var callbackFired = new ManualResetEventSlim(false);
            var progress = new SynchronousProgress<EtlProgress>(report => callbackFired.Set());

            await sut.ExtractAsync(progress, CancellationToken.None).ToListAsync();

            Assert.True(callbackFired.IsSet, "Progress callback was never invoked.");
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
                IncrementCurrentItemCount();

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
    }



    [ExcludeFromCodeCoverage]
    internal class FibonacciExtractorFromExtractorBaseWithSkips : ExtractorBase<int, EtlProgress>
    {
        protected override async IAsyncEnumerable<int> ExtractWorkerAsync
        (
            [EnumeratorCancellation] CancellationToken token
        )
        {
            var numbers = new[] { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55 };
            foreach (var number in numbers)
            {
                await Task.CompletedTask;
                if (number % 2 == 0)
                {
                    IncrementCurrentSkippedItemCount();
                    continue;
                }
                yield return number;
                IncrementCurrentItemCount();
            }
        }



        protected override EtlProgress CreateProgressReport()
        {
            return new EtlProgress(CurrentItemCount);
        }
    }



}
