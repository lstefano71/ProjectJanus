using System.Windows.Input;

namespace Janus.App;

public class RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null) : ICommand {
  private readonly Action<object?> execute = execute ?? throw new ArgumentNullException(nameof(execute));
  private readonly Predicate<object?>? canExecute = canExecute;

  public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;
  public void Execute(object? parameter) => execute(parameter);
  public event EventHandler? CanExecuteChanged {
    add => CommandManager.RequerySuggested += value; remove => CommandManager.RequerySuggested -= value;
  }

  public static void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}

// AsyncRelayCommand for async/await command support
public class AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null) : ICommand {
  private readonly Func<Task> execute = execute ?? throw new ArgumentNullException(nameof(execute));
  private readonly Func<bool>? canExecute = canExecute;
  private bool isExecuting;

  public bool CanExecute(object? parameter) => !isExecuting && (canExecute?.Invoke() ?? true);

  public async void Execute(object? parameter) {
    if (!CanExecute(parameter)) {
      return;
    }

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
