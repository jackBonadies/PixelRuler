using PixelRuler.CanvasElements;
using PixelRuler.Common;
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
    class ColorPickElement : AbstractZoomCanvasShape 
    {
        Rectangle rect;

        public ColorPickElement(Canvas canvas)  : base(canvas)
        {
            this.rect = UiUtils.CreateRectangle();
            this.rect.Stroke = new SolidColorBrush(Colors.Red);
            owningCanvas.Children.Add(rect);
            Canvas.SetZIndex(rect, App.COLOR_PICKER_INDEX);
        }


        public override void UpdateForZoomChange()
        {
            rect.Width = 1 + 1 * getUIUnit();
            rect.Height = 1 + 1 * getUIUnit();
            //rect.LayoutTransform = new TranslateTransform(-.5 * getBoundingBoxStrokeThickness(), -.5 * getBoundingBoxStrokeThickness());
            rect.StrokeThickness = getUIUnit();
            //var st = BoundingBoxLabel.RenderTransform as ScaleTransform;
            //st.ScaleX = 1.0 / this.owningCanvas.GetScaleTransform().ScaleX;
            //st.ScaleY = 1.0 / this.owningCanvas.GetScaleTransform().ScaleY;
        }

        public override void Clear()
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
