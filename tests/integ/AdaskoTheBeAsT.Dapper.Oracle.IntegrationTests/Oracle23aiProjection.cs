using System;

namespace AdaskoTheBeAsT.Dapper.Oracle.IntegrationTests;

public sealed class Oracle23aiProjection
{
    public int Id { get; set; }

    public DateOnly EventDate { get; set; }

    public TimeOnly EventTime { get; set; }

    public string PayloadJson { get; set; } = string.Empty;

    public string LargeNotes { get; set; } = string.Empty;
}
