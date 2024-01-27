using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace PixelRuler.CanvasElements
{
    public abstract class AbstractZoomCanvasShape : IZoomCanvasShape
    {
        protected Canvas owningCanvas;
        public AbstractZoomCanvasShape(Canvas owningCanvas)
        {
            this.owningCanvas = owningCanvas;
        }

        protected double getSinglePixelUISize()
        {
            // we need to perform dpi scaling here bc our parent undid dpi scaling
            var dpi = owningCanvas.GetDpi();
            return dpi / this.owningCanvas.GetScaleTransform().ScaleX;
        }

        protected Line createLine()
        {
            var line = new Line();
            line.StrokeThickness = 1;
            line.SnapsToDevicePixels = true;
            return line;
        }

        protected Rectangle createRectangle()
        {
            var rect = new Rectangle();
            rect.Width = 0;
            rect.Height = 0;
            rect.StrokeThickness = 1;
            rect.SnapsToDevicePixels = true;
            return rect;
        }

        public abstract void Clear();
        public abstract void UpdateForZoomChange();
    }
}
