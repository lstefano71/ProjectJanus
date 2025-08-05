
using Janus.App.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Threading.Tasks;

namespace Janus.App.Services;

public class EventLogService
{
    public async Task<List<ScannedEvent>> ScanEventsAsync(DateTime centerPoint, int minutesBefore, int minutesAfter, IProgress<string> progress, CancellationToken cancellationToken)
    {
        var entries = new List<ScannedEvent>();
        var startTime = centerPoint.AddMinutes(-minutesBefore);
        var endTime = centerPoint.AddMinutes(minutesAfter);

        await Task.Run(() =>
        {
            var logNames = EventLogSession.GlobalSession.GetLogNames();

            foreach (var logName in logNames)
            {
                cancellationToken.ThrowIfCancellationRequested();
                progress.Report($"Scanning {logName}...");

                string startIso = startTime.ToUniversalTime().ToString("o");
                string endIso = endTime.ToUniversalTime().ToString("o");
                string xpath = $"*[System[TimeCreated[@SystemTime>='{startIso}' and @SystemTime<='{endIso}']]]";
                var query = new EventLogQuery(logName, PathType.LogName, xpath)
                {
                    ReverseDirection = true,
                    TolerateQueryErrors = true
                };

                try
                {
                    using (var reader = new EventLogReader(query))
                    {
                        for (var record = reader.ReadEvent(); record != null; record = reader.ReadEvent())
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            entries.Add(new ScannedEvent
                            {
                                TimeCreated = record.TimeCreated,
                                EventId = record.Id,
                                LevelDisplayName = record.LevelDisplayName,
                                ProviderName = record.ProviderName,
                                TaskDisplayName = record.TaskDisplayName,
                                Message = record.FormatDescription(),
                                LogName = logName
                            });
                        }
                    }
                }
                catch (EventLogException)
                {
                    // Ignore logs that can't be accessed
                }
            }
        }, cancellationToken);

        return entries;
    }
}
