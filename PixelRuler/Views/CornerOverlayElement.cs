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
        private Canvas owningCanvas;

        private Path mainPath;

        public Point StartPoint
        {
            set
            {
                pathFigure.StartPoint = value;
            }
        }

        public Point MidPoint
        {
            set
            {
                lineSegment1.Point = value;
            }
        }

        public Point EndPoint
        {
            set
            {
                lineSegment2.Point = value;
            }
        }

        private PathFigure pathFigure;
        private LineSegment lineSegment1;
        private LineSegment lineSegment2;
        // Function to create the Path
        public void ConfigurePath()
        {
            mainPath = new Path
            {
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round
            };

            PathGeometry pathGeometry = new PathGeometry();

            pathFigure = new PathFigure
            {
                StartPoint = new Point(10, 10),
                IsClosed = false
            };

            lineSegment1 = new LineSegment(new Point(10, 20), true);
            lineSegment2 = new LineSegment(new Point(10, 20), true);
            PathSegmentCollection pathSegments = new PathSegmentCollection()
            {
                lineSegment1,
                lineSegment2,
            };

            pathFigure.Segments = pathSegments;

            pathGeometry.Figures.Add(pathFigure);

            mainPath.Data = pathGeometry;
        }

        public CornerOverlayElement(Canvas owningCanvas, SizerEnum corner)
        {
            this.owningCanvas = owningCanvas;
            this.ConfigurePath();
            Corner = corner;

            mainPath!.StrokeThickness = 3;
            mainPath.Stroke = new SolidColorBrush(Colors.Red);

            this.owningCanvas.Children.Add(mainPath);
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

            var midX = Point.X + offsetX;
            var midY = Point.Y + offsetY;

            var startX = Point.X + sizeX + offsetX;
            var startY = midY;

            var endX = midX;
            var endY = Point.Y + sizeY + offsetY;

            StartPoint = new Point(startX, startY); 
            MidPoint = new Point(midX, midY);
            EndPoint = new Point(endX, endY);
        }

    }
}
