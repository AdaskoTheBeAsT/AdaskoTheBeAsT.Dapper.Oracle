using System;

namespace AdaskoTheBeAsT.Dapper.Oracle.IntegrationTests;

public class GuidCustomer
{
    public Guid CustomerId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public Guid OtherGuid { get; set; }
}
