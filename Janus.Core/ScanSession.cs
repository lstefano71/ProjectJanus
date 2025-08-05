namespace Janus.Core;

/// <summary>
/// Represents a scan session for event log analysis.
/// </summary>
public class ScanSession
{
    public required Guid Id { get; init; }
    /// <summary>
    /// The timestamp of interest for the scan.
    /// </summary>
    public required DateTime Timestamp { get; init; }
    /// <summary>
    /// Minutes before the central timestamp.
    /// </summary>
    public required int MinutesBefore { get; init; }
    /// <summary>
    /// Minutes after the central timestamp.
    /// </summary>
    public required int MinutesAfter { get; init; }
    /// <summary>
    /// The total time window used for the scan.
    /// </summary>
    public TimeSpan Window => TimeSpan.FromMinutes(MinutesBefore + MinutesAfter);
    /// <summary>
    /// The time the snapshot was created (when saved).
    /// </summary>
    public required DateTime SnapshotCreated { get; init; }
    public required string MachineName { get; init; }
    public required string? UserNotes { get; init; }
    public required ICollection<EventLogEntry> Entries { get; init; }
}
