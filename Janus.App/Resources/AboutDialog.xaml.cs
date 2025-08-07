using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Controls;

namespace Janus.App;

public partial class AboutDialog : Window
{
  private readonly CancellationTokenSource _cts = new();

  public AboutDialog()
  {
    InitializeComponent();
    VersionText.Text = $"Version {ThisAssembly.AssemblyInformationalVersion}";
    BranchText.Text = "Branch: <not available>"; // Branch info not available in ThisAssembly
    ShaText.Text = $"Commit: {ThisAssembly.GitCommitId}";
    DateText.Text = $"Commit Date: {ThisAssembly.GitCommitDate.ToLocalTime():yyyy-MM-dd HH:mm:ss}";
    SysInfoText.Text = $"System: .NET {System.Environment.Version} on {System.Runtime.InteropServices.RuntimeInformation.OSDescription}";
    AnimateCopyrightAsync(_cts.Token);
    Loaded += Window_Loaded;
  }

  protected override void OnClosed(EventArgs e)
  {
    _cts.Cancel();
    base.OnClosed(e);
  }

  private static readonly Color[] DarkVividColors = new[]
  {
    Color.FromRgb(180, 0, 0),      // Dark Red
    Color.FromRgb(255, 80, 0),     // Orange Red
    Color.FromRgb(200, 160, 0),    // Dark Gold
    Color.FromRgb(0, 120, 0),      // Dark Green
    Color.FromRgb(0, 180, 180),    // Teal
    Color.FromRgb(0, 0, 180),      // Dark Blue
    Color.FromRgb(80, 0, 180),     // Purple
    Color.FromRgb(180, 0, 120),    // Magenta
    Color.FromRgb(180, 40, 40),    // Brick
    Color.FromRgb(0, 80, 160),     // Deep Blue
    Color.FromRgb(0, 160, 80),     // Deep Green
    Color.FromRgb(120, 0, 80),     // Plum
    Color.FromRgb(120, 80, 0),     // Brown
    Color.FromRgb(0, 80, 80),      // Deep Teal
    Color.FromRgb(80, 80, 0),      // Olive
  };

  private async void AnimateCopyrightAsync(CancellationToken token)
  {
    var rnd = new Random();
    await Task.Delay(2000, token);

    while (!token.IsCancellationRequested)
    {
      await Dispatcher.InvokeAsync(() =>
      {
        var transform = (TransformGroup)CopyrightText.RenderTransform;
        var scale = (ScaleTransform)transform.Children[0];
        var rotate = (RotateTransform)transform.Children[1];
        var translate = (TranslateTransform)transform.Children[2];

        int animTypeRaw = rnd.Next(48);
        int animType;
        if (animTypeRaw == 16) animType = 4; // Cartwheel (rare)
        else if (animTypeRaw == 17) animType = 5; // Barrel (rare)
        else if (animTypeRaw == 18) animType = 6; // Shiver (rare)
        else if (animTypeRaw == 19) animType = 7; // Pop (rare)
        else if (animTypeRaw == 20) animType = 8; // Flip (rare)
        else if (animTypeRaw == 21) animType = 9; // Wobble (rare)
        else if (animTypeRaw == 22) animType = 10; // Rainbow pulse (rare)
        else if (animTypeRaw == 23) animType = 11; // Slide out/in (rare)
        else if (animTypeRaw == 24) animType = 12; // Bounce (rare)
        else if (animTypeRaw == 25) animType = 13; // Squash/stretch (rare)
        else if (animTypeRaw == 26) animType = 14; // Fade out/in (rare)
        else if (animTypeRaw == 27) animType = 15; // Spiral (rare)
        else animType = animTypeRaw % 5;

        switch (animType)
        {
          case 0: AnimateMove(rnd); break;
          case 1: AnimateRotate(rnd); break;
          case 2: AnimateScale(rnd); break;
          case 3: AnimateCombo(rnd); break;
          case 4: AnimateCartwheel(rnd, rotate.Angle); break;
          case 5: AnimateBarrel(rnd); break;
          case 6: AnimateShiver(rnd); break;
          case 7: AnimatePop(); break;
          case 8: AnimateFlip(); break;
          case 9: AnimateWobble(rnd, rotate.Angle); break;
          case 10: AnimateRainbowPulse(rnd); break;
          case 11: AnimateSlideOutIn(rnd); break;
          case 12: AnimateBounce(); break;
          case 13: AnimateSquashStretch(); break;
          case 14: AnimateFade(); break;
          case 15: AnimateSpiral(rnd, translate.X, translate.Y); break;
        }
      });

      try
      {
        await Task.Delay((int)(700 + rnd.NextDouble() * 1000), token);
      }
      catch (TaskCanceledException)
      {
        break;
      }
    }
  }

