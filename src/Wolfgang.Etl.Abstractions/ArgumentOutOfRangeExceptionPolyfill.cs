#if !NET8_0_OR_GREATER

using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Polyfill for <c>ArgumentOutOfRangeException.ThrowIfLessThan</c> on target frameworks older than
/// .NET 8.0, so every range guard in this assembly is written once, in the built-in spelling, and
/// throws the same exception — <c>ParamName</c>, <c>ActualValue</c> and message — on every target.
/// The message formats its values with <see cref="CultureInfo.CurrentCulture"/>, as the runtime does.
/// Compiled out where the runtime provides the real method.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal static class ArgumentOutOfRangeExceptionPolyfill
{
    extension(ArgumentOutOfRangeException)
    {
        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less
        /// than <paramref name="other"/>.
        /// </summary>
        /// <typeparam name="T">The type of the values to compare.</typeparam>
        /// <param name="value">The argument to validate.</param>
        /// <param name="other">The inclusive lower bound <paramref name="value"/> must satisfy.</param>
        /// <param name="paramName">
        /// The name of the parameter being validated. Supplied by the compiler from the argument
        /// expression; do not pass it explicitly.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than <paramref name="other"/>.</exception>
        public static void ThrowIfLessThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : IComparable<T>
        {
            if (value.CompareTo(other) < 0)
            {
                throw new ArgumentOutOfRangeException
                (
                    paramName,
                    value,
                    string.Format(CultureInfo.CurrentCulture, "{0} ('{1}') must be greater than or equal to '{2}'.", paramName, value, other)
                );
            }
        }
    }
}

#endif
