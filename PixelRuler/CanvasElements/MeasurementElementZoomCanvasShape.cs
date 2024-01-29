using System;
using System.Transactions;
using System.Windows;
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

        public abstract bool FinishedDrawing { get; set; }

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

        public Point StartPoint { get; protected set; }
        public Point EndPoint { get; protected set; } = new Point(double.NaN, double.NaN);

        public abstract void SetState();

        public void Move(int deltaX, int deltaY)
        {
            // this can just be the base method and non virtual TODO
            this.StartPoint = new Point(this.StartPoint.X + deltaX, this.StartPoint.Y + deltaY);
            this.EndPoint = new Point(this.EndPoint.X + deltaX, this.EndPoint.Y + deltaY);
            this.SetState();
        }

        public void MoveStartPoint(int deltaX, int deltaY)
        {
            this.StartPoint = new Point(this.StartPoint.X + deltaX, this.StartPoint.Y + deltaY);
            this.SetState();
        }

        public void MoveEndPoint(int deltaX, int deltaY)
        {
            this.EndPoint = new Point(this.EndPoint.X + deltaX, this.EndPoint.Y + deltaY);
            this.SetState();
        }

        public abstract void SetSelectedState();

        public void OnMoving(System.Windows.Point pt)
        {
            Moving?.Invoke(this, pt);
        }

        public event EventHandler<System.Windows.Point> Moving;

    }
}