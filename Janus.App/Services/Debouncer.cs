using System.Windows.Threading;

namespace Janus.App.Services;

/// <summary>
/// Provides a generic debounce mechanism for async actions on the UI thread.
/// </summary>
public sealed class Debouncer
{
  private readonly DispatcherTimer timer;
  private Func<Task>? action;
  private bool pending;

  public Debouncer(TimeSpan interval)
  {
    timer = new DispatcherTimer { Interval = interval };
    timer.Tick += async (_, __) => {
      timer.Stop();
      if (pending && action is not null) {
        await action();
        pending = false;
      }
    };
  }

  /// <summary>
  /// Debounces the specified async action.
  /// </summary>
  public void Debounce(Func<Task> action)
  {
    this.action = action;
    pending = true;
    timer.Stop();
    timer.Start();
  }
}
