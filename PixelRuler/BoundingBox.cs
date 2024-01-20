using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace DesignRuler
{
    public class BoundingBox
    {
        private const bool marching_ants = true;
        private readonly SolidColorBrush brush1 = new SolidColorBrush(Colors.Black);
        private readonly SolidColorBrush brush2 = new SolidColorBrush(Colors.White);

        Canvas owningCanvas;

        public BoundingBox(Canvas owningCanvas, Point startPoint) 
        {
            this.owningCanvas = owningCanvas;

            rect1 = createRectangle();
            rect1.Stroke = brush1;

            this.owningCanvas.Children.Add(rect1);
            Canvas.SetZIndex(rect1, 500);

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
            Canvas.SetZIndex(rect2, 501);

            StartPoint = startPoint;

            BoundingBoxLabel = new BoundingBoxLabel();
            BoundingBoxLabel.RenderTransform = new ScaleTransform() { ScaleX = 1.0, ScaleY = 1.0 };

            this.owningCanvas.Children.Add(BoundingBoxLabel);
            Canvas.SetZIndex(BoundingBoxLabel, 500);

            UpdateForZoomChange();
        }

        private Rectangle createRectangle()
        {
            var rect = new Rectangle();
            rect.Width = 0;
            rect.Height = 0;
            rect.StrokeThickness = 1;
            rect.SnapsToDevicePixels = true;
            return rect;
        }

        private double getBoundingBoxStrokeThickness()
        {
            // we need to perform dpi scaling here bc our parent undid dpi scaling
            var dpi = owningCanvas.GetDpi();
            return dpi / this.owningCanvas.GetScaleTransform().ScaleX;
        }

        private void SetShapeState(Rectangle rect)
        {
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

        private void SetShapeState()
        {
            SetShapeState(rect1);
            SetShapeState(rect2);

        }

        public Point StartPoint
        {
            get; private set;
        }

        private Point endPoint;
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
        private Rectangle rect2;

        public BoundingBoxLabel BoundingBoxLabel
        {
            get;
            set;
        }

        public void UpdateForZoomChange()
        {
            rect1.StrokeThickness = getBoundingBoxStrokeThickness();
            rect2.StrokeThickness = getBoundingBoxStrokeThickness();
            var st = BoundingBoxLabel.RenderTransform as ScaleTransform;
            st.ScaleX = 1.0 / this.owningCanvas.GetScaleTransform().ScaleX;
            st.ScaleY = 1.0 / this.owningCanvas.GetScaleTransform().ScaleY;
        }

        internal void Clear()
        {
            this.owningCanvas.Children.Remove(rect1);
            this.owningCanvas.Children.Remove(rect2);
            this.owningCanvas.Children.Remove(BoundingBoxLabel);
        }
    }
}