  private void AnimateMove(Random rnd)
  {
    var sb = new Storyboard();
    double x = rnd.NextDouble() * 240 - 120;
    double y = rnd.NextDouble() * 80 - 40;
    var ax = new DoubleAnimation(x, TimeSpan.FromSeconds(rnd.NextDouble() * 0.7 + 0.6)) { EasingFunction = new SineEase() };
    var ay = new DoubleAnimation(y, TimeSpan.FromSeconds(rnd.NextDouble() * 0.7 + 0.6)) { EasingFunction = new SineEase() };
    Storyboard.SetTarget(ax, CopyrightText);
    Storyboard.SetTargetProperty(ax, new PropertyPath("RenderTransform.Children[2].X"));
    Storyboard.SetTarget(ay, CopyrightText);
    Storyboard.SetTargetProperty(ay, new PropertyPath("RenderTransform.Children[2].Y"));
    sb.Children.Add(ax);
    sb.Children.Add(ay);
    sb.Begin();
  }

  private void AnimateRotate(Random rnd)
  {
    var sb = new Storyboard();
    double angle = rnd.NextDouble() * 1080 - 540;
    var a = new DoubleAnimation(angle, TimeSpan.FromSeconds(rnd.NextDouble() * 0.7 + 0.6)) { EasingFunction = new SineEase() };
    Storyboard.SetTarget(a, CopyrightText);
    Storyboard.SetTargetProperty(a, new PropertyPath("RenderTransform.Children[1].Angle"));
    sb.Children.Add(a);
    sb.Begin();
  }

  private void AnimateScale(Random rnd)
  {
    var sb = new Storyboard();
    double scaleTo = rnd.NextDouble() * 1.2 + 0.4;
    var ax = new DoubleAnimation(scaleTo, TimeSpan.FromSeconds(rnd.NextDouble() * 0.7 + 0.6)) { EasingFunction = new SineEase() };
    var ay = new DoubleAnimation(scaleTo, TimeSpan.FromSeconds(rnd.NextDouble() * 0.7 + 0.6)) { EasingFunction = new SineEase() };
    Storyboard.SetTarget(ax, CopyrightText);
    Storyboard.SetTargetProperty(ax, new PropertyPath("RenderTransform.Children[0].ScaleX"));
    Storyboard.SetTarget(ay, CopyrightText);
    Storyboard.SetTargetProperty(ay, new PropertyPath("RenderTransform.Children[0].ScaleY"));
    sb.Children.Add(ax);
    sb.Children.Add(ay);
    sb.Begin();
  }

