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
      IProgress<Janus.App.SourceScanProgress>? perSourceProgress,
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
        bool isTotalKnown = false;
        int? totalEvents = null;
        try {
          cancellationToken.ThrowIfCancellationRequested();
          var xpath = $"*[System[TimeCreated[@SystemTime>='{from:O}' and @SystemTime<='{to:O}']]]";
          using var reader = new EventLogReader(new EventLogQuery(logName, PathType.LogName, xpath));
          // No way to know total events in advance, so isTotalKnown = false
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
            perSourceProgress?.Report(new Janus.App.SourceScanProgress {
              SourceName = logName,
              EventsRetrieved = sourceEventCount,
              Status = status,
              IsTotalKnown = isTotalKnown,
              TotalEvents = totalEvents,
              IsActive = true,
              ExceptionMessage = null
            });
          }
        } catch (EventLogException ex) {
          status = ScanStatus.Failed;
          perSourceProgress?.Report(new Janus.App.SourceScanProgress {
            SourceName = logName,
            EventsRetrieved = sourceEventCount,
            Status = status,
            IsTotalKnown = isTotalKnown,
            TotalEvents = totalEvents,
            IsActive = false,
            ExceptionMessage = ex.Message
          });
        } catch (UnauthorizedAccessException ex) {
          status = ScanStatus.Failed;
          perSourceProgress?.Report(new Janus.App.SourceScanProgress {
            SourceName = logName,
            EventsRetrieved = sourceEventCount,
            Status = status,
            IsTotalKnown = isTotalKnown,
            TotalEvents = totalEvents,
            IsActive = false,
            ExceptionMessage = ex.Message
          });
        } catch (OperationCanceledException) {
          status = ScanStatus.Partial;
          perSourceProgress?.Report(new Janus.App.SourceScanProgress {
            SourceName = logName,
            EventsRetrieved = sourceEventCount,
            Status = status,
            IsTotalKnown = isTotalKnown,
            TotalEvents = totalEvents,
            IsActive = false,
            ExceptionMessage = null
          });
        }
        lock (scannedSources) {
          scannedSources.Add(new ScannedSource {
            SourceName = logName,
            EventsRetrieved = sourceEventCount,
            Status = status
          });
        }
        // Final update for this source
        perSourceProgress?.Report(new Janus.App.SourceScanProgress {
          SourceName = logName,
          EventsRetrieved = sourceEventCount,
          Status = status,
          IsTotalKnown = isTotalKnown,
          TotalEvents = totalEvents,
          IsActive = false,
          ExceptionMessage = status == ScanStatus.Failed ? "Scan failed" : null
        });
      }, cancellationToken));
    }
    await Task.WhenAll(tasks);
    return new ScanResult {
      Entries = entries,
      ScannedSources = scannedSources
    };
  }
}
