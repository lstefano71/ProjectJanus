
using Janus.Core;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Janus.App.Services;

public class SnapshotService
{
    public async Task SaveSnapshotAsync(string filePath, ScanSession session)
    {
        await using var dbContext = new JanusDbContext(filePath);
        await dbContext.Database.EnsureCreatedAsync();

        dbContext.ScanSessions.Add(session);

        await dbContext.SaveChangesAsync();
    }

    public async Task<ScanSession?> LoadSnapshotAsync(string filePath)
    {
        await using var dbContext = new JanusDbContext(filePath);
        return await dbContext.ScanSessions.Include(s => s.Entries).FirstOrDefaultAsync();
    }
}
