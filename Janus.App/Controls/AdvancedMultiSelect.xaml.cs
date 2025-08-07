using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Janus.App.Controls;

public partial class AdvancedMultiSelect : UserControl, INotifyPropertyChanged {
  public AdvancedMultiSelect() {
    InitializeComponent();
    SelectAllVisibleCommand = new RelayCommand(_ => SelectAllVisible());
    ShowOnlySelectedCommand = new RelayCommand(_ => ShowOnlySelected = !ShowOnlySelected);
    FilteredItems = [];
    UpdateFilteredItems();
  }

  // Dependency Properties
  public static readonly DependencyProperty ItemsSourceProperty =
      DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(AdvancedMultiSelect),
          new PropertyMetadata(null, (d, e) => ((AdvancedMultiSelect)d).UpdateFilteredItems()));

  public static readonly DependencyProperty SelectedItemsProperty =
      DependencyProperty.Register(nameof(SelectedItems), typeof(IList), typeof(AdvancedMultiSelect),
          new PropertyMetadata(null));

  public static readonly DependencyProperty SearchTextProperty =
      DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(AdvancedMultiSelect),
          new PropertyMetadata(string.Empty, (d, e) => ((AdvancedMultiSelect)d).UpdateFilteredItems()));

  public static readonly DependencyProperty ShowOnlySelectedProperty =
      DependencyProperty.Register(nameof(ShowOnlySelected), typeof(bool), typeof(AdvancedMultiSelect),
          new PropertyMetadata(false, (d, e) => ((AdvancedMultiSelect)d).UpdateFilteredItems()));

  public static readonly DependencyProperty ItemCountProviderProperty =
      DependencyProperty.Register(nameof(ItemCountProvider), typeof(Func<object, int>), typeof(AdvancedMultiSelect),
          new PropertyMetadata(null, (d, e) => ((AdvancedMultiSelect)d).UpdateFilteredItems()));

  public static readonly DependencyProperty PlaceholderTextProperty =
      DependencyProperty.Register(nameof(PlaceholderText), typeof(string), typeof(AdvancedMultiSelect),
          new PropertyMetadata("Search..."));

  public static readonly DependencyProperty HeaderProperty =
      DependencyProperty.Register(nameof(Header), typeof(string), typeof(AdvancedMultiSelect),
          new PropertyMetadata(string.Empty));

  // Properties
  public IEnumerable ItemsSource {
    get => (IEnumerable)GetValue(ItemsSourceProperty);
    set => SetValue(ItemsSourceProperty, value);
  }

  public IList SelectedItems {
    get => (IList)GetValue(SelectedItemsProperty);
    set => SetValue(SelectedItemsProperty, value);
  }

  public string SearchText {
    get => (string)GetValue(SearchTextProperty);
    set => SetValue(SearchTextProperty, value);
  }

  public bool ShowOnlySelected {
    get => (bool)GetValue(ShowOnlySelectedProperty);
    set => SetValue(ShowOnlySelectedProperty, value);
  }

  public Func<object, int> ItemCountProvider {
    get => (Func<object, int>)GetValue(ItemCountProviderProperty);
    set => SetValue(ItemCountProviderProperty, value);
  }

  public string PlaceholderText {
    get => (string)GetValue(PlaceholderTextProperty);
    set => SetValue(PlaceholderTextProperty, value);
  }

  public string Header {
    get => (string)GetValue(HeaderProperty);
    set => SetValue(HeaderProperty, value);
  }

  // Commands
  public ICommand SelectAllVisibleCommand { get; }
  public ICommand ShowOnlySelectedCommand { get; }
  public ICommand ItemToggleCommand => new RelayCommand(param => ToggleItem(param));

  // Filtered items for display
  public ObservableCollection<ItemViewModel> FilteredItems { get; }

  private void UpdateFilteredItems() {
    FilteredItems.Clear();
    if (ItemsSource == null) {
      return;
    }

    IEnumerable<object> items = ItemsSource.Cast<object>();
    if (!string.IsNullOrWhiteSpace(SearchText)) {
      items = items.Where(i => i?.ToString()?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    if (ShowOnlySelected && SelectedItems != null) {
      items = items.Where(i => SelectedItems.Contains(i));
    }

    foreach (object? item in items) {
      FilteredItems.Add(new ItemViewModel(this) {
        Value = item,
        DisplayText = item?.ToString() ?? "",
        IsSelected = SelectedItems?.Contains(item) ?? false,
        Count = ItemCountProvider?.Invoke(item) ?? 0
      });
    }
    OnPropertyChanged(nameof(FilteredItems));
  }

  private void SelectAllVisible() {
    if (SelectedItems == null) {
      return;
    }

    foreach (object? item in FilteredItems.Select(vm => vm.Value).ToList()) {
      if (!SelectedItems.Contains(item)) {
        SelectedItems.Add(item);
      }
    }
    UpdateFilteredItems();
  }

  private void ToggleItem(object param) {
    if (SelectedItems == null || param == null) {
      return;
    }

    if (SelectedItems.Contains(param)) {
      SelectedItems.Remove(param);
    } else {
      SelectedItems.Add(param);
    }

    UpdateFilteredItems();
  }

  public event PropertyChangedEventHandler? PropertyChanged;
  protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

  public class ItemViewModel : INotifyPropertyChanged {
    private readonly AdvancedMultiSelect parent;
    private bool isSelected;
    public ItemViewModel(AdvancedMultiSelect parent) {
      this.parent = parent;
    }
    public object Value { get; set; }
    public string DisplayText { get; set; }
    public int Count { get; set; }
    public bool IsSelected {
      get => isSelected;
      set {
        if (isSelected != value) {
          isSelected = value;
          if (parent.SelectedItems != null && Value != null) {
            if (isSelected && !parent.SelectedItems.Contains(Value)) {
              parent.SelectedItems.Add(Value);
            } else if (!isSelected && parent.SelectedItems.Contains(Value)) {
              parent.SelectedItems.Remove(Value);
            }
          }
          OnPropertyChanged(nameof(IsSelected));
        }
      }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }
}
