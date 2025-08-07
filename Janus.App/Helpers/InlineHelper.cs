using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Janus.App.Helpers
{
  public static class InlineHelper
  {
    public static readonly DependencyProperty InlinesSourceProperty = DependencyProperty.RegisterAttached(
        "InlinesSource",
        typeof(IEnumerable<Inline>),
        typeof(InlineHelper),
        new PropertyMetadata(null, OnInlinesSourceChanged));

    public static void SetInlinesSource(DependencyObject element, IEnumerable<Inline> value)
        => element.SetValue(InlinesSourceProperty, value);

    public static IEnumerable<Inline> GetInlinesSource(DependencyObject element)
        => (IEnumerable<Inline>)element.GetValue(InlinesSourceProperty);

    private static void OnInlinesSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is TextBlock textBlock) {
        textBlock.Inlines.Clear();
        if (e.NewValue is IEnumerable<Inline> inlines) {
          foreach (var inline in inlines)
            textBlock.Inlines.Add(inline);
        }
      }
    }
  }
}
