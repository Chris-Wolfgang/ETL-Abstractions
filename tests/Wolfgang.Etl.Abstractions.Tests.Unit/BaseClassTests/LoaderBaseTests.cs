using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests
{
    public class LoaderBaseTests
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
    }



    public class ConsoleLoaderFromBase(List<string> buffer) : LoaderBase<string, EtlProgress>
    {

        private readonly List<string> _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));


        protected override async Task LoadWorkerAsync(IAsyncEnumerable<string> items, CancellationToken token)
        {
            await foreach (var item in items.WithCancellation(token))
            {
                Console.WriteLine($"Loading item: {item}");
                _buffer.Add(item);
            }
        }



        protected override EtlProgress CreateProgressReport()
        {
            return new EtlProgress(CurrentItemCount);
        }
    }
}
