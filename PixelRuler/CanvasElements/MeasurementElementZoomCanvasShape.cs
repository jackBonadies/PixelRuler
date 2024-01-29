using System;
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

        public bool FinishedDrawing { get; set; } = false;

        public event EventHandler<EventArgs>? SelectedChanged;

        private bool selected = false;
        public bool Selected
        {
            get 
            { 
                return selected; 
            }
            set
            {
                if (value != selected) 
                {
                    selected = value;
                    SetSelectedState();
                    SelectedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public abstract void SetSelectedState();

        public virtual void Move(int v1, int v2)
        {
            //this.SetEndPoint();
        }
    }
}