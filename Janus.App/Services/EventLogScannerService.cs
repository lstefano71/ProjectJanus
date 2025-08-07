using Janus.Core;

using System.Diagnostics.Eventing.Reader;

namespace Janus.App;

/// <summary>
/// Service for scanning Windows event logs around a critical timestamp.
/// </summary>
public class EventLogScannerService
{
  /// <summary>
  /// Retrieves all available system event log names using EventLogSession.
  /// </summary>
  public static IReadOnlyList<string> GetAllLogNames()
  {
    var session = EventLogSession.GlobalSession;
    var logNames = new List<string>();
    foreach (var log in session.GetLogNames()) {
      logNames.Add(log);
    }
    return logNames;
  }

  /// <summary>
  /// Scans all event logs for events within the specified time window around the timestamp.
  /// </summary>
  /// <param name="timestamp">The central timestamp to scan around.</param>
  /// <param name="window">The time window (before and after timestamp).</param>
  /// <param name="progress">Progress reporter for scan status and event count.</param>
  /// <param name="cancellationToken">Cancellation token for scan operation.</param>
  /// <returns>List of EventLogEntry objects found.</returns>
  public static async Task<ScanResult> ScanAllLogsAsync(
      DateTime timestamp,
      TimeSpan before,
      TimeSpan after,
      IProgress<(string status, int eventCount)>? progress,
      CancellationToken cancellationToken)
  {
    var session = EventLogSession.GlobalSession;
    var logNames = session.GetLogNames();
    var entries = new List<EventLogEntry>();
    var scannedSources = new List<ScannedSource>();
    var tasks = new List<Task>();
    var eventCount = 0;
    var scanEventIdCounter = 0;
    var from = (timestamp - before).ToUniversalTime();
    var to = (timestamp + after).ToUniversalTime();

    foreach (var logName in logNames) {
      tasks.Add(Task.Run(() => {
        int sourceEventCount = 0;
        ScanStatus status = ScanStatus.Success;
        try {
          cancellationToken.ThrowIfCancellationRequested();
          var xpath = $"*[System[TimeCreated[@SystemTime>='{from:O}' and @SystemTime<='{to:O}']]]";
          progress?.Report(($"Scanning: {logName} | Query: {xpath}", eventCount));
          using var reader = new EventLogReader(new EventLogQuery(logName, PathType.LogName, xpath));
          for (EventRecord? record = reader.ReadEvent(); record is not null; record = reader.ReadEvent()) {
            cancellationToken.ThrowIfCancellationRequested();
            var entry = new EventLogEntry {
              Id = record.RecordId ?? 0,
              LogName = logName,
              TimeCreated = record.TimeCreated ?? DateTime.MinValue,
              Level = record.LevelDisplayName ?? "Unknown",
              Source = record.ProviderName ?? "Unknown",
              EventId = record.Id,
              Message = record.FormatDescription() ?? string.Empty,
              MachineName = record.MachineName ?? Environment.MachineName,
              ScanSessionId = Guid.Empty, // To be set by caller
              ScanEventId = Interlocked.Increment(ref scanEventIdCounter)
            };
            lock (entries) {
              entries.Add(entry);
              sourceEventCount++;
              Interlocked.Increment(ref eventCount);
            }
            progress?.Report(($"Scanning: {logName} | {eventCount} events", eventCount));
          }
        } catch (EventLogException ex) {
          status = ScanStatus.Failed;
          progress?.Report(($"Error scanning {logName}: {ex.Message}", eventCount));
        } catch (UnauthorizedAccessException ex) {
          status = ScanStatus.Failed;
          progress?.Report(($"Access denied to {logName}: {ex.Message}", eventCount));
        } catch (OperationCanceledException) {
          status = ScanStatus.Partial;
        }
        lock (scannedSources) {
          scannedSources.Add(new ScannedSource {
            SourceName = logName,
            EventsRetrieved = sourceEventCount,
            Status = status
          });
        }
      }, cancellationToken));
    }
    await Task.WhenAll(tasks);
    progress?.Report(("Scan complete", eventCount));
    return new ScanResult {
      Entries = entries,
      ScannedSources = scannedSources
    };
  }
}
