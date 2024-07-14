using PixelRuler.CanvasElements;
using PixelRuler.Common;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PixelRuler
{
    class ColorPickElement : AbstractZoomCanvasShape
    {
        Rectangle innerRect;
        Rectangle outerRect;

        public ColorPickElement(Canvas canvas) : base(canvas)
        {
            this.innerRect = UiUtils.CreateRectangle();
            innerRect.SnapsToDevicePixels = false;
            innerRect.UseLayoutRounding = false;
            innerRect.Stroke = new SolidColorBrush(UiConsts.CurrentPixelInnerStroke);
            Canvas.SetZIndex(innerRect, App.COLOR_PICKER_INDEX);
            
            this.outerRect = UiUtils.CreateRectangle();
            outerRect.SnapsToDevicePixels = false;
            outerRect.UseLayoutRounding = false;
            outerRect.Stroke = new SolidColorBrush(UiConsts.CurrentPixelOuterStroke);
            Canvas.SetZIndex(outerRect, App.COLOR_PICKER_INDEX - 1);

            AddToOwnerCanvas();
        }


        public override void UpdateForZoomChange()
        {
            innerRect.Width = 1 + 1 * getUIUnit();
            innerRect.Height = 1 + 1 * getUIUnit();
            outerRect.Width = 1 + 3 * getUIUnit();
            outerRect.Height = 1 + 3 * getUIUnit();
            //innerRect.LayoutTransform = new TranslateTransform(-.5 * getBoundingBoxStrokeThickness(), -.5 * getBoundingBoxStrokeThickness());
            innerRect.StrokeThickness = getUIUnit();
            outerRect.StrokeThickness = getUIUnit();
            SetPosition(this.point);
            //var st = BoundingBoxLabel.RenderTransform as ScaleTransform;
            //st.ScaleX = 1.0 / this.owningCanvas.GetScaleTransform().ScaleX;
            //st.ScaleY = 1.0 / this.owningCanvas.GetScaleTransform().ScaleY;
        }

        public override void Clear()
        {
            this.owningCanvas.Children.Remove(innerRect);
            this.owningCanvas.Children.Remove(outerRect);
            //this.owningCanvas.Children.Remove(BoundingBoxLabel);
        }

        internal void SetPosition(Point point)
        {
            this.point = point;
            Canvas.SetLeft(innerRect, point.X - .5 * getUIUnit());
            Canvas.SetTop(innerRect, point.Y - .5 * getUIUnit());

            Canvas.SetLeft(outerRect, point.X - 1.5 * getUIUnit());
            Canvas.SetTop(outerRect, point.Y - 1.5 * getUIUnit());
        }

        private Point point;

        public override void AddToOwnerCanvas()
        {
            owningCanvas.Children.Add(innerRect);
            owningCanvas.Children.Add(outerRect);
        }

        public void SetState()
        {
            this.SetVisibility();
            this.UpdateForZoomChange();
        }

        private void SetVisibility()
        {
            if (owningCanvas.GetScaleTransform().ScaleX < 4)
            {
                this.innerRect.Visibility = Visibility.Collapsed;
                this.outerRect.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.innerRect.Visibility = Visibility.Visible;
                this.outerRect.Visibility = Visibility.Visible;
            }
        }
    }
}
