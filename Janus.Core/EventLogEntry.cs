namespace Janus.Core;

/// <summary>
/// Represents a single event log entry scanned by Project Janus.
/// </summary>
public class EventLogEntry
{
  public required long Id { get; init; }
  public required string LogName { get; init; }
  public required DateTime TimeCreated { get; init; }
  public required string Level { get; init; }
  public required string Source { get; init; }
  public required int EventId { get; init; }
  public required string Message { get; init; }
  public required string MachineName { get; init; }
  public required Guid ScanSessionId { get; init; }
}
