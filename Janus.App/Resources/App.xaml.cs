using System.Configuration;
using System.Data;
using System.Windows;

namespace Janus.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        this.DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        LogException(e.Exception);
        MessageBox.Show($"A fatal error occurred. Details have been saved to janus-crash.log.\n{e.Exception}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
        Shutdown();
    }

    private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogException(ex);
        }
        else
        {
            LogException(new Exception($"Non-Exception object: {e.ExceptionObject}"));
        }
    }

    private void LogException(Exception ex)
    {
        try
        {
            var path = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "janus-crash.log");
            System.IO.File.AppendAllText(path, $"[{DateTime.Now:O}] {ex}\n{ex.StackTrace}\n\n");
        }
        catch { /* ignore logging errors */ }
    }
}

