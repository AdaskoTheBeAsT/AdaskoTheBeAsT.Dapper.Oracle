using System;
using System.Diagnostics.CodeAnalysis;

namespace AdaskoTheBeAsT.Dapper.Oracle.Util;

internal static class ThrowHelper
{
#if NET10_0_OR_GREATER
    [SuppressMessage(
        "Minor Code Smell",
        "S3236:Caller information arguments should not be provided explicitly",
        Justification = "Forwards explicit caller parameter name across multi-targeted ThrowHelper.")]
#else
    [SuppressMessage(
        "Roslynator",
        "RCS1256:Invalid argument null check",
        Justification = "Forwards explicit caller parameter name across multi-targeted ThrowHelper.")]
#endif
    public static void ThrowIfNull(
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        [NotNull]
#endif
        object? argument,
        string paramName)
    {
#if NET10_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(argument, paramName);
#else
        if (argument is null)
        {
            throw new ArgumentNullException(paramName);
        }
#endif
    }

#if NET10_0_OR_GREATER
    [SuppressMessage(
        "Minor Code Smell",
        "S3236:Caller information arguments should not be provided explicitly",
        Justification = "Forwards explicit caller parameter name across multi-targeted ThrowHelper.")]
#endif
    public static T EnsureNotNull<T>(
        T? argument,
        string paramName)
        where T : class
    {
#if NET10_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(argument, paramName);
        return argument;
#else
        return argument ?? throw new ArgumentNullException(paramName);
#endif
    }
}
