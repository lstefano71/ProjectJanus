using System.Windows;

namespace Janus.App;

public partial class AboutDialog : Window {
  public AboutDialog() {
    InitializeComponent();
    VersionText.Text = $"Version {ThisAssembly.AssemblyInformationalVersion}";
    BranchText.Text = "Branch: <not available>"; // Branch info not available in ThisAssembly
    ShaText.Text = $"Commit: {ThisAssembly.GitCommitId}";
    DateText.Text = $"Commit Date: {ThisAssembly.GitCommitDate.ToLocalTime():yyyy-MM-dd HH:mm:ss}";
    SysInfoText.Text = $"System: .NET {System.Environment.Version} on {System.Runtime.InteropServices.RuntimeInformation.OSDescription}";
  }

  private void OnOkClick(object sender, RoutedEventArgs e) => Close();


  private void OnLinkClick(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
    e.Handled = true;
  }

  private void OnCopyInfoClick(object sender, RoutedEventArgs e) {
    var info = $"{VersionText.Text}\n{BranchText.Text}\n{ShaText.Text}\n{DateText.Text}\n{SysInfoText.Text}";
    Clipboard.SetText(info);
  }
}
