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
            mainLine.StrokeThickness = 1;

            hitBoxCanvas = new Canvas();
            hitBoxCanvas.Cursor = Cursors.SizeWE;
            if(isHorizontal)
            {
                hitBoxCanvas.Width = 30000;
                hitBoxCanvas.Height = 5;
            }
            else
            {
                hitBoxCanvas.Width = 5;
                hitBoxCanvas.Height = 30000;
            }
            hitBoxCanvas.Background = new SolidColorBrush(Colors.Transparent);

            if (isHorizontal)
            {
                this.mainLine.Y1 = coordinate;
                this.mainLine.Y2 = coordinate;

                Canvas.SetLeft(hitBoxCanvas, 0);
                Canvas.SetTop(hitBoxCanvas, coordinate - (int)(hitBoxCanvas.Height / 2));
            }
            else
            {
                this.mainLine.X1 = coordinate;
                this.mainLine.X2 = coordinate;

                Canvas.SetLeft(hitBoxCanvas, coordinate - (int)(hitBoxCanvas.Width / 2));
                Canvas.SetTop(hitBoxCanvas, 0);
            }

        }

        public override bool IsEmpty => true;

        public override bool FinishedDrawing { get => true; set { } }

        public override void AddToOwnerCanvas()
        {
            mainCanvas.innerCanvas.Children.Add(hitBoxCanvas);
            mainCanvas.innerCanvas.Children.Add(mainLine);
        }

        public void Clear()
        {
            mainCanvas.innerCanvas.Children.Remove(hitBoxCanvas);
            mainCanvas.innerCanvas.Children.Remove(mainLine);
        }

        public override List<UIElement> GetZoomCanvasElements()
        {
            return new List<UIElement>()
            {
                new Line() {Stroke=new SolidColorBrush(Colors.Aqua), StrokeThickness= 1}
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
            mainLine.StrokeThickness = getUIUnit();
            //lineBeginCap.StrokeThickness = getUIUnit();
            //lineEndCap.StrokeThickness = getUIUnit();

            //var st = rulerLengthLabel.RenderTransform as ScaleTransform;
            //st.ScaleX = 1.0 / this.owningCanvas.GetScaleTransform().ScaleX;
            //st.ScaleY = 1.0 / this.owningCanvas.GetScaleTransform().ScaleY;

            base.UpdateForZoomChange();
        }

        public void UpdateForCoordinatesChanged()
        {
            // TODO REMOVE...
            
            var overlayPt = this.mainCanvas.mainImage.TranslatePoint(new Point(Coordinate, Coordinate), this.mainCanvas.overlayCanvas);
            if (IsHorizontal)
            {
                this.mainLine.Y1 = overlayPt.Y;
                this.mainLine.Y2 = overlayPt.Y;

                Canvas.SetLeft(hitBoxCanvas, 0);
                Canvas.SetTop(hitBoxCanvas, overlayPt.Y - (int)(hitBoxCanvas.Height / 2));
            }
            else
            {
                this.mainLine.X1 = overlayPt.X;
                this.mainLine.X2 = overlayPt.X;

                Canvas.SetLeft(hitBoxCanvas, overlayPt.X - (int)(hitBoxCanvas.Width / 2));
                Canvas.SetTop(hitBoxCanvas, 0);
            }
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
