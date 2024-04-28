using PixelRuler.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PixelRuler.Views
{
    public class CornerOverlayElement
    {
        private Line horizontalLine;
        private Line verticalLine; 
        private Canvas owningCanvas; 
        public CornerOverlayElement(Canvas owningCanvas, SizerEnum corner)
        {
            this.owningCanvas = owningCanvas;
            Corner = corner;

            horizontalLine = UiUtils.CreateLine();
            horizontalLine.StrokeThickness = 3;
            horizontalLine.Stroke = new SolidColorBrush(Colors.Red);
            verticalLine = UiUtils.CreateLine();
            verticalLine.StrokeThickness = 3;
            verticalLine.Stroke = new SolidColorBrush(Colors.Red);

            this.owningCanvas.Children.Add(horizontalLine);
            this.owningCanvas.Children.Add(verticalLine);
        }

        public SizerEnum Corner { get; init; }
        public Point Point { get; set; }

        private readonly double offsetExtent = 3;
        private readonly double sizeExtent = 16;
        public void Update()
        {
            double offsetX = 0;
            double offsetY = 0;
            double sizeX = 0;
            double sizeY = 0;
            switch(Corner)
            {
                case SizerEnum.TopLeft:
                    offsetX = -offsetExtent;
                    offsetY = -offsetExtent;
                    sizeX = sizeExtent;
                    sizeY = sizeExtent;
                    break;
                case SizerEnum.TopRight:
                    offsetX = offsetExtent;
                    offsetY = -offsetExtent;
                    sizeX = -sizeExtent;
                    sizeY = sizeExtent;
                    break;
                case SizerEnum.BottomLeft:
                    offsetX = -offsetExtent;
                    offsetY = offsetExtent;
                    sizeX = sizeExtent;
                    sizeY = -sizeExtent;
                    break;
                case SizerEnum.BottomRight:
                    offsetX = offsetExtent;
                    offsetY = offsetExtent;
                    sizeX = -sizeExtent;
                    sizeY = -sizeExtent;
                    break;
            }

            horizontalLine.X1 = Point.X + sizeX + offsetX;
            horizontalLine.X2 = Point.X + offsetX + offsetX / 2.0;
            horizontalLine.Y1 = Point.Y + offsetY;
            horizontalLine.Y2 = Point.Y + offsetY;

            verticalLine.X1 = Point.X + offsetX;
            verticalLine.X2 = Point.X + offsetX;
            verticalLine.Y1 = Point.Y + sizeY + offsetY;
            verticalLine.Y2 = Point.Y + offsetY + offsetY / 2.0;
        }

    }
}
