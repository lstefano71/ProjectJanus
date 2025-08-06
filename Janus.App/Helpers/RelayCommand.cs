using System.Windows.Input;

namespace Janus.App;

public class RelayCommand : ICommand
{
  private readonly Action<object?> execute;
  private readonly Predicate<object?>? canExecute;

  public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
  {
    this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
    this.canExecute = canExecute;
  }

  public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;
  public void Execute(object? parameter) => execute(parameter);
  public event EventHandler? CanExecuteChanged {
    add { CommandManager.RequerySuggested += value; }
    remove { CommandManager.RequerySuggested -= value; }
  }
}

// AsyncRelayCommand for async/await command support
public class AsyncRelayCommand : ICommand
{
  private readonly Func<Task> execute;
  private readonly Func<bool>? canExecute;
  private bool isExecuting;

  public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
  {
    this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
    this.canExecute = canExecute;
  }

  public bool CanExecute(object? parameter) => !isExecuting && (canExecute?.Invoke() ?? true);

  public async void Execute(object? parameter)
  {
    if (!CanExecute(parameter)) return;
    isExecuting = true;
    RaiseCanExecuteChanged();
    try {
      await execute();
    } finally {
      isExecuting = false;
      RaiseCanExecuteChanged();
    }
  }

  public event EventHandler? CanExecuteChanged;
  public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
