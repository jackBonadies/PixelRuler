using ABI.System.Collections.Generic;
using PixelRuler.CanvasElements;
using PixelRuler.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace PixelRuler
{
    public class BoundingBoxElement : MeasurementElementZoomCanvasShape
    {
        private const bool marching_ants = false;
        private readonly SolidColorBrush brush1 = new SolidColorBrush(Colors.Black);
        private readonly SolidColorBrush brush2 = new SolidColorBrush(Colors.White);



        public BoundingBoxElement(Canvas owningCanvas) : base(owningCanvas)
        {
            this.owningCanvas = owningCanvas;
            rect1 = UiUtils.CreateRectangle();
            boundingBoxLabelForEndPoint = new BoundingBoxLabel();
            widthLabel = new LengthLabel();
            heightLabel = new LengthLabel();
        }

        public override void AddToOwnerCanvas()
        {
            if(marching_ants)
            {
                rect1.Stroke = brush1;

            }
            else
            {
                //var binding = new Binding("Settings.AnnotationColor")
                //{
                //    Converter = new PixelRuler.ColorConverter(),
                //    Source = owningCanvas.DataContext as PixelRulerViewModel,
                //};
                rect1.SetResourceReference(Rectangle.StrokeProperty, App.AnnotationColorKey);
            }

            this.owningCanvas.Children.Add(rect1);
            Canvas.SetZIndex(rect1, App.SHAPE_INDEX);

            if (marching_ants)
            {
                rect2 = UiUtils.CreateRectangle();
                rect2.Stroke = brush2;

                var dashArray = new double[] { 4, 4 };
                rect2.StrokeDashArray = new DoubleCollection(dashArray);

                DoubleAnimation animation = new DoubleAnimation
                {
                    From = 0,
                    To = -dashArray.Sum() * 6,
                    Duration = TimeSpan.FromSeconds(2),
                    RepeatBehavior = RepeatBehavior.Forever
                };

                rect2.BeginAnimation(Shape.StrokeDashOffsetProperty, animation);

                this.owningCanvas.Children.Add(rect2);
                Canvas.SetZIndex(rect2, App.SHAPE_INDEX + 1);
            }

            boundingBoxLabelForEndPoint.RenderTransform = new ScaleTransform() { ScaleX = 1.0, ScaleY = 1.0 };

            widthLabel.RenderTransform = new ScaleTransform() { ScaleX = 1.0, ScaleY = 1.0 };

            heightLabel.RenderTransform = new ScaleTransform() { ScaleX = 1.0, ScaleY = 1.0 };

            var sizerEnumValues = Enum.GetValues(typeof(SizerEnum));
            foreach (var sizerEnumValue in sizerEnumValues)
            {
                var circleSizer = new CircleSizerControl();
                circleSizer.Tag = sizerEnumValue;
                circleSizerControls.Add(circleSizer);
                SetupResizer(circleSizer);
            }

            this.owningCanvas.Children.Add(boundingBoxLabelForEndPoint);
            Canvas.SetZIndex(boundingBoxLabelForEndPoint, App.LABEL_INDEX);

            this.owningCanvas.Children.Add(widthLabel);
            Canvas.SetZIndex(widthLabel, App.LABEL_INDEX);

            this.owningCanvas.Children.Add(heightLabel);
            Canvas.SetZIndex(heightLabel, App.LABEL_INDEX);

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
            switch(circleSizerControl.Tag)
            {
                case SizerEnum.TopLeft:
                case SizerEnum.BottomRight:
                    circleSizerControl.Cursor = Cursors.SizeNWSE;
                    break;
                case SizerEnum.TopRight:
                case SizerEnum.BottomLeft:
                    circleSizerControl.Cursor = Cursors.SizeNESW;
                    break;
                case SizerEnum.CenterLeft:
                case SizerEnum.CenterRight:
                    circleSizerControl.Cursor = Cursors.SizeWE;
                    break;
                case SizerEnum.TopCenter:
                case SizerEnum.BottomCenter:
                    circleSizerControl.Cursor = Cursors.SizeNS;
                    break;

            }
        }

        private void StartResizeCircle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isManipulating = false;
            (sender as UIElement).ReleaseMouseCapture();
            CleanUpStartEndPoints();
            OnEndResize(sizerManipulating);
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

            //MoveStartInfo.mouseStart = new Point(xMove, yMove).Add(MoveStartInfo.mouseStart);

            //int moveXStart = 0;
            //int moveYStart = 0;
            //int moveXEnd = 0;
            //int moveYEnd = 0;

            //bool movingCorner = true;
            bool movingXEnd = false;
            bool movingYEnd = false;

            //if (sizerManipulating.IsBottom())
            //{
            //    moveYEnd = yMove;
            //}
            //else if (sizerManipulating.IsTop())
            //{
            //    moveYStart = yMove;
            //}
            //else
            //{
            //    movingCorner = false;
            //    moveYStart = 0;
            //}

            //if (sizerManipulating.IsLeft())
            //{
            //    moveXStart = xMove;
            //}
            //else if (sizerManipulating.IsRight())
            //{
            //    moveXEnd = xMove;
            //}
            //else
            //{
            //    movingCorner = false;
            //    moveXStart = 0;
            //}
            bool movingCorner = (sizerManipulating.IsBottom() || sizerManipulating.IsTop()) &&
                                (sizerManipulating.IsLeft() || sizerManipulating.IsRight());

            movingXEnd = sizerManipulating.IsRight();
            movingYEnd = sizerManipulating.IsBottom();

            //var candidateStart = new Point(this.StartPoint.X + moveXStart, this.StartPoint.Y + moveYStart);
            //var candidateEnd = new Point(this.EndPoint.X + moveXEnd, this.EndPoint.Y + moveYEnd);

            // create newPos start and end by combining components....
            var desiredEndX = this.EndPoint.X;
            var desiredEndY = this.EndPoint.Y;

            var desiredStartX = this.StartPoint.X;
            var desiredStartY = this.StartPoint.Y;

            if(movingXEnd)
            {
                desiredEndX = MoveStartInfo.shapeEnd.X + xMove;
            }
            else
            {
                desiredStartX = MoveStartInfo.shapeStart.X + xMove;
            }

            if(movingYEnd)
            {
                desiredEndY = MoveStartInfo.shapeEnd.Y + yMove;
            }
            else
            {
                desiredStartY = MoveStartInfo.shapeStart.Y + yMove;
            }

            var desiredStart = UiUtils.RoundPoint(new Point(desiredStartX, desiredStartY));
            var desiredEnd = UiUtils.RoundPoint(new Point(desiredEndX, desiredEndY));

            if(movingCorner)
            {
                bool constrain = KeyUtil.IsCtrlDown();
                if(constrain)
                {
                    (desiredStart, desiredEnd) = constrainToAspectRatio(movingXEnd, movingYEnd, desiredStart, desiredEnd);
                }
                this.StartPoint = desiredStart;
                this.EndPoint = desiredEnd;
            }
            else
            {
                if(sizerManipulating.IsLeft())
                {
                    this.StartPoint = new Point(desiredStart.X, this.StartPoint.Y);
                }
                else if(sizerManipulating.IsRight())
                {
                    this.EndPoint = new Point(desiredEnd.X, this.EndPoint.Y);
                }
                else if(sizerManipulating.IsTop())
                {
                    this.StartPoint = new Point(this.StartPoint.X, desiredStart.Y);
                }
                else if(sizerManipulating.IsBottom())
                {
                    this.EndPoint = new Point(this.EndPoint.X, desiredEnd.Y);
                }

            }


            //this.StartPoint = new Point(this.StartPoint.X + moveXStart, this.StartPoint.Y + moveYStart);
            //this.EndPoint = new Point(this.EndPoint.X + moveXEnd, this.EndPoint.Y + moveYEnd);
            this.SetState();

            //if (isHorizontal())
            //{
            //    if (isStartPointManipulating)
            //    {
            //MoveStartPoint(xMove, 0);
            //        //this.StartPoint = new Point(MoveStartInfo.shapeStart.X + xMove, MoveStartInfo.shapeStart.Y);
            //    }
            //    else
            //    {
            //        MoveEndPoint(xMove, 0);
            //        //this.EndPoint = new Point(MoveStartInfo.shapeEnd.X + xMove, MoveStartInfo.shapeEnd.Y);
            //    }
            //}
            //else
            //{
            //    if (isStartPointManipulating)
            //    {
            //        MoveStartPoint(0, yMove);
            //        //this.StartPoint = new Point(MoveStartInfo.shapeStart.X, MoveStartInfo.shapeStart.Y + yMove);
            //    }
            //    else
            //    {
            //        MoveEndPoint(0, yMove);
            //        //this.EndPoint = new Point(MoveStartInfo.shapeEnd.X, MoveStartInfo.shapeEnd.Y + yMove);
            //    }
            //}
            e.Handled = true;
            OnResizing(sizerManipulating);

            //this.EndPoint = new Point(MoveStartInfo.shapeEnd.X + xMove, MoveStartInfo.shapeEnd.Y + yMove);
        }


        SizerEnum sizerManipulating = SizerEnum.CenterRight;

        private void StartResizeCircle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.FinishedDrawing)
            {
                return;
            }

            sizerManipulating = (SizerEnum)(sender as CircleSizerControl).Tag;
            //isStartPointManipulating = TODOisStartPointCircleResizer(sender as CircleSizerControl);

            this.Selected = true;

            isManipulating = true;
            (sender as UIElement).Focus();
            MoveStartInfo = (e.GetPosition(this.owningCanvas), StartPoint, EndPoint);
            (sender as UIElement).CaptureMouse();
            OnStartResize(sizerManipulating);
            e.Handled = true;
        }

        //private bool isStartPointCircleResizer(CircleSizerControl circleSizerControl)
        //{
        //    bool nominalStart = circleSizerControl == startResizeCircle;
        //    if (isHorizontal())
        //    {
        //        if (Canvas.GetLeft(startResizeCircle) > Canvas.GetLeft(endResizeCircle))
        //        {
        //            nominalStart = !nominalStart;
        //        }
        //    }
        //    else
        //    {
        //        if (Canvas.GetTop(startResizeCircle) > Canvas.GetTop(endResizeCircle))
        //        {
        //            nominalStart = !nominalStart;
        //        }
        //    }
        //    return nominalStart;
        //}


        public override void SetState()
        {
            SetShapeState();
            SetLabelState();
        }

        private void SetShapeState(Rectangle? rect)
        {
            if (rect == null)
            {
                return;
            }

            if (StartPoint.X <= EndPoint.X)
            {
                Canvas.SetLeft(rect, StartPoint.X);
                rect.Width = Math.Max(EndPoint.X - Canvas.GetLeft(rect), 0);
            }
            else
            {
                Canvas.SetLeft(rect, EndPoint.X);
                rect.Width = Math.Max(StartPoint.X - EndPoint.X, 0);
            }

            if (StartPoint.Y <= EndPoint.Y)
            {
                Canvas.SetTop(rect, StartPoint.Y);
                rect.Height = Math.Max(EndPoint.Y - Canvas.GetTop(rect), 0);
            }
            else
            {
                Canvas.SetTop(rect, EndPoint.Y);
                rect.Height = Math.Max(StartPoint.Y - EndPoint.Y, 0);
            }

        }

        public void SetShapeState()
        {
            SetShapeState(rect1);
            SetShapeState(rect2);
            SetResizeControls();
        }

        private void SetResizeControls()
        {
            var minX = Canvas.GetLeft(rect1); // always leftmost
            var minY = Canvas.GetTop(rect1); // always topmost
            var maxX = Canvas.GetLeft(rect1) + rect1.Width; // always leftmost
            var maxY = Canvas.GetTop(rect1) + rect1.Height; // always topmost

            var padding = getUIUnit() * 6;

            Canvas.SetLeft(hitBoxManipulate, minX - padding);
            hitBoxManipulate.Width = maxX - minX + padding * 2;
            Canvas.SetTop(hitBoxManipulate, minY - padding);
            hitBoxManipulate.Height = maxY - minY + padding * 2;

            foreach (var sizer in circleSizerControls)
            {
                var sizerEnum = (SizerEnum)sizer.Tag;

                if (sizerEnum.IsLeft())
                {
                    Canvas.SetLeft(sizer, padding - sizer.ActualWidth / 2 * getUIUnit() / owningCanvas.GetDpi());
                }
                else if (sizerEnum.IsRight())
                {
                    Canvas.SetRight(sizer, -sizer.ActualWidth / 2 * getUIUnit() / owningCanvas.GetDpi() + padding);
                }
                else
                {
                    Canvas.SetLeft(sizer, hitBoxManipulate.Width / 2.0 - sizer.ActualWidth / 2 * getUIUnit() / owningCanvas.GetDpi());
                }

                if (sizerEnum.IsTop())
                {
                    Canvas.SetTop(sizer, padding - sizer.ActualHeight / 2 * getUIUnit() / owningCanvas.GetDpi());
                }
                else if (sizerEnum.IsBottom())
                {
                    Canvas.SetBottom(sizer, -sizer.ActualHeight / 2 * getUIUnit() / owningCanvas.GetDpi() + padding);
                }
                else
                {
                    Canvas.SetTop(sizer, hitBoxManipulate.Height / 2.0 - sizer.ActualHeight / 2 * getUIUnit() / owningCanvas.GetDpi());
                }
            }

        }

        private void SetLabelState()
        {
            Canvas.SetTop(boundingBoxLabelForEndPoint, EndPoint.Y);
            Canvas.SetLeft(boundingBoxLabelForEndPoint, EndPoint.X);// - BoundingBoxLabel.ActualWidth * 1.5);
            boundingBoxLabelForEndPoint.BoundingBoxWidth = (int)rect1.Width;
            boundingBoxLabelForEndPoint.BoundingBoxHeight = (int)rect1.Height;

            Canvas.SetLeft(widthLabel, (StartPoint.X + EndPoint.X) / 2.0 - widthLabel.ActualWidth * getUIUnit() / 2.0);// - BoundingBoxLabel.ActualWidth * 1.5);
            Canvas.SetTop(widthLabel, StartPoint.Y - (widthLabel.ActualHeight + 2) * getUIUnit());

            Canvas.SetLeft(heightLabel, EndPoint.X + 2 * getUIUnit());
            Canvas.SetTop(heightLabel, (StartPoint.Y + EndPoint.Y) / 2.0 - heightLabel.ActualHeight * getUIUnit() / 2.0);// - BoundingBoxLabel.ActualWidth * 1.5);
            heightLabel.Dim1 = (int)rect1.Height;
            widthLabel.Dim1 = (int)rect1.Width;
        }

        public int Height
        {
            get
            {
                return (int)rect1.Height;
            }
        }

        public int Width
        {
            get
            {
                return (int)rect1.Width;
            }
        }

        private Rectangle rect1;
        private Rectangle? rect2;

        public BoundingBoxLabel boundingBoxLabelForEndPoint
        {
            get;
            private set;
        }

        public LengthLabel widthLabel
        {
            get;
            private set;
        }

        public LengthLabel heightLabel
        {
            get;
            private set;
        }

        public override void UpdateForZoomChange()
        {
            rect1.StrokeThickness = getUIUnit();
            if (rect2 != null)
            {
                rect2.StrokeThickness = getUIUnit();
            }
            var st = boundingBoxLabelForEndPoint.RenderTransform as ScaleTransform;
            st.ScaleX = 1.0 / this.owningCanvas.GetScaleTransform().ScaleX;
            st.ScaleY = 1.0 / this.owningCanvas.GetScaleTransform().ScaleY;

            st = widthLabel.RenderTransform as ScaleTransform;
            st.ScaleX = 1.0 / this.owningCanvas.GetScaleTransform().ScaleX;
            st.ScaleY = 1.0 / this.owningCanvas.GetScaleTransform().ScaleY;

            st = heightLabel.RenderTransform as ScaleTransform;
            st.ScaleX = 1.0 / this.owningCanvas.GetScaleTransform().ScaleX;
            st.ScaleY = 1.0 / this.owningCanvas.GetScaleTransform().ScaleY;

            base.UpdateForZoomChange();
        }

        public override void Clear()
        {
            base.Clear();
            this.owningCanvas.Children.Remove(rect1);
            this.owningCanvas.Children.Remove(rect2);
            this.owningCanvas.Children.Remove(boundingBoxLabelForEndPoint);
            this.owningCanvas.Children.Remove(widthLabel);
            this.owningCanvas.Children.Remove(heightLabel);
        }

        private (System.Windows.Point, System.Windows.Point) constrainToAspectRatio(
            bool movingXEnd, 
            bool movingYEnd, 
            System.Windows.Point candidateStart, 
            System.Windows.Point candidateEnd, 
            double aspectRatio = 1)
        {
            var diffX = (candidateEnd.X - candidateStart.X);
            var diffY = (candidateEnd.Y - candidateStart.Y);
            var diffXAbs = Math.Abs(diffX);
            var xAbsSign = Math.Sign(diffX);
            var diffYAbs = Math.Abs(diffY);
            var yAbsSign = Math.Sign(diffY);

            var diffAbsMin = Math.Min(diffXAbs, diffYAbs);
            // TODO if contraining, constrain end point.
            double newStartX = StartPoint.X;
            double newStartY = StartPoint.Y;
            double newEndX = EndPoint.X;
            double newEndY = EndPoint.Y;

            if(movingXEnd)
            {
                newEndX = StartPoint.X + xAbsSign * diffAbsMin;
            }
            else
            {
                newStartX = EndPoint.X - xAbsSign * diffAbsMin;
            }

            if(movingYEnd)
            {
                newEndY = StartPoint.Y + yAbsSign * diffAbsMin;
            }
            else
            {
                newStartY = EndPoint.Y - yAbsSign * diffAbsMin;
            }

            var constrainedStart = new Point(newStartX, newStartY);
            var constrainedEnd = new Point(newEndX, newEndY);
            return (constrainedStart, constrainedEnd);
        }

        public override void SetEndPoint(System.Windows.Point roundedPoint)
        {
            var diffX = (roundedPoint.X - StartPoint.X);
            var diffY = (roundedPoint.Y - StartPoint.Y);
            var diffXAbs = Math.Abs(diffX);
            var xAbsSign = Math.Sign(diffX);
            var diffYAbs = Math.Abs(diffY);
            var yAbsSign = Math.Sign(diffY);

            var diffAbsMin = Math.Min(diffXAbs, diffYAbs);
            // TODO if contraining, constrian end point.
            var x = StartPoint.X + xAbsSign * diffAbsMin;
            var y = StartPoint.Y + yAbsSign * diffAbsMin;

            //this.EndPoint = new Point(x, y);
            bool constrain = KeyUtil.IsCtrlDown();
            if (constrain)
            {
                (_, roundedPoint) = constrainToAspectRatio(true, true, StartPoint, roundedPoint, 1);
            }
            this.EndPoint = roundedPoint;


            this.SetState();
        }

        public override bool IsEmpty
        {
            get
            {
                return this.Width == 0 && this.Height == 0;
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
                if (finishedDrawing != value)
                {
                    finishedDrawing = value;
                    CleanUpStartEndPoints();

                }
            }
        }

        /// <summary>
        /// Cleanup so Start is at top left, End is at bottom right
        /// </summary>
        private void CleanUpStartEndPoints()
        {
            var x1 = this.StartPoint.X;
            var y1 = this.StartPoint.Y;
            var x2 = this.EndPoint.X;
            var y2 = this.EndPoint.Y;
            this.StartPoint = new Point(Math.Min(x1, x2), Math.Min(y1, y2));
            this.EndPoint = new Point(Math.Max(x1, x2), Math.Max(y1, y2));
        }

        public override List<UIElement> GetZoomCanvasElements()
        {
            var rect = new Rectangle() { StrokeThickness = 1 };
            rect.SetStrokeToAnnotationColor();
            return new List<UIElement>()
            {
                rect
            };
        }
    }
}
