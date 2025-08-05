
using Microsoft.EntityFrameworkCore;

namespace Janus.Core;

public class JanusDbContext : DbContext
{
    public DbSet<ScanSession> ScanSessions { get; set; }
    public DbSet<EventLogEntry> EventLogEntries { get; set; }

    private readonly string _dbPath;

    // This constructor is for design-time tools
    public JanusDbContext() : this("janus-design-time.db") { }

    public JanusDbContext(string dbPath)
    {
        _dbPath = dbPath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScanSession>()
            .HasMany(s => s.Entries)
            .WithOne(e => e.ScanSession)
            .HasForeignKey(e => e.ScanSessionId);
    }
}
