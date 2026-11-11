using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.Oracle.IntegrationTests;

[CollectionDefinition("OracleDocker")]
[SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "xUnit collection fixture convention.")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>;
