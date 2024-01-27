using System.Windows.Controls;

namespace PixelRuler.CanvasElements
{
    public abstract class MeasurementElementZoomCanvasShape : AbstractZoomCanvasShape
    {
        protected MeasurementElementZoomCanvasShape(Canvas owningCanvas) : base(owningCanvas)
        {
        }

        public abstract void SetEndPoint(System.Windows.Point roundedPoint);

        public abstract bool IsEmpty { get; }
    }
}