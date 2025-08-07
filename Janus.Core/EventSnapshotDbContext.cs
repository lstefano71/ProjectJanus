using Microsoft.EntityFrameworkCore;

namespace Janus.Core;

/// <summary>
/// EF Core context for event log snapshots.
/// </summary>
public class EventSnapshotDbContext(DbContextOptions<EventSnapshotDbContext> options) : DbContext(options)
{
  public DbSet<EventLogEntry> EventLogEntries => Set<EventLogEntry>();
  public DbSet<ScanSession> ScanSessions => Set<ScanSession>();
  public DbSet<ScannedSource> ScannedSources => Set<ScannedSource>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<ScanSession>()
      .HasMany(s => s.ScannedSources)
      .WithOne()
      .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<ScannedSource>()
      .Property(s => s.Status)
      .HasConversion<string>();
  }
}
