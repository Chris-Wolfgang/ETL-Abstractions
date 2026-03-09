using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Abstractions.Tests.Unit
{
    public class ReportTests
    {
        [Fact]
        public void Constructor_when_passed_a_valid_value_stores_the_value()
        {
            var sut = new Report(42);
            Assert.Equal(42, sut.CurrentItemCount);
        }



        [Fact]
        public void Constructor_when_passed_zero_stores_zero()
        {
            var sut = new Report(0);
            Assert.Equal(0, sut.CurrentItemCount);
        }



        [Fact]
        public void Constructor_when_passed_a_negative_value_throws_ArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Report(-1));
        }



        [Fact]
        public void Constructor_when_passed_a_negative_value_throws_with_correct_param_name()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Report(-1));
            Assert.Equal("currentItemCount", ex.ParamName);
        }



        [Fact]
        public void CurrentItemCount_returns_the_value_passed_to_the_constructor()
        {
            var sut = new Report(99);
            Assert.Equal(99, sut.CurrentItemCount);
        }



#pragma warning disable CS0618
        [Fact]
        public void CurrentCount_returns_the_same_value_as_CurrentItemCount()
        {
            var sut = new Report(7);
            Assert.Equal(sut.CurrentItemCount, sut.CurrentCount);
        }
#pragma warning restore CS0618



        [Fact]
        public void Two_Report_instances_with_same_value_are_equal()
        {
            var a = new Report(5);
            var b = new Report(5);
            Assert.Equal(a, b);
        }



        [Fact]
        public void Two_Report_instances_with_different_values_are_not_equal()
        {
            var a = new Report(5);
            var b = new Report(10);
            Assert.NotEqual(a, b);
        }
    }
}
