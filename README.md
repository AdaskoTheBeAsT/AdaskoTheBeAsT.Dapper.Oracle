# 🦁 AdaskoTheBeAsT.Dapper.Oracle

> **Supercharged Oracle Database extensions for [Dapper](https://github.com/DapperLib/Dapper)** — built for **Oracle Database 23ai**, **ODP.NET (`Oracle.ManagedDataAccess.Core` 23.x)**, and **.NET 8 / 10 / 11** (plus `.NET Standard 2.0` & `.NET Framework 4.6.1`). 🚀

[![CI](https://github.com/AdaskoTheBeAsT/AdaskoTheBeAsT.Dapper.Oracle/actions/workflows/ci.yml/badge.svg)](https://github.com/AdaskoTheBeAsT/AdaskoTheBeAsT.Dapper.Oracle/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/AdaskoTheBeAsT.Dapper.Oracle.svg?logo=nuget)](https://www.nuget.org/packages/AdaskoTheBeAsT.Dapper.Oracle/)
[![NuGet StrongName](https://img.shields.io/nuget/v/AdaskoTheBeAsT.Dapper.Oracle.StrongName.svg?logo=nuget&label=nuget%20%28strong-name%29)](https://www.nuget.org/packages/AdaskoTheBeAsT.Dapper.Oracle.StrongName/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/AdaskoTheBeAsT/AdaskoTheBeAsT.Dapper.Oracle/blob/main/LICENSE)

---

## 🧭 Table of Contents

- [⚡ Install in 10 Seconds](#-install-in-10-seconds)
- [🤔 Why `AdaskoTheBeAsT.Dapper.Oracle`?](#-why-adaskothebeastdapperoracle)
  - [🔄 1-Minute Migration from `Dapper.Oracle`](#-1-minute-migration-from-dapperoracle)
  - [🐛 Critical Bug Fixes Over Upstream `2.0.4`](#-critical-bug-fixes-over-upstream-204)
  - [✨ New Features & Oracle 23ai Superpowers](#-new-features--oracle-23ai-superpowers)
- [🚀 Copy-Paste Developer Cookbook](#-copy-paste-developer-cookbook)
  - [1️⃣ Stored Procedures, `RefCursor` & `OUT` Parameters](#1️⃣-stored-procedures-refcursor--out-parameters)
  - [2️⃣ Single-Round-Trip `CLOB` / `BLOB` Fetching](#2️⃣-single-round-trip-clob--blob-fetching)
  - [3️⃣ Oracle 23ai: AI Vector Search, OSON `JSON` & Native `BOOLEAN`](#3️⃣-oracle-23ai-ai-vector-search-oson-json--native-boolean)
  - [4️⃣ PL/SQL Associative Arrays & Array Binding](#4️⃣-plsql-associative-arrays--array-binding)
  - [5️⃣ TypeHandlers (`Guid` / `UUIDv7`, `DateOnly`, `TimeOnly`, `bool`)](#5️⃣-typehandlers-guid--uuidv7-dateonly-timeonly-bool)
  - [6️⃣ Blazing-Fast Bulk Inserts & Updates (`SqlBulk` / `SqlBulkAsync`)](#6️⃣-blazing-fast-bulk-inserts--updates-sqlbulk--sqlbulkasync)
- [🎛️ `OracleDynamicParameters` Tuning Cheat Sheet](#️-oracledynamicparameters-tuning-cheat-sheet)
- [📚 Deep-Dive Documentation](#-deep-dive-documentation)
- [🛠️ Building & Testing Locally](#️-building--testing-locally)
- [🙏 Acknowledgments & Credits](#-acknowledgments--credits)

---

## ⚡ Install in 10 Seconds

```powershell
dotnet add package AdaskoTheBeAsT.Dapper.Oracle
```

Need a strong-named assembly? We've got you covered too:

```powershell
dotnet add package AdaskoTheBeAsT.Dapper.Oracle.StrongName
```

| 📦 Package | 🎯 Use Case | 🧩 Target Frameworks |
| --- | --- | --- |
| `AdaskoTheBeAsT.Dapper.Oracle` | Standard projects (recommended) | `net11.0`, `net10.0`, `net8.0`, `netstandard2.0`, `net461` |
| `AdaskoTheBeAsT.Dapper.Oracle.StrongName` | Strong-name signed assemblies | `net11.0`, `net10.0`, `net8.0`, `netstandard2.0`, `net461` |

> 💡 **Zero ODP.NET Version Lock-In!**
> `AdaskoTheBeAsT.Dapper.Oracle` binds `OracleCommand` and `OracleParameter` properties via **cached, compiled expression trees** — meaning no hard dependency on a specific `Oracle.ManagedDataAccess.Core` version and near-zero reflection overhead at runtime! ⚡

---

## 🤔 Why `AdaskoTheBeAsT.Dapper.Oracle`?

Upstream `Dapper.Oracle` (`2.0.4`) served the .NET community well for years, but stopped receiving updates in the .NET 7 / `Dapper 2.0.123` era — before **Oracle Database 23ai**, **ODP.NET 23.x**, **.NET 8 / 10 / 11**, `DateOnly` / `TimeOnly`, and sequential `Guid.CreateVersion7()` (`UUIDv7`). Meanwhile, production teams kept hitting subtle runtime bugs around inline LOB fetching, cursor-sharing plan pollution, `ORA-06502` output buffer overflows, and ODP.NET value conversions.

`AdaskoTheBeAsT.Dapper.Oracle` carries the torch forward as a modern, actively maintained successor tested directly against **Oracle Database 23ai Free**! 🔥

### 🔄 1-Minute Migration from `Dapper.Oracle`

Migrating an existing codebase takes less than a minute — swap the NuGet package and update your `using` directives from `Dapper.Oracle*` to `AdaskoTheBeAsT.Dapper.Oracle*`:

```diff
- <PackageReference Include="Dapper.Oracle" Version="2.0.4" />
+ <PackageReference Include="AdaskoTheBeAsT.Dapper.Oracle" Version="*" />
```

```diff
- using Dapper.Oracle;
- using Dapper.Oracle.TypeHandler;
- using Dapper.Oracle.BulkSql;
+ using AdaskoTheBeAsT.Dapper.Oracle;
+ using AdaskoTheBeAsT.Dapper.Oracle.TypeHandler;
+ using AdaskoTheBeAsT.Dapper.Oracle.BulkSql;
```

All class names (`OracleDynamicParameters`, `OracleMappingType`, `OracleTypeMapper`, `GuidRaw16TypeHandler`, `BulkMapping<T>`, etc.) remain familiar so your existing queries keep working right away! ✅

### 🐛 Critical Bug Fixes Over Upstream `2.0.4`

- 🏎️ **`InitialLOBFetchSize = -1` finally works:** Upstream checked `InitialLOBFetchSize > 0`, silently dropping `-1` — Oracle's #1 recommended setting for fetching `CLOB`, `NCLOB`, and `BLOB` columns inline in a **single database round-trip** without extra LOB locator calls!
- 🎯 **`BindByName` no longer clobbers `OracleCommand` defaults:** Changed `BindByName` from `bool` to `bool?`. Leaving it `null` respects whatever `BindByName` you already configured on `OracleCommand`, while explicit `true` or `false` applies deterministically.
- 🧪 **`OracleValueConverter` fixed for real-world ODP.NET types:**
  - ⏱️ `OracleIntervalDS` now converts cleanly to `TimeSpan` / `TimeSpan?` (previously returned `00:00:00`!).
  - 🌍 `OracleTimeStampTZ` now converts to `DateTimeOffset` / `DateTimeOffset?` preserving the exact timezone offset via `GetTimeZoneOffset()`.
  - 🧠 `OracleVector` unwraps directly to `float[]`, `double[]`, or `OracleVector`.
  - 📜 `OracleRefCursor` and native `Oracle.ManagedDataAccess.Types.*` structs can be retrieved directly via `parameters.Get<T>()` instead of returning `null`.
  - 🆔 16-byte `OracleBinary` (`RAW(16)`) output parameters convert directly to `Guid` / `Guid?`.
- 🔢 **`BooleanNumericTypeHandler.Parse` fixed across all Oracle numeric precisions:** Upstream used `(int)value != 0`, throwing `InvalidCastException` whenever ODP.NET returned `short` (`NUMBER(1)`), `decimal` (`NUMBER`), `long`, `byte`, or `bool`. Every numeric and boolean type now parses cleanly.
- 🛡️ **Smarter cursor-sharing parameter sizing & zero surprise `ORA-06502` errors:**
  - Never clamps `Size = 4000` onto `Clob`, `NClob`, `Json`, `XmlType`, or `Long` parameters.
  - Supports both standard and `MAX_STRING_SIZE = EXTENDED` buckets (`VARCHAR2`/`CHAR`: `4000` & `32767`; `NVARCHAR2`/`NCHAR`: `2000` & `16383`) to prevent Oracle hard-parses and shared-pool fragmentation.
  - Automatically assigns safe default buffer sizes (`4000` / `2000`) to `Output`, `InputOutput`, and `ReturnValue` string/raw parameters when `size` is omitted — saying goodbye to `ORA-06502: PL/SQL: numeric or value error: character string buffer too small`! 🎉

### ✨ New Features & Oracle 23ai Superpowers

- 🧠 **Full Oracle Database 23ai `OracleMappingType` coverage:**
  - **OSON Native JSON:** `Json`, `ArrayAsJson`, `ObjectAsJson`
  - **AI Vector Search:** `Vector`, `VectorInt8`, `VectorFloat32`, `VectorFloat64`, `VectorBinary`
  - **ISO SQL Boolean:** `Boolean`
  - **UDTs & References:** `Array`, `Object`, `Ref`
- 🎛️ **New ODP.NET performance knobs on `OracleDynamicParameters`:**
  - `InitialLONGFetchSize` (`int?`, including `-1`), `FetchSize` (`long?`), and `AddToStatementCache` (`bool?`).
- 🔌 **Modern .NET `TypeHandler` suite (`AdaskoTheBeAsT.Dapper.Oracle.TypeHandler`):**
  - `BooleanTypeHandler` — native Oracle 23ai SQL & PL/SQL `BOOLEAN` (`OracleDbType.Boolean`).
  - `DateOnlyTypeHandler` & `TimeOnlyTypeHandler` (`.NET 8+`) — seamless `DateOnly` (`DATE`) and `TimeOnly` (`INTERVAL DAY TO SECOND`) support (raw ODP.NET rejects `DateOnly`/`TimeOnly` on `OracleParameter.Value`!).
  - `GuidRaw16TypeHandler(useBigEndian: true)` — RFC 4122 big-endian `RAW(16)` byte ordering so sequential `.NET 9+` `Guid.CreateVersion7()` keys stay **monotonically ordered in Oracle B-tree indexes** and match Oracle `HEXTORAW` / `SYS_GUID()` text order!
- 📦 **Richer `BulkSql` (`SqlBulk` / `SqlBulkAsync`):**
  - Automatic type inference and array-bind normalization for `short`, `float`, `double`, `DateTimeOffset`, `TimeSpan`, `DateOnly`, `TimeOnly`, `float[]` (`VectorFloat32`), `double[]` (`VectorFloat64`), and nullable value types (`Guid?`, `DateOnly?`, `TimeOnly?`, `bool?`).
- 🏗️ **Modern .NET Engineering:**
  - Built on `Dapper` `2.1.89`, nullable reference types enabled, deterministic builds, SourceLink + `.snupkg` symbol packages, zero-warning static analysis, xUnit v3 on Microsoft Testing Platform, and Testcontainers integration tests against `gvenzl/oracle-free:23-slim-faststart`.

---

## 🚀 Copy-Paste Developer Cookbook

### 1️⃣ Stored Procedures, `RefCursor` & `OUT` Parameters

Call Oracle packages and stored procedures cleanly without touching concrete `OracleParameter` types in your data-access layer:

```csharp
using System.Data;
using AdaskoTheBeAsT.Dapper.Oracle;
using Dapper;
using Oracle.ManagedDataAccess.Client;

await using var connection = new OracleConnection(connectionString);

var parameters = new OracleDynamicParameters
{
    BindByName = true,
};
parameters.Add("p_department_id", 10, OracleMappingType.Int32, ParameterDirection.Input);
parameters.Add("p_cursor", dbType: OracleMappingType.RefCursor, direction: ParameterDirection.Output);
parameters.Add("p_summary", dbType: OracleMappingType.Varchar2, direction: ParameterDirection.Output); // Safe 4000-char buffer auto-applied!

var employees = await connection.QueryAsync<Employee>(
    "hr_pkg.get_employees",
    parameters,
    commandType: CommandType.StoredProcedure);

var summary = parameters.Get<string>("p_summary");
```

### 2️⃣ Single-Round-Trip `CLOB` / `BLOB` Fetching

By default, ODP.NET fetches a LOB locator first and makes extra network round-trips for every row's `CLOB`/`BLOB`. Set `InitialLOBFetchSize = -1` to stream LOB data inline in a single trip:

```csharp
using AdaskoTheBeAsT.Dapper.Oracle;
using Dapper;

var parameters = new OracleDynamicParameters
{
    BindByName = true,
    InitialLOBFetchSize = -1, // ⚡ Inline CLOB/NCLOB/BLOB fetch — huge latency win!
    FetchSize = 262_144L,     // 256 KB row fetch buffer
};
parameters.Add("ReportId", reportId, OracleMappingType.Int64);

var report = await connection.QuerySingleOrDefaultAsync<ReportDocument>(
    "SELECT Id, Title, LargeHtmlClob FROM Reports WHERE Id = :ReportId",
    parameters);
```

### 3️⃣ Oracle 23ai: AI Vector Search, OSON `JSON` & Native `BOOLEAN`

First-class support for Oracle Database 23ai's headline features — similarity search with `VECTOR`, binary OSON `JSON`, and SQL `BOOLEAN`:

```csharp
using AdaskoTheBeAsT.Dapper.Oracle;
using Dapper;

var searchParams = new OracleDynamicParameters
{
    BindByName = true,
    InitialLOBFetchSize = -1,
};
searchParams.Add("IsPublished", true, OracleMappingType.Boolean);
searchParams.Add("MetadataFilter", """{"category":"architecture"}""", OracleMappingType.Json);
searchParams.Add("QueryEmbedding", new float[] { 0.12f, -0.34f, 0.88f }, OracleMappingType.VectorFloat32);

var matches = await connection.QueryAsync<KnowledgeChunk>(
    """
    SELECT Id,
           Title,
           Content,
           VECTOR_DISTANCE(Embedding, :QueryEmbedding, COSINE) AS Score
    FROM KnowledgeBase
    WHERE IsPublished = :IsPublished
      AND JSON_TEXTCONTAINS(Metadata, '$.category', 'architecture')
    ORDER BY Score
    FETCH FIRST 10 ROWS ONLY
    """,
    searchParams);
```

### 4️⃣ PL/SQL Associative Arrays & Array Binding

Pass an entire array to PL/SQL `INDEX BY PLS_INTEGER` associative arrays or execute batched DML in one round-trip:

```csharp
using System.Data;
using AdaskoTheBeAsT.Dapper.Oracle;
using Dapper;

var parameters = new OracleDynamicParameters
{
    BindByName = true,
};
parameters.Add(
    name: "p_ids",
    value: new[] { 101, 102, 103, 104 },
    dbType: OracleMappingType.Int32,
    direction: ParameterDirection.Input,
    collectionType: OracleMappingCollectionType.PLSQLAssociativeArray);

await connection.ExecuteAsync(
    "order_pkg.archive_orders",
    parameters,
    commandType: CommandType.StoredProcedure);
```

### 5️⃣ TypeHandlers (`Guid` / `UUIDv7`, `DateOnly`, `TimeOnly`, `bool`)

Register once at application startup (`Program.cs`) and use idiomatic .NET types everywhere in your POCOs and query parameters:

```csharp
using AdaskoTheBeAsT.Dapper.Oracle;
using AdaskoTheBeAsT.Dapper.Oracle.TypeHandler;

// 🆔 RAW(16) <-> Guid with RFC 4122 big-endian byte layout
// Ideal for .NET 9+ Guid.CreateVersion7() sequential B-tree indexes & Oracle HEXTORAW parity!
OracleTypeMapper.AddTypeHandler(new GuidRaw16TypeHandler(useBigEndian: true));

// ✅ Native Oracle 23ai BOOLEAN (or BooleanNumericTypeHandler / BooleanStringTypeHandler for 19c schemas)
OracleTypeMapper.AddTypeHandler(new BooleanTypeHandler());

// 📅 .NET 8+ DateOnly (DATE) & TimeOnly (INTERVAL DAY TO SECOND)
OracleTypeMapper.AddTypeHandler(new DateOnlyTypeHandler());
OracleTypeMapper.AddTypeHandler(new TimeOnlyTypeHandler());
```

### 6️⃣ Blazing-Fast Bulk Inserts & Updates (`SqlBulk` / `SqlBulkAsync`)

Stop looping `connection.ExecuteAsync(...)` row-by-row! `SqlBulk` and `SqlBulkAsync` bind columnar arrays via ODP.NET `ArrayBindCount` and execute thousands of inserts or updates in **one network round-trip**:

```csharp
using AdaskoTheBeAsT.Dapper.Oracle.BulkSql;

var result = await connection.SqlBulkAsync(
    """
    INSERT INTO Customers (CustomerId, Name, BirthDate, Embedding)
    VALUES (:CustomerId, :Name, :BirthDate, :Embedding)
    """,
    customers,
    new BulkMapping<Customer>[]
    {
        new("CustomerId", c => c.CustomerId), // Guid / Guid? -> RAW
        new("Name", c => c.Name),             // string -> VARCHAR2
        new("BirthDate", c => c.BirthDate),   // DateOnly -> DATE
        new("Embedding", c => c.Embedding),   // float[] -> VECTOR (Float32)
    });

Console.WriteLine($"Inserted {result.ExecuteResult} rows in 1 round-trip!");
```

---

## 🎛️ `OracleDynamicParameters` Tuning Cheat Sheet

| ⚙️ Property | 🏷️ Type | 💡 When & Why to Use It |
| --- | --- | --- |
| `BindByName` | `bool?` | Set `true` so `:ParamName` binds by name (even when repeated in SQL) instead of parameter order. Leave `null` to preserve your `OracleCommand` default. |
| `InitialLOBFetchSize` | `int?` | Set `-1` to inline entire `CLOB`/`NCLOB`/`BLOB` columns in a single round-trip, or a byte/char threshold (e.g. `32768`). |
| `InitialLONGFetchSize` | `int?` | Set `-1` (or byte size) when querying legacy `LONG` or `LONG RAW` dictionary views (e.g. `ALL_VIEWS.TEXT`, `ALL_TAB_PARTITIONS`). |
| `FetchSize` | `long?` | Tune ODP.NET's internal row-fetch buffer size in bytes for large analytical or vector result sets. |
| `AddToStatementCache` | `bool?` | Set `false` for one-off dynamic SQL strings (e.g. dynamic `IN (...)` lists) so they don't evict hot prepared statements from ODP.NET's cache. |
| `ArrayBindCount` | `int` | Execute a single `INSERT` / `UPDATE` / `DELETE` across `N` rows using array parameters. |

---

## 📚 Deep-Dive Documentation

- 🔌 **[Type Handlers Guide (`doc/TypeHandlers.md`)](https://github.com/AdaskoTheBeAsT/AdaskoTheBeAsT.Dapper.Oracle/blob/main/doc/TypeHandlers.md)** — `Guid` & `UUIDv7` endianness explained, Oracle 23ai vs 19c boolean strategies, `DateOnly` / `TimeOnly`, and writing custom `TypeHandlerBase<T>` implementations.
- 📦 **[High-Performance Bulk SQL Guide (`doc/BulkSql.md`)](https://github.com/AdaskoTheBeAsT/AdaskoTheBeAsT.Dapper.Oracle/blob/main/doc/BulkSql.md)** — `SqlBulk` & `SqlBulkAsync`, auto-inferred CLR-to-Oracle type matrix, transactions, stored procedures, and tuning tips.

---

## 🛠️ Building & Testing Locally

### Prerequisites

- 🧰 **.NET 11 SDK** (`11.0.100`, configured via `global.json` with `rollForward: latestMajor`)
- 🐳 **Docker** (Linux containers) — only needed when running the Testcontainers **Oracle Database 23ai Free** integration tests

```powershell
# 🎨 Format code (restores local tools + runs dotnet-format, ReSharper cleanupcode & StyleCop fixers)
.\format.ps1

# 🔨 Build entire solution (0 warnings, 0 errors enforced across all 5 TFMs)
dotnet build AdaskoTheBeAsT.Dapper.Oracle.sln -c Release

# ⚡ Run fast unit tests (no Docker or Oracle instance needed!)
dotnet test --solution WithoutDb.slnf -c Release

# 🧪 Run full suite including Oracle Database 23ai Free Testcontainers integration tests
dotnet test --solution AdaskoTheBeAsT.Dapper.Oracle.sln -c Release
```

> 💡 Want to run integration tests against an existing Oracle server instead of Testcontainers? Set the `DA_OR_CONNECTION` environment variable to your Oracle connection string!

---

## 🙏 Acknowledgments & Credits

Huge thanks and heartfelt appreciation to **[DIPS AS](https://github.com/DIPSAS/Dapper.Oracle)**, original creator **Erling Paulsen ([@epaulsen](https://github.com/epaulsen))**, and every contributor whose ideas, code, and bug reports built the foundation of `Dapper.Oracle`:

- 🌟 **Erling Paulsen** ([@epaulsen](https://github.com/epaulsen))
- 🌟 **Helene Brinchmann Halvorsen** ([@hel1e](https://github.com/hel1e))
- 🌟 **Ognjen Pejanovic** ([@opejanovic](https://github.com/opejanovic))
- 🌟 **Havagan** ([@Havagan](https://github.com/Havagan))
- 🌟 **Morten Nilsen**
- 🌟 **Andreas Mosti** ([@andmos](https://github.com/andmos))
- 🌟 **Gaudico** ([@Gaudico](https://github.com/Gaudico))
- 🌟 **Runar Ovesen Hjerpbakk**
- 🌟 **Stjepan Majdak**

Modernized and maintained with ❤️ by **Adam "AdaskoTheBeAsT" Pluciński**.
