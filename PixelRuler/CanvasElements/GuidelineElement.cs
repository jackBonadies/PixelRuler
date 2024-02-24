using PixelRuler.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PixelRuler.CanvasElements
{
    public class GuidelineElement : IOverlayCanvasShape
    {
        private bool isHorizontal = false;
        private double coordinate = double.NaN;

        private Line mainLine;
        private Canvas hitBoxCanvas;
        private MainCanvas mainCanvas;

        public GuidelineElement(MainCanvas mainCanvas, double coordinate, bool isHorizontal)
        {
            this.mainCanvas = mainCanvas;

            mainLine = UiUtils.CreateLine();
            mainLine.Y1 = 0;
            mainLine.Y2 = 10000;
            
            this.coordinate = coordinate;
            this.isHorizontal = isHorizontal;

 
            mainLine.Stroke = new SolidColorBrush(Colors.Aqua);
            mainLine.StrokeThickness = 1;

            hitBoxCanvas = new Canvas();

        }

        public void AddToCanvas()
        {
            mainCanvas.overlayCanvas.Children.Add(hitBoxCanvas);
            mainCanvas.overlayCanvas.Children.Add(mainLine);
        }

        public void Clear()
        {
            mainCanvas.overlayCanvas.Children.Remove(hitBoxCanvas);
            mainCanvas.overlayCanvas.Children.Remove(mainLine);
        }

        public void UpdateForCoordinatesChanged()
        {
            var overlayPt = this.mainCanvas.mainImage.TranslatePoint(new Point(coordinate, 100), this.mainCanvas.overlayCanvas);
            this.mainLine.X1 = overlayPt.X;
            this.mainLine.X2 = overlayPt.X;
        }
    }
}
