using Janus.Core;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Janus.App;

/// <summary>
/// Service for saving and loading event log scan snapshots.
/// </summary>
public class SnapshotService
{
  private static DbContextOptions<EventSnapshotDbContext> CreateOptions(string filePath)
  {
    return new DbContextOptionsBuilder<EventSnapshotDbContext>()
      .UseSqlite($"Data Source={filePath};Pooling=false")
      .LogTo(s => System.Diagnostics.Trace.WriteLine(s), Microsoft.Extensions.Logging.LogLevel.Information)
      .Options;
  }

  public static async Task SaveSnapshotAsync(string filePath, ScanSession session)
  {
    try {
      var options = CreateOptions(filePath);
      await using var db = new EventSnapshotDbContext(options);
      await db.Database.EnsureCreatedAsync();
      db.ScanSessions.Add(new ScanSession {
        Id = session.Id,
        Timestamp = session.Timestamp,
        MinutesBefore = session.MinutesBefore,
        MinutesAfter = session.MinutesAfter,
        SnapshotCreated = session.SnapshotCreated,
        MachineName = session.MachineName,
        UserNotes = session.UserNotes,
        Entries = session.Entries,
        ScannedSources = session.ScannedSources
      });
      await db.SaveChangesAsync();
    } catch (SqliteException ex) {
      // Handle schema mismatch or file errors
      throw new InvalidOperationException("Failed to save snapshot.", ex);
    }
  }

  public static async Task<ScanSession?> LoadSnapshotAsync(string filePath)
  {
    try {
      var options = CreateOptions(filePath);
      await using var db = new EventSnapshotDbContext(options);
      await db.Database.EnsureCreatedAsync();
      var res = await db.ScanSessions
        .AsSplitQuery()
        .Include(s => s.Entries)
        .Include(s => s.ScannedSources)
        .FirstOrDefaultAsync();
      return res;
    } catch (SqliteException ex) {
      // Handle schema mismatch or file errors
      throw new InvalidOperationException("Failed to load snapshot.", ex);
    }
  }


  public static async Task<(ScanSession?, int?)> LoadSnapshotMetadataAsync(string filePath)
  {
    try {
      var options = CreateOptions(filePath);
      await using var db = new EventSnapshotDbContext(options);
      await db.Database.EnsureCreatedAsync();
      var res = await db.ScanSessions
        .Include(s => s.ScannedSources)
        .Select(s => new {
          Session = s,
          Count = s.Entries.Count
        }).FirstOrDefaultAsync();

      return (res?.Session, res?.Count);
    } catch (SqliteException ex) {
      // Handle schema mismatch or file errors
      throw new InvalidOperationException("Failed to load snapshot metadata.", ex);
    }
  }

}
