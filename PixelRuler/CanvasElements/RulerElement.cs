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
using System.Windows.Input;

namespace PixelRuler.CanvasElements
{
    class RulerElement : MeasurementElementZoomCanvasShape
    {
        Line line1;
        Line lineBeginCap;
        Line lineEndCap;
        CircleSizerControl startResizeCircle;
        CircleSizerControl endResizeCircle;
        LengthLabel rulerLengthLabel;
        Canvas hitBoxManipulate;

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

            hitBoxManipulate = new Canvas();
            hitBoxManipulate.GotFocus += HitBoxManipulate_GotFocus;
            hitBoxManipulate.LostFocus += HitBoxManipulate_LostFocus;
            hitBoxManipulate.Focusable = true;
            hitBoxManipulate.Cursor = Cursors.SizeAll;
            hitBoxManipulate.Background = new SolidColorBrush(Colors.Transparent);
            hitBoxManipulate.MouseLeftButtonDown += HitBoxManipulate_MouseDown;
            hitBoxManipulate.MouseMove += HitBoxManipulate_MouseMove;
            hitBoxManipulate.MouseLeftButtonUp += HitBoxManipulate_MouseUp;



            startResizeCircle = new CircleSizerControl();
            hitBoxManipulate.Children.Add(startResizeCircle);
            endResizeCircle = new CircleSizerControl();
            hitBoxManipulate.Children.Add(endResizeCircle);

            startResizeCircle.MouseLeftButtonDown += StartResizeCircle_MouseLeftButtonDown;
            startResizeCircle.MouseMove += StartResizeCircle_MouseMove;
            startResizeCircle.MouseLeftButtonUp += StartResizeCircle_MouseLeftButtonUp;

            this.owningCanvas.Children.Add(hitBoxManipulate);
            Canvas.SetZIndex(hitBoxManipulate, 500);

            StartPoint = startPoint;

            rulerLengthLabel = new LengthLabel();
            rulerLengthLabel.RenderTransform = new ScaleTransform() { ScaleX = 1.0, ScaleY = 1.0 };

            this.owningCanvas.Children.Add(rulerLengthLabel);
            Canvas.SetZIndex(rulerLengthLabel, 500);

            UpdateForZoomChange();
            SetSelectedState();
        }



        private void HitBoxManipulate_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        private void HitBoxManipulate_LostFocus(object sender, RoutedEventArgs e)
        {
        }



