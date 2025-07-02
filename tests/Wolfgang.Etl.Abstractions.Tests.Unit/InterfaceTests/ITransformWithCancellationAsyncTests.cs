using System.Runtime.CompilerServices;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.InterfaceTests
{
    // ReSharper disable once InconsistentNaming
    public class ITransformWithCancellationAsyncTests
    {

        [Fact]
        public async Task ITransformWithCancellationAsync_works_with_specified_versions_of_dotnet()
        {
            var items = new List<int> { 1, 2, 3, 4, 5 };

            var sut = new IntToStringTransformer();

            var actual = await sut.TransformAsync(items.ToAsyncEnumerable(), CancellationToken.None).ToListAsync();

            Assert.Equal(["1", "2", "3", "4", "5"], actual);

        }


        public class IntToStringTransformer 
            : ITransformWithCancellationAsync<int, string>
        {
            public IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<int> items)
            {
                throw new NotImplementedException();
            }

            public async IAsyncEnumerable<string> TransformAsync
            (
                IAsyncEnumerable<int> items, 
                [EnumeratorCancellation] CancellationToken token)
            {
                await foreach (var item in items.WithCancellation(token))
                {
                    yield return item.ToString();
                }
            }
        }
    }
}