  private void AnimateCombo(Random rnd)
  {
    var sb = new Storyboard();
    double x = rnd.NextDouble() * 240 - 120;
    double y = rnd.NextDouble() * 80 - 40;
    double angle = rnd.NextDouble() * 1080 - 540;
    double scaleTo = rnd.NextDouble() * 1.2 + 0.4;
    var ax = new DoubleAnimation(x, TimeSpan.FromSeconds(rnd.NextDouble() * 0.7 + 0.6)) { EasingFunction = new SineEase() };
    var ay = new DoubleAnimation(y, TimeSpan.FromSeconds(rnd.NextDouble() * 0.7 + 0.6)) { EasingFunction = new SineEase() };
    var arot = new DoubleAnimation(angle, TimeSpan.FromSeconds(rnd.NextDouble() * 0.7 + 0.6)) { EasingFunction = new SineEase() };
    var asx = new DoubleAnimation(scaleTo, TimeSpan.FromSeconds(rnd.NextDouble() * 0.7 + 0.6)) { EasingFunction = new SineEase() };
    var asy = new DoubleAnimation(scaleTo, TimeSpan.FromSeconds(rnd.NextDouble() * 0.7 + 0.6)) { EasingFunction = new SineEase() };
    Storyboard.SetTarget(ax, CopyrightText);
    Storyboard.SetTargetProperty(ax, new PropertyPath("RenderTransform.Children[2].X"));
    Storyboard.SetTarget(ay, CopyrightText);
    Storyboard.SetTargetProperty(ay, new PropertyPath("RenderTransform.Children[2].Y"));
    Storyboard.SetTarget(arot, CopyrightText);
    Storyboard.SetTargetProperty(arot, new PropertyPath("RenderTransform.Children[1].Angle"));
    Storyboard.SetTarget(asx, CopyrightText);
    Storyboard.SetTargetProperty(asx, new PropertyPath("RenderTransform.Children[0].ScaleX"));
    Storyboard.SetTarget(asy, CopyrightText);
    Storyboard.SetTargetProperty(asy, new PropertyPath("RenderTransform.Children[0].ScaleY"));
    sb.Children.Add(ax); sb.Children.Add(ay); sb.Children.Add(arot); sb.Children.Add(asx); sb.Children.Add(asy);
    sb.Begin();
  }

  private void AnimateCartwheel(Random rnd, double currentAngle)
  {
    var sb = new Storyboard();
    double angle = currentAngle + (rnd.Next(2) == 0 ? 360 : -360) * (rnd.Next(2) + 1);
    var a = new DoubleAnimation(angle, TimeSpan.FromSeconds((rnd.NextDouble() * 0.7 + 0.6) * 0.7)) { EasingFunction = new CubicEase() };
    Storyboard.SetTarget(a, CopyrightText);
    Storyboard.SetTargetProperty(a, new PropertyPath("RenderTransform.Children[1].Angle"));
    sb.Children.Add(a);
    sb.Begin();
  }

  private void AnimateBarrel(Random rnd)
  {
    var sb = new Storyboard();
    double sx = rnd.NextDouble() * 1.2 + 0.4;
    double sy = 2.0 - sx;
    var ax = new DoubleAnimation(sx, TimeSpan.FromSeconds(rnd.NextDouble() * 0.7 + 0.6)) { EasingFunction = new SineEase() };
    var ay = new DoubleAnimation(sy, TimeSpan.FromSeconds(rnd.NextDouble() * 0.7 + 0.6)) { EasingFunction = new SineEase() };
    Storyboard.SetTarget(ax, CopyrightText);
    Storyboard.SetTargetProperty(ax, new PropertyPath("RenderTransform.Children[0].ScaleX"));
    Storyboard.SetTarget(ay, CopyrightText);
    Storyboard.SetTargetProperty(ay, new PropertyPath("RenderTransform.Children[0].ScaleY"));
    sb.Children.Add(ax);
    sb.Children.Add(ay);
    sb.Begin();
  }

  private void AnimateShiver(Random rnd)
  {
    var sb = new Storyboard();
    for (int i = 0; i < 6; i++)
    {
      double x = (i % 2 == 0 ? 1 : -1) * (rnd.NextDouble() * 20 + 10);
      var ax = new DoubleAnimation(x, TimeSpan.FromMilliseconds(40)) { BeginTime = TimeSpan.FromMilliseconds(i * 40) };
      Storyboard.SetTarget(ax, CopyrightText);
      Storyboard.SetTargetProperty(ax, new PropertyPath("RenderTransform.Children[2].X"));
      sb.Children.Add(ax);
    }
    var axReset = new DoubleAnimation(0, TimeSpan.FromMilliseconds(40)) { BeginTime = TimeSpan.FromMilliseconds(240) };
    Storyboard.SetTarget(axReset, CopyrightText);
    Storyboard.SetTargetProperty(axReset, new PropertyPath("RenderTransform.Children[2].X"));
    sb.Children.Add(axReset);
    sb.Begin();
  }

