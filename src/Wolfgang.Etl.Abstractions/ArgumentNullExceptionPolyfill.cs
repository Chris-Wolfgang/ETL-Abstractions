#if !NET6_0_OR_GREATER

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Polyfill for <c>ArgumentNullException.ThrowIfNull</c> on target frameworks older than .NET 6.0,
/// so every guard in this assembly is written once, in the built-in spelling, and behaves
/// identically on every target. Compiled out where the runtime provides the real method.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal static class ArgumentNullExceptionPolyfill
{
    extension(ArgumentNullException)
    {
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is
        /// <see langword="null"/>.
        /// </summary>
        /// <param name="argument">The reference-type argument to validate as non-null.</param>
        /// <param name="paramName">
        /// The name of the parameter being validated. Supplied by the compiler from the argument
        /// expression; do not pass it explicitly.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        public static void ThrowIfNull(object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}

#endif
