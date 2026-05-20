using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace LibraryApp.Helpers
{
    public static class TextBoxPlaceholder
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached(
                "Text",
                typeof(string),
                typeof(TextBoxPlaceholder),
                new PropertyMetadata(null, OnChanged));

        public static void SetText(DependencyObject obj, string value)
            => obj.SetValue(TextProperty, value);

        public static string GetText(DependencyObject obj)
            => (string)obj.GetValue(TextProperty);

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox tb)
            {
                tb.Loaded += Tb_Loaded;
                tb.TextChanged += Tb_TextChanged;
                tb.GotKeyboardFocus += Tb_GotKeyboardFocus;
                tb.LostKeyboardFocus += Tb_LostKeyboardFocus;
            }
        }

        private static void Tb_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            Update(sender as TextBox);
        }

        private static void Tb_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            Update(sender as TextBox);
        }

        private static void Tb_Loaded(object sender, RoutedEventArgs e)
        {
            Update(sender as TextBox);
        }

        private static void Tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            Update(sender as TextBox);
        }

        private static void Update(TextBox tb)
        {
            if (tb == null) return;

            var layer = AdornerLayer.GetAdornerLayer(tb);
            if (layer == null) return;

            // удалить старые
            var adorners = layer.GetAdorners(tb);
            if (adorners != null)
            {
                foreach (var a in adorners)
                {
                    if (a is PlaceholderAdorner)
                        layer.Remove(a);
                }
            }

            // ✔ ВАЖНОЕ ИЗМЕНЕНИЕ: скрываем при фокусе или при вводе текста
            if (!tb.IsKeyboardFocused && string.IsNullOrEmpty(tb.Text))
            {
                layer.Add(new PlaceholderAdorner(tb, GetText(tb)));
            }
        }

    }
}
