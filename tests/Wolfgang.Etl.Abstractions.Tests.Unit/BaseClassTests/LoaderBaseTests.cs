using System.Diagnostics.CodeAnalysis;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;
using Xunit.Abstractions;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests
{
    public class LoaderBaseTests(ITestOutputHelper testOutputHelper)
    {


        [Fact]
        public async Task LoaderBase_works_with_specified_versions_of_dotnet()
        {
            var actualItems = new List<string>();
            var sut = new ConsoleLoaderFromBase(actualItems);

            var expected = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
            var items = expected.ToAsyncEnumerable();

            await sut.LoadAsync(items);

            Assert.Equal(expected, actualItems.ToArray());
        }



        [Fact]
        public async Task LoadAsync_when_passed_null_items_throws_ArgumentNullException()
        {
            var actualResults = new List<string>();

            var sut = new ConsoleLoaderFromBase(actualResults);

            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.LoadAsync(null!));
        }



        [Fact]
        public async Task LoadAsync_returns_expected_results()
        {
            var expectedResults = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };

            var actualResults = new List<string>();

            var sut = new ConsoleLoaderFromBase(actualResults);

            await sut.LoadAsync(expectedResults.ToAsyncEnumerable());

            Assert.Equal(expectedResults, actualResults);
        }



        [Fact]
        public async Task LoadWithCancellationAsync_when_passed_null_items_throws_ArgumentNullException()
        {
            var actualResults = new List<string>();
            var sut = new ConsoleLoaderFromBase(actualResults);

            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.LoadAsync(null!, CancellationToken.None));
        }



        [Fact]
        public async Task LoadWithCancellationAsync_returns_expected_results()
        {
            var expectedResults = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };

            var actualResults = new List<string>();
            var sut = new ConsoleLoaderFromBase(actualResults);

            await sut.LoadAsync(expectedResults.ToAsyncEnumerable(), CancellationToken.None);

            Assert.Equal(expectedResults, actualResults);
        }



        [Fact(Skip = "Need to determine why cancellation request isn't being honored.")]
        public async Task LoadWithCancellationAsync_throws_exception_when_cancellation_is_requested()
        {
            var buffer = new List<string>();
            const int delay = 100; // Delay in milliseconds
            var sut = new ConsoleLoaderFromBase(buffer, delay);

            using var cts = new CancellationTokenSource();
            var expectedResults = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };

            // Start the load operation
            var task = sut.LoadAsync(expectedResults.ToAsyncEnumerable(), cts.Token);

            try
            {
                cts.Cancel();

                await task;
                Assert.Fail("Expected OperationCanceledException was not thrown.");
            }
            catch (OperationCanceledException)
            {
                // Expected exception
            }
        }

        

        [Fact]
        public async Task LoadWithProgressAsync_when_passed_null_items_throws_ArgumentNullException()
        {
            var expectedResults = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };

            var actualResults = new List<string>();

            var sut = new ConsoleLoaderFromBase(actualResults);

            var progress = new Progress<EtlProgress>(_ => { });

            await Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await sut.LoadAsync(null!, progress));
        }



        [Fact]
        public async Task LoadWithProgressAsync_when_passed_null_progress_throws_ArgumentNullException()
        {
            var expectedResults = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };

            var actualResults = new List<string>();

            var sut = new ConsoleLoaderFromBase(actualResults);
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await sut.LoadAsync(expectedResults.ToAsyncEnumerable(), null!));
        }



        [Fact]
        public async Task LoadWithProgressAsync_returns_expected_results()
        {
            var expectedResults = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };

            var actualResults = new List<string>();

            var sut = new ConsoleLoaderFromBase(actualResults);
            var progress = new Progress<EtlProgress>(_ => { });

            await sut.LoadAsync(expectedResults.ToAsyncEnumerable(), progress);

            Assert.Equal(expectedResults, actualResults);
        }



        [Fact(Skip = "Need to investigate why this test fails most of the time but occasionally passes")]
        public async Task LoadWithProgressAsync_reports_progress_expected_results()
        {
            var expectedResults = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };

            var actualResults = new List<string>();

            var sut = new ConsoleLoaderFromBase(actualResults)
            {
                ReportingInterval = 100
            };

            var progressReportCount = 0;
            var progress = new Progress<EtlProgress>(_ =>
            {
                testOutputHelper.WriteLine("Progress reported.");
                Interlocked.Increment(ref progressReportCount);
            });

            await sut.LoadAsync(expectedResults.ToAsyncEnumerable(), progress);
            Assert.True(progressReportCount > 0, $"Value was expected to be greater than 0 but was {progressReportCount}");
        }



        [Fact(Skip = "Need to determine why cancellation request isn't being honored.")]
        public async Task LoadWithProgressAsync_throws_exception_when_cancellation_is_requested()
        {
            var expectedResults = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
            var actualResults = new List<string>();

            var sut = new ConsoleLoaderFromBase(actualResults, 1000);

            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

            await Assert.ThrowsAsync<TaskCanceledException>(async () => 
                await sut.LoadAsync(expectedResults.ToAsyncEnumerable(), cts.Token));
        }



        [Fact]
        public async Task LoadWithProgressAndCancellationAsync_when_passed_null_items_throws_ArgumentNullException()
        {
            var actualResults = new List<string>();

            var sut = new ConsoleLoaderFromBase(actualResults);

            var progressReportCount = 0;
            var progress = new Progress<EtlProgress>(_ =>
            {
                testOutputHelper.WriteLine("Progress reported.");
                Interlocked.Increment(ref progressReportCount);
            });

            await Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await sut.LoadAsync(null!, progress, CancellationToken.None));
        }



        [Fact]
        public async Task LoadWithProgressAndCancellationAsync_when_passed_null_progress_throws_ArgumentNullException()
        {
            var expectedResults = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
            var actualResults = new List<string>();

            var sut = new ConsoleLoaderFromBase(actualResults);

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await sut.LoadAsync(expectedResults.ToAsyncEnumerable(), null!, CancellationToken.None));
        }



        [Fact]
        public async Task LoadWithProgressAndCancellationAsync_returns_expected_results()
        {
            var expectedResults = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
            var actualResults = new List<string>();

            var sut = new ConsoleLoaderFromBase(actualResults);
            var progress = new Progress<EtlProgress>(_ => { });

            await sut.LoadAsync(expectedResults.ToAsyncEnumerable(), progress, CancellationToken.None);

            Assert.Equal(expectedResults, actualResults);
        }



        [Fact(Skip = "Need to investigate why this test fails most of the time but occasionally passes")]
        public async Task LoadWithProgressAndCancellationAsync_reports_progress_expected_results()
        {
            var expectedResults = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
            var actualResults = new List<string>();

            var sut = new ConsoleLoaderFromBase(actualResults, 1000)
            {
                ReportingInterval = 100
            };

            var progressReportCount = 0;
            var progress = new Progress<EtlProgress>(_ =>
            {
                testOutputHelper.WriteLine("Progress reported.");
                Interlocked.Increment(ref progressReportCount);
            });

            await sut.LoadAsync(expectedResults.ToAsyncEnumerable(), progress, CancellationToken.None);

            Assert.True(progressReportCount > 0, $"Value was expected to be greater than 0 but was {progressReportCount}");
        }



        [Fact]
        public void CurrentItemCount_when_assigned_a_value_less_than_0_throws_ArgumentOutOfRangeException()
        {

            var sut = new ConsoleLoaderFromBase(new List<string>());
            Assert.Throws<ArgumentOutOfRangeException>(() => sut.TestSettingCurrentItemCount(-1));
        }



        [Fact]
        public void CurrentItemCount_when_assigned_a_valid_value_stores_the_value()
        {

            var sut = new ConsoleLoaderFromBase(new List<string>());
            sut.TestSettingCurrentItemCount(10);

            Assert.Equal(10, sut.CurrentItemCount);
        }



        [Fact]
        public void ReportingInterval_when_assigned_a_value_less_than_0_throws_ArgumentOutOfRangeException()
        {

            var sut = new ConsoleLoaderFromBase(new List<string>());
            Assert.Throws<ArgumentOutOfRangeException>(() => sut.ReportingInterval = -1);
        }



        [Fact]
        public void ReportingInterval_when_assigned_a_valid_value_stores_the_value()
        {

            var sut = new ConsoleLoaderFromBase(new List<string>())
            {
                ReportingInterval = 10
            };
            Assert.Equal(10, sut.ReportingInterval);
        }



        [Fact]
        public void MaximumItemCount_when_assigned_a_value_less_than_1_throws_ArgumentOutOfRangeException()
        {

            var sut = new ConsoleLoaderFromBase(new List<string>());

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.MaximumItemCount = 0);
        }



        [Fact]
        public void MaximumItemCount_when_assigned_a_valid_value_stores_the_value()
        {

            var sut = new ConsoleLoaderFromBase(new List<string>())
            {
                MaximumItemCount = 10
            };
            Assert.Equal(10, sut.MaximumItemCount);
        }



        [Fact]
        public void SkipItemCount_when_assigned_a_value_less_than_0_throws_ArgumentOutOfRangeException()
        {

            var sut = new ConsoleLoaderFromBase(new List<string>());
            Assert.Throws<ArgumentOutOfRangeException>(() => sut.SkipItemCount = -1);
        }



        [Fact]
        public void SkipItemCount_when_assigned_a_valid_value_stores_the_value()
        {

            var sut = new ConsoleLoaderFromBase(new List<string>())
            {
                SkipItemCount = 10
            };
            Assert.Equal(10, sut.SkipItemCount);
        }
    }



    [ExcludeFromCodeCoverage]
    internal class ConsoleLoaderFromBase : LoaderBase<string, EtlProgress>
    {

        private readonly List<string> _buffer;
        private readonly int _delay;

        public ConsoleLoaderFromBase(List<string> buffer, int delay = 0)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            if (delay < 0)
                throw new ArgumentOutOfRangeException(nameof(delay), "Delay must be non-negative.");
            _delay = delay;
        }


        protected override async Task LoadWorkerAsync(IAsyncEnumerable<string> items, CancellationToken token)
        {
            Console.WriteLine($"Delay {_delay}");

            await foreach (var item in items)
            {
                Console.WriteLine($"Waiting {_delay}ms");
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("Operation was cancelled.");
                    throw new TaskCanceledException("The load operation was cancelled.");
                }
                Console.WriteLine($"Loading item: {item}");
                await Task.Delay(_delay, token);
                _buffer.Add(item);
            }
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
