using PixelRuler.Common;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PixelRuler.CanvasElements
{
    // TODO dead code
    public class ScreenshotBoxSelectionElement : AbstractZoomCanvasShape
    {
        private Rectangle rect;

        public ScreenshotBoxSelectionElement(Canvas owningCanvas) : base(owningCanvas)
        {
            rect = UiUtils.CreateRectangle();
            rect.Stroke = new SolidColorBrush(Colors.Red);
            rect.StrokeThickness = getUIStrokeThicknessUnit();
        }

        public void SetPosition(MouseEventArgs e)
        {
            var canvasPosition = UiUtils.RoundPoint(e.GetPosition(owningCanvas));
            var startX = Math.Min(startPos.X, canvasPosition.X);
            var startY = Math.Min(startPos.Y, canvasPosition.Y);

            var endX = Math.Max(startPos.X, canvasPosition.X);
            var endY = Math.Max(startPos.Y, canvasPosition.Y);

            Canvas.SetLeft(rect, startX);
            Canvas.SetTop(rect, startY);
            rect.Width = endX - startX;
            rect.Height = endY - startY;
        }

        public void SetStartPosition(MouseEventArgs e)
        {
            startPos = UiUtils.RoundPoint(e.GetPosition(owningCanvas));
        }

        private Point startPos;

        public bool Visible
        {
            get
            {
                return rect.Visibility == System.Windows.Visibility.Visible;
            }
            set
            {
                var visibility = value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                rect.Visibility = visibility;
            }
        }

        public override void AddToOwnerCanvas()
        {
            owningCanvas.Children.Add(rect);
        }

        public override void Clear()
        {
            this.owningCanvas.Children.Remove(rect);
        }

        public override void UpdateForZoomChange()
        {
            rect.StrokeThickness = getUIUnit();
        }

    }
}
