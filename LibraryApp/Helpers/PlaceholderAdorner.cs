using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;

namespace LibraryApp.Helpers
{
    public class PlaceholderAdorner : Adorner
    {
        private readonly TextBlock _textBlock;

        public PlaceholderAdorner(UIElement element, string text)
            : base(element)
        {
            _textBlock = new TextBlock
            {
                Text = text,
                Foreground = Brushes.Gray,
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false,
                FontSize = 14
            };
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            _textBlock.Arrange(new Rect(new Point(0, 0), AdornedElement.RenderSize));
        }

        protected override Visual GetVisualChild(int index) => _textBlock;
        protected override int VisualChildrenCount => 1;
    }
}