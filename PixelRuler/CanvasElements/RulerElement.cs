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

        private Canvas canvas;
        public RulerElement(Canvas canvas) : base(canvas)
        {
            this.canvas = canvas;
            line1 = createLine();
            lineBeginCap = createLine();
            lineEndCap = createLine();
            startResizeCircle = new CircleSizerControl();
            endResizeCircle = new CircleSizerControl();
            rulerLengthLabel = new LengthLabel();
        }

        public override void AddToOwnerCanvas()
        {
            line1.SetResourceReference(Line.StrokeProperty, "AnnotationColor");

            this.owningCanvas.Children.Add(line1);
            Canvas.SetZIndex(line1, App.SHAPE_INDEX);

            lineBeginCap.SetResourceReference(Line.StrokeProperty, "AnnotationColor");

            this.owningCanvas.Children.Add(lineBeginCap);
            Canvas.SetZIndex(lineBeginCap, App.SHAPE_INDEX);

            lineEndCap.SetResourceReference(Line.StrokeProperty, "AnnotationColor");

            this.owningCanvas.Children.Add(lineEndCap);
            Canvas.SetZIndex(lineEndCap, App.SHAPE_INDEX);

            hitBoxManipulate.GotFocus += HitBoxManipulate_GotFocus;
            hitBoxManipulate.LostFocus += HitBoxManipulate_LostFocus;
            //hitBoxManipulate.IsHitTestVisible = false;


            SetupResizer(startResizeCircle);
            SetupResizer(endResizeCircle);

            rulerLengthLabel.RenderTransform = new ScaleTransform() { ScaleX = 1.0, ScaleY = 1.0 };

            this.owningCanvas.Children.Add(rulerLengthLabel);
            Canvas.SetZIndex(rulerLengthLabel, App.MANIPULATE_HITBOX_INDEX);

            this.circleSizerControls.Add(startResizeCircle);
            this.circleSizerControls.Add(endResizeCircle);

            UpdateForZoomChange();
            SetSelectedState();
        }

        private void SetupResizer(CircleSizerControl circleSizerControl)
        {
            hitBoxManipulate.Children.Add(circleSizerControl);
            circleSizerControl.MouseLeftButtonDown += StartResizeCircle_MouseLeftButtonDown;
            circleSizerControl.MouseMove += StartResizeCircle_MouseMove;
            circleSizerControl.MouseLeftButtonUp += StartResizeCircle_MouseLeftButtonUp;
            Canvas.SetZIndex(circleSizerControl, App.RESIZE_INDEX);
        }


        private void HitBoxManipulate_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        private void HitBoxManipulate_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void CleanUpStartEndPoints()
        {
            if (IsHorizontal())
            {
                EndPoint = new Point(EndPoint.X, StartPoint.Y);
                if (StartPoint.X > EndPoint.X)
                {
                    var temp = StartPoint;
                    StartPoint = EndPoint;
                    EndPoint = temp;
                }
            }
            else
            {
                EndPoint = new Point(StartPoint.X, EndPoint.Y);
                if (StartPoint.Y > EndPoint.Y) // if start point is lower
                {
                    var temp = StartPoint;
                    StartPoint = EndPoint;
                    EndPoint = temp;
                }
            }
        }



        private void StartResizeCircle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isManipulating = false;
            (sender as UIElement).ReleaseMouseCapture();
            CleanUpStartEndPoints();
            OnEndResize(null);
        }

        private void StartResizeCircle_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isManipulating)
            {
                return;
            }
            var newPos = e.GetPosition(this.owningCanvas);
            var delta = (MoveStartInfo.mouseStart - newPos) / App.ResizeSpeedFactor;
            System.Diagnostics.Trace.WriteLine($"x: {delta.X} y: {delta.Y}");

            int xMove = -(int)delta.X;
            int yMove = -(int)delta.Y;

            MoveStartInfo.mouseStart = new Point(xMove, yMove).Add(MoveStartInfo.mouseStart);

            if (IsHorizontal())
            {
                if (isStartPointManipulating)
                {
                    MoveStartPoint(xMove, 0);
                    //this.StartPoint = new Point(MoveStartInfo.shapeStart.X + xMove, MoveStartInfo.shapeStart.Y);
                }
                else
                {
                    MoveEndPoint(xMove, 0);
                    //this.EndPoint = new Point(MoveStartInfo.shapeEnd.X + xMove, MoveStartInfo.shapeEnd.Y);
                }
            }
            else
            {
                if (isStartPointManipulating)
                {
                    MoveStartPoint(0, yMove);
                    //this.StartPoint = new Point(MoveStartInfo.shapeStart.X, MoveStartInfo.shapeStart.Y + yMove);
                }
                else
                {
                    MoveEndPoint(0, yMove);
                    //this.EndPoint = new Point(MoveStartInfo.shapeEnd.X, MoveStartInfo.shapeEnd.Y + yMove);
                }
            }

            SetShapeState();
            SetLabelState();
            OnResizing(isStartPointManipulating);
            e.Handled = true;
            //this.EndPoint = new Point(MoveStartInfo.shapeEnd.X + xMove, MoveStartInfo.shapeEnd.Y + yMove);
        }

        bool isStartPointManipulating = false;

        private void StartResizeCircle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.FinishedDrawing)
            {
                return;
            }

            isStartPointManipulating = isStartPointCircleResizer(sender as CircleSizerControl);

            this.Selected = true;

            isManipulating = true;
            (sender as UIElement).Focus();
            MoveStartInfo = (e.GetPosition(this.owningCanvas), StartPoint, EndPoint);
            (sender as UIElement).CaptureMouse();
            OnStartResize(isStartPointManipulating);
            e.Handled = true;
        }

        private bool isStartPointCircleResizer(CircleSizerControl circleSizerControl)
        {
            bool nominalStart = circleSizerControl == startResizeCircle;
            if (IsHorizontal())
            {
                if (Canvas.GetLeft(startResizeCircle) > Canvas.GetLeft(endResizeCircle))
                {
                    nominalStart = !nominalStart;
                }
            }
            else
            {
                if (Canvas.GetTop(startResizeCircle) > Canvas.GetTop(endResizeCircle))
                {
                    nominalStart = !nominalStart;
                }
            }
            return nominalStart;
        }



        //public override void Move(int deltaX, int deltaY)
        //{
        //    // this can just be the base method and non virtual TODO
        //    this.StartPoint = new Point(this.StartPoint.X + deltaX, this.StartPoint.Y + deltaY);
        //    this.endPoint = new Point(this.EndPoint.X + deltaX, this.EndPoint.Y + deltaY);
        //    this.SetShapeState();
        //    this.SetLabelState();
        //}
        private bool? isHorizontalState = null;

        public bool IsHorizontal()
        {
            if (isHorizontalState == null)
            {
                var xDist = Math.Abs(EndPoint.X - StartPoint.X);
                var yDist = Math.Abs(EndPoint.Y - StartPoint.Y);

                if (xDist > yDist)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return isHorizontalState.Value;
            }

        }

        public override double ShapeWidth
        {
            get
            {
                if(!IsHorizontal())
                {
                    return double.NaN;
                }
                return base.ShapeWidth;
            }
        }

        public override double ShapeHeight
        {
            get
            {
                if(IsHorizontal())
                {
                    return double.NaN;
                }
                return base.ShapeHeight;
            }
        }

        private void SetShapeState()
        {
            if(double.IsNaN(EndPoint.X))
            {
                return;
            }

            line1.X1 = StartPoint.X;
            line1.Y1 = StartPoint.Y;


            if(IsHorizontal())
            {
                line1.X2 = EndPoint.X;
                line1.Y2 = line1.Y1;

                lineBeginCap.X1 = lineBeginCap.X2 = line1.X1;
                lineEndCap.X1 = lineEndCap.X2 = line1.X2;

                lineBeginCap.Y1 = lineEndCap.Y1 = line1.Y1 + getEndCapsSize();
                lineBeginCap.Y2 = lineEndCap.Y2 = line1.Y1 - getEndCapsSize();
            }
            else
            {
                line1.X2 = line1.X1;
                line1.Y2 = EndPoint.Y;

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


            if(IsHorizontal())
            {
                Canvas.SetTop(startResizeCircle, hitBoxManipulate.Height / 2 - startResizeCircle.ActualHeight / 2 * getUIUnit() / owningCanvas.GetDpi());
                Canvas.SetTop(endResizeCircle, hitBoxManipulate.Height / 2 - startResizeCircle.ActualHeight / 2 * getUIUnit() / owningCanvas.GetDpi());
                Canvas.SetLeft(startResizeCircle, padding - startResizeCircle.ActualWidth / 2 * getUIUnit() / owningCanvas.GetDpi());
                Canvas.SetLeft(endResizeCircle, double.NaN); // clear left, otherwise it can override
                Canvas.SetRight(endResizeCircle, -endResizeCircle.ActualWidth / 2 * getUIUnit() / owningCanvas.GetDpi() + padding);
            }
            else
            {
                Canvas.SetLeft(startResizeCircle, hitBoxManipulate.Width / 2 - startResizeCircle.ActualWidth / 2 * getUIUnit() / owningCanvas.GetDpi());
                Canvas.SetLeft(endResizeCircle, hitBoxManipulate.Width / 2 - startResizeCircle.ActualWidth / 2 * getUIUnit() / owningCanvas.GetDpi());
                Canvas.SetTop(startResizeCircle, padding - startResizeCircle.ActualHeight / 2 * getUIUnit() / owningCanvas.GetDpi());
                Canvas.SetTop(endResizeCircle, double.NaN);
                Canvas.SetBottom(endResizeCircle, -endResizeCircle.ActualHeight / 2 * getUIUnit() / owningCanvas.GetDpi() + padding);
            }
        }


        private void SetLabelState()
        {
            if (double.IsNaN(EndPoint.X))
            {
                return;
            }

            if (IsHorizontal())
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

            base.UpdateForZoomChange();
        }

        public override void Clear()
        {
            base.Clear();
            this.owningCanvas.Children.Remove(line1);
            this.owningCanvas.Children.Remove(lineBeginCap);
            this.owningCanvas.Children.Remove(lineEndCap);
            this.owningCanvas.Children.Remove(rulerLengthLabel);
        }

        public override void SetEndPoint(Point roundedPoint)
        {
            this.EndPoint = roundedPoint;
            this.SetState();
        }

        public override void SetState()
        {
            SetShapeState();
            SetLabelState();
        }

        public double Extent
        {
            get
            {
                return IsHorizontal() ? Math.Abs(line1.X1 - line1.X2) : Math.Abs(line1.Y1 - line1.Y2);
            }
        }

        public override bool IsEmpty
        {
            get
            {
                return this.Extent == 0;
            }
        }

        private bool finishedDrawing;
        public override bool FinishedDrawing
        {
            get
            {
                return finishedDrawing;
            }
            set
            {
                if(finishedDrawing != value)
                {
                    finishedDrawing = value;
                    isHorizontalState = IsHorizontal();
                    foreach(var sizer in circleSizerControls)
                    {
                        sizer.Cursor = IsHorizontal() ? Cursors.SizeWE : Cursors.SizeNS;
                    }
                    CleanUpStartEndPoints();
                }
            }
        }
    }
}
