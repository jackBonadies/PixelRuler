using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PixelRuler.CanvasElements
{
    /// <summary>
    /// Drawn between guideline elements
    /// </summary>
    public class DistanceIndicator : AbstractZoomCanvasShape
    {
        Line lineBodyBegin;
        Line lineBodyEnd;
        Line lineStart1;
        Line lineStart2;
        Line lineEnd1;
        Line lineEnd2;
        SubtleLengthLabel lengthLabel;

        public DistanceIndicator(Canvas owningCanvas, bool isHorizontal) : base(owningCanvas)
        {
            lineBodyBegin = GetLine();
            lineBodyBegin.StrokeDashArray = new DoubleCollection(new double[] { 2, 4 });
            lineBodyEnd = GetLine();
            lineBodyEnd.StrokeDashArray = new DoubleCollection(new double[] { 2, 4 });
            lineStart1 = GetLine();
            lineStart2 = GetLine();
            lineEnd1 = GetLine();
            lineEnd2 = GetLine();
            lengthLabel = new SubtleLengthLabel();
            IsHorizontal = isHorizontal;
            this.UpdateForZoomChange();
        }

        public Point StartPoint { get; private set; }
        public Point EndPoint { get; private set; }
        public bool IsHorizontal { get; private set; }

        private void SetDistance()
        {
            SetDistance(StartPoint, EndPoint);
        }

        public void SetDistance(Point startPoint, Point endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;

            double xPadding = IsHorizontal ? getPadding() : 0;
            double yPadding = IsHorizontal ? 0 : getPadding();

            lineBodyBegin.X1 = StartPoint.X + xPadding;
            lineBodyBegin.Y1 = StartPoint.Y + yPadding;
            lineBodyBegin.X2 = EndPoint.X - xPadding;
            lineBodyBegin.Y2 = EndPoint.Y - yPadding;

            lineBodyEnd.X1 = StartPoint.X + xPadding;
            lineBodyEnd.Y1 = StartPoint.Y + yPadding;
            lineBodyEnd.X2 = EndPoint.X - xPadding;
            lineBodyEnd.Y2 = EndPoint.Y - yPadding;

            lineStart1.X1 = StartPoint.X + xPadding;
            lineStart1.Y1 = StartPoint.Y + yPadding;
            lineStart1.X2 = StartPoint.X + getArrowDistance() + xPadding;
            lineStart1.Y2 = StartPoint.Y + getArrowDistance() + yPadding;
            if (IsHorizontal)
            {
                lineStart1.Y2 = StartPoint.Y + getArrowDistance() + yPadding;
            }
            else
            {
                lineStart1.Y2 = StartPoint.Y + getArrowDistance() + yPadding;
            }

            lineStart2.X1 = StartPoint.X + xPadding;
            lineStart2.Y1 = StartPoint.Y + yPadding;
            if (IsHorizontal)
            {
                lineStart2.X2 = StartPoint.X + getArrowDistance() + xPadding;
                lineStart2.Y2 = StartPoint.Y - getArrowDistance();
            }
            else
            {
                lineStart2.X2 = StartPoint.X - getArrowDistance();
                lineStart2.Y2 = StartPoint.Y + getArrowDistance() + yPadding;
            }

            lineEnd1.X1 = EndPoint.X - xPadding;
            lineEnd1.Y1 = EndPoint.Y - yPadding;
            if (IsHorizontal)
            {
                lineEnd1.X2 = EndPoint.X - getArrowDistance() - xPadding;
                lineEnd1.Y2 = EndPoint.Y + getArrowDistance() - yPadding;
            }
            else
            {
                lineEnd1.X2 = EndPoint.X + getArrowDistance() - xPadding;
                lineEnd1.Y2 = EndPoint.Y - getArrowDistance() - yPadding;
            }

            lineEnd2.X1 = EndPoint.X - xPadding;
            lineEnd2.Y1 = EndPoint.Y - yPadding;
            lineEnd2.X2 = EndPoint.X - getArrowDistance() - xPadding;
            lineEnd2.Y2 = EndPoint.Y - getArrowDistance() - yPadding;

            if (IsHorizontal)
            {
                lengthLabel.Length = (int)(EndPoint.X - StartPoint.X);
            }
            else
            {
                lengthLabel.Length = (int)(EndPoint.Y - StartPoint.Y);
            }

            lengthLabel.Measure(new Size(double.MaxValue, double.MaxValue)); // TODO UiUtils readonly MaxSize
            double desiredWidth = lengthLabel.DesiredSize.Width;
            double desiredHeight = lengthLabel.DesiredSize.Height;
            if (IsHorizontal)
            {
                Canvas.SetLeft(lengthLabel, (lineBodyBegin.X1 + lineBodyBegin.X2) / 2.0 - desiredWidth / 2.0);
                Canvas.SetTop(lengthLabel, lineBodyBegin.Y1 - desiredHeight / 2.0);

                lineBodyBegin.X1 = StartPoint.X + xPadding;
                lineBodyBegin.X2 = StartPoint.X + (EndPoint.X - StartPoint.X) / 2 - desiredWidth / 2.0;

                lineBodyEnd.X2 = StartPoint.X + (EndPoint.X - StartPoint.X) / 2 + desiredWidth / 2.0;
                lineBodyEnd.X1 = EndPoint.X - xPadding;
            }
            else
            {
                Canvas.SetTop(lengthLabel, (lineBodyBegin.Y1 + lineBodyBegin.Y2) / 2.0 - desiredHeight / 2.0);
                Canvas.SetLeft(lengthLabel, lineBodyBegin.X1 - desiredWidth / 2.0);

                lineBodyBegin.Y1 = StartPoint.Y + yPadding;
                lineBodyBegin.Y2 = StartPoint.Y + (EndPoint.Y - StartPoint.Y) / 2 - desiredHeight / 2.0;

                lineBodyEnd.Y2 = StartPoint.Y + (EndPoint.Y - StartPoint.Y) / 2 + desiredHeight / 2.0;
                lineBodyEnd.Y1 = EndPoint.Y - yPadding;
            }
        }

        private double getArrowDistance()
        {
            return getUIUnit() * 5;
        }

        private double getPadding()
        {
            return getUIUnit() * 3;
        }

        private Line GetLine()
        {
            Line line = new Line()
            {
                Stroke = new SolidColorBrush(Colors.Aqua),
                StrokeDashCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeStartLineCap = PenLineCap.Round,
            };
            return line;
        }

        public override void AddToOwnerCanvas()
        {
            this.owningCanvas.Children.Add(lineBodyBegin);
            this.owningCanvas.Children.Add(lineBodyEnd);
            this.owningCanvas.Children.Add(lineStart1);
            this.owningCanvas.Children.Add(lineStart2);
            this.owningCanvas.Children.Add(lineEnd1);
            this.owningCanvas.Children.Add(lineEnd2);

            this.owningCanvas.Children.Add(lengthLabel);
        }

        public override void Clear()
        {
            this.owningCanvas.Children.Remove(lineBodyBegin);
            this.owningCanvas.Children.Remove(lineBodyEnd);
            this.owningCanvas.Children.Remove(lineStart1);
            this.owningCanvas.Children.Remove(lineStart2);
            this.owningCanvas.Children.Remove(lineEnd1);
            this.owningCanvas.Children.Remove(lineEnd2);

            this.owningCanvas.Children.Remove(lengthLabel);
        }

        public override void UpdateForZoomChange()
        {
            lineBodyBegin.StrokeThickness = this.getUIStrokeThicknessUnit();
            lineStart1.StrokeThickness = this.getUIStrokeThicknessUnit();
            lineStart2.StrokeThickness = this.getUIStrokeThicknessUnit();
            lineEnd1.StrokeThickness = this.getUIStrokeThicknessUnit();
            lineEnd2.StrokeThickness = this.getUIStrokeThicknessUnit();

            var st = lengthLabel.RenderTransform as ScaleTransform;
            st.ScaleX = 1.0 / this.owningCanvas.GetScaleTransform().ScaleX;
            st.ScaleY = 1.0 / this.owningCanvas.GetScaleTransform().ScaleY;

            SetDistance();
        }
    }
}
