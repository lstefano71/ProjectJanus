using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Threading;
using System.Threading.Tasks;
using Janus.Core;

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
        foreach (var log in session.GetLogNames())
        {
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
    public async Task<IReadOnlyList<EventLogEntry>> ScanAllLogsAsync(
        DateTime timestamp,
        TimeSpan window,
        IProgress<(string status, int eventCount)>? progress,
        CancellationToken cancellationToken)
    {
        var session = EventLogSession.GlobalSession;
        var logNames = session.GetLogNames();
        var entries = new List<EventLogEntry>();
        var tasks = new List<Task>();
        var eventCount = 0;
        var from = timestamp - window;
        var to = timestamp + window;

        foreach (var logName in logNames)
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    progress?.Report(($"Scanning: {logName}", eventCount));
                    var query = $"<QueryList><Query Id='0' Path='{logName}'><Select Path='{logName}'>*[System[TimeCreated[@SystemTime>='{from:O}' and @SystemTime<='{to:O}']]]</Select></Query></QueryList>";
                    using var reader = new EventLogReader(new EventLogQuery(logName, PathType.LogName, query));
                    for (EventRecord? record = reader.ReadEvent(); record is not null; record = reader.ReadEvent())
                    {
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
                            ScanSessionId = Guid.Empty // To be set by caller
                        };
                        lock (entries)
                        {
                            entries.Add(entry);
                            eventCount++;
                        }
                        progress?.Report(($"Scanning: {logName}", eventCount));
                    }
                }
                catch (EventLogException)
                {
                    // Log is corrupted or inaccessible, skip
                }
            }, cancellationToken));
        }
        await Task.WhenAll(tasks);
        progress?.Report(("Scan complete", eventCount));
        return entries;
    }
}