        private void StartResizeCircle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isManipulating = false;
            (sender as UIElement).ReleaseMouseCapture();
        }

        private void StartResizeCircle_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isManipulating)
            {
                return;
            }
            var newPos = e.GetPosition(this.owningCanvas);
            var delta = MoveStartInfo.mouseStart - newPos;
            System.Diagnostics.Trace.WriteLine($"x: {delta.X} y: {delta.Y}");

            int xMove = -(int)delta.X;
            int yMove = -(int)delta.Y;

            this.StartPoint = new Point(MoveStartInfo.shapeStart.X + xMove, MoveStartInfo.shapeStart.Y);
            SetShapeState();
            SetLabelState();
            e.Handled = true;
            //this.EndPoint = new Point(MoveStartInfo.shapeEnd.X + xMove, MoveStartInfo.shapeEnd.Y + yMove);
        }

        private void StartResizeCircle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.FinishedDrawing)
            {
                return;
            }

            this.Selected = true;

            isManipulating = true;
            (sender as UIElement).Focus();
            MoveStartInfo = (e.GetPosition(this.owningCanvas), StartPoint, EndPoint);
            (sender as UIElement).CaptureMouse();
            e.Handled = true;
        }


        bool isManipulating = false;

        private (Point mouseStart, Point shapeStart, Point shapeEnd) MoveStartInfo;

        private void HitBoxManipulate_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Selected = true;
            isManipulating = false;
            this.hitBoxManipulate.ReleaseMouseCapture();
        }

        private void HitBoxManipulate_MouseMove(object sender, MouseEventArgs e)
        {
            if(!isManipulating)
            {
                return;
            }
            var newPos = e.GetPosition(this.owningCanvas);
            var delta = MoveStartInfo.mouseStart - newPos;
            System.Diagnostics.Trace.WriteLine($"x: {delta.X} y: {delta.Y}");

            int xMove = -(int)delta.X;
            int yMove = -(int)delta.Y;

            this.StartPoint = new Point(MoveStartInfo.shapeStart.X + xMove, MoveStartInfo.shapeStart.Y + yMove);
            this.EndPoint = new Point(MoveStartInfo.shapeEnd.X + xMove, MoveStartInfo.shapeEnd.Y + yMove);
        }


        private void HitBoxManipulate_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(!this.FinishedDrawing)
            {
                return;
            }


            isManipulating = true;
            this.hitBoxManipulate.Focus();
            MoveStartInfo = (e.GetPosition(this.owningCanvas), StartPoint, EndPoint);
            this.hitBoxManipulate.CaptureMouse();
            e.Handled = true;
        }

        public override void Move(int v1, int v2)
        {
            this.StartPoint = new Point(this.StartPoint.X + v1, this.StartPoint.Y + v2);
            this.endPoint = new Point(this.EndPoint.X + v1, this.EndPoint.Y + v2);
            this.SetShapeState();
            this.SetLabelState();
            //this.SetEndPoint();
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

            var minX = Math.Min(Math.Min(Math.Min(line1.X1, line1.X2), lineBeginCap.X1), lineBeginCap.X2);
            var maxX = Math.Max(Math.Max(Math.Max(line1.X1, line1.X2), lineBeginCap.X1), lineBeginCap.X2);
            var minY = Math.Min(Math.Min(Math.Min(line1.Y1, line1.Y2), lineBeginCap.Y1), lineBeginCap.Y2);
            var maxY = Math.Max(Math.Max(Math.Max(line1.Y1, line1.Y2), lineBeginCap.Y1), lineBeginCap.Y2);

            var padding = getUIUnit() * 6;

            Canvas.SetLeft(hitBoxManipulate, minX - padding);
            hitBoxManipulate.Width = maxX - minX + padding * 2;
            Canvas.SetTop(hitBoxManipulate, minY - padding);
            hitBoxManipulate.Height = maxY - minY + padding * 2;

            //hitBoxManipulate.Background = new SolidColorBrush(Colors.Green);

            Canvas.SetTop(startResizeCircle, hitBoxManipulate.ActualHeight / 2 - startResizeCircle.ActualHeight / 2 * getUIUnit() / owningCanvas.GetDpi());
            Canvas.SetTop(endResizeCircle, hitBoxManipulate.ActualHeight / 2 - startResizeCircle.ActualHeight / 2 * getUIUnit() / owningCanvas.GetDpi());
            Canvas.SetLeft(startResizeCircle, padding - startResizeCircle.ActualWidth / 2 * getUIUnit() / owningCanvas.GetDpi());
            Canvas.SetLeft(endResizeCircle, hitBoxManipulate.ActualWidth -  padding - startResizeCircle.ActualWidth / 2 * getUIUnit() / owningCanvas.GetDpi());
        }


        private void SetLabelState()
        {
            if (double.IsNaN(endPoint.X))
            {
                return;
            }

            if (isHorizontal())
            {
                Canvas.SetTop(rulerLengthLabel, line1.Y1 - (rulerLengthLabel.ActualHeight + 2) * getUIUnit());
                Canvas.SetLeft(rulerLengthLabel, (line1.X1 + line1.X2) / 2.0 - rulerLengthLabel.ActualWidth * getUIUnit() / 2.0  );// - BoundingBoxLabel.ActualWidth * 1.5);
            }
            else
            {
                Canvas.SetLeft(rulerLengthLabel, line1.X1 +  2 * getUIUnit());
                Canvas.SetTop(rulerLengthLabel, (line1.Y1 + line1.Y2) / 2.0 - rulerLengthLabel.ActualHeight * getUIUnit() / 2.0  );// - BoundingBoxLabel.ActualWidth * 1.5);
            }
            rulerLengthLabel.Dim1 = (int)Extent;
        }

        private double getEndCapsSize()
        {
            if(endcapsInUIUnits)
            {
                return 8 * getUIUnit();
            }
            else
            {
                return 8;
            }
        }

        bool endcapsInUIUnits = true;

        public override void UpdateForZoomChange()
        {
            line1.StrokeThickness = getUIUnit();
            lineBeginCap.StrokeThickness = getUIUnit();
            lineEndCap.StrokeThickness = getUIUnit();

            var st = rulerLengthLabel.RenderTransform as ScaleTransform;
            st.ScaleX = 1.0 / this.owningCanvas.GetScaleTransform().ScaleX;
            st.ScaleY = 1.0 / this.owningCanvas.GetScaleTransform().ScaleY;

            st = startResizeCircle.LayoutTransform as ScaleTransform;
            st.ScaleX = 1.0 / this.owningCanvas.GetScaleTransform().ScaleX;
            st.ScaleY = 1.0 / this.owningCanvas.GetScaleTransform().ScaleY;

            st = endResizeCircle.LayoutTransform as ScaleTransform;
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
            this.owningCanvas.Children.Remove(hitBoxManipulate);
        }

        public override void SetEndPoint(Point roundedPoint)
        {
            this.EndPoint = roundedPoint;
        }

        public override void SetSelectedState()
        {
            if (this.Selected)
            {
                this.startResizeCircle.Visibility = Visibility.Visible;
                this.endResizeCircle.Visibility = Visibility.Visible;
            }
            else
            {
                this.startResizeCircle.Visibility = Visibility.Hidden;
                this.endResizeCircle.Visibility = Visibility.Hidden;
            }
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
