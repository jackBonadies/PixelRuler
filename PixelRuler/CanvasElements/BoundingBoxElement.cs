﻿using ABI.System.Collections.Generic;
using PixelRuler.CanvasElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using static System.Windows.Forms.LinkLabel;

namespace PixelRuler
{
    public class BoundingBoxElement : MeasurementElementZoomCanvasShape
    {
        private const bool marching_ants = false;
        private readonly SolidColorBrush brush1 = new SolidColorBrush(Colors.Black);
        private readonly SolidColorBrush brush2 = new SolidColorBrush(Colors.White);




        public BoundingBoxElement(Canvas owningCanvas, Point startPoint) : base(owningCanvas)
        {
            rect1 = createRectangle();
            rect1.Stroke = marching_ants ? brush1 : new SolidColorBrush(Colors.Red);

            this.owningCanvas.Children.Add(rect1);
            Canvas.SetZIndex(rect1, App.SHAPE_INDEX);

            if (marching_ants)
            {
                rect2 = createRectangle();
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

            StartPoint = startPoint;

            BoundingBoxLabel = new BoundingBoxLabel();
            BoundingBoxLabel.RenderTransform = new ScaleTransform() { ScaleX = 1.0, ScaleY = 1.0 };

            var sizerEnumValues = Enum.GetValues(typeof(SizerEnum));
            foreach (var sizerEnumValue in sizerEnumValues)
            {
                var circleSizer = new CircleSizerControl();
                circleSizer.Tag = sizerEnumValue;
                circleSizerControls.Add(circleSizer);
                SetupResizer(circleSizer);
            }

            this.owningCanvas.Children.Add(BoundingBoxLabel);
            Canvas.SetZIndex(BoundingBoxLabel, App.LABEL_INDEX);

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

        private void StartResizeCircle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isManipulating = false;
            (sender as UIElement).ReleaseMouseCapture();
            CleanUpStartEndPoints();
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

            MoveStartInfo.mouseStart = new Point(xMove, yMove).Add(MoveStartInfo.mouseStart);

            int moveXStart = 0;
            int moveYStart = 0;
            int moveXEnd = 0;
            int moveYEnd = 0;

            if (sizerManipulating.IsBottom())
            {
                moveYEnd = yMove;
            }
            else if (sizerManipulating.IsTop())
            {
                moveYStart = yMove;
            }
            else
            {
                moveYStart = 0;
            }

            if (sizerManipulating.IsLeft())
            {
                moveXStart = xMove;
            }
            else if (sizerManipulating.IsRight())
            {
                moveXEnd = xMove;
            }
            else
            {
                moveXStart = 0;
            }


            this.StartPoint = new Point(this.StartPoint.X + moveXStart, this.StartPoint.Y + moveYStart);
            this.EndPoint = new Point(this.EndPoint.X + moveXEnd, this.EndPoint.Y + moveYEnd);
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

        //public Point StartPoint
        //{
        //    get; private set;
        //}

        //private Point endPoint;
        //public Point EndPoint 
        //{ 
        //    get
        //    {
        //        return endPoint;
        //    }
        //    set
        //    {
        //        endPoint = value;
        //        SetShapeState();
        //        SetLabelState();
        //    }
        //}

        private void SetLabelState()
        {
            Canvas.SetTop(BoundingBoxLabel, EndPoint.Y);
            Canvas.SetLeft(BoundingBoxLabel, EndPoint.X);// - BoundingBoxLabel.ActualWidth * 1.5);
            BoundingBoxLabel.BoundingBoxWidth = (int)rect1.Width;
            BoundingBoxLabel.BoundingBoxHeight = (int)rect1.Height;
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

        public BoundingBoxLabel BoundingBoxLabel
        {
            get;
            set;
        }

        public override void UpdateForZoomChange()
        {
            rect1.StrokeThickness = getUIUnit();
            if (rect2 != null)
            {
                rect2.StrokeThickness = getUIUnit();
            }
            var st = BoundingBoxLabel.RenderTransform as ScaleTransform;
            st.ScaleX = 1.0 / this.owningCanvas.GetScaleTransform().ScaleX;
            st.ScaleY = 1.0 / this.owningCanvas.GetScaleTransform().ScaleY;

            base.UpdateForZoomChange();
        }

        public override void Clear()
        {
            base.Clear();
            this.owningCanvas.Children.Remove(rect1);
            this.owningCanvas.Children.Remove(rect2);
            this.owningCanvas.Children.Remove(BoundingBoxLabel);
        }

        public override void SetEndPoint(System.Windows.Point roundedPoint)
        {
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
    }
}
