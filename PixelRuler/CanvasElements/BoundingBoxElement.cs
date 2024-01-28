﻿using PixelRuler.CanvasElements;
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
            Canvas.SetZIndex(rect1, 500);

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
                Canvas.SetZIndex(rect2, 501);
            }

            StartPoint = startPoint;

            BoundingBoxLabel = new BoundingBoxLabel();
            BoundingBoxLabel.RenderTransform = new ScaleTransform() { ScaleX = 1.0, ScaleY = 1.0 };

            this.owningCanvas.Children.Add(BoundingBoxLabel);
            Canvas.SetZIndex(BoundingBoxLabel, 500);

            UpdateForZoomChange();
        }

        private void SetShapeState(Rectangle? rect)
        {
            if(rect == null)
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
        private Rectangle? rect2;

        public BoundingBoxLabel BoundingBoxLabel
        {
            get;
            set;
        }

        public override void UpdateForZoomChange()
        {
            rect1.StrokeThickness = getUIUnit();
            if(rect2 != null)
            {
                rect2.StrokeThickness = getUIUnit();
            }
            var st = BoundingBoxLabel.RenderTransform as ScaleTransform;
            st.ScaleX = 1.0 / this.owningCanvas.GetScaleTransform().ScaleX;
            st.ScaleY = 1.0 / this.owningCanvas.GetScaleTransform().ScaleY;
        }

        public override void Clear()
        {
            this.owningCanvas.Children.Remove(rect1);
            this.owningCanvas.Children.Remove(rect2);
            this.owningCanvas.Children.Remove(BoundingBoxLabel);
        }

        public override void SetEndPoint(System.Windows.Point roundedPoint)
        {
            this.EndPoint = roundedPoint;
        }

        public override void SetSelectedState()
        {
            
        }

        public override bool IsEmpty
        {
            get
            {
                return this.Width == 0 && this.Height == 0;
            }
        }
    }
}
