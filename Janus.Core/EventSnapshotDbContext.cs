using Microsoft.EntityFrameworkCore;

namespace Janus.Core;

/// <summary>
/// EF Core context for event log snapshots.
/// </summary>
public class EventSnapshotDbContext(DbContextOptions<EventSnapshotDbContext> options) : DbContext(options)
{
    public DbSet<EventLogEntry> EventLogEntries => Set<EventLogEntry>();
    public DbSet<ScanSession> ScanSessions => Set<ScanSession>();
}
