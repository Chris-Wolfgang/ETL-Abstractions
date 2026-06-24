#if !NET5_0_OR_GREATER

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.CompilerServices;

/// <summary>
/// Polyfill for the compiler-required <c>IsExternalInit</c> marker type, which enables
/// <see langword="init"/>-only property setters on target frameworks (.NET Framework,
/// .NET Standard 2.0) whose reference assemblies do not ship it. Built-in on .NET 5.0+.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[ExcludeFromCodeCoverage]
internal static class IsExternalInit
{
}

#endif
