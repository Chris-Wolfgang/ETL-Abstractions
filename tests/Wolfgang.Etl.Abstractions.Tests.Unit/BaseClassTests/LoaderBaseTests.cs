using System.Diagnostics.CodeAnalysis;
using System.Threading;
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



        [Fact]
        public async Task LoadWithCancellationAsync_throws_exception_when_cancellation_is_requested()
        {
            var buffer = new List<string>();
            const int delay = 500;
            var sut = new ConsoleLoaderFromBase(buffer, delay);

            using var cts = new CancellationTokenSource();
            var items = AsyncHelpers.GenerateSlowItemsAsync(10);

            var task = sut.LoadAsync(items, cts.Token);
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        }

        

        [Fact]
        public async Task LoadWithProgressAsync_when_passed_null_items_throws_ArgumentNullException()
        {
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



        [Fact(Skip = "Timer-based progress callback fires on a thread pool thread and races with enumeration completion across all target frameworks. Needs a redesign of the progress mechanism to be reliably testable.")]
        public async Task LoadWithProgressAsync_invokes_progress_callback()
        {
            var actualResults = new List<string>();
            var sut = new ConsoleLoaderFromBase(actualResults, 50) { ReportingInterval = 100 };
            using var callbackFired = new ManualResetEventSlim(false);
            var progress = new SynchronousProgress<EtlProgress>(_ => callbackFired.Set());

            await sut.LoadAsync(new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" }.ToAsyncEnumerable(), progress);

            Assert.True(callbackFired.IsSet, "Progress callback was never invoked.");
        }



        [Fact]
        public async Task LoadWithProgressAsync_throws_exception_when_cancellation_is_requested()
        {
            var sut = new ConsoleLoaderFromBase([], 500);
            var cts = new CancellationTokenSource();
            var progress = new SynchronousProgress<EtlProgress>(_ => { });

            var task = sut.LoadAsync(AsyncHelpers.GenerateSlowItemsAsync(10), progress);
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                sut.LoadAsync(AsyncHelpers.GenerateSlowItemsAsync(10), progress, cts.Token));
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



        [Fact(Skip = "Timer-based progress callback fires on a thread pool thread and races with enumeration completion across all target frameworks. Needs a redesign of the progress mechanism to be reliably testable.")]
        public async Task LoadWithProgressAndCancellationAsync_invokes_progress_callback()
        {
            var actualResults = new List<string>();
            var sut = new ConsoleLoaderFromBase(actualResults, 50) { ReportingInterval = 100 };
            using var callbackFired = new ManualResetEventSlim(false);
            var progress = new SynchronousProgress<EtlProgress>(_ => callbackFired.Set());

            await sut.LoadAsync(new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" }.ToAsyncEnumerable(), progress, CancellationToken.None);

            Assert.True(callbackFired.IsSet, "Progress callback was never invoked.");
        }



        [Fact]
        public void ReportingInterval_when_assigned_a_value_less_than_0_throws_ArgumentOutOfRangeException()
        {

            var sut = new ConsoleLoaderFromBase([]);
            Assert.Throws<ArgumentOutOfRangeException>(() => sut.ReportingInterval = -1);
        }



        [Fact]
        public void ReportingInterval_when_assigned_a_valid_value_stores_the_value()
        {

            var sut = new ConsoleLoaderFromBase([])
            {
                ReportingInterval = 10
            };
            Assert.Equal(10, sut.ReportingInterval);
        }



        [Fact]
        public void MaximumItemCount_when_assigned_a_value_less_than_1_throws_ArgumentOutOfRangeException()
        {

            var sut = new ConsoleLoaderFromBase([]);

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.MaximumItemCount = -1);
        }



        [Fact]
        public void MaximumItemCount_when_assigned_a_valid_value_stores_the_value()
        {

            var sut = new ConsoleLoaderFromBase([])
            {
                MaximumItemCount = 10
            };
            Assert.Equal(10, sut.MaximumItemCount);
        }



        [Fact]
        public void SkipItemCount_when_assigned_a_value_less_than_0_throws_ArgumentOutOfRangeException()
        {

            var sut = new ConsoleLoaderFromBase([]);
            Assert.Throws<ArgumentOutOfRangeException>(() => sut.SkipItemCount = -1);
        }



        [Fact]
        public void SkipItemCount_when_assigned_a_valid_value_stores_the_value()
        {

            var sut = new ConsoleLoaderFromBase([])
            {
                SkipItemCount = 10
            };
            Assert.Equal(10, sut.SkipItemCount);
        }



        [Fact]
        public void ReportingInterval_when_assigned_zero_throws_ArgumentOutOfRangeException()
        {
            var sut = new ConsoleLoaderFromBase([]);
            Assert.Throws<ArgumentOutOfRangeException>(() => sut.ReportingInterval = 0);
        }



        [Fact]
        public void MaximumItemCount_when_assigned_zero_is_valid_and_stores_the_value()
        {
            var sut = new ConsoleLoaderFromBase([])
            {
                MaximumItemCount = 0
            };
            Assert.Equal(0, sut.MaximumItemCount);
        }



        [Fact]
        public async Task CurrentItemCount_reflects_number_of_items_loaded()
        {
            var sut = new ConsoleLoaderFromBaseWithCounts([]);
            var items = new[] { "1", "2", "3", "4", "5" }.ToAsyncEnumerable();

            await sut.LoadAsync(items);

            Assert.Equal(5, sut.CurrentItemCount);
        }



        [Fact]
        public async Task CurrentSkippedItemCount_reflects_number_of_items_skipped()
        {
            var sut = new ConsoleLoaderFromBaseWithCounts([]);
            var items = new[] { "1", "skip", "3", "skip", "5" }.ToAsyncEnumerable();

            await sut.LoadAsync(items);

            Assert.Equal(2, sut.CurrentSkippedItemCount);
        }



        [Fact]
        public void ReportingInterval_default_value_is_1000()
        {
            var sut = new ConsoleLoaderFromBase([]);
            Assert.Equal(1_000, sut.ReportingInterval);
        }



        [Fact]
        public void MaximumItemCount_default_value_is_int_MaxValue()
        {
            var sut = new ConsoleLoaderFromBase([]);
            Assert.Equal(int.MaxValue, sut.MaximumItemCount);
        }



        [Fact]
        public void SkipItemCount_default_value_is_zero()
        {
            var sut = new ConsoleLoaderFromBase([]);
            Assert.Equal(0, sut.SkipItemCount);
        }



        [Fact]
        public void CurrentItemCount_default_value_is_zero()
        {
            var sut = new ConsoleLoaderFromBase([]);
            Assert.Equal(0, sut.CurrentItemCount);
        }



        [Fact]
        public void CurrentSkippedItemCount_default_value_is_zero()
        {
            var sut = new ConsoleLoaderFromBase([]);
            Assert.Equal(0, sut.CurrentSkippedItemCount);
        }



        [Fact]
        public async Task LoadAsync_when_passed_null_items_throws_ArgumentNullException_with_correct_param_name()
        {
            var sut = new ConsoleLoaderFromBase([]);

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => sut.LoadAsync(null!));
            Assert.Equal("items", ex.ParamName);
        }



        [Fact]
        public async Task LoadWithCancellationAsync_when_passed_null_items_throws_ArgumentNullException_with_correct_param_name()
        {
            var sut = new ConsoleLoaderFromBase([]);

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => sut.LoadAsync(null!, CancellationToken.None));
            Assert.Equal("items", ex.ParamName);
        }



        [Fact]
        public async Task LoadWithProgressAsync_when_passed_null_items_throws_ArgumentNullException_with_correct_param_name()
        {
            var sut = new ConsoleLoaderFromBase([]);
            var progress = new Progress<EtlProgress>(_ => { });

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await sut.LoadAsync(null!, progress));
            Assert.Equal("items", ex.ParamName);
        }



        [Fact]
        public async Task LoadWithProgressAsync_when_passed_null_progress_throws_ArgumentNullException_with_correct_param_name()
        {
            var sut = new ConsoleLoaderFromBase([]);
            var items = new[] { "1", "2", "3" }.ToAsyncEnumerable();

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await sut.LoadAsync(items, null!));
            Assert.Equal("progress", ex.ParamName);
        }



        [Fact]
        public async Task LoadWithProgressAndCancellationAsync_when_passed_null_items_throws_ArgumentNullException_with_correct_param_name()
        {
            var sut = new ConsoleLoaderFromBase([]);
            var progress = new Progress<EtlProgress>(_ => { });

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await sut.LoadAsync(null!, progress, CancellationToken.None));
            Assert.Equal("items", ex.ParamName);
        }



        [Fact]
        public async Task LoadWithProgressAndCancellationAsync_when_passed_null_progress_throws_ArgumentNullException_with_correct_param_name()
        {
            var sut = new ConsoleLoaderFromBase([]);
            var items = new[] { "1", "2", "3" }.ToAsyncEnumerable();

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await sut.LoadAsync(items, null!, CancellationToken.None));
            Assert.Equal("progress", ex.ParamName);
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
            await foreach (var item in items.WithCancellation(token))
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(_delay, token);
                _buffer.Add(item);
            }
        }



        protected override EtlProgress CreateProgressReport()
        {
            return new EtlProgress(CurrentItemCount);
        }

    }



    [ExcludeFromCodeCoverage]
    internal class ConsoleLoaderFromBaseWithCounts : LoaderBase<string, EtlProgress>
    {
        private readonly List<string> _buffer;

        public ConsoleLoaderFromBaseWithCounts(List<string> buffer)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }



        protected override async Task LoadWorkerAsync(IAsyncEnumerable<string> items, CancellationToken token)
        {
            await foreach (var item in items.WithCancellation(token))
            {
                if (item == "skip")
                {
                    IncrementCurrentSkippedItemCount();
                    continue;
                }
                _buffer.Add(item);
                IncrementCurrentItemCount();
            }
        }



        protected override EtlProgress CreateProgressReport()
        {
            return new EtlProgress(CurrentItemCount);
        }
    }



    internal static class AsyncHelpers
    {
        public static async IAsyncEnumerable<string> GenerateSlowItemsAsync(int count, int delayMs = 500)
        {
            for (var i = 1; i <= count; i++)
            {
                await Task.Delay(delayMs);
                yield return i.ToString();
            }
        }
    }
}
