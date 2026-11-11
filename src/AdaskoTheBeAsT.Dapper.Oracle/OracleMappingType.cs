using System.Diagnostics.CodeAnalysis;

namespace AdaskoTheBeAsT.Dapper.Oracle;

/// <summary>
/// Enum for mapping to Oracle native DB types. Only for use together with Dapper and <see cref="OracleDynamicParameters"/>.
/// Covers ODP.NET <c>OracleDbType</c> values through Oracle Database 23ai (including native <see cref="Boolean"/>, <see cref="Json"/>, and AI <see cref="Vector"/> types).
/// </summary>
[SuppressMessage(
    "Naming",
    "CA1720:Identifier contains type name",
    Justification = "Matches OracleDbType enum names in ODP.NET.")]
public enum OracleMappingType
{
    BFile = 101,
    Blob = 102,
    Byte = 103,
    Char = 104,
    Clob = 105,
    Date = 106,
    Decimal = 107,
    Double = 108,
    Long = 109,
    LongRaw = 110,
    Int16 = 111,
    Int32 = 112,
    Int64 = 113,
    IntervalDS = 114,
    IntervalYM = 115,
    NClob = 116,
    NChar = 117,
    NVarchar2 = 119,
    Raw = 120,
    RefCursor = 121,
    Single = 122,
    TimeStamp = 123,
    TimeStampLTZ = 124,
    TimeStampTZ = 125,
    Varchar2 = 126,
    XmlType = 127,
    Array = 128,
    Object = 129,
    Ref = 130,
    BinaryDouble = 132,
    BinaryFloat = 133,
    Boolean = 134,
    Json = 135,
    ArrayAsJson = 136,
    ObjectAsJson = 137,
    Vector = 138,
    VectorInt8 = 139,
    VectorFloat32 = 140,
    VectorFloat64 = 141,
    VectorBinary = 142,
}
