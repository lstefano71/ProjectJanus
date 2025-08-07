using Janus.Core;

namespace Janus.App;

public class ScanResult {
  public required IReadOnlyList<EventLogEntry> Entries { get; init; }
  public required IReadOnlyList<ScannedSource> ScannedSources { get; init; }
}