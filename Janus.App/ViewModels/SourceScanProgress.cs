using Janus.Core;

using System.ComponentModel;

namespace Janus.App;

public class SourceScanProgress : INotifyPropertyChanged
{
  // Remove 'required' from SourceName to avoid type confusion
  public string SourceName { get; set; } = string.Empty;
  public int EventsRetrieved { get; set; }
  public ScanStatus Status { get; set; } = ScanStatus.Success;
  public bool IsTotalKnown { get; set; }
  public int? TotalEvents { get; set; }
  public bool IsActive { get; set; }
  public string? ExceptionMessage { get; set; }

  public event PropertyChangedEventHandler? PropertyChanged;

  public void OnPropertyChanged(string propertyName)
      => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

  public SourceScanProgress()
  {
    Status = ScanStatus.Success;
    SourceName = string.Empty;
  }
}