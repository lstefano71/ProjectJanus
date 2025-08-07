namespace Janus.Core;

public enum ScanStatus
{
    Success,
    Partial,
    Failed
}

public class ScannedSource
{
    public int Id { get; set; } // Primary key for EF Core
    public required string SourceName { get; init; }
    public required int EventsRetrieved { get; init; }
    public required ScanStatus Status { get; init; }
}