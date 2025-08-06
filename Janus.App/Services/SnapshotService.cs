using Janus.Core;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Janus.App;

/// <summary>
/// Service for saving and loading event log scan snapshots.
/// </summary>
public class SnapshotService
{
  public static async Task SaveSnapshotAsync(string filePath, ScanSession session)
  {
    try {
      var options = new DbContextOptionsBuilder<EventSnapshotDbContext>()
          .UseSqlite($"Data Source={filePath}")
          .Options;
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
        Entries = session.Entries
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
      var options = new DbContextOptionsBuilder<EventSnapshotDbContext>()
          .UseSqlite($"Data Source={filePath}")
          .Options;
      await using var db = new EventSnapshotDbContext(options);
      await db.Database.EnsureCreatedAsync();
      return await db.ScanSessions.Include(s => s.Entries).FirstOrDefaultAsync();
    } catch (SqliteException ex) {
      // Handle schema mismatch or file errors
      throw new InvalidOperationException("Failed to load snapshot.", ex);
    }
  }
}
