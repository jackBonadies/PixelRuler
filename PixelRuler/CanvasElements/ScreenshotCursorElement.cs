using PixelRuler.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PixelRuler.CanvasElements
{
    // TODO dead code
    public class ScreenshotCursorElement : AbstractZoomCanvasShape
    {
        private Line lineHorz;
        private Line lineVert;

        public bool Visible
        {
            get
            {
                return lineHorz.Visibility == System.Windows.Visibility.Visible;
            }
            set
            { 
                var visibility = value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                lineHorz.Visibility = visibility;
                lineVert.Visibility = visibility;
            }
        }

        public ScreenshotCursorElement(Canvas owningCanvas) : base(owningCanvas)
        {
            lineHorz = UiUtils.CreateLine();
            lineHorz.Stroke = new SolidColorBrush(Colors.Red);
            lineHorz.StrokeDashArray = new DoubleCollection(new double[] { 6, 6 });
            lineHorz.StrokeThickness = getUIStrokeThicknessUnit();

            lineVert = UiUtils.CreateLine();
            lineVert.Stroke = new SolidColorBrush(Colors.Red);
            lineVert.StrokeDashArray = new DoubleCollection(new double[] { 6, 6 });
            lineVert.StrokeThickness = getUIStrokeThicknessUnit();
        }

        public void SetPosition(MouseEventArgs e)
        {
            var canvasPosition = UiUtils.RoundPoint(e.GetPosition(owningCanvas));
            lineHorz.X1 = canvasPosition.X - 1000;
            lineHorz.X2 = canvasPosition.X + 1000;
            lineHorz.Y1 = canvasPosition.Y;
            lineHorz.Y2 = canvasPosition.Y;

            lineVert.X1 = canvasPosition.X;
            lineVert.X2 = canvasPosition.X;
            lineVert.Y1 = canvasPosition.Y - 1000;
            lineVert.Y2 = canvasPosition.Y + 1000;

            lineHorz.StrokeDashOffset = lineHorz.X1;
            lineVert.StrokeDashOffset = lineVert.Y1;
        }

        public override void AddToOwnerCanvas()
        {
            owningCanvas.Children.Add(lineHorz);
            owningCanvas.Children.Add(lineVert);
        }

        public override void UpdateForZoomChange()
        {
            lineHorz.StrokeThickness = getUIUnit();
            lineVert.StrokeThickness = getUIUnit();
            //lineHorz.StrokeDashArray = new DoubleCollection(new double[] { 6 / getUIUnit(), 6 / getUIUnit() });
            //lineVert.StrokeDashArray = new DoubleCollection(new double[] { 6 / getUIUnit(), 6 / getUIUnit() });
        }

        public override void Clear()
        {
            this.owningCanvas.Children.Remove(lineHorz);
            this.owningCanvas.Children.Remove(lineVert);
        }
    }
}
