using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DesignRuler
{
    public class BoundingBox
    {
        Canvas owningCanvas;

        public BoundingBox(Canvas owningCanvas, Point startPoint) 
        {
            this.owningCanvas = owningCanvas;
            Shape = new Rectangle();
            Shape.Width = 0;
            Shape.Height = 0;
            Shape.Stroke = new SolidColorBrush(Colors.Red);
            StartPoint = startPoint;

            BoundingBoxLabel = new BoundingBoxLabel();
            BoundingBoxLabel.RenderTransform = new ScaleTransform() { ScaleX = 1.0, ScaleY = 1.0 };

            this.owningCanvas.Children.Add(BoundingBoxLabel);
            Canvas.SetZIndex(BoundingBoxLabel, 500);

            this.owningCanvas.Children.Add(Shape);
            Canvas.SetZIndex(Shape, 500);

            UpdateForZoomChange();
        }

        private double getBoundingBoxStrokeThickness()
        {
            return 1.0 / this.owningCanvas.GetScaleTransform().ScaleX;
        }

        private void SetShapeState()
        {
            if(StartPoint.X <= EndPoint.X)
            {
                Canvas.SetLeft(Shape, StartPoint.X);
                Shape.Width = Math.Max(EndPoint.X - Canvas.GetLeft(Shape), 0);
            }
            else
            {
                Canvas.SetLeft(Shape, EndPoint.X);
                Shape.Width = Math.Max(StartPoint.X - EndPoint.X, 0);
            }

            if (StartPoint.Y <= EndPoint.Y)
            {
                Canvas.SetTop(Shape, StartPoint.Y);
                Shape.Height = Math.Max(EndPoint.Y - Canvas.GetTop(Shape), 0);
            }
            else
            {
                Canvas.SetTop(Shape, EndPoint.Y);
                Shape.Height = Math.Max(StartPoint.Y - EndPoint.Y, 0);
            }
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
            Canvas.SetLeft(BoundingBoxLabel, EndPoint.X);
            BoundingBoxLabel.BoundingBoxWidth = (int)Shape.Width;
            BoundingBoxLabel.BoundingBoxHeight = (int)Shape.Height;
        }

        public Rectangle Shape
        {
            get;
            set;
        }

        public BoundingBoxLabel BoundingBoxLabel
        {
            get;
            set;
        }

        public void UpdateForZoomChange()
        {
            Shape.StrokeThickness = getBoundingBoxStrokeThickness();
            var st = BoundingBoxLabel.RenderTransform as ScaleTransform;
            st.ScaleX = 1.0 / this.owningCanvas.GetScaleTransform().ScaleX;
            st.ScaleY = 1.0 / this.owningCanvas.GetScaleTransform().ScaleY;
        }
    }
}
