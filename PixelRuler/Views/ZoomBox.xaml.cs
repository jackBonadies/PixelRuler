﻿using PixelRuler.CanvasElements;
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
using System.Xml;

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
        private ZoomViewModel zoomViewModel;
        private VisualBrush zoomboxBrush;
        private int borderThickness = -1;

        public ZoomBox(MainCanvas owningCanvas, double windowSize, PixelRulerViewModel mainViewModel)
        {
            this.DataContext = mainViewModel;
            this.zoomViewModel = mainViewModel.Settings.ZoomViewModel;
            Visual visualForBrush = useCanvas ? owningCanvas.innerCanvas : owningCanvas.mainImage;
            
            InitializeComponent();

            this.zoomCanvas.Children.Add(line1);

            this.owningCanvas = owningCanvas;
            this.owningCanvas.EffectiveZoomChanged += OnEffectiveZoomChanged;
            ZoomWindowSize = windowSize;
            ZoomFactor = zoomViewModel.ZoomFactor;
            borderThickness = zoomViewModel.BorderThickness;

            zoomCanvas.Width = ZoomWindowSize;
            zoomCanvas.Height = ZoomWindowSize;
            this.IsHitTestVisible = false;
            //zoomBox.StrokeThickness = 4;
            //zoomBox.Stroke = new SolidColorBrush(Color.FromArgb(127,255,255,255));

            int offset = useCanvas ? 0 : 10000;

            zoomboxBrush = new VisualBrush(visualForBrush)
            {
                Viewbox = new Rect(offset, offset, ZoomWindowSize, ZoomWindowSize),
                ViewboxUnits = BrushMappingMode.Absolute,
                Transform = new ScaleTransform(TotalZoom, TotalZoom, 0, 0),
            };
            imageBackgroundBorder.Background = zoomboxBrush;

            currentPixelIndicator = new Rectangle()
            {
                Width = TotalZoom,
                Height = TotalZoom,
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

            currentLineGuideVert.SetResourceReference(Line.StrokeProperty, App.AnnotationColorKey);
            currentLineGuideHorz.SetResourceReference(Line.StrokeProperty, App.AnnotationColorKey);
            currentPixelIndicator.SetResourceReference(Line.StrokeProperty, App.AnnotationColorKey);

            Canvas.SetLeft(currentPixelIndicator, ZoomWindowSize/2);
            Canvas.SetTop(currentPixelIndicator, ZoomWindowSize/2);

            zoomCanvas.Children.Add(currentLineGuideVert);
            zoomCanvas.Children.Add(currentLineGuideHorz);
            zoomCanvas.Children.Add(currentPixelIndicator);

            this.Visibility = Visibility.Collapsed;
        }

        private void OnEffectiveZoomChanged(object? sender, double e)
        {
            this.zoomboxBrush.Transform = new ScaleTransform(TotalZoom, TotalZoom);
        }

        private double getZoomRelativeToCanvas()
        {
            return TotalZoom / owningCanvas.EffectiveZoom;
        }

        private double TotalZoom
        {
            get
            {
                if(zoomViewModel.ZoomMode == ZoomMode.Fixed)
                {
                    return Math.Min(zoomViewModel.ZoomFactor, zoomViewModel.ZoomLimitEffectiveZoom);
                }
                else if(zoomViewModel.ZoomMode == ZoomMode.Relative)
                {
                    return Math.Min(zoomViewModel.ZoomFactor * owningCanvas.EffectiveZoom, zoomViewModel.ZoomLimitEffectiveZoom);
                }
                else
                {
                    throw new Exception("Zoom Mode Unexpected");
                }
            }
        }

        public void OnOverlayCanvasMouseMove()
        {
            SetPositions();
        }



        Line line = null;
        private void SetPositions()
        {
            Point innerCanvasLocation = default;
            Point overlayCanvasLocation = default;

            innerCanvasLocation = Mouse.GetPosition(owningCanvas.innerCanvas);
            overlayCanvasLocation = Mouse.GetPosition(owningCanvas.overlayCanvas);
            var translatedPoint1 = owningCanvas.innerCanvas.TranslatePoint(new Point(10200, 10000), this);


            bool floatXPos = false;
            bool floatYPos = false;

            if (currentZoomBoxInfo != null)
            {
                if(currentZoomBoxInfo.MeasEl is RulerElement ruler)
                {
                    var innerCanvasLocationToTrack = (currentZoomBoxInfo.Tag is true) ? currentZoomBoxInfo.MeasEl.StartPoint : currentZoomBoxInfo.MeasEl.EndPoint;

                    if(App.FloatingZoomBoxPosAllowed)
                    {
                        if (ruler.IsHorizontal())
                        {
                            floatYPos = true;
                            innerCanvasLocationToTrack.Y = innerCanvasLocation.Y;
                        }
                        else
                        {
                            floatXPos = true;
                            innerCanvasLocationToTrack.X = innerCanvasLocation.X;
                        }
                    }

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
                        _ => throw new NotImplementedException(),
                    };

                    var canvasLocY = sizerEnum.GetYFlag() switch
                    {
                        SizerPosY.Above => currentZoomBoxInfo.MeasEl.StartPoint.Y,
                        SizerPosY.Centered => (currentZoomBoxInfo.MeasEl.StartPoint.Y + currentZoomBoxInfo.MeasEl.EndPoint.Y) / 2.0,
                        SizerPosY.Below => currentZoomBoxInfo.MeasEl.EndPoint.Y,
                        _ => throw new NotImplementedException(),
                    };

                    if (App.FloatingZoomBoxPosAllowed)
                    {
                        if (sizerEnum.GetXFlag() == SizerPosX.Centered)
                        {
                            floatXPos = true;
                            canvasLocX = innerCanvasLocation.X;
                        }

                        if (sizerEnum.GetYFlag() == SizerPosY.Centered)
                        {
                            floatYPos = true;
                            canvasLocY = innerCanvasLocation.Y;
                        }
                    }

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
                //Canvas.SetLeft(this, overlayCanvasLocation.X - boxWidth / 2);
                //Canvas.SetTop(this, overlayCanvasLocation.Y + 28);// - outerBorder.ActualHeight / 2 + 156);
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
                    innerCanvasLocation.X - (ZoomWindowSize / 2) / TotalZoom,
                    innerCanvasLocation.Y - (ZoomWindowSize / 2) / TotalZoom,
                    ZoomWindowSize,
                    ZoomWindowSize);

            //TODO: this is just bottom Y.. among other things.
            var outOfBoundY = Canvas.GetTop(this) + boxHeight - this.owningCanvas.ActualHeight;
            if(outOfBoundY > 0)
            {
                Canvas.SetTop(this, Canvas.GetTop(this) - outOfBoundY);
            }


            // left have to offset by width/2
            var pixelSize = TotalZoom;
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

            if (line is null)
            {
                line = new Line()
                {
                    X1 = translatedPoint1.X,
                    X2 = translatedPoint1.X,
                    Y1 = 0,
                    Y2 = 20000,
                    StrokeThickness = 1 / this.GetDpi(),
                    Stroke = new SolidColorBrush(Colors.Red)
                };
                this.zoomCanvas.Children.Add(line);
            }
            else
            {
                var zoomBox = Canvas.GetLeft(this) + this.zoomCanvas.Width / 2;
                line.X1 = (100 - zoomBox) * ZoomFactor;
                line.X2 = (100 - zoomBox) * ZoomFactor;
            }
                

            foreach(var elInfo in ZoomCanvasElementInfo)
            {
                if(elInfo.Item1 is RulerElement r1)
                {

                    var finalPtStart = getZoomBoxCoorFromZoomCanvas(r1.StartPoint);
                    var finalPtEnd = getZoomBoxCoorFromZoomCanvas(r1.EndPoint);

                    var lineIt = (elInfo.Item2[0] as Line);
                    // scale it *ZoomFactor from the center
                    (lineIt).X1 = finalPtStart.X - borderThickness;
                    (lineIt).X2 = finalPtEnd.X - borderThickness;
                    (lineIt).Y1 = finalPtStart.Y - borderThickness;
                    (lineIt).Y2 = finalPtEnd.Y - borderThickness;

                }
                else if (elInfo.Item1 is BoundingBoxElement b1)
                {

                    var finalPtStart = getZoomBoxCoorFromZoomCanvas(b1.StartPoint);
                    var finalPtEnd = getZoomBoxCoorFromZoomCanvas(b1.EndPoint);

                    var rectIt = (elInfo.Item2[0] as Rectangle);
                    // scale it *ZoomFactor from the center
                    Canvas.SetLeft(rectIt, finalPtStart.X - borderThickness);
                    Canvas.SetTop(rectIt, finalPtStart.Y - borderThickness);
                    rectIt.Width = finalPtEnd.X - finalPtStart.X;
                    rectIt.Height = finalPtEnd.Y - finalPtStart.Y; //TODO exception

                }
                else if (elInfo.Item1 is GuidelineElement g1)
                {
                    var finalPtStart = getZoomBoxCoorFromZoomCanvas(new Point(g1.Coordinate, g1.Coordinate));

                    var lineIt = (elInfo.Item2[0] as Line);

                    (lineIt).X1 = 0;
                    (lineIt).X2 = this.zoomCanvas.Width;
                    (lineIt).Y1 = 0;
                    (lineIt).Y2 = this.zoomCanvas.Height;

                    if(g1.IsHorizontal)
                    {
                        (lineIt).Y1 = finalPtStart.Y - borderThickness;
                        (lineIt).Y2 = finalPtStart.Y - borderThickness;
                    }
                    else
                    {
                        (lineIt).X1 = finalPtStart.X - borderThickness;
                        (lineIt).X2 = finalPtStart.X - borderThickness;
                    }
                }
                //elInfo.Item1.UpdateZoomCanvasElements(elInfo.Item2, zoomBox, ZoomFactor,)
            }
        }
        private Line line1 = new Line() { Stroke = new SolidColorBrush(Colors.Aqua), StrokeThickness = 1, UseLayoutRounding = false, SnapsToDevicePixels=true };


        private Point getZoomBoxCoorFromZoomCanvas(Point zoomCanvasPt)
        {
            // zoomCanvasPt is 10k, 10k..
            var overlayPt = this.owningCanvas.innerCanvas.TranslatePoint(zoomCanvasPt, this.Parent as Canvas);
            // this is overlay pt
            var mainImageX = overlayPt.X + offsetAmount;
            var mainImageY = overlayPt.Y + offsetAmount;

            // where it is on the zoom box
            var zoomBoxX = Canvas.GetLeft(this) + 0;
            var zoomBoxY = Canvas.GetTop(this) + 0;
            var zoomBoxStartX = mainImageX - zoomBoxX;
            var zoomBoxStartY = mainImageY - zoomBoxY;

            // scale it *ZoomFactor from the center
            var finalX = ((zoomBoxStartX) - this.ActualWidth / 2) * getZoomRelativeToCanvas() + this.ActualWidth / 2;
            var finalY = ((zoomBoxStartY) - this.ActualHeight / 2) * getZoomRelativeToCanvas() + this.ActualHeight / 2;

            return new Point(finalX, finalY);
        }


        private double offsetAmount = 0;

        public void UpdateForElementResize(MeasurementElementZoomCanvasShape? measurementElementZoomCanvasShape, MeasureElementResizeData e)
        {
            this.currentZoomBoxInfo = e;
            //Canvas.SetLeft(currentPixelIndicator, measurementElementZoomCanvasShape.EndPoint - (this.zoomCanvas.Background as VisualBrush).Viewbox.Left xOffset);
            //Canvas.SetTop(currentPixelIndicator, center + yOffset);
        }

        MeasureElementResizeData? currentZoomBoxInfo;
        ZoomBoxCase currentZoomBoxCase = ZoomBoxCase.None;

        public void Show(MeasureElementResizeData? measEl, MouseEventArgs? e, ZoomBoxCase zoomBoxCase)
        {
            if(currentZoomBoxCase == zoomBoxCase)
            {
                // since keydown fires repeatedly..
                return;
            }

            if(TotalZoom <= owningCanvas.EffectiveZoom)
            {
                return;
            }

            currentZoomBoxCase = zoomBoxCase;
            this.Visibility = Visibility.Visible;
            currentZoomBoxInfo = measEl;

            zoomCanvas.Children.Clear(); // TODO need to readd the other guidelines....
                                         //this.currentLineGuideVert.Visibility = Visibility.Collapsed;
                                         //this.currentLineGuideHorz.Visibility = Visibility.Collapsed;
                                         //this.currentPixelIndicator.Visibility = Visibility.Collapsed;
            colorDisplay.Visibility = currentZoomBoxCase == ZoomBoxCase.ColorPicker ? Visibility.Visible : Visibility.Collapsed;  

            switch (currentZoomBoxCase)
            {
                case ZoomBoxCase.ColorPicker:
                    this.currentPixelIndicator.Visibility = Visibility.Visible;
                    zoomCanvas.Children.Add(currentPixelIndicator);
                    break;
                case ZoomBoxCase.Resizer:
                    if (measEl.MeasEl is RulerElement r)
                    {
                        if (r.IsHorizontal())
                        {
                            this.currentLineGuideVert.Visibility = Visibility.Visible;
                            zoomCanvas.Children.Add(currentLineGuideVert);
                        }
                        else
                        {
                            this.currentLineGuideHorz.Visibility = Visibility.Visible;
                            zoomCanvas.Children.Add(currentLineGuideHorz);
                        }
                    }
                    else if (measEl.MeasEl is BoundingBoxElement b)
                    {
                        var sizerEnum = (SizerEnum)measEl.Tag;
                        if (sizerEnum.IsBottom() || sizerEnum.IsTop())
                        {
                            this.currentLineGuideHorz.Visibility = Visibility.Visible;
                            zoomCanvas.Children.Add(currentLineGuideHorz);
                        }
                        if (sizerEnum.IsLeft() || sizerEnum.IsRight())
                        {
                            this.currentLineGuideVert.Visibility = Visibility.Visible;
                            zoomCanvas.Children.Add(currentLineGuideVert);
                        }
                    }
                    break;
                case ZoomBoxCase.ScreenshotBoundSelection:
                    // same as resizer for corner
                    this.currentLineGuideHorz.Visibility = Visibility.Visible;
                    zoomCanvas.Children.Add(currentLineGuideHorz);
                    this.currentLineGuideVert.Visibility = Visibility.Visible;
                    zoomCanvas.Children.Add(currentLineGuideVert);
                    break;
                case ZoomBoxCase.QuickZoom:
                    ZoomCanvasElementInfo.Clear();
                    // copy a version of current annotations
                    foreach (var measurementEl in owningCanvas.MeasurementElements)
                    {
                        // keep dictionary measElOriginal -> zoombox version
                        // update zoombox version from orig..
                        var canvasElemenets = measurementEl.GetZoomCanvasElements();
                        ZoomCanvasElementInfo.Add((measurementEl, canvasElemenets));
                        foreach(var canvasEl in canvasElemenets)
                        {
                            zoomCanvas.Children.Add(canvasEl);
                        }
                        //measurementEl.UpdateZoomCanvasElements();
                        //measurementEl.UpdateZoomCanvasElements();
                        //this.zoomCanvas.Children.Add();
                    }
                    break;
            }

            SetPositions();
        }

        private List<(MeasurementElementZoomCanvasShape, List<UIElement>)> ZoomCanvasElementInfo = new();

        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
            currentZoomBoxCase = ZoomBoxCase.None;
        }

        public ZoomBox()
        {
            InitializeComponent();
        }
    }
}
