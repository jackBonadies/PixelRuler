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
        Line currentLineGuideVert;
        Line currentLineGuideHorz;
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

            imageBackgroundBorder.Background = //new SolidColorBrush(Colors.Red);
                
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
                StrokeThickness = 1,
            };

            currentLineGuideVert = new Line() 
            {
                StrokeThickness = 1,
                Y1 = 0,
                Y2 = ZoomWindowSize,
                SnapsToDevicePixels = true
            };

            currentLineGuideHorz = new Line() 
            {
                StrokeThickness = 1,
                X1 = 0,
                X2 = ZoomWindowSize,
                SnapsToDevicePixels = true
            };

            currentLineGuideVert.SetResourceReference(Line.StrokeProperty, "AnnotationColor");
            currentLineGuideHorz.SetResourceReference(Line.StrokeProperty, "AnnotationColor");
            currentPixelIndicator.SetResourceReference(Line.StrokeProperty, "AnnotationColor");

            Canvas.SetLeft(currentPixelIndicator, ZoomWindowSize/2);
            Canvas.SetTop(currentPixelIndicator, ZoomWindowSize/2);

            zoomCanvas.Children.Add(currentLineGuideVert);
            zoomCanvas.Children.Add(currentLineGuideHorz);
            zoomCanvas.Children.Add(currentPixelIndicator);

            this.Visibility = Visibility.Collapsed;
        }

        public void OnOverlayCanvasMouseMove()
        {
            SetPositions();
        }



        private void SetPositions()
        {
            Point innerCanvasLocation = default;
            Point overlayCanvasLocation = default;

            innerCanvasLocation = Mouse.GetPosition(owningCanvas.innerCanvas);
            overlayCanvasLocation = Mouse.GetPosition(owningCanvas.overlayCanvas);

            if (currentZoomBoxInfo != null)
            {
                if(currentZoomBoxInfo.MeasEl is RulerElement)
                {
                    var innerCanvasLocationToTrack = (currentZoomBoxInfo.Tag is true) ? currentZoomBoxInfo.MeasEl.StartPoint : currentZoomBoxInfo.MeasEl.EndPoint;
                    innerCanvasLocation = innerCanvasLocationToTrack;
                    overlayCanvasLocation =  this.owningCanvas.innerCanvas.TranslatePoint(innerCanvasLocationToTrack, this.Parent as Canvas);
                }
                else
                {
                    var sizerEnum = (SizerEnum)currentZoomBoxInfo.Tag;

                    var canvasLocX = sizerEnum.GetXFlag() switch
                    {
                        SizerPosX.Left => currentZoomBoxInfo.MeasEl.StartPoint.X,
                        SizerPosX.Centered => (currentZoomBoxInfo.MeasEl.StartPoint.X + currentZoomBoxInfo.MeasEl.EndPoint.X) / 2.0,
                        SizerPosX.Right => currentZoomBoxInfo.MeasEl.EndPoint.X,
                    };

                    var canvasLocY = sizerEnum.GetYFlag() switch
                    {
                        SizerPosY.Above => currentZoomBoxInfo.MeasEl.StartPoint.Y,
                        SizerPosY.Centered => (currentZoomBoxInfo.MeasEl.StartPoint.Y + currentZoomBoxInfo.MeasEl.EndPoint.Y) / 2.0,
                        SizerPosY.Below => currentZoomBoxInfo.MeasEl.EndPoint.Y,
                    };
                      

                    var innerCanvasLocationToTrack = new Point(canvasLocX, canvasLocY);
                    innerCanvasLocation = innerCanvasLocationToTrack;
                    overlayCanvasLocation =  this.owningCanvas.innerCanvas.TranslatePoint(innerCanvasLocationToTrack, this.Parent as Canvas);
                }
            }

            outerBorder.Measure(new Size(double.MaxValue, double.MaxValue));
            var boxWidth = outerBorder.DesiredSize.Width;
            var boxHeight = outerBorder.DesiredSize.Height;



            SizerPosX boxOffsetX = SizerPosX.Centered;
            SizerPosY boxOffsetY = SizerPosY.Centered;

            // innerCanvasLocation is the zoom canvas so 10k
            // overlayCanvasLocation is location on the overlay canvas i.e. b/t 0 and screen size
            if (currentZoomBoxInfo?.MeasEl is RulerElement r)
            {
                if(r.IsHorizontal())
                {
                    boxOffsetX = SizerPosX.Centered;
                    boxOffsetY = SizerPosY.Below;
                }
                else
                {
                    boxOffsetX = SizerPosX.Right;
                    boxOffsetY = SizerPosY.Centered;
                }
            }
            else if(currentZoomBoxInfo?.MeasEl is BoundingBoxElement b)
            {
                var sizerEnum = (SizerEnum)currentZoomBoxInfo.Tag;
                boxOffsetX = sizerEnum.GetXFlag();
                boxOffsetY = sizerEnum.GetYFlag();
            }
            else
            {
                Canvas.SetLeft(this, overlayCanvasLocation.X - boxWidth / 2);
                Canvas.SetTop(this, overlayCanvasLocation.Y + 28);// - outerBorder.ActualHeight / 2 + 156);
            }

            if(boxOffsetX is SizerPosX.Centered)
            {
                Canvas.SetLeft(this, overlayCanvasLocation.X - boxWidth / 2);
            }
            else if(boxOffsetX is SizerPosX.Left)
            {
                Canvas.SetLeft(this, overlayCanvasLocation.X - boxWidth - 28);
            }
            else if (boxOffsetX is SizerPosX.Right)
            {
                Canvas.SetLeft(this, overlayCanvasLocation.X + 28);
            }


            if (boxOffsetY is SizerPosY.Centered)
            {
                Canvas.SetTop(this, overlayCanvasLocation.Y - boxHeight / 2);
            }
            else if (boxOffsetY is SizerPosY.Above)
            {
                Canvas.SetTop(this, overlayCanvasLocation.Y - boxHeight - 28);
            }
            else if (boxOffsetY is SizerPosY.Below)
            {
                Canvas.SetTop(this, overlayCanvasLocation.Y + 28);// - outerBorder.ActualHeight / 2 + 156);
            }
            //var transform = owningCanvas.TransformToAncestor(owningCanvas);
            //var pt = owningCanvas.innerCanvas.RenderTransform.Transform(overlayCanvasLocation);
            //var offsetX = useCanvas ? 0 : owningCanvas.CanvasTranslateTransform.X / owningCanvas.CanvasScaleTransform.ScaleX;
            //var offsetY = useCanvas ? 0 : owningCanvas.CanvasTranslateTransform.Y / owningCanvas.CanvasScaleTransform.ScaleY;

            (this.imageBackgroundBorder.Background as VisualBrush).Viewbox =
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

            Canvas.SetLeft(currentLineGuideVert, center + xOffset);
            Canvas.SetTop(currentLineGuideVert, 0);

            Canvas.SetLeft(currentLineGuideHorz, 0);
            Canvas.SetTop(currentLineGuideHorz, center + yOffset);
        }

        public void UpdateForElementResize(MeasurementElementZoomCanvasShape? measurementElementZoomCanvasShape, MeasureElementResizeData e)
        {
            this.currentZoomBoxInfo = e;
            //Canvas.SetLeft(currentPixelIndicator, measurementElementZoomCanvasShape.EndPoint - (this.zoomCanvas.Background as VisualBrush).Viewbox.Left xOffset);
            //Canvas.SetTop(currentPixelIndicator, center + yOffset);
        }

        MeasureElementResizeData? currentZoomBoxInfo;

        public void Show(MeasureElementResizeData? measEl, MouseEventArgs? e)
        {
            this.Visibility = Visibility.Visible;
            currentZoomBoxInfo = measEl;
            if (measEl == null)
            {
                this.currentLineGuideVert.Visibility = Visibility.Collapsed;
                this.currentLineGuideHorz.Visibility = Visibility.Collapsed;
                this.currentPixelIndicator.Visibility = Visibility.Visible;
            }
            else
            {
                if (measEl.MeasEl is RulerElement r)
                {
                    if(r.IsHorizontal())
                    {
                        this.currentLineGuideHorz.Visibility = Visibility.Collapsed;
                        this.currentLineGuideVert.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        this.currentLineGuideHorz.Visibility = Visibility.Visible;
                        this.currentLineGuideVert.Visibility = Visibility.Collapsed;
                    }
                }
                else if(measEl.MeasEl is BoundingBoxElement b)
                {
                    var sizerEnum = (SizerEnum)measEl.Tag;
                    this.currentLineGuideVert.Visibility = Visibility.Collapsed;
                    this.currentLineGuideHorz.Visibility = Visibility.Collapsed;
                    if (sizerEnum.IsBottom() || sizerEnum.IsTop())
                    {
                        this.currentLineGuideHorz.Visibility = Visibility.Visible;
                    }
                    if(sizerEnum.IsLeft() || sizerEnum.IsRight())
                    {
                        this.currentLineGuideVert.Visibility = Visibility.Visible;
                    }
                }
                this.currentPixelIndicator.Visibility = Visibility.Collapsed;
            }
            SetPositions();
        }

        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
        }

        public ZoomBox()
        {
            InitializeComponent();
        }
    }
}
