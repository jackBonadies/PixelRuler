using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
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

        /// <summary>
        /// This is so that UI elements are dpi scaled.  i.e. a circle appears twice as big on a 200% dpi screen.
        /// </summary>
        /// <returns></returns>
        protected double getUIUnit()
        {
            // we need to perform dpi scaling here bc our parent undid dpi scaling
            var dpi = owningCanvas.GetDpi();
            return dpi / this.owningCanvas.GetScaleTransform().ScaleX;
        }

        /// <summary>
        /// This is so that UI elements are dpi independent.  i.e. a line thickness of 1 is always 1.
        /// </summary>
        /// <returns></returns>
        protected double getUIStrokeThicknessUnit()
        {
            return 1 / this.owningCanvas.GetScaleTransform().ScaleX;
        }

        public abstract void Clear();
        public abstract void UpdateForZoomChange();
    }
}
