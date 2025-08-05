using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Janus.App;

public partial class ResultsView : UserControl
{
    private ResultsViewModel? ViewModel => DataContext as ResultsViewModel;
    private Window? parentWindow;
    private GridSplitter? resultsSplitter;
    private GridSplitter? detailsSplitter;

    public ResultsView()
    {
        InitializeComponent();
        Loaded += ResultsView_Loaded;
    }

    private void ResultsView_Loaded(object sender, RoutedEventArgs e)
    {
        parentWindow = Window.GetWindow(this);
        if (parentWindow != null)
        {
            parentWindow.SizeChanged += ParentWindow_SizeChanged;
        }
        // Find splitters
        resultsSplitter = FindSplitter("GridSplitter", 0);
        detailsSplitter = FindSplitter("GridSplitter", 1);
        if (resultsSplitter != null)
            resultsSplitter.DragDelta += ResultsSplitter_DragDelta;
        if (detailsSplitter != null)
            detailsSplitter.DragDelta += DetailsSplitter_DragDelta;
    }

    private void ParentWindow_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        // No longer update ViewModel with window size
    }

    private void ResultsSplitter_DragDelta(object? sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
    {
        if (ViewModel != null)
        {
            // Calculate relative position (column 0/1 width vs total)
            var grid = resultsSplitter?.Parent as Grid;
            if (grid != null)
            {
                double totalWidth = grid.ActualWidth;
                double leftWidth = grid.ColumnDefinitions[0].ActualWidth + grid.ColumnDefinitions[1].ActualWidth;
                ViewModel.ResultsSplitterPosition = leftWidth / totalWidth;
            }
        }
    }

    private void DetailsSplitter_DragDelta(object? sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
    {
        if (ViewModel != null)
        {
            // Calculate relative position (column 0 width vs total)
            var grid = detailsSplitter?.Parent as Grid;
            if (grid != null)
            {
                double totalWidth = grid.ActualWidth;
                double leftWidth = grid.ColumnDefinitions[0].ActualWidth;
                ViewModel.DetailsSplitterPosition = leftWidth / totalWidth;
            }
        }
    }

    private GridSplitter? FindSplitter(string name, int index)
    {
        // Find the nth GridSplitter in the visual tree
        int count = 0;
        foreach (var child in LogicalTreeHelper.GetChildren(this))
        {
            if (child is Grid grid)
            {
                foreach (var element in grid.Children)
                {
                    if (element is GridSplitter splitter)
                    {
                        if (count == index)
                            return splitter;
                        count++;
                    }
                }
            }
        }
        return null;
    }
}
