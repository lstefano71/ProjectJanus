
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Janus.Core;

public class ScanSession
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public required string ComputerName { get; set; }

    public required DateTime ScanTimestamp { get; set; }

    public required DateTime CenterPoint { get; set; }

    public required int MinutesBefore { get; set; }

    public required int MinutesAfter { get; set; }

    public ICollection<EventLogEntry> Entries { get; set; } = new List<EventLogEntry>();
}
