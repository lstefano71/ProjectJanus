using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;

namespace Janus.App;

public partial class NumericUpDown : UserControl, INotifyPropertyChanged
{
  private int value = 0;
  public int Value {
    get => value;
    set { this.value = value; OnPropertyChanged(); }
  }

  public ICommand IncrementCommand { get; }
  public ICommand DecrementCommand { get; }

  public NumericUpDown()
  {
    InitializeComponent();
    DataContext = this;
    IncrementCommand = new RelayCommand(_ => Value++);
    DecrementCommand = new RelayCommand(_ => Value--);
  }

  public event PropertyChangedEventHandler? PropertyChanged;
  private void OnPropertyChanged([CallerMemberName] string? name = null)
      => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
