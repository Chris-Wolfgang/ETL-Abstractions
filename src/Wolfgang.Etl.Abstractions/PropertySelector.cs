using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Resolves a property-selector lambda such as <c>x =&gt; x.Name</c> to the
/// <see cref="PropertyInfo"/> it selects. The shared primitive behind the per-library code-first
/// schema builders, so every ETL library validates selectors the same way.
/// </summary>
/// <example>
/// <code>
/// var amount = PropertySelector.Resolve((Order o) =&gt; o.Amount);
/// var boxed = PropertySelector.Resolve&lt;Order, object&gt;(o =&gt; o.Amount); // value type selected as object
/// </code>
/// </example>
public static class PropertySelector
{
    private const string ExpectedShape =
        "The selector must be a direct property access on the lambda parameter, e.g. x => x.Name.";



    /// <summary>
    /// Resolves <paramref name="selector"/> to the <see cref="PropertyInfo"/> of the property it reads.
    /// </summary>
    /// <typeparam name="T">The type that declares (or inherits) the property.</typeparam>
    /// <typeparam name="TProperty">
    /// The selector's result type. May be <see cref="object"/> for a value-type property; the boxing
    /// conversion the compiler inserts is unwrapped.
    /// </typeparam>
    /// <param name="selector">
    /// A lambda whose body is a single property access on its parameter, such as <c>x =&gt; x.Name</c>.
    /// Inherited properties are accepted.
    /// </param>
    /// <returns>The <see cref="PropertyInfo"/> for the selected property.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="selector"/> selects a field, calls a method, chains member access
    /// (<c>x =&gt; x.Address.City</c>), returns the parameter itself, or computes a value instead of
    /// reading a property of its parameter.
    /// </exception>
    public static PropertyInfo Resolve<T, TProperty>(Expression<Func<T, TProperty>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);

        var body = selector.Body;
        if (body is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } conversion)
        {
            body = conversion.Operand;
        }

        if
        (
            body is MemberExpression { Member: PropertyInfo property } member
            && member.Expression == selector.Parameters[0]
        )
        {
            return property;
        }

        throw new ArgumentException
        (
            $"{ExpectedShape} Got: {selector}.",
            nameof(selector)
        );
    }
}
