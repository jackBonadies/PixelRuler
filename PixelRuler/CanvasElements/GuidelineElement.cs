using PixelRuler.Common;
using System;
using System.Collections.Generic;
using System.Linq;
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
            mainLine.X1 = 0;
            mainLine.X2 = 10000;
            mainLine.Y1 = 0;
            mainLine.Y2 = 10000;
            mainLine.IsHitTestVisible = false;


            this.coordinate = coordinate;
            this.isHorizontal = isHorizontal;

 
            mainLine.Stroke = new SolidColorBrush(Colors.Aqua);
            mainLine.StrokeThickness = 1;

            hitBoxCanvas = new Canvas();
            hitBoxCanvas.Cursor = Cursors.SizeWE;
            if(isHorizontal)
            {
                hitBoxCanvas.Width = 10000;
                hitBoxCanvas.Height = 5;
            }
            else
            {
                hitBoxCanvas.Width = 5;
                hitBoxCanvas.Height = 10000;
            }
            hitBoxCanvas.Background = new SolidColorBrush(Colors.Transparent);

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
            
            var overlayPt = this.mainCanvas.mainImage.TranslatePoint(new Point(coordinate, coordinate), this.mainCanvas.overlayCanvas);
            if (isHorizontal)
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
}
