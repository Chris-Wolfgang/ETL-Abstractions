using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests
{
    public class TransformerBaseTests
    {


        [Fact]
        public async Task TransformerBase_works_with_specified_versions_of_dotnet()
        {
            var numbers = new[] { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55 };
            var sut = new IntToStringTransformerFromTransformerBase();

            var actual = await sut.TransformAsync(numbers.ToAsyncEnumerable()).ToListAsync();
            var expected = numbers.Select(n => n.ToString()).ToList();

            Assert.Equal(expected, actual);
        }



        // Transform tests

        [Fact]
        public async Task TransformAsync_when_passed_null_items_throws_ArgumentNullException()
        {
            var sut = new IntToStringTransformerFromTransformerBase();

            await Assert.ThrowsAsync<ArgumentNullException> (async ()=> await sut.TransformAsync(null!).ToListAsync());
        }



        [Fact]
        public async Task TransformAsync_returns_expected_values()
        {
            var sut = new IntToStringTransformerFromTransformerBase();

            var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToAsyncEnumerable();

            var expectedResults = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
            var actualResults = await sut.TransformAsync(items).ToListAsync();

            Assert.Equal(expectedResults, actualResults);
        }



        // Transform With Cancellation tests


        [Fact]
        public async Task TransformWithCancellationAsync_when_passed_null_items_throws_ArgumentNullException()
        {
            var sut = new IntToStringTransformerFromTransformerBase();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await sut.TransformAsync(null!, CancellationToken.None).ToListAsync());
        }



        [Fact]
        public async Task TransformWithCancellationAsync_returns_expected_values()
        {
            var sut = new IntToStringTransformerFromTransformerBase();

            var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToAsyncEnumerable();

            var expectedResults = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
            var actualResults = await sut.TransformAsync(items, CancellationToken.None).ToListAsync();

            Assert.Equal(expectedResults, actualResults);
        }



        [Fact]
        public async Task TransformWithCancellationAsync_when_cancelled_throws_OperationCancelledException()
        {
            var sut = new IntToStringTransformerFromTransformerBase(1000);

            var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToAsyncEnumerable();
            var cts = new CancellationTokenSource();

            var task = sut.TransformAsync(items, cts.Token);

            try
            {
                cts.Cancel();
                await task.ToListAsync();
                Assert.Fail("OperationCanceledException was expected but not thrown");
            }
            catch (OperationCanceledException)
            {
                // Expected exception was thrown
            }
        }




        // Transform With Progress tests


        [Fact]
        public async Task TransformWithProgressAsync_when_passed_null_items_throws_ArgumentNullException()
        {
            var sut = new IntToStringTransformerFromTransformerBase();

            var progress = new Progress<EtlProgress>(_ => { });

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await sut.TransformAsync(null!, progress).ToListAsync());
            Assert.Equal("items", ex.ParamName);
        }



        [Fact]
        public async Task TransformWithProgressAsync_when_passed_null_progress_throws_ArgumentNullException()
        {
            var sut = new IntToStringTransformerFromTransformerBase();

            var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToAsyncEnumerable();

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await sut.TransformAsync(items, null!).ToListAsync());
            Assert.Equal("progress", ex.ParamName);
        }



        [Fact]
        public async Task TransformWithProgressAsync_returns_expected_values()
        {
            var sut = new IntToStringTransformerFromTransformerBase();

            var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToAsyncEnumerable();
            var progress = new Progress<EtlProgress>(_ => { });

            var expectedResults = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
            var actualResults = await sut.TransformAsync(items, progress).ToListAsync();

            Assert.Equal(expectedResults, actualResults);
        }



        [Fact(Skip = "Test succeeds on its own, but fails when run with other tests. Need to determine what is going on and get it working")]
        public async Task TransformWithProgressAsync_reports_progress()
        {
            var sut = new IntToStringTransformerFromTransformerBase(100)
            {
                ReportingInterval = 100 // Report progress every second
            };

            var progressReported = false;

            var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToAsyncEnumerable();
            var progress = new Progress<EtlProgress>(_ => progressReported = true);

            await sut.TransformAsync(items, progress).ToListAsync();

            Assert.True(progressReported, "Progress was not reported");
        }



        // Transform With Progress And Cancellation tests


        [Fact]
        public async Task TransformWithProgressAndCancellationAsync_when_passed_null_items_throws_ArgumentNullException()
        {
            var sut = new IntToStringTransformerFromTransformerBase();

            var progress = new Progress<EtlProgress>(_ => { });

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await sut.TransformAsync(null!, progress, CancellationToken.None).ToListAsync());
            Assert.Equal("items", ex.ParamName);
        }



        [Fact]
        public async Task TransformWithProgressAndCancellationAsync_when_passed_null_progress_throws_ArgumentNullException()
        {
            var sut = new IntToStringTransformerFromTransformerBase();

            var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToAsyncEnumerable();

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await sut.TransformAsync(items, null!, CancellationToken.None).ToListAsync());
            Assert.Equal("progress", ex.ParamName);
        }


        [Fact]
        public async Task TransformWithProgressAndCancellationAsync_returns_expected_values()
        {
            var sut = new IntToStringTransformerFromTransformerBase();

            var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToAsyncEnumerable();
            var progress = new Progress<EtlProgress>(_ => { });

            var expectedResults = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
            var actualResults = await sut.TransformAsync(items, progress, CancellationToken.None).ToListAsync();

            Assert.Equal(expectedResults, actualResults);
        }



        [Fact(Skip = "Test succeeds on its own, but fails when run with other tests. Need to determine what is going on and get it working")]
        public async Task TransformWithProgressAndCancellationAsync_reports_progress()
        {
            var sut = new IntToStringTransformerFromTransformerBase
            {
                ReportingInterval = 1000 // Report progress every second
            };

            var progressReported = false;

            var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToAsyncEnumerable();
            var progress = new Progress<EtlProgress>(_ =>
            {
                progressReported = true;
            });

            await sut.TransformAsync(items, progress, CancellationToken.None).ToListAsync();

            Assert.True(progressReported, "Progress was not reported");
        }




        [Fact]
        public async Task TransformWithProgressAndCancellationAsync_when_cancelled_throws_OperationCancelledException()
        {
            var sut = new IntToStringTransformerFromTransformerBase(100);

            var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToAsyncEnumerable();
            var cts = new CancellationTokenSource();

            var progress = new Progress<EtlProgress>();
            var task = sut.TransformAsync(items, progress, cts.Token);

            try
            {
                cts.Cancel();
                await task.ToListAsync();
                Assert.Fail("OperationCanceledException was expected but not thrown");
            }
            catch (OperationCanceledException)
            {
                // Expected exception was thrown
            }
        }



        [Fact]
        public void CurrentItemCount_when_assigned_a_value_less_than_0_throws_ArgumentOutOfRangeException()
        {

            var sut = new IntToStringTransformerFromTransformerBase();
            Assert.Throws<ArgumentOutOfRangeException>(() => sut.TestSettingCurrentItemCount(-1));
        }



        [Fact]
        public void CurrentItemCount_when_assigned_a_valid_value_stores_the_value()
        {

            var sut = new IntToStringTransformerFromTransformerBase();
            sut.TestSettingCurrentItemCount(10);

            Assert.Equal(10, sut.CurrentItemCount);
        }



        [Fact]
        public void ReportingInterval_when_assigned_a_value_less_than_0_throws_ArgumentOutOfRangeException()
        {

            var sut = new IntToStringTransformerFromTransformerBase();
            Assert.Throws<ArgumentOutOfRangeException>(() => sut.ReportingInterval = -1);
        }



        [Fact]
        public void ReportingInterval_when_assigned_a_valid_value_stores_the_value()
        {

            var sut = new IntToStringTransformerFromTransformerBase
            {
                ReportingInterval = 10
            };
            Assert.Equal(10, sut.ReportingInterval);
        }



        [Fact]
        public void MaximumItemCount_when_assigned_a_value_less_than_1_throws_ArgumentOutOfRangeException()
        {

            var sut = new IntToStringTransformerFromTransformerBase();

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.MaximumItemCount = -1);
        }



        [Fact]
        public void MaximumItemCount_when_assigned_a_valid_value_stores_the_value()
        {

            var sut = new IntToStringTransformerFromTransformerBase
            {
                MaximumItemCount = 10
            };
            Assert.Equal(10, sut.MaximumItemCount);
        }



        [Fact]
        public void SkipItemCount_when_assigned_a_value_less_than_0_throws_ArgumentOutOfRangeException()
        {

            var sut = new IntToStringTransformerFromTransformerBase();
            Assert.Throws<ArgumentOutOfRangeException>(() => sut.SkipItemCount = -1);
        }



        [Fact]
        public void SkipItemCount_when_assigned_a_valid_value_stores_the_value()
        {

            var sut = new IntToStringTransformerFromTransformerBase
            {
                SkipItemCount = 10
            };
            Assert.Equal(10, sut.SkipItemCount);
        }



        [ExcludeFromCodeCoverage]
        internal class IntToStringTransformerFromTransformerBase : TransformerBase<int, string, EtlProgress>
        {
            private readonly int _delay;

            public IntToStringTransformerFromTransformerBase(int delay = 0)
            {
                if (delay < 0) throw new ArgumentOutOfRangeException(nameof(delay));
                _delay = delay;
            }



            protected override async IAsyncEnumerable<string> TransformWorkerAsync
            (
                IAsyncEnumerable<int> items,
                [EnumeratorCancellation] CancellationToken token
            )
            {
                await foreach (var item in items.WithCancellation(token))
                {
                    await Task.Delay(_delay, token); // Simulate some delay in processing
                    if (token.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(token);
                    }
                    yield return item.ToString();
                    ++CurrentItemCount;
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
}
