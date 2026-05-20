using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace LibraryApp.Helpers
{
    public static class ClickClearFocusBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(ClickClearFocusBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);
        public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                if ((bool)e.NewValue)
                    element.PreviewMouseLeftButtonUp += OnPreviewMouseUp;
                else
                    element.PreviewMouseLeftButtonUp -= OnPreviewMouseUp;
            }
        }

        private static void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Window window) || !window.IsLoaded) return;

            var source = e.OriginalSource as DependencyObject;
            if (source == null) return;

            if (IsBackgroundClick(source))
            {
                window.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (Keyboard.FocusedElement != null)
                        Keyboard.ClearFocus();

                    var scope = FocusManager.GetFocusScope(window);
                    if (scope != null)
                        FocusManager.SetFocusedElement(scope, null);

                    ClearAllSelectors(window);
                }), System.Windows.Threading.DispatcherPriority.Normal);
            }
        }

        private static bool IsBackgroundClick(DependencyObject source)
        {
            var current = source;
            while (current != null && !(current is Window))
            {
                // Элементы, клик по которым НЕ должен сбрасывать выделение
                if (current is DataGridRow || current is DataGridCell || current is DataGridColumnHeader ||
                    current is ListBoxItem || current is TreeViewItem || current is TabItem || current is MenuItem ||
                    current is ButtonBase || current is TextBoxBase || current is PasswordBox ||
                    current is ComboBox || current is DatePicker || current is Calendar ||
                    current is Hyperlink || current is ScrollBar || current is Thumb || current is RepeatButton)
                {
                    return false;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return true;
        }

        private static void ClearAllSelectors(Window window)
        {
            var queue = new Queue<DependencyObject>();
            queue.Enqueue(window);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                int count = VisualTreeHelper.GetChildrenCount(current);

                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    if (child == null) continue;

                    if (child is Selector selector && selector.IsLoaded && !(selector is TabControl))
                    {
                        // Исключаем редактируемые ComboBox — они хранят пользовательский ввод в Text
                        if (!(selector is ComboBox comboBox && comboBox.IsEditable))
                        {
                            selector.SelectedItem = null;
                        }
                    }

                    queue.Enqueue(child);
                }
            }
        }
    }
}