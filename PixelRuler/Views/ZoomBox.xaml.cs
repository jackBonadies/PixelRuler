using PixelRuler.CanvasElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PixelRuler.Views
{
    /// <summary>
    /// Interaction logic for ZoomBox.xaml
    /// </summary>
    public partial class ZoomBox : UserControl
    {
        public double ZoomWindowSize;
        public double ZoomFactor;
        MainCanvas owningCanvas;
        Rectangle currentPixelIndicator;
        readonly bool useCanvas = false;
        public ZoomBox(MainCanvas owningCanvas, double windowSize, double zoomFactor)
        {
            Visual visualForBrush = useCanvas ? owningCanvas.innerCanvas : owningCanvas.mainImage;
            
            InitializeComponent();

            this.owningCanvas = owningCanvas;
            ZoomWindowSize = windowSize;
            ZoomFactor = zoomFactor;

            zoomCanvas.Width = ZoomWindowSize;
            zoomCanvas.Height = ZoomWindowSize;
            this.IsHitTestVisible = false;
            //zoomBox.StrokeThickness = 4;
            //zoomBox.Stroke = new SolidColorBrush(Color.FromArgb(127,255,255,255));

            int offset = useCanvas ? 0 : 10000;

            zoomCanvas.Background = //new SolidColorBrush(Colors.Red);
                
            new VisualBrush(visualForBrush)
            {
                Viewbox = new Rect(offset, offset, ZoomWindowSize, ZoomWindowSize),
                ViewboxUnits = BrushMappingMode.Absolute,
                Transform = new ScaleTransform(ZoomFactor, ZoomFactor, 0, 0),
            };

            currentPixelIndicator = new Rectangle()
            {
                Width = ZoomFactor,
                Height = ZoomFactor,
                Stroke = new SolidColorBrush(Colors.Red),
                StrokeThickness = 1,
            };
            Canvas.SetLeft(currentPixelIndicator, ZoomWindowSize/2);
            Canvas.SetTop(currentPixelIndicator, ZoomWindowSize/2);

            zoomCanvas.Children.Add(currentPixelIndicator);
        }

        public void OnOverlayCanvasMouseMove(Point overlayCanvasLocation, Point innerCanvasLocation)
        {
            // innerCanvasLocation is the zoom canvas so 10k
            // overlayCanvasLocation is location on the overlay canvas i.e. b/t 0 and screen size
            Canvas.SetLeft(this, overlayCanvasLocation.X - ZoomWindowSize / 2);
            Canvas.SetTop(this, overlayCanvasLocation.Y - ZoomWindowSize / 2 + 140);

            //var transform = owningCanvas.TransformToAncestor(owningCanvas);
            //var pt = owningCanvas.innerCanvas.RenderTransform.Transform(overlayCanvasLocation);
            //var offsetX = useCanvas ? 0 : owningCanvas.CanvasTranslateTransform.X / owningCanvas.CanvasScaleTransform.ScaleX;
            //var offsetY = useCanvas ? 0 : owningCanvas.CanvasTranslateTransform.Y / owningCanvas.CanvasScaleTransform.ScaleY;

            (this.zoomCanvas.Background as VisualBrush).Viewbox =
                new Rect(
                    innerCanvasLocation.X - (ZoomWindowSize / 2) / ZoomFactor,
                    innerCanvasLocation.Y - (ZoomWindowSize / 2) / ZoomFactor,
                    ZoomWindowSize,
                    ZoomWindowSize);


            // left have to offset by width/2
            var pixelSize = ZoomFactor;
            currentPixelIndicator.Width = pixelSize;
            currentPixelIndicator.Height = pixelSize;
            var center = ZoomWindowSize / 2;// - pixelSize / 2;
            var xOffset = (int)(innerCanvasLocation.X) * pixelSize - innerCanvasLocation.X * pixelSize;
            var yOffset = (int)(innerCanvasLocation.Y) * pixelSize - innerCanvasLocation.Y * pixelSize;
            ////var xOffset = innerCanvasLocation.X - Math.Round((innerCanvasLocation.X / pixelSize)) * pixelSize;
            //var yOffset = innerCanvasLocation.Y - Math.Round((innerCanvasLocation.Y / pixelSize)) * pixelSize;

            Canvas.SetLeft(currentPixelIndicator, center + xOffset);
            Canvas.SetTop(currentPixelIndicator, center + yOffset);
        }

        public void UpdateForElementResize(MeasurementElementZoomCanvasShape? measurementElementZoomCanvasShape, object e)
        {
            //Canvas.SetLeft(currentPixelIndicator, measurementElementZoomCanvasShape.EndPoint - (this.zoomCanvas.Background as VisualBrush).Viewbox.Left xOffset);
            //Canvas.SetTop(currentPixelIndicator, center + yOffset);
        }

        public ZoomBox()
        {
            InitializeComponent();
        }
    }
}
