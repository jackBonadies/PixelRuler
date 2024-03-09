using PixelRuler.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PixelRuler.CanvasElements
{
    public class GuidelineElement : MeasurementElementZoomCanvasShape
    {
        public bool IsHorizontal { get; private set; } = false;
        public int Coordinate { get; private set; } = int.MaxValue;
        public int ImageCoordinate
        {
            get
            {
                return this.Coordinate - 10000;
            }
        }

        private Line mainLine;
        private Canvas hitBoxCanvas;
        private MainCanvas mainCanvas;

        public GuidelineElement(MainCanvas mainCanvas, int coordinate, bool isHorizontal) : base(mainCanvas.innerCanvas)
        {
            this.mainCanvas = mainCanvas;

            mainLine = UiUtils.CreateLine();
            mainLine.X1 = 9000;
            mainLine.X2 = 12000;
            mainLine.Y1 = 9000;
            mainLine.Y2 = 11000;
            mainLine.IsHitTestVisible = false;


            this.Coordinate = coordinate;
            this.IsHorizontal = isHorizontal;

 
            mainLine.Stroke = new SolidColorBrush(Colors.Aqua);
            mainLine.StrokeThickness = getUIStrokeThicknessUnit();

            RenderOptions.SetEdgeMode(mainLine, EdgeMode.Aliased);

            hitBoxCanvas = new Canvas();
            hitBoxCanvas.Cursor = isHorizontal ? Cursors.SizeNS : Cursors.SizeWE;
            if(isHorizontal)
            {
                hitBoxCanvas.Width = 30000;
                hitBoxCanvas.Height = getUIUnit() * 5;
            }
            else
            {
                hitBoxCanvas.Width = getUIUnit() * 5;
                hitBoxCanvas.Height = 1000;
            }
            hitBoxCanvas.Background = new SolidColorBrush(Colors.Red);
            hitBoxCanvas.MouseLeftButtonDown += HitBoxCanvas_MouseLeftButtonDown;
            hitBoxCanvas.MouseMove += HitBoxCanvas_MouseMove;
            hitBoxCanvas.MouseLeftButtonUp += HitBoxCanvas_MouseLeftButtonUp;

            hitBoxCanvas.MouseEnter += HitBoxCanvas_MouseEnter;
            hitBoxCanvas.MouseLeave += HitBoxCanvas_MouseLeave;

            SetPositionState();
        }

        private void HitBoxCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if(!closeButtonView?.IsMouseOver ?? true)
            {
                this.owningCanvas.Children.Remove(closeButtonView);
            }
        }

        private void HitBoxCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            var overlayCanvas = this.owningCanvas.Parent as Canvas;
            var pt = overlayCanvas.TranslatePoint(new Point(34, 34), this.owningCanvas);
            if(closeButtonView == null)
            {
                closeButtonView = new CloseButtonView();
                closeButtonView.MouseLeave += CloseButtonView_MouseLeave;
                closeButtonView.MouseLeftButtonDown += CloseButtonView_MouseLeftButtonDown;
                closeButtonView.Cursor = Cursors.Hand;
                //var st = new ScaleTransform();
                //st.ScaleX = st.ScaleY = 1.0 / this.owningCanvas.GetScaleTransform().ScaleY;
                //closeButtonView.RenderTransform = st;
                Canvas.SetZIndex(closeButtonView, 5000);
            }
            if(!this.owningCanvas.Children.Contains(closeButtonView))
            {
                this.owningCanvas.Children.Add(closeButtonView);
            }
            closeButtonView.Measure(new Size(double.MaxValue, double.MaxValue));
            if(this.IsHorizontal)
            {
                Canvas.SetLeft(closeButtonView, pt.X);
                Canvas.SetTop(closeButtonView, this.Coordinate - closeButtonView.DesiredSize.Height / 2.0);
            }
            else
            {
                Canvas.SetLeft(closeButtonView, this.Coordinate - closeButtonView.DesiredSize.Width / 2.0);
                Canvas.SetTop(closeButtonView, pt.Y);
            }
        }

        private void CloseButtonView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Clear();
        }

        private void CloseButtonView_MouseLeave(object sender, MouseEventArgs e)
        {
            if(!hitBoxCanvas.IsMouseOver)
            {
                this.owningCanvas.Children.Remove(closeButtonView);
            }
        }

        CloseButtonView closeButtonView;

        private void SetPositionState()
        {
            if (IsHorizontal)
            {
                this.mainLine.Y1 = Coordinate;
                this.mainLine.Y2 = Coordinate;

                Canvas.SetLeft(hitBoxCanvas, 0);
                Canvas.SetTop(hitBoxCanvas, Coordinate - (int)(hitBoxCanvas.Height / 2));
            }
            else
            {
                this.mainLine.X1 = Coordinate;
                this.mainLine.X2 = Coordinate;

                Canvas.SetLeft(hitBoxCanvas, Coordinate - hitBoxCanvas.Width / 2);
                Canvas.SetTop(hitBoxCanvas, 10000);
            }

        }

        private void HitBoxCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isManipulating = false;
            (sender as UIElement).ReleaseMouseCapture();
        }

        private void HitBoxCanvas_MouseMove(object sender, MouseEventArgs e)
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

            MoveStartInfo.mouseStart = new Point(xMove * App.ResizeSpeedFactor, yMove * App.ResizeSpeedFactor).Add(MoveStartInfo.mouseStart);

            // move by xMove...
            if(IsHorizontal)
            {
                this.Coordinate += yMove;
            }
            else
            {
                this.Coordinate += xMove;
            }

            SetPositionState();

            e.Handled = true;
        }

        private void HitBoxCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isManipulating = true;
            (sender as UIElement).Focus();
            MoveStartInfo = (e.GetPosition(this.owningCanvas), StartPoint, EndPoint);
            (sender as UIElement).CaptureMouse();
            e.Handled = true;
        }

        public override bool IsEmpty => true;

        public override bool FinishedDrawing { get => true; set { } }

        public override void AddToOwnerCanvas()
        {
            mainCanvas.innerCanvas.Children.Add(hitBoxCanvas);
            mainCanvas.innerCanvas.Children.Add(mainLine);
        }

        public override void Clear()
        {
            mainCanvas.innerCanvas.Children.Remove(hitBoxCanvas);
            mainCanvas.innerCanvas.Children.Remove(mainLine);
            mainCanvas.innerCanvas.Children.Remove(closeButtonView);
        }

        public override List<UIElement> GetZoomCanvasElements()
        {
            var line = new Line() { StrokeThickness = getUIStrokeThicknessUnit() };
            line.SetStrokeToAnnotationColor();
            return new List<UIElement>()
            {
                line,
            };
        }

        public override void SetEndPoint(Point roundedPoint)
        {
        }

        public override void SetState()
        {
        }

        public override void UpdateForZoomChange()
        {
            //mainLine.RenderTransform = new ScaleTransform(getUIUnit()*2, 1);
            mainLine.StrokeThickness = getUIStrokeThicknessUnit();
            //lineBeginCap.StrokeThickness = getUIUnit();
            //lineEndCap.StrokeThickness = getUIUnit();

            var st = closeButtonView?.RenderTransform as ScaleTransform;
            if (st != null)
            {
                //st.ScaleX = st.ScaleY = 1.0 / this.owningCanvas.GetScaleTransform().ScaleX;
            }

            if(IsHorizontal)
            {
                //hitBoxCanvas.Height = getUIUnit() * 5;
            }
            else
            {
                hitBoxCanvas.Width = getUIUnit() * 5;
            }
            SetPositionState();

            base.UpdateForZoomChange();
        }

    }

    //public class GuidelineElementOverlay : IOverlayCanvasShape
    //{
    //    private bool isHorizontal = false;
    //    private double coordinate = double.NaN;

    //    private Line mainLine;
    //    private Canvas hitBoxCanvas;
    //    private MainCanvas mainCanvas;

    //    public GuidelineElementOverlay(MainCanvas mainCanvas, double coordinate, bool isHorizontal)
    //    {
    //        this.mainCanvas = mainCanvas;

    //        mainLine = UiUtils.CreateLine();
    //        mainLine.X1 = 0;
    //        mainLine.X2 = 10000;
    //        mainLine.Y1 = 0;
    //        mainLine.Y2 = 10000;
    //        mainLine.IsHitTestVisible = false;


    //        this.coordinate = coordinate;
    //        this.isHorizontal = isHorizontal;

 
    //        mainLine.Stroke = new SolidColorBrush(Colors.Aqua);
    //        mainLine.StrokeThickness = 1;

    //        hitBoxCanvas = new Canvas();
    //        hitBoxCanvas.Cursor = Cursors.SizeWE;
    //        if(isHorizontal)
    //        {
    //            hitBoxCanvas.Width = 10000;
    //            hitBoxCanvas.Height = 5;
    //        }
    //        else
    //        {
    //            hitBoxCanvas.Width = 5;
    //            hitBoxCanvas.Height = 10000;
    //        }
    //        hitBoxCanvas.Background = new SolidColorBrush(Colors.Transparent);

    //    }

    //    public void AddToCanvas()
    //    {
    //        mainCanvas.overlayCanvas.Children.Add(hitBoxCanvas);
    //        mainCanvas.overlayCanvas.Children.Add(mainLine);
    //    }

    //    public void Clear()
    //    {
    //        mainCanvas.overlayCanvas.Children.Remove(hitBoxCanvas);
    //        mainCanvas.overlayCanvas.Children.Remove(mainLine);
    //    }

    //    public void UpdateForCoordinatesChanged()
    //    {
            
    //        var overlayPt = this.mainCanvas.mainImage.TranslatePoint(new Point(coordinate, coordinate), this.mainCanvas.overlayCanvas);
    //        if (isHorizontal)
    //        {
    //            this.mainLine.Y1 = overlayPt.Y;
    //            this.mainLine.Y2 = overlayPt.Y;

    //            Canvas.SetLeft(hitBoxCanvas, 0);
    //            Canvas.SetTop(hitBoxCanvas, overlayPt.Y - (int)(hitBoxCanvas.Height / 2));
    //        }
    //        else
    //        {
    //            this.mainLine.X1 = overlayPt.X;
    //            this.mainLine.X2 = overlayPt.X;

    //            Canvas.SetLeft(hitBoxCanvas, overlayPt.X - (int)(hitBoxCanvas.Width / 2));
    //            Canvas.SetTop(hitBoxCanvas, 0);
    //        }
    //    }
    //}
}
