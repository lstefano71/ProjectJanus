using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Janus.App
{
    public static class MultiSelectBehavior
    {
        public static readonly DependencyProperty SyncedSelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "SyncedSelectedItems",
                typeof(IList),
                typeof(MultiSelectBehavior),
                new PropertyMetadata(null, OnSyncedSelectedItemsChanged));

        public static void SetSyncedSelectedItems(DependencyObject element, IList value)
            => element.SetValue(SyncedSelectedItemsProperty, value);

        public static IList GetSyncedSelectedItems(DependencyObject element)
            => (IList)element.GetValue(SyncedSelectedItemsProperty);

        private static void OnSyncedSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListBox listBox)
            {
                listBox.SelectionChanged -= ListBox_SelectionChanged;
                listBox.SelectionChanged += ListBox_SelectionChanged;
                SyncSelectedItems(listBox);
            }
        }

        private static void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                var synced = GetSyncedSelectedItems(listBox);
                if (synced == null) return;
                synced.Clear();
                foreach (var item in listBox.SelectedItems)
                    synced.Add(item);
            }
        }

        private static void SyncSelectedItems(ListBox listBox)
        {
            var synced = GetSyncedSelectedItems(listBox);
            if (synced == null) return;
            synced.Clear();
            foreach (var item in listBox.SelectedItems)
                synced.Add(item);
        }
    }
}
