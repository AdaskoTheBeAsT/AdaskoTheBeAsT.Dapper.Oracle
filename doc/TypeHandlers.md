# 🔌 Type Handlers Guide (`AdaskoTheBeAsT.Dapper.Oracle.TypeHandler`)

Several everyday .NET types don't map 1-to-1 out of the box with Oracle or ODP.NET:

- 🆔 **`.NET Guid`** — Oracle stores 16-byte UUIDs in `RAW(16)`, which ODP.NET surfaces as `byte[]`.
- ✅ **`.NET bool`** — Oracle Database 23ai supports native SQL `BOOLEAN`, while Oracle 19c/21c schemas typically use `NUMBER(1)` (`1` / `0`) or `CHAR(1)` (`'Y'` / `'N'`).
- 📅 **`.NET DateOnly` & `TimeOnly`** — ODP.NET throws if you assign `DateOnly` or `TimeOnly` directly to `OracleParameter.Value`.

`AdaskoTheBeAsT.Dapper.Oracle` solves all of these with `OracleTypeMapper` and a rich set of built-in TypeHandlers! 🚀

---

## ⚡ Quick Registration (`Program.cs`)

Register your TypeHandlers **once at application startup** via `OracleTypeMapper.AddTypeHandler(...)` — this registers them with both **Dapper (`SqlMapper`)** and **`OracleDynamicParameters` / `BulkSql`**:

```csharp
using AdaskoTheBeAsT.Dapper.Oracle;
using AdaskoTheBeAsT.Dapper.Oracle.TypeHandler;

// 🆔 Guid <-> RAW(16) (useBigEndian: true recommended for UUIDv7 / cross-platform systems)
OracleTypeMapper.AddTypeHandler(new GuidRaw16TypeHandler(useBigEndian: true));

// ✅ Choose the boolean handler that matches your Oracle schema:
OracleTypeMapper.AddTypeHandler(new BooleanTypeHandler());                 // Oracle 23ai native BOOLEAN
// OracleTypeMapper.AddTypeHandler(new BooleanNumericTypeHandler());       // Legacy NUMBER(1) (1 / 0)
// OracleTypeMapper.AddTypeHandler(new BooleanStringTypeHandler("Y", "N")); // Legacy CHAR(1) / VARCHAR2

// 📅 .NET 8+ DateOnly & TimeOnly
OracleTypeMapper.AddTypeHandler(new DateOnlyTypeHandler());
OracleTypeMapper.AddTypeHandler(new TimeOnlyTypeHandler());
```

> 💡 **Why opt-in registration?**
> Different Oracle schemas map `bool` (`BOOLEAN` vs `NUMBER(1)` vs `CHAR(1)`) and `Guid` endianness differently. Explicit startup registration keeps behavior 100% predictable and backward-compatible!

---

## 🧰 Built-In Type Handlers Reference

| 🏷️ Handler (`AdaskoTheBeAsT.Dapper.Oracle.TypeHandler`) | 🧩 .NET Type | 🏛️ Oracle Column Type | ✨ Highlights |
| --- | --- | --- | --- |
| `GuidRaw16TypeHandler` | `Guid` | `RAW(16)` | Supports both .NET legacy mixed-endian (`false`, default) and RFC 4122 big-endian (`true`) |
| `BooleanTypeHandler` | `bool` | `BOOLEAN` *(Oracle 23ai)* | Binds `OracleDbType.Boolean` for Oracle 23ai SQL columns & PL/SQL parameters |
| `BooleanNumericTypeHandler` | `bool` | `NUMBER(1)` / `NUMBER` | Writes `1` / `0`; safely parses `short`, `int`, `long`, `decimal`, `byte`, and `bool` |
| `BooleanStringTypeHandler` | `bool` | `CHAR(1)` / `VARCHAR2` | Configurable true/false strings (e.g. `"Y"`/`"N"`, `"T"`/`"F"`, case-insensitive optional) |
| `DateOnlyTypeHandler` *(NET 8+)* | `DateOnly` | `DATE` | Binds `OracleDbType.Date` at `00:00:00`; parses `DateTime`, `OracleDate`, and ISO strings |
| `TimeOnlyTypeHandler` *(NET 8+)* | `TimeOnly` | `INTERVAL DAY TO SECOND` | Binds `OracleDbType.IntervalDS`; parses `TimeSpan`, `DateTime`, `OracleIntervalDS`, and strings |

---

## 🆔 `GuidRaw16TypeHandler`: Mixed-Endian vs Big-Endian (`UUIDv7`)

By history, `.NET`'s `Guid.ToByteArray()` uses a **mixed-endian** layout (the first three fields are little-endian), whereas Oracle `HEXTORAW('0196...')`, Java, Python, Go, PostgreSQL, and **RFC 4122 / RFC 9562 (`Guid.CreateVersion7()`)** expect **big-endian** network byte order!

### When to use `useBigEndian: true` (Recommended for Greenfields & `UUIDv7` 🚀)

- Your primary keys use `.NET 9+` **`Guid.CreateVersion7()`** — big-endian keeps the 48-bit Unix timestamp in the leading bytes of `RAW(16)`, giving you **sequential Oracle B-tree index inserts** with minimal block splits!
- Your Oracle database is shared with non-.NET services or SQL scripts using `HEXTORAW(...)` / `RAWTOHEX(...)`.

```csharp
OracleTypeMapper.AddTypeHandler(new GuidRaw16TypeHandler(useBigEndian: true));
```

### When to use `useBigEndian: false` (Default — Existing Databases 🛡️)

- You have an existing Oracle database populated by `Dapper.Oracle` or `Guid.ToByteArray()` and want 100% binary compatibility with existing `RAW(16)` rows:

```csharp
OracleTypeMapper.AddTypeHandler(new GuidRaw16TypeHandler()); // useBigEndian: false
```

---

## 🛠️ Creating Your Own Custom Type Handler

Need to map a custom Strongly Typed ID (like `OrderId`), `Ulid`, or JSON value object? Inherit from `TypeHandlerBase<T>` and call `SetOracleDbTypeOnParameter(...)`:

```csharp
using System.Data;
using AdaskoTheBeAsT.Dapper.Oracle;
using AdaskoTheBeAsT.Dapper.Oracle.TypeHandler;

public readonly record struct OrderId(long Value);

public sealed class OrderIdTypeHandler : TypeHandlerBase<OrderId>
{
    public override void SetValue(IDbDataParameter parameter, OrderId value)
    {
        SetOracleDbTypeOnParameter(parameter, "Int64");
        parameter.Value = value.Value;
    }

    public override OrderId Parse(object value)
    {
        return new OrderId(Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture));
    }
}

// Register once at startup:
OracleTypeMapper.AddTypeHandler(new OrderIdTypeHandler());
```

That's it — `OrderId` and `OrderId?` will now work automatically in `QueryAsync<T>`, `ExecuteAsync`, `OracleDynamicParameters`, and `SqlBulkAsync`! 🎉
