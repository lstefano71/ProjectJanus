
using System;

namespace Janus.App.Models;

public class ScannedEvent
{
    public DateTime? TimeCreated { get; set; }

    public int EventId { get; set; }

    public string? LevelDisplayName { get; set; }

    public string? ProviderName { get; set; }

    public string? TaskDisplayName { get; set; }

    public string? Message { get; set; }

    public string? LogName { get; set; }
}
