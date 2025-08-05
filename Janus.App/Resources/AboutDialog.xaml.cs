using System.Reflection;
using System.Windows;

namespace Janus.App;

public partial class AboutDialog : Window
{
    public AboutDialog()
    {
        InitializeComponent();
        VersionText.Text = $"Version {ThisAssembly.AssemblyInformationalVersion}";
        BranchText.Text = "Branch: <not available>"; // Branch info not available in ThisAssembly
        ShaText.Text = $"Commit: {ThisAssembly.GitCommitId}";
        DateText.Text = $"Commit Date: {ThisAssembly.GitCommitDate.ToLocalTime():yyyy-MM-dd HH:mm:ss}";
    }

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
