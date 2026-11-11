using System;
using System.Data;
using AdaskoTheBeAsT.Dapper.Oracle.Util;

namespace AdaskoTheBeAsT.Dapper.Oracle.TypeHandler;

/// <summary>
/// Conversion between <see cref="Guid"/> and <c>RAW(16)</c> Oracle data type.
/// Supports both .NET default mixed-endian layout and RFC 4122 big-endian layout (recommended for UUIDv7 and cross-platform <c>RAW(16)</c> indexing).
/// </summary>
public class GuidRaw16TypeHandler : TypeHandlerBase<Guid>
{
    public GuidRaw16TypeHandler()
        : this(useBigEndian: false)
    {
    }

    public GuidRaw16TypeHandler(bool useBigEndian)
    {
        UseBigEndian = useBigEndian;
    }

    public bool UseBigEndian { get; }

    public override void SetValue(
        IDbDataParameter parameter,
        Guid value)
    {
        ThrowHelper.ThrowIfNull(parameter, nameof(parameter));

        SetOracleDbTypeOnParameter(parameter, "Raw", 16);
#if NET8_0_OR_GREATER
        parameter.Value = value.ToByteArray(UseBigEndian);
#else
        var bytes = value.ToByteArray();
        if (UseBigEndian
            && BitConverter.IsLittleEndian)
        {
            SwapEndianFields(bytes);
        }

        parameter.Value = bytes;
#endif
    }

    public override Guid Parse(object value)
    {
        ThrowHelper.ThrowIfNull(value, nameof(value));

        if (value is Guid guidValue)
        {
            return guidValue;
        }

        if (value is byte[] byteArray)
        {
#if NET8_0_OR_GREATER
            return new Guid(byteArray, UseBigEndian);
#else
            if (!UseBigEndian
                || !BitConverter.IsLittleEndian
                || byteArray.Length != 16)
            {
                return new Guid(byteArray);
            }

            var copy = (byte[])byteArray.Clone();
            SwapEndianFields(copy);
            return new Guid(copy);
#endif
        }

        if (value is string stringValue
            && Guid.TryParse(stringValue, out var parsedGuid))
        {
            return parsedGuid;
        }

        throw new NotSupportedException($"Dont know how to convert a {value.GetType().Name} to a System.Guid.");
    }

#if !NET8_0_OR_GREATER
    private static void SwapEndianFields(byte[] bytes)
    {
        Swap(bytes, 0, 3);
        Swap(bytes, 1, 2);
        Swap(bytes, 4, 5);
        Swap(bytes, 6, 7);
    }

    private static void Swap(
        byte[] bytes,
        int left,
        int right)
    {
        (bytes[left], bytes[right]) = (bytes[right], bytes[left]);
    }
#endif
}
