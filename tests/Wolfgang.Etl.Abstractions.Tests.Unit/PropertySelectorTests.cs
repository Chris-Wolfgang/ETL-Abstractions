using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Xunit;

namespace Wolfgang.Etl.Abstractions.Tests.Unit;

public class PropertySelectorTests
{
    [ExcludeFromCodeCoverage]
    private class Address
    {
        public string City { get; set; } = string.Empty;
    }



    [ExcludeFromCodeCoverage]
    private class Person
    {
        public string Name { get; set; } = string.Empty;

        public int Age { get; set; }

        public Address Home { get; set; } = new();

#pragma warning disable S1104, CA1051 // A public field is exactly the shape under test.
        public string Nickname = string.Empty;
#pragma warning restore S1104, CA1051

        public string Describe() => Name;
    }



    [ExcludeFromCodeCoverage]
    private sealed class Employee : Person
    {
        public string Department { get; set; } = string.Empty;
    }



    [Fact]
    public void Resolve_when_selector_reads_a_reference_type_property_returns_that_property()
    {
        var property = PropertySelector.Resolve((Person p) => p.Name);

        Assert.Equal(typeof(Person).GetProperty(nameof(Person.Name)), property);
    }



    [Fact]
    public void Resolve_when_selector_boxes_a_value_type_property_unwraps_the_conversion()
    {
        var property = PropertySelector.Resolve<Person, object>(p => p.Age);

        Assert.Equal(typeof(Person).GetProperty(nameof(Person.Age)), property);
    }



    [Fact]
    public void Resolve_when_selector_uses_a_checked_conversion_unwraps_it()
    {
        var parameter = Expression.Parameter(typeof(Person), "p");
        var selector = Expression.Lambda<Func<Person, long>>
        (
            Expression.ConvertChecked
            (
                Expression.Property(parameter, nameof(Person.Age)),
                typeof(long)
            ),
            parameter
        );

        var property = PropertySelector.Resolve(selector);

        Assert.Equal(nameof(Person.Age), property.Name);
    }



    [Fact]
    public void Resolve_when_selector_reads_an_inherited_property_returns_that_property()
    {
        var property = PropertySelector.Resolve((Employee e) => e.Name);

        Assert.Equal(nameof(Person.Name), property.Name);
        Assert.Equal(typeof(Person), property.DeclaringType);
    }



    [Fact]
    public void Resolve_when_selector_reads_a_derived_type_property_returns_that_property()
    {
        var property = PropertySelector.Resolve((Employee e) => e.Department);

        Assert.Equal(typeof(Employee).GetProperty(nameof(Employee.Department)), property);
    }



    [Fact]
    public void Resolve_when_selector_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => PropertySelector.Resolve<Person, string>(null!));

        Assert.Equal("selector", ex.ParamName);
    }



    [Fact]
    public void Resolve_when_selector_reads_a_field_throws_ArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => PropertySelector.Resolve((Person p) => p.Nickname));

        Assert.Equal("selector", ex.ParamName);
        Assert.Contains("x => x.Name", ex.Message, StringComparison.Ordinal);
    }



    [Fact]
    public void Resolve_when_selector_calls_a_method_throws_ArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => PropertySelector.Resolve((Person p) => p.Describe()));

        Assert.Equal("selector", ex.ParamName);
    }



    [Fact]
    public void Resolve_when_selector_chains_member_access_throws_ArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => PropertySelector.Resolve((Person p) => p.Home.City));

        Assert.Equal("selector", ex.ParamName);
    }



    [Fact]
    public void Resolve_when_selector_returns_a_constant_throws_ArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => PropertySelector.Resolve((Person _) => "constant"));

        Assert.Equal("selector", ex.ParamName);
    }



    [Fact]
    public void Resolve_when_selector_returns_the_parameter_itself_throws_ArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => PropertySelector.Resolve((Person p) => p));

        Assert.Equal("selector", ex.ParamName);
    }



    [Fact]
    public void Resolve_when_selector_reads_a_property_of_a_captured_object_throws_ArgumentException()
    {
        var other = new Person();

        var ex = Assert.Throws<ArgumentException>(() => PropertySelector.Resolve((Person _) => other.Name));

        Assert.Equal("selector", ex.ParamName);
    }



    [Fact]
    public void Resolve_when_selector_reads_a_static_property_throws_ArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => PropertySelector.Resolve((Person _) => DateTime.Now));

        Assert.Equal("selector", ex.ParamName);
    }
}
