using System.Reflection;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

/// <summary>
/// Asserts that two freshly constructed stages are observably identical: every public, readable,
/// non-indexed instance property — declared or inherited — compares equal. Reflecting over the
/// type rather than naming properties means a setting added to a base later is covered without
/// editing the test, which is the point: the hidden parameterless constructor is retained
/// permanently on the promise that it behaves exactly like passing <see langword="null"/> to the
/// options constructor (ADR-0009), and that promise must not drift.
/// </summary>
internal static class StageStateAssert
{
    /// <summary>
    /// Settings every stage kind is known to carry. The reflected set must include all of them,
    /// so a reflection that silently sees nothing cannot pass.
    /// </summary>
    private static readonly string[] RequiredProperties =
    [
        "ReportingInterval",
        "MaximumItemCount",
        "SkipItemCount",
        "ErrorPolicy",
        "WorkerResilience",
    ];



    /// <summary>
    /// Asserts that <paramref name="actual"/> exposes the same value as <paramref name="expected"/>
    /// for every public readable instance property of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The concrete stage type under test.</typeparam>
    /// <param name="expected">The stage whose state is the reference.</param>
    /// <param name="actual">The stage that must match it.</param>
    public static void Identical<T>(T expected, T actual) where T : notnull
    {
        var properties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .ToArray();

        var names = properties.Select(p => p.Name).ToArray();
        foreach (var required in RequiredProperties)
        {
            Assert.Contains(required, names);
        }

        foreach (var property in properties)
        {
            Assert.True
            (
                Same(property.GetValue(expected), property.GetValue(actual), expected, actual),
                $"{typeof(T).Name}.{property.Name} differs between the two constructors."
            );
        }
    }



    /// <summary>
    /// Delegates are the same when they invoke the same method and their targets are either equal
    /// (a shared static default, or two targets that compare equal) or each delegate's own owner
    /// (a per-instance method group, which is necessarily bound to a different instance on each
    /// side). A delegate bound to some other stateful object that differs between the two paths
    /// therefore fails. Everything else uses <see cref="object.Equals(object?, object?)"/>.
    /// </summary>
    private static bool Same(object? expected, object? actual, object expectedOwner, object actualOwner)
    {
        if (expected is Delegate expectedDelegate && actual is Delegate actualDelegate)
        {
            var sameMethod = expectedDelegate.Method == actualDelegate.Method;
            var sameTarget = Equals(expectedDelegate.Target, actualDelegate.Target)
                || (ReferenceEquals(expectedDelegate.Target, expectedOwner) && ReferenceEquals(actualDelegate.Target, actualOwner));
            return sameMethod && sameTarget;
        }

        return Equals(expected, actual);
    }
}