  private void AnimatePop()
  {
    var sb = new Storyboard();
    var ax = new DoubleAnimation(1.8, TimeSpan.FromMilliseconds(120)) { EasingFunction = new BackEase { Amplitude = 0.7 } };
    var ay = new DoubleAnimation(1.8, TimeSpan.FromMilliseconds(120)) { EasingFunction = new BackEase { Amplitude = 0.7 } };
    var axBack = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(220)) { BeginTime = TimeSpan.FromMilliseconds(120), EasingFunction = new BounceEase { Bounces = 2, Bounciness = 2 } };
    var ayBack = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(220)) { BeginTime = TimeSpan.FromMilliseconds(120), EasingFunction = new BounceEase { Bounces = 2, Bounciness = 2 } };
    Storyboard.SetTarget(ax, CopyrightText);
    Storyboard.SetTargetProperty(ax, new PropertyPath("RenderTransform.Children[0].ScaleX"));
    Storyboard.SetTarget(ay, CopyrightText);
    Storyboard.SetTargetProperty(ay, new PropertyPath("RenderTransform.Children[0].ScaleY"));
    Storyboard.SetTarget(axBack, CopyrightText);
    Storyboard.SetTargetProperty(axBack, new PropertyPath("RenderTransform.Children[0].ScaleX"));
    Storyboard.SetTarget(ayBack, CopyrightText);
    Storyboard.SetTargetProperty(ayBack, new PropertyPath("RenderTransform.Children[0].ScaleY"));
    sb.Children.Add(ax); sb.Children.Add(ay); sb.Children.Add(axBack); sb.Children.Add(ayBack);
    sb.Begin();
  }

  private void AnimateFlip()
  {
    var sb = new Storyboard();
    var ax = new DoubleAnimation(-1.0, TimeSpan.FromMilliseconds(180)) { EasingFunction = new SineEase() };
    var axBack = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(180)) { BeginTime = TimeSpan.FromMilliseconds(180), EasingFunction = new SineEase() };
    Storyboard.SetTarget(ax, CopyrightText);
    Storyboard.SetTargetProperty(ax, new PropertyPath("RenderTransform.Children[0].ScaleX"));
    Storyboard.SetTarget(axBack, CopyrightText);
    Storyboard.SetTargetProperty(axBack, new PropertyPath("RenderTransform.Children[0].ScaleX"));
    sb.Children.Add(ax); sb.Children.Add(axBack);
    sb.Begin();
  }

  private void AnimateWobble(Random rnd, double baseAngle)
  {
    var sb = new Storyboard();
    for (int i = 0; i < 6; i++)
    {
      double angle = baseAngle + Math.Sin(i * Math.PI / 3) * 40;
      var a = new DoubleAnimation(angle, TimeSpan.FromMilliseconds(80)) { BeginTime = TimeSpan.FromMilliseconds(i * 80), EasingFunction = new SineEase() };
      Storyboard.SetTarget(a, CopyrightText);
      Storyboard.SetTargetProperty(a, new PropertyPath("RenderTransform.Children[1].Angle"));
      sb.Children.Add(a);
    }
    var aReset = new DoubleAnimation(baseAngle, TimeSpan.FromMilliseconds(80)) { BeginTime = TimeSpan.FromMilliseconds(480) };
    Storyboard.SetTarget(aReset, CopyrightText);
    Storyboard.SetTargetProperty(aReset, new PropertyPath("RenderTransform.Children[1].Angle"));
    sb.Children.Add(aReset);
    sb.Begin();
  }

  private void AnimateRainbowPulse(Random rnd)
  {
    var brush = CopyrightText.Foreground as SolidColorBrush;
    var sb = new Storyboard();
    for (int i = 0; i < 6; i++)
    {
      var to = DarkVividColors[rnd.Next(DarkVividColors.Length)];
      var anim = new ColorAnimation(to, TimeSpan.FromMilliseconds(100)) { BeginTime = TimeSpan.FromMilliseconds(i * 100) };
      Storyboard.SetTarget(anim, brush);
      Storyboard.SetTargetProperty(anim, new PropertyPath(SolidColorBrush.ColorProperty));
      sb.Children.Add(anim);
    }
    sb.Begin();
  }

  private void AnimateSlideOutIn(Random rnd)
  {
    var sb = new Storyboard();
    double x = (rnd.Next(2) == 0 ? 1 : -1) * (rnd.NextDouble() * 400 + 200);
    var ax = new DoubleAnimation(x, TimeSpan.FromMilliseconds(300)) { EasingFunction = new CubicEase() };
    var axBack = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200)) { BeginTime = TimeSpan.FromMilliseconds(300), EasingFunction = new BounceEase { Bounces = 2, Bounciness = 2 } };
    Storyboard.SetTarget(ax, CopyrightText);
    Storyboard.SetTargetProperty(ax, new PropertyPath("RenderTransform.Children[2].X"));
    Storyboard.SetTarget(axBack, CopyrightText);
    Storyboard.SetTargetProperty(axBack, new PropertyPath("RenderTransform.Children[2].X"));
    sb.Children.Add(ax); sb.Children.Add(axBack);
    sb.Begin();
  }

  private void AnimateBounce()
  {
    var sb = new Storyboard();
    var ay = new DoubleAnimation(-60, TimeSpan.FromMilliseconds(180)) { EasingFunction = new QuadraticEase() };
    var ayBack = new DoubleAnimation(0, TimeSpan.FromMilliseconds(320)) { BeginTime = TimeSpan.FromMilliseconds(180), EasingFunction = new BounceEase { Bounces = 2, Bounciness = 2 } };
    Storyboard.SetTarget(ay, CopyrightText);
    Storyboard.SetTargetProperty(ay, new PropertyPath("RenderTransform.Children[2].Y"));
    Storyboard.SetTarget(ayBack, CopyrightText);
    Storyboard.SetTargetProperty(ayBack, new PropertyPath("RenderTransform.Children[2].Y"));
    sb.Children.Add(ay); sb.Children.Add(ayBack);
    sb.Begin();
  }

  private void AnimateSquashStretch()
  {
    var sb = new Storyboard();
    var ax = new DoubleAnimation(1.6, TimeSpan.FromMilliseconds(120)) { EasingFunction = new SineEase() };
    var ay = new DoubleAnimation(0.6, TimeSpan.FromMilliseconds(120)) { EasingFunction = new SineEase() };
    var axBack = new DoubleAnimation(0.6, TimeSpan.FromMilliseconds(120)) { BeginTime = TimeSpan.FromMilliseconds(120), EasingFunction = new SineEase() };
    var ayBack = new DoubleAnimation(1.6, TimeSpan.FromMilliseconds(120)) { BeginTime = TimeSpan.FromMilliseconds(120), EasingFunction = new SineEase() };
    var axReset = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(120)) { BeginTime = TimeSpan.FromMilliseconds(240), EasingFunction = new SineEase() };
    var ayReset = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(120)) { BeginTime = TimeSpan.FromMilliseconds(240), EasingFunction = new SineEase() };
    Storyboard.SetTarget(ax, CopyrightText);
    Storyboard.SetTargetProperty(ax, new PropertyPath("RenderTransform.Children[0].ScaleX"));
    Storyboard.SetTarget(ay, CopyrightText);
    Storyboard.SetTargetProperty(ay, new PropertyPath("RenderTransform.Children[0].ScaleY"));
    Storyboard.SetTarget(axBack, CopyrightText);
    Storyboard.SetTargetProperty(axBack, new PropertyPath("RenderTransform.Children[0].ScaleX"));
    Storyboard.SetTarget(ayBack, CopyrightText);
    Storyboard.SetTargetProperty(ayBack, new PropertyPath("RenderTransform.Children[0].ScaleY"));
    Storyboard.SetTarget(axReset, CopyrightText);
    Storyboard.SetTargetProperty(axReset, new PropertyPath("RenderTransform.Children[0].ScaleX"));
    Storyboard.SetTarget(ayReset, CopyrightText);
    Storyboard.SetTargetProperty(ayReset, new PropertyPath("RenderTransform.Children[0].ScaleY"));
    sb.Children.Add(ax); sb.Children.Add(ay); sb.Children.Add(axBack); sb.Children.Add(ayBack); sb.Children.Add(axReset); sb.Children.Add(ayReset);
    sb.Begin();
  }

  private void AnimateFade()
  {
    var sb = new Storyboard();
    var aOut = new DoubleAnimation(0.2, TimeSpan.FromMilliseconds(180)) { EasingFunction = new SineEase() };
    var aIn = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(320)) { BeginTime = TimeSpan.FromMilliseconds(180), EasingFunction = new SineEase() };
    Storyboard.SetTarget(aOut, CopyrightText);
    Storyboard.SetTargetProperty(aOut, new PropertyPath("Opacity"));
    Storyboard.SetTarget(aIn, CopyrightText);
    Storyboard.SetTargetProperty(aIn, new PropertyPath("Opacity"));
    sb.Children.Add(aOut); sb.Children.Add(aIn);
    sb.Begin();
  }

  private void AnimateSpiral(Random rnd, double baseX, double baseY)
  {
    var sb = new Storyboard();
    for (int i = 0; i < 8; i++)
    {
      double angle = i * Math.PI / 4;
      double radius = 30 + i * 10;
      double x = baseX + Math.Cos(angle) * radius;
      double y = baseY + Math.Sin(angle) * radius;
      var ax = new DoubleAnimation(x, TimeSpan.FromMilliseconds(60)) { BeginTime = TimeSpan.FromMilliseconds(i * 60) };
      var ay = new DoubleAnimation(y, TimeSpan.FromMilliseconds(60)) { BeginTime = TimeSpan.FromMilliseconds(i * 60) };
      Storyboard.SetTarget(ax, CopyrightText);
      Storyboard.SetTargetProperty(ax, new PropertyPath("RenderTransform.Children[2].X"));
      Storyboard.SetTarget(ay, CopyrightText);
      Storyboard.SetTargetProperty(ay, new PropertyPath("RenderTransform.Children[2].Y"));
      sb.Children.Add(ax); sb.Children.Add(ay);
    }
    var axReset = new DoubleAnimation(0, TimeSpan.FromMilliseconds(120)) { BeginTime = TimeSpan.FromMilliseconds(480) };
    var ayReset = new DoubleAnimation(0, TimeSpan.FromMilliseconds(120)) { BeginTime = TimeSpan.FromMilliseconds(480) };
    Storyboard.SetTarget(axReset, CopyrightText);
    Storyboard.SetTargetProperty(axReset, new PropertyPath("RenderTransform.Children[2].X"));
    Storyboard.SetTarget(ayReset, CopyrightText);
    Storyboard.SetTargetProperty(ayReset, new PropertyPath("RenderTransform.Children[2].Y"));
    sb.Children.Add(axReset); sb.Children.Add(ayReset);
    sb.Begin();
  }

  // Continuous color animation for the copyright text
  private async void Window_Loaded(object sender, RoutedEventArgs e)
  {
    var brush = new SolidColorBrush(DarkVividColors[0]);
    CopyrightText.Foreground = brush;
    var rnd = new Random();

    while (IsVisible)
    {
      var from = brush.Color;
      var to = DarkVividColors[rnd.Next(DarkVividColors.Length)];
      var anim = new ColorAnimation(from, to, new Duration(TimeSpan.FromSeconds(rnd.NextDouble() * 1.5 + 1.0)))
      {
        EasingFunction = new SineEase()
      };
      brush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
      await Task.Delay((int)(1000 + rnd.NextDouble() * 1200));
    }
  }

  private void OnOkClick(object sender, RoutedEventArgs e) => Close();

  private void OnLinkClick(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
  {
    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
    e.Handled = true;
  }

  private void OnCopyInfoClick(object sender, RoutedEventArgs e)
  {
    var info = $"{VersionText.Text}\n{BranchText.Text}\n{ShaText.Text}\n{DateText.Text}\n{SysInfoText.Text}";
    Clipboard.SetText(info);
  }
}
