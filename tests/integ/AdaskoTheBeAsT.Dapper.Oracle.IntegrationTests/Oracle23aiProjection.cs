using System;

namespace AdaskoTheBeAsT.Dapper.Oracle.IntegrationTests;

public sealed class Oracle23aiProjection
{
    public int Id { get; set; }

#if NET10_0_OR_GREATER
    public DateOnly EventDate { get; set; }

    public TimeOnly EventTime { get; set; }
#else
    public DateTime EventDate { get; set; }

    public TimeSpan EventTime { get; set; }
#endif

    public string PayloadJson { get; set; } = string.Empty;

    public string LargeNotes { get; set; } = string.Empty;
}
