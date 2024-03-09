using PixelRuler.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PixelRuler.CanvasElements
{
    public class GuidelineTick
    {
        public GuidelineElement? GuidelineElement { get; init; }
        public Gridline OwningGridLine { get; init; }
        /// <summary>
        /// For current indicators
        /// </summary>
        public int ImageCoordinate { get; set; }
        public Line tickLine { get; set; }
        public enum GridlineTickType
        {
            Guideline = 0,
            CurrentMarker = 1,
        }

        public GridlineTickType TickType { get; private set; }
        

        public GuidelineTick(Gridline gridLine, GuidelineElement? guidelineElement, GridlineTickType tickType)
        {
            GuidelineElement = guidelineElement;
            OwningGridLine = gridLine;
            TickType = tickType;
            tickLine = new Line
            {
                X1 = 0,
                X2 = 30,
                Y1 = 0,
                Y2 = 30,
                StrokeThickness = .8,
                Stroke = new SolidColorBrush(
                    tickType == GridlineTickType.Guideline ? 
                    Colors.Aqua : Colors.Aqua),
                SnapsToDevicePixels = true,
                UseLayoutRounding = true,
            };
            RenderOptions.SetEdgeMode(tickLine, EdgeMode.Aliased);
        }


        public void AddToGridline()
        {
            UpdatePosition();
            if (!OwningGridLine.canvas.Children.Contains(tickLine))
            {
                OwningGridLine.canvas.Children.Add(tickLine);
            }
        }

        public void UpdatePosition()
        {
            var gridLineCoor = getImageCoordinate() * this.OwningGridLine.Scale + 10000;
            tickLine.X1 = tickLine.X2 = gridLineCoor + .5; // TODO: Why
        }

        private int getImageCoordinate()
        {
            if (this.GuidelineElement != null)
            {
                return this.GuidelineElement.ImageCoordinate;
            }
            else
            {
                return this.ImageCoordinate;
            }
        }
    }
}
