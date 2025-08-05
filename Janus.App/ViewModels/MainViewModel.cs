
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Janus.App.Models;
using Janus.App.Services;
using Janus.Core;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Janus.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly EventLogService _eventLogService;
    private readonly SnapshotService _snapshotService;
    private CancellationTokenSource? _cancellationTokenSource;

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty]
    private string _selectedTime = DateTime.Now.ToString("HH:mm:ss");

    [ObservableProperty]
    private int _minutesBefore = 5;

    [ObservableProperty]
    private int _minutesAfter = 5;

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ScannedEvent? _selectedEvent;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private string _progressText = string.Empty;

    [ObservableProperty]
    private bool _isGroupingEnabled = false;

    private readonly ObservableCollection<ScannedEvent> _allEvents = new();
    public ListCollectionView FilteredEvents { get; }

    public MainViewModel(EventLogService eventLogService, SnapshotService snapshotService)
    {
        _eventLogService = eventLogService;
        _snapshotService = snapshotService;

        FilteredEvents = (ListCollectionView)CollectionViewSource.GetDefaultView(_allEvents);
        FilteredEvents.Filter = FilterEvents;
        FilteredEvents.SortDescriptions.Add(new System.ComponentModel.SortDescription("TimeCreated", System.ComponentModel.ListSortDirection.Ascending));
    }

    private bool FilterEvents(object obj)
    {
        if (obj is not ScannedEvent scannedEvent) return false;

        var filter = FilterText.ToLower();
        var search = SearchText.ToLower();

        var passesFilter = string.IsNullOrWhiteSpace(filter) || (scannedEvent.LevelDisplayName?.ToLower().Contains(filter) ?? false);
        var passesSearch = string.IsNullOrWhiteSpace(search) || (scannedEvent.Message?.ToLower().Contains(search) ?? false) || (scannedEvent.ProviderName?.ToLower().Contains(search) ?? false);

        return passesFilter && passesSearch;
    }

    private DateTime CenterPoint => SelectedDate.Date + TimeSpan.Parse(SelectedTime);

    [RelayCommand]
    private async Task Scan()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        IsScanning = true;
        ProgressText = "Starting scan...";
        _allEvents.Clear();

        var progress = new Progress<string>(p => ProgressText = p);

        try
        {
            var entries = await _eventLogService.ScanEventsAsync(CenterPoint, MinutesBefore, MinutesAfter, progress, _cancellationTokenSource.Token);
            foreach (var entry in entries.OrderBy(e => e.TimeCreated))
            {
                _allEvents.Add(entry);
            }
            ProgressText = $"Scan complete. Found {_allEvents.Count} events.";
        }
        catch (OperationCanceledException)
        {
            ProgressText = "Scan canceled.";
        }
        finally
        {
            IsScanning = false;
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancelScan))]
    private void CancelScan()
    {
        _cancellationTokenSource?.Cancel();
    }

    private bool CanCancelScan()
    {
        return IsScanning;
    }

    [RelayCommand]
    private async Task SaveSnapshot()
    {
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "Janus Snapshot|*.db",
            Title = "Save a Janus Snapshot"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            var session = new ScanSession
            {
                ComputerName = Environment.MachineName,
                ScanTimestamp = DateTime.Now,
                CenterPoint = CenterPoint,
                MinutesBefore = MinutesBefore,
                MinutesAfter = MinutesAfter
            };

            session.Entries = _allEvents.Select(e => new EventLogEntry
            {
                ScanSession = session,
                TimeCreated = e.TimeCreated,
                EventId = e.EventId,
                LevelDisplayName = e.LevelDisplayName,
                ProviderName = e.ProviderName,
                TaskDisplayName = e.TaskDisplayName,
                Message = e.Message,
                LogName = e.LogName
            }).ToList();

            await _snapshotService.SaveSnapshotAsync(saveFileDialog.FileName, session);
        }
    }

    [RelayCommand]
    private async Task LoadSnapshot()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Janus Snapshot|*.db",
            Title = "Load a Janus Snapshot"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            var session = await _snapshotService.LoadSnapshotAsync(openFileDialog.FileName);
            if (session != null)
            {
                SelectedDate = session.CenterPoint.Date;
                SelectedTime = session.CenterPoint.ToString("HH:mm:ss");
                MinutesBefore = session.MinutesBefore;
                MinutesAfter = session.MinutesAfter;
                _allEvents.Clear();
                foreach (var entry in session.Entries)
                {
                    _allEvents.Add(new ScannedEvent
                    {
                        TimeCreated = entry.TimeCreated,
                        EventId = entry.EventId,
                        LevelDisplayName = entry.LevelDisplayName,
                        ProviderName = entry.ProviderName,
                        TaskDisplayName = entry.TaskDisplayName,
                        Message = entry.Message,
                        LogName = entry.LogName
                    });
                }
            }
        }
    }

    partial void OnFilterTextChanged(string value)
    {
        FilteredEvents.Refresh();
    }

    partial void OnSearchTextChanged(string value)
    {
        FilteredEvents.Refresh();
    }

    partial void OnIsGroupingEnabledChanged(bool value)
    {
        if (value)
        {
            FilteredEvents.GroupDescriptions.Add(new PropertyGroupDescription("LogName"));
        }
        else
        {
            FilteredEvents.GroupDescriptions.Clear();
        }
    }
}
