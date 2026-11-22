#if NET10_0_OR_GREATER
using System;
#endif

namespace AdaskoTheBeAsT.Dapper.Oracle.IntegrationTests.Util;

internal static class StringExtensions
{
    public static string ReplaceOrdinal(
        this string value,
        string oldValue,
        string newValue)
    {
#if NET10_0_OR_GREATER
        return value.Replace(oldValue, newValue, StringComparison.Ordinal);
#else
        return value.Replace(oldValue, newValue);
#endif
    }
}
