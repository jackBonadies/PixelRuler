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
        public GuidelineElement GuidelineElement { get; init; }
        public Gridline OwningGridLine { get; init; }

        public GuidelineTick(Gridline gridLine, GuidelineElement guidelineElement)
        {
            GuidelineElement = guidelineElement;
            OwningGridLine = gridLine;
        }

        public void AddToGridline()
        {
            var gridLineCoor = this.GuidelineElement.ImageCoordinate * this.OwningGridLine.Scale + 10000;
            var tickLine = new Line
            {
                X1 = 0,
                X2 = 30,
                Y1 = 0,
                Y2 = 30,
                StrokeThickness = 1,
                Stroke = new SolidColorBrush(Colors.Aqua),
                SnapsToDevicePixels = true,
                UseLayoutRounding = true,
            };
            tickLine.X1 = tickLine.X2 = gridLineCoor;
            OwningGridLine.canvas.Children.Add(tickLine);
        }
    }
}
