namespace Janus.App;

using Janus.Core;
using System.ComponentModel;

public class SourceScanProgress : INotifyPropertyChanged
{
    public string SourceName { get; set; }
    public int EventsRetrieved { get; set; }
    public ScanStatus Status { get; set; }
    public bool IsTotalKnown { get; set; }
    public int? TotalEvents { get; set; }
    public bool IsActive { get; set; }
    public string? ExceptionMessage { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}