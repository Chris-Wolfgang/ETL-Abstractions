#if !NET5_0_OR_GREATER

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.CompilerServices;

/// <summary>
/// Polyfill for the compiler-recognised <c>CallerArgumentExpressionAttribute</c>, which makes the
/// compiler pass the source text of an argument to a parameter. The guard polyfills use it to
/// report <c>paramName</c> exactly as the built-in guards do. Built-in on .NET Core 3.0+ and
/// .NET 5.0+; absent from the .NET Framework and .NET Standard 2.0 reference assemblies.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
internal sealed class CallerArgumentExpressionAttribute(string parameterName) : Attribute
{
    /// <summary>
    /// The name of the parameter whose argument expression the compiler captures.
    /// </summary>
    public string ParameterName { get; } = parameterName;
}

#endif
