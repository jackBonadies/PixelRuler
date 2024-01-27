using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Security.Policy;

namespace PixelRuler.CanvasElements
{
    class RulerElement : MeasurementElementZoomCanvasShape
    {
        Line line1;
        Line lineBeginCap;
        Line lineEndCap;
        RulerLengthLabel rulerLengthLabel;

        public RulerElement(Canvas canvas, Point startPoint) : base(canvas) 
        {
            line1 = createLine();
            line1.Stroke = new SolidColorBrush(Colors.Red);

            this.owningCanvas.Children.Add(line1);
            Canvas.SetZIndex(line1, 500);

            lineBeginCap = createLine();
            lineBeginCap.Stroke = new SolidColorBrush(Colors.Red);

            this.owningCanvas.Children.Add(lineBeginCap);
            Canvas.SetZIndex(lineBeginCap, 500);

            lineEndCap = createLine();
            lineEndCap.Stroke = new SolidColorBrush(Colors.Red);

            this.owningCanvas.Children.Add(lineEndCap);
            Canvas.SetZIndex(lineEndCap, 500);

            StartPoint = startPoint;

            rulerLengthLabel = new RulerLengthLabel();
            rulerLengthLabel.RenderTransform = new ScaleTransform() { ScaleX = 1.0, ScaleY = 1.0 };

            this.owningCanvas.Children.Add(rulerLengthLabel);
            Canvas.SetZIndex(rulerLengthLabel, 500);

            UpdateForZoomChange();
        }

        public Point StartPoint { get; private set; }


        private Point endPoint = new Point(double.NaN, double.NaN);
        public Point EndPoint
        {
            get
            {
                return endPoint;
            }
            set
            {
                endPoint = value;
                SetShapeState();
                SetLabelState();
            }
        }

        private bool isHorizontal()
        {
            var xDist = Math.Abs(endPoint.X - StartPoint.X);
            var yDist = Math.Abs(endPoint.Y - StartPoint.Y);

            if(xDist > yDist)
            {
                return true;
            }
            return false;

        }

        private void SetShapeState()
        {
            if(double.IsNaN(endPoint.X))
            {
                return;
            }

            line1.X1 = StartPoint.X;
            line1.Y1 = StartPoint.Y;


            if(isHorizontal())
            {
                line1.X2 = endPoint.X;
                line1.Y2 = line1.Y1;

                lineBeginCap.X1 = lineBeginCap.X2 = line1.X1;
                lineEndCap.X1 = lineEndCap.X2 = line1.X2;

                lineBeginCap.Y1 = lineEndCap.Y1 = line1.Y1 + getEndCapsSize();
                lineBeginCap.Y2 = lineEndCap.Y2 = line1.Y1 - getEndCapsSize();
            }
            else
            {
                line1.X2 = line1.X1;
                line1.Y2 = endPoint.Y;

                lineBeginCap.Y1 = lineBeginCap.Y2 = line1.Y1;
                lineEndCap.Y1 = lineEndCap.Y2 = line1.Y2;

                lineBeginCap.X1 = lineEndCap.X1 = line1.X1 + getEndCapsSize();
                lineBeginCap.X2 = lineEndCap.X2 = line1.X1 - getEndCapsSize();
            }
        }


        private void SetLabelState()
        {
            if (double.IsNaN(endPoint.X))
            {
                return;
            }

            if (isHorizontal())
            {
                Canvas.SetTop(rulerLengthLabel, line1.Y1 - (rulerLengthLabel.ActualHeight + 2) * getSinglePixelUISize());
                Canvas.SetLeft(rulerLengthLabel, (line1.X1 + line1.X2) / 2.0 - rulerLengthLabel.ActualWidth * getSinglePixelUISize() / 2.0  );// - BoundingBoxLabel.ActualWidth * 1.5);
            }
            else
            {
                Canvas.SetLeft(rulerLengthLabel, line1.X1 +  2 * getSinglePixelUISize());
                Canvas.SetTop(rulerLengthLabel, (line1.Y1 + line1.Y2) / 2.0 - rulerLengthLabel.ActualHeight * getSinglePixelUISize() / 2.0  );// - BoundingBoxLabel.ActualWidth * 1.5);
            }
            rulerLengthLabel.Extent = (int)Extent;
        }

        private double getEndCapsSize()
        {
            if(endcapsInUIUnits)
            {
                return 8 * getSinglePixelUISize();
            }
            else
            {
                return 8;
            }
        }

        bool endcapsInUIUnits = true;

        public override void UpdateForZoomChange()
        {
            line1.StrokeThickness = getSinglePixelUISize();
            lineBeginCap.StrokeThickness = getSinglePixelUISize();
            lineEndCap.StrokeThickness = getSinglePixelUISize();

            var st = rulerLengthLabel.RenderTransform as ScaleTransform;
            st.ScaleX = 1.0 / this.owningCanvas.GetScaleTransform().ScaleX;
            st.ScaleY = 1.0 / this.owningCanvas.GetScaleTransform().ScaleY;

            SetShapeState();
            SetLabelState();
        }

        public override void Clear()
        {
            this.owningCanvas.Children.Remove(line1);
            this.owningCanvas.Children.Remove(lineBeginCap);
            this.owningCanvas.Children.Remove(lineEndCap);
            this.owningCanvas.Children.Remove(rulerLengthLabel);
        }

        public override void SetEndPoint(Point roundedPoint)
        {
            this.EndPoint = roundedPoint;
        }

        public double Extent
        {
            get
            {
                return isHorizontal() ? Math.Abs(line1.X1 - line1.X2) : Math.Abs(line1.Y1 - line1.Y2);
            }
        }

        public override bool IsEmpty
        {
            get
            {
                return this.Extent == 0;
            }
        }
    }
}
