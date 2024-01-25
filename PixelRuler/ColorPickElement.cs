using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PixelRuler
{
    class ColorPickElement : IZoomCanvasShape
    {
        Rectangle rect;

        public ColorPickElement(Canvas canvas) 
        {
            this.owningCanvas = canvas;
            this.rect = createRectangle();
            this.rect.Stroke = new SolidColorBrush(Colors.Red);
            owningCanvas.Children.Add(rect);
            Canvas.SetZIndex(rect, 503);
        }

        private double getBoundingBoxStrokeThickness()
        {
            // we need to perform dpi scaling here bc our parent undid dpi scaling
            var dpi = owningCanvas.GetDpi();
            return dpi / this.owningCanvas.GetScaleTransform().ScaleX;
        }

        private Rectangle createRectangle()
        {
            var rect = new Rectangle();
            rect.Width = 1;
            rect.Height = 1;
            rect.StrokeThickness = 1;
            rect.SnapsToDevicePixels = true;
            return rect;
        }

        Canvas owningCanvas;

        public void UpdateForZoomChange()
        {
            rect.Width = 1 + 1 * getBoundingBoxStrokeThickness();
            rect.Height = 1 + 1 * getBoundingBoxStrokeThickness();
            //rect.LayoutTransform = new TranslateTransform(-.5 * getBoundingBoxStrokeThickness(), -.5 * getBoundingBoxStrokeThickness());
            rect.StrokeThickness = getBoundingBoxStrokeThickness();
            //var st = BoundingBoxLabel.RenderTransform as ScaleTransform;
            //st.ScaleX = 1.0 / this.owningCanvas.GetScaleTransform().ScaleX;
            //st.ScaleY = 1.0 / this.owningCanvas.GetScaleTransform().ScaleY;
        }

        public void Clear()
        {
            this.owningCanvas.Children.Remove(rect);
            //this.owningCanvas.Children.Remove(BoundingBoxLabel);
        }

        internal void SetPosition(Point point)
        {
            Canvas.SetLeft(rect, point.X);
            Canvas.SetTop(rect, point.Y);
        }
    }
}
