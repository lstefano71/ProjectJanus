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

        // Cartwheel is rare (1 in 12), others distributed
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

        double duration = rnd.NextDouble() * 0.7 + 0.6;
        var sb = new Storyboard();

        if (animType == 0) // Move (more daring)
        {
          double x = rnd.NextDouble() * 240 - 120;
          double y = rnd.NextDouble() * 80 - 40;
          var ax = new DoubleAnimation(x, TimeSpan.FromSeconds(duration)) { EasingFunction = new SineEase() };
          var ay = new DoubleAnimation(y, TimeSpan.FromSeconds(duration)) { EasingFunction = new SineEase() };
          Storyboard.SetTarget(ax, CopyrightText);
          Storyboard.SetTargetProperty(ax, new PropertyPath("RenderTransform.Children[2].X"));
          Storyboard.SetTarget(ay, CopyrightText);
          Storyboard.SetTargetProperty(ay, new PropertyPath("RenderTransform.Children[2].Y"));
          sb.Children.Add(ax);
          sb.Children.Add(ay);
        }
        else if (animType == 1) // Rotate (more daring)
        {
          double angle = rnd.NextDouble() * 1080 - 540; // -540° to +540°
          var a = new DoubleAnimation(angle, TimeSpan.FromSeconds(duration)) { EasingFunction = new SineEase() };
          Storyboard.SetTarget(a, CopyrightText);
          Storyboard.SetTargetProperty(a, new PropertyPath("RenderTransform.Children[1].Angle"));
          sb.Children.Add(a);
        }
        else if (animType == 2) // Scale (pulse)
        {
          double scaleTo = rnd.NextDouble() * 1.2 + 0.4; // 0.4 to 1.6
          var ax = new DoubleAnimation(scaleTo, TimeSpan.FromSeconds(duration)) { EasingFunction = new SineEase() };
          var ay = new DoubleAnimation(scaleTo, TimeSpan.FromSeconds(duration)) { EasingFunction = new SineEase() };
          Storyboard.SetTarget(ax, CopyrightText);
          Storyboard.SetTargetProperty(ax, new PropertyPath("RenderTransform.Children[0].ScaleX"));
          Storyboard.SetTarget(ay, CopyrightText);
          Storyboard.SetTargetProperty(ay, new PropertyPath("RenderTransform.Children[0].ScaleY"));
          sb.Children.Add(ax);
          sb.Children.Add(ay);
        }
        else if (animType == 6) // Shiver (quick jitter)
        {
          var sbShiver = new Storyboard();
          for (int i = 0; i < 6; i++)
          {
            double x = (i % 2 == 0 ? 1 : -1) * (rnd.NextDouble() * 20 + 10);
            var ax = new DoubleAnimation(x, TimeSpan.FromMilliseconds(40)) { BeginTime = TimeSpan.FromMilliseconds(i * 40) };
            Storyboard.SetTarget(ax, CopyrightText);
            Storyboard.SetTargetProperty(ax, new PropertyPath("RenderTransform.Children[2].X"));
            sbShiver.Children.Add(ax);
          }
          var axReset = new DoubleAnimation(0, TimeSpan.FromMilliseconds(40)) { BeginTime = TimeSpan.FromMilliseconds(240) };
          Storyboard.SetTarget(axReset, CopyrightText);
          Storyboard.SetTargetProperty(axReset, new PropertyPath("RenderTransform.Children[2].X"));
          sbShiver.Children.Add(axReset);
          sbShiver.Begin();
          return;
        }
        else if (animType == 9) // Wobble (oscillate rotation)
        {
          var sbWobble = new Storyboard();
          double baseAngle = rotate.Angle;
          for (int i = 0; i < 6; i++)
          {
            double angle = baseAngle + Math.Sin(i * Math.PI / 3) * 40;
            var a = new DoubleAnimation(angle, TimeSpan.FromMilliseconds(80)) { BeginTime = TimeSpan.FromMilliseconds(i * 80), EasingFunction = new SineEase() };
            Storyboard.SetTarget(a, CopyrightText);
            Storyboard.SetTargetProperty(a, new PropertyPath("RenderTransform.Children[1].Angle"));
            sbWobble.Children.Add(a);
          }
          var aReset = new DoubleAnimation(baseAngle, TimeSpan.FromMilliseconds(80)) { BeginTime = TimeSpan.FromMilliseconds(480) };
          Storyboard.SetTarget(aReset, CopyrightText);
          Storyboard.SetTargetProperty(aReset, new PropertyPath("RenderTransform.Children[1].Angle"));
          sbWobble.Children.Add(aReset);
          sbWobble.Begin();
          return;
        }
        else if (animType == 12) // Bounce (vertical)
        {
          var sbBounce = new Storyboard();
          var ay = new DoubleAnimation(-60, TimeSpan.FromMilliseconds(180)) { EasingFunction = new QuadraticEase() };
          var ayBack = new DoubleAnimation(0, TimeSpan.FromMilliseconds(320)) { BeginTime = TimeSpan.FromMilliseconds(180), EasingFunction = new BounceEase { Bounces = 2, Bounciness = 2 } };
          Storyboard.SetTarget(ay, CopyrightText);
          Storyboard.SetTargetProperty(ay, new PropertyPath("RenderTransform.Children[2].Y"));
          Storyboard.SetTarget(ayBack, CopyrightText);
          Storyboard.SetTargetProperty(ayBack, new PropertyPath("RenderTransform.Children[2].Y"));
          sbBounce.Children.Add(ay); sbBounce.Children.Add(ayBack);
          sbBounce.Begin();
          return;
        }
        else if (animType == 13) // Squash/stretch (scaleY/scaleX alternately)
        {
          var sbSquash = new Storyboard();
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
          sbSquash.Children.Add(ax); sbSquash.Children.Add(ay); sbSquash.Children.Add(axBack); sbSquash.Children.Add(ayBack); sbSquash.Children.Add(axReset); sbSquash.Children.Add(ayReset);
          sbSquash.Begin();
          return;
        }
        else if (animType == 14) // Fade out/in (opacity pulse)
        {
          var sbFade = new Storyboard();
          var aOut = new DoubleAnimation(0.2, TimeSpan.FromMilliseconds(180)) { EasingFunction = new SineEase() };
          var aIn = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(320)) { BeginTime = TimeSpan.FromMilliseconds(180), EasingFunction = new SineEase() };
          Storyboard.SetTarget(aOut, CopyrightText);
          Storyboard.SetTargetProperty(aOut, new PropertyPath("Opacity"));
          Storyboard.SetTarget(aIn, CopyrightText);
          Storyboard.SetTargetProperty(aIn, new PropertyPath("Opacity"));
          sbFade.Children.Add(aOut); sbFade.Children.Add(aIn);
          sbFade.Begin();
          return;
        }
        else if (animType == 15) // Spiral (move in a spiral path)
        {
          var sbSpiral = new Storyboard();
          double baseX = translate.X;
          double baseY = translate.Y;
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
            sbSpiral.Children.Add(ax); sbSpiral.Children.Add(ay);
          }
          var axReset = new DoubleAnimation(0, TimeSpan.FromMilliseconds(120)) { BeginTime = TimeSpan.FromMilliseconds(480) };
          var ayReset = new DoubleAnimation(0, TimeSpan.FromMilliseconds(120)) { BeginTime = TimeSpan.FromMilliseconds(480) };
          Storyboard.SetTarget(axReset, CopyrightText);
          Storyboard.SetTargetProperty(axReset, new PropertyPath("RenderTransform.Children[2].X"));
          Storyboard.SetTarget(ayReset, CopyrightText);
          Storyboard.SetTargetProperty(ayReset, new PropertyPath("RenderTransform.Children[2].Y"));
          sbSpiral.Children.Add(axReset); sbSpiral.Children.Add(ayReset);
          sbSpiral.Begin();
          return;
        }
        else if (animType == 10) // Rainbow pulse (rapid color cycling)
        {
          var vividColors = new[]
          {
            Colors.Red, Colors.Orange, Colors.Yellow, Colors.LimeGreen, Colors.Green,
            Colors.Cyan, Colors.DeepSkyBlue, Colors.Blue, Colors.MediumPurple,
            Colors.Magenta, Colors.HotPink, Colors.Gold, Colors.Turquoise
          };
          var brush = CopyrightText.Foreground as SolidColorBrush;
          var sbRainbow = new Storyboard();
          for (int i = 0; i < 6; i++)
          {
            var to = vividColors[rnd.Next(vividColors.Length)];
            var anim = new ColorAnimation(to, TimeSpan.FromMilliseconds(100)) { BeginTime = TimeSpan.FromMilliseconds(i * 100) };
            Storyboard.SetTarget(anim, brush);
            Storyboard.SetTargetProperty(anim, new PropertyPath(SolidColorBrush.ColorProperty));
            sbRainbow.Children.Add(anim);
          }
          sbRainbow.Begin();
          return;
        }
        else if (animType == 11) // Slide out and snap back
        {
          var sbSlide = new Storyboard();
          double x = (rnd.Next(2) == 0 ? 1 : -1) * (rnd.NextDouble() * 400 + 200);
          var ax = new DoubleAnimation(x, TimeSpan.FromMilliseconds(300)) { EasingFunction = new CubicEase() };
          var axBack = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200)) { BeginTime = TimeSpan.FromMilliseconds(300), EasingFunction = new BounceEase { Bounces = 2, Bounciness = 2 } };
          Storyboard.SetTarget(ax, CopyrightText);
          Storyboard.SetTargetProperty(ax, new PropertyPath("RenderTransform.Children[2].X"));
          Storyboard.SetTarget(axBack, CopyrightText);
          Storyboard.SetTargetProperty(axBack, new PropertyPath("RenderTransform.Children[2].X"));
          sbSlide.Children.Add(ax); sbSlide.Children.Add(axBack);
          sbSlide.Begin();
          return;
        }
        else if (animType == 7) // Pop (scale up and bounce back)
        {
          var sbPop = new Storyboard();
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
          sbPop.Children.Add(ax); sbPop.Children.Add(ay); sbPop.Children.Add(axBack); sbPop.Children.Add(ayBack);
          sbPop.Begin();
          return;
        }
        else if (animType == 8) // Flip (scaleX to -1 and back)
        {
          var sbFlip = new Storyboard();
          var ax = new DoubleAnimation(-1.0, TimeSpan.FromMilliseconds(180)) { EasingFunction = new SineEase() };
          var axBack = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(180)) { BeginTime = TimeSpan.FromMilliseconds(180), EasingFunction = new SineEase() };
          Storyboard.SetTarget(ax, CopyrightText);
          Storyboard.SetTargetProperty(ax, new PropertyPath("RenderTransform.Children[0].ScaleX"));
          Storyboard.SetTarget(axBack, CopyrightText);
          Storyboard.SetTargetProperty(axBack, new PropertyPath("RenderTransform.Children[0].ScaleX"));
          sbFlip.Children.Add(ax); sbFlip.Children.Add(axBack);
          sbFlip.Begin();
          return;
        }
        else if (animType == 3) // Combo (more daring)
        {
          double x = rnd.NextDouble() * 240 - 120;
          double y = rnd.NextDouble() * 80 - 40;
          double angle = rnd.NextDouble() * 1080 - 540;
          double scaleTo = rnd.NextDouble() * 1.2 + 0.4;
          var ax = new DoubleAnimation(x, TimeSpan.FromSeconds(duration)) { EasingFunction = new SineEase() };
          var ay = new DoubleAnimation(y, TimeSpan.FromSeconds(duration)) { EasingFunction = new SineEase() };
          var arot = new DoubleAnimation(angle, TimeSpan.FromSeconds(duration)) { EasingFunction = new SineEase() };
          var asx = new DoubleAnimation(scaleTo, TimeSpan.FromSeconds(duration)) { EasingFunction = new SineEase() };
          var asy = new DoubleAnimation(scaleTo, TimeSpan.FromSeconds(duration)) { EasingFunction = new SineEase() };
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
          return;
        }
        else if (animType == 4) // Cartwheel (rapid full rotation, rare)
        {
          double angle = rotate.Angle + (rnd.Next(2) == 0 ? 360 : -360) * (rnd.Next(2) + 1); // 360° or 720° in either direction
          var a = new DoubleAnimation(angle, TimeSpan.FromSeconds(duration * 0.7)) { EasingFunction = new CubicEase() };
          Storyboard.SetTarget(a, CopyrightText);
          Storyboard.SetTargetProperty(a, new PropertyPath("RenderTransform.Children[1].Angle"));
          sb.Children.Add(a);
          return;
        }
        else if (animType == 5) // Barrel distortion (scale X and Y in opposite directions)
        {
          double sx = rnd.NextDouble() * 1.2 + 0.4; // 0.4 to 1.6
          double sy = 2.0 - sx; // Opposite
          var ax = new DoubleAnimation(sx, TimeSpan.FromSeconds(duration)) { EasingFunction = new SineEase() };
          var ay = new DoubleAnimation(sy, TimeSpan.FromSeconds(duration)) { EasingFunction = new SineEase() };
          Storyboard.SetTarget(ax, CopyrightText);
          Storyboard.SetTargetProperty(ax, new PropertyPath("RenderTransform.Children[0].ScaleX"));
          Storyboard.SetTarget(ay, CopyrightText);
          Storyboard.SetTargetProperty(ay, new PropertyPath("RenderTransform.Children[0].ScaleY"));
          sb.Children.Add(ax);
          sb.Children.Add(ay);
          return;
        }

        sb.Begin();
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

  // Continuous color animation for the copyright text
  private async void Window_Loaded(object sender, RoutedEventArgs e)
  {
    var vividColors = new[]
    {
      Colors.Red, Colors.Orange, Colors.Yellow, Colors.LimeGreen, Colors.Green,
      Colors.Cyan, Colors.DeepSkyBlue, Colors.Blue, Colors.MediumPurple,
      Colors.Magenta, Colors.HotPink, Colors.Gold, Colors.Turquoise
    };
    var brush = new SolidColorBrush(Colors.Black);
    CopyrightText.Foreground = brush;
    var rnd = new Random();

    while (IsVisible)
    {
      var from = brush.Color;
      var to = vividColors[rnd.Next(vividColors.Length)];
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
