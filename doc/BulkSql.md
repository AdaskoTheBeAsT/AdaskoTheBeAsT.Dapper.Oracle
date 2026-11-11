# 📦 High-Performance Bulk SQL (`AdaskoTheBeAsT.Dapper.Oracle.BulkSql`)

When inserting, updating, or deleting hundreds or thousands of rows in Oracle, executing one SQL statement per row is a **latency killer** — every single iteration pays a full network round-trip! 🐢

Oracle's ODP.NET client supports **Array Binding** (`ArrayBindCount`), sending columnar parameter arrays to Oracle in **one single round-trip** (`10,000+` rows/sec with ease), but configuring parallel arrays manually is verbose and error-prone.

`AdaskoTheBeAsT.Dapper.Oracle.BulkSql` wraps Oracle Array Binding in a clean, strongly typed API! ⚡

---

## 🐢 Before vs 🚀 After

### ❌ The Slow Way (N Network Round-Trips)

```csharp
// Performs 5,000 separate database round-trips for 5,000 customers! 😱
foreach (var customer in customers)
{
    await connection.ExecuteAsync(
        """
        INSERT INTO Customers (CustomerId, Name, Address, PostalCode, City)
        VALUES (:CustomerId, :Name, :Address, :PostalCode, :City)
        """,
        customer);
}
```

### ✅ The Fast Way with `SqlBulkAsync` (1 Network Round-Trip!)

```csharp
using AdaskoTheBeAsT.Dapper.Oracle.BulkSql;

var mapping = new BulkMapping<Customer>[]
{
    new("CustomerId", c => c.CustomerId),
    new("Name", c => c.Name),
    new("Address", c => c.Address),
    new("PostalCode", c => c.PostalCode),
    new("City", c => c.City),
};

var result = await connection.SqlBulkAsync(
    """
    INSERT INTO Customers (CustomerId, Name, Address, PostalCode, City)
    VALUES (:CustomerId, :Name, :Address, :PostalCode, :City)
    """,
    customers,
    mapping);

Console.WriteLine($"Inserted {result.ExecuteResult} rows in a single round-trip! 🎉");
```

Need synchronous execution? Use `connection.SqlBulk(...)`:

```csharp
int affectedRows = connection.SqlBulk(insertSql, customers, mapping, transaction: tx);
```

---

## 🧭 Auto-Inferred CLR-to-Oracle Type Matrix

When you omit `dbType` on `BulkMapping<T>`, `BulkSql` automatically inspects your property values (including `Nullable<T>`) and normalizes values that ODP.NET cannot bind raw:

| 🧩 .NET Property Type (incl. `T?`) | 🏛️ Inferred `OracleMappingType` | ✨ Automatic Value Normalization |
| --- | --- | --- |
| `Guid` / `Guid?` | `Raw` | Uses registered `GuidRaw16TypeHandler` (respecting `useBigEndian`!) or 16-byte `byte[]` |
| `byte[]` | `Raw` | Passed directly |
| `string` | `Varchar2` | Passed directly |
| `short` / `short?` | `Int16` | Passed directly |
| `int` / `int?` | `Int32` | Passed directly |
| `long` / `long?` | `Int64` | Passed directly |
| `float` / `float?` | `BinaryFloat` | Passed directly |
| `double` / `double?` | `BinaryDouble` | Passed directly |
| `decimal` / `decimal?` | `Decimal` | Passed directly |
| `bool` / `bool?` | `Byte` *(or registered handler)* | Uses registered `ITypeHandler` (`BooleanTypeHandler`, `BooleanStringTypeHandler`, etc.) if configured! |
| `DateTime` / `DateTime?` | `TimeStamp` | Passed directly |
| `DateTimeOffset` / `DateTimeOffset?` | `TimeStampTZ` | Preserves timezone offset |
| `TimeSpan` / `TimeSpan?` | `IntervalDS` | Passed directly |
| `DateOnly` / `DateOnly?` *(NET 8+)* | `Date` | Converted to midnight `DateTime` for ODP.NET |
| `TimeOnly` / `TimeOnly?` *(NET 8+)* | `IntervalDS` | Converted to `TimeSpan` for ODP.NET |
| `float[]` | `VectorFloat32` | Ready for Oracle 23ai `VECTOR(*, FLOAT32)` columns 🧠 |
| `double[]` | `VectorFloat64` | Ready for Oracle 23ai `VECTOR(*, FLOAT64)` columns 🧠 |

---

## 🎯 Explicit `OracleMappingType`, Transactions & Stored Procedures

Need explicit Oracle types (such as `Json`, `Clob`, `NVarchar2`, `Char`, or `Boolean`), explicit `size` / `precision` / `scale`, or an active `IDbTransaction`? Everything is supported right on `BulkMapping<T>`:

```csharp
using System.Data;
using AdaskoTheBeAsT.Dapper.Oracle;
using AdaskoTheBeAsT.Dapper.Oracle.BulkSql;

using var transaction = connection.BeginTransaction();

var mappings = new BulkMapping<ProductEmbedding>[]
{
    new("p_id", p => p.Id, dbType: OracleMappingType.Raw, size: 16),
    new("p_sku", p => p.Sku, dbType: OracleMappingType.Varchar2, size: 64),
    new("p_price", p => p.Price, dbType: OracleMappingType.Decimal, precision: 18, scale: 2),
    new("p_active", p => p.IsActive, dbType: OracleMappingType.Boolean),
    new("p_metadata", p => p.MetadataJson, dbType: OracleMappingType.Json),
    new("p_vector", p => p.Vector, dbType: OracleMappingType.VectorFloat32),
};

var bulkResult = await connection.SqlBulkAsync(
    "catalog_pkg.upsert_product_embedding",
    products,
    mappings,
    cmdType: CommandType.StoredProcedure,
    transaction: transaction);

transaction.Commit();
```

> 💡 **Tip:** Leading `:` or `@` characters in parameter names (for example `":CustomerId"` or `"CustomerId"`) are stripped automatically by `BulkSql`, so both styles work seamlessly!
