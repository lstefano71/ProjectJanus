
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Janus.Core;

public class EventLogEntry
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public int ScanSessionId { get; set; }

    public required ScanSession ScanSession { get; set; }

    public DateTime? TimeCreated { get; set; }

    public int EventId { get; set; }

    public string? LevelDisplayName { get; set; }

    public string? ProviderName { get; set; }

    public string? TaskDisplayName { get; set; }

    public string? Message { get; set; }

    public string? LogName { get; set; }
}
