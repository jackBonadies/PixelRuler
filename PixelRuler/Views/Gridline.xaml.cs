using PixelRuler.CanvasElements;
using PixelRuler.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PixelRuler.Views
{
    /// <summary>
    /// Interaction logic for Gridline.xaml
    /// </summary>
    /// <remarks>
    /// This control has an inverse dpi scale (so that horizontally 1 pixel
    ///   is 1 pixel).  But that does mean we have to scale the UI elements 
    ///   manually (i.e. height and text size).
    /// </remarks>
    public partial class Gridline : UserControl
    {
        public double Scale { get; private set; } = -1;
        private int startCoor = int.MaxValue;
        private int endCoor = int.MaxValue;
        private int borderSizePixels;

        public bool IsVertical
        {
            get;
            set;
        }

        public MainCanvas MainCanvas { get; set; }
        private GuidelineTick currentMousePosTick;

        public Gridline()
        {
            InitializeComponent();

            //SetTickmarks(1);
            this.Cursor = Cursors.Hand;
            this.Loaded += Gridline_Loaded;

            currentMousePosTick = new GuidelineTick(this, null, GuidelineTick.GridlineTickType.CurrentMarker);
        }

        public void ShowCurrentPosIndicator()
        {
            currentMousePosTick.tickLine.Visibility = Visibility.Visible;
        }

        public void HideCurrentPosIndicator()
        {
            currentMousePosTick.tickLine.Visibility = Visibility.Collapsed;
        }

        public void SetupForDpi()
        {
            borderSizePixels = UiUtils.GetBorderPixelSize(this.GetDpi());

            SetBorder();
            canvas.Width = 20000;
            canvas.Height = borderSizePixels;
        }

        private void Gridline_Loaded(object sender, RoutedEventArgs e)
        {
            SetupForDpi();
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            SetupForDpi();
            UpdateAll();
            base.OnDpiChanged(oldDpi, newDpi);
        }

        private void SetBorder()
        {
            Line line;
            //line = new Line()
            //{
            //    X1 = 0,
            //    X2 = 20000,
            //    Y1 = 1,
            //    Y2 = 1,
            //    Stroke = new SolidColorBrush(Color.FromRgb(0x90, 0x90, 0x90)),
            //    StrokeThickness = 1,
            //};
            //RenderOptions.SetEdgeMode(line, EdgeMode.Aliased);
            //this.canvas.Children.Add(line);
            //Canvas.SetLeft(line, 0);
            //Canvas.SetTop(line, 0);
            //Canvas.SetZIndex(line, 10);

            line = new Line()
            {
                X1 = 0,
                X2 = 20000,
                Y1 = borderSizePixels,
                Y2 = borderSizePixels,
                Stroke = new SolidColorBrush(Color.FromRgb(0x90, 0x90, 0x90)),
                StrokeThickness = 1,
            };
            RenderOptions.SetEdgeMode(line, EdgeMode.Aliased);
            this.canvas.Children.Add(line);
            Canvas.SetLeft(line, 0);
            Canvas.SetTop(line, 0);
            Canvas.SetZIndex(line, 10);
        }

        public void SetZoom(double scale)
        {
            if (this.Scale == scale)
            {
                return;
            }

            startCoor = int.MaxValue;
            endCoor = int.MaxValue;

            this.Scale = scale;

            UpdateAll();

            currentMousePosTick?.UpdateForZoomChanged();
        }

        private void UpdateAll()
        {
            var lines = canvas.Children.OfType<Line>();

            ClearCanvas();
            SetBorder();
            UpdateTickmarks();
            SetGuidelineTicks();
            HidePartiallyOccludedText(); //too soon???
        }

        private void ClearCanvas()
        {
            this.textBlocks.Clear();
            this.canvas.Children.Clear();
            currentMousePosTick.AddToGridline();
        }

        private void SetGuidelineTicks()
        {
            foreach (var tick in guideLineTicks)
            {
                tick.AddToGridline();
            }
        }

        private int getMajorTickSpacing()
        {
            int majorTickSpacing = (int)(100 / Scale);
            if (Scale == 8)
            {
                majorTickSpacing = 10;
            }
            if (Scale == 16)
            {
                // cant do 4 (since 50 / 4)
                majorTickSpacing = 5;
            }

            return Math.Max(majorTickSpacing, 5);

        }

        private void AddTickmarksForRange(int start, int end)
        {
            int majorTickSpacing = getMajorTickSpacing();

            int minorTickInterval = 5;
            int minorTickSpacing = Math.Max(majorTickSpacing / minorTickInterval, 1);

            int curVal = start;
            while (curVal < end)
            {
                if (curVal % minorTickSpacing == 0)
                {
                    var curValLoc = curVal * Scale;
                    var line = new Line()
                    {
                        X1 = curValLoc + 10000,
                        X2 = curValLoc + 10000,
                        Stroke = new SolidColorBrush(Color.FromRgb(0x90, 0x90, 0x90)),
                        StrokeThickness = 1,
                        SnapsToDevicePixels = true,
                        UseLayoutRounding = false
                    };
                    Canvas.SetZIndex(line, -1000);
                    RenderOptions.SetEdgeMode(line, EdgeMode.Aliased);

                    if (curVal % majorTickSpacing == 0)
                    {
                        var txtBlock = new TextBlock()
                        {
                            Text = curVal.ToString(),
                            Foreground = new SolidColorBrush(Color.FromRgb(0x90, 0x90, 0x90)),
                        };
                        txtBlock.FontSize = 10.5 * this.GetDpi();
                        if (this.IsVertical)
                        {
                            txtBlock.LayoutTransform = new TransformGroup()
                            {
                                Children =
                                {
                                    new ScaleTransform() {ScaleX = -1 },
                                }
                            };
                        }
                        canvas.Children.Add(txtBlock);
                        textBlocks.Add(txtBlock);
                        Canvas.SetLeft(txtBlock, curValLoc + 5 + 10000);
                        Canvas.SetTop(txtBlock, 0);
                        line.Y1 = 0;
                        line.Y2 = UiUtils.GetBorderPixelSize(this.GetDpi());
                    }
                    else if ((curVal * 2) % majorTickSpacing == 0)
                    {
                        line.Y1 = borderSizePixels * .55;
                        line.Y2 = borderSizePixels;
                    }
                    else
                    {
                        line.Y1 = borderSizePixels * .7;
                        line.Y2 = borderSizePixels;
                    }

                    canvas.Children.Add(line);
                    Canvas.SetLeft(line, 0);
                    Canvas.SetTop(line, 0);

                }
                curVal++;
            }
        }

        public void UpdateTranslation()
        {
            var tt = this.MainCanvas.innerCanvas.GetTranslateTransform();
            var st = this.MainCanvas.innerCanvas.GetScaleTransform();
            if (IsVertical)
            {
                this.translation.X = tt.Y + st.ScaleX * 10000 + borderSizePixels;
            }
            else
            {
                this.translation.X = tt.X + st.ScaleX * 10000 + borderSizePixels;
            }
        }

        public void UpdateTickmarks()
        {
            UpdateTickmarks(GetRange());
        }

        private void UpdateTickmarks((int start, int end) newRange)
        {
            if (startCoor == int.MaxValue || endCoor == int.MaxValue)
            {
                AddTickmarksForRange(newRange.start, newRange.end);
                startCoor = newRange.start;
                endCoor = newRange.end;
            }
            else
            {
                if (newRange.start < startCoor)
                {
                    AddTickmarksForRange(newRange.start, startCoor);
                    startCoor = newRange.start;
                }
                if (endCoor < newRange.end)
                {
                    AddTickmarksForRange(endCoor, newRange.end);
                    endCoor = newRange.end;
                }
            }

        }

        public (int start, int end) GetRange()
        {
            var startPoint = MainCanvas.overlayCanvas.TranslatePoint(new Point(0, 0), MainCanvas.mainImage);
            var endPont = MainCanvas.overlayCanvas.TranslatePoint(new Point(MainCanvas.overlayCanvas.ActualWidth, MainCanvas.overlayCanvas.ActualHeight), MainCanvas.mainImage);

            int start = (int)startPoint.X;
            int end = (int)endPont.X;
            if (this.IsVertical)
            {
                start = (int)startPoint.Y;
                end = (int)endPont.Y;
            }
            return (start - 1, end + 1);
        }

        public double GetGridlineStart()
        {
            var startPoint = MainCanvas.overlayCanvas.TranslatePoint(new Point(this.ActualHeight, this.ActualHeight), MainCanvas.mainImage);
            if (this.IsVertical)
            {
                return startPoint.Y;
            }
            else
            {
                return startPoint.X;
            }
        }


        private List<GuidelineTick> guideLineTicks = new List<GuidelineTick>();
        private List<TextBlock> textBlocks = new List<TextBlock>();

        internal void AddTick(GuidelineTick gridLineTick)
        {
            guideLineTicks.Add(gridLineTick);
            gridLineTick.AddToGridline();
        }

        internal void SetCurrentMousePosition(Point roundedPoint)
        {
            if (IsVertical)
            {
                // we should always keep a tick...
                // and then on mouse enter/leave just set visibility
                // ClearCanvas should add these back..
                currentMousePosTick.ImageCoordinate = (int)roundedPoint.Y;
            }
            else
            {
                currentMousePosTick.ImageCoordinate = (int)roundedPoint.X;
            }
            currentMousePosTick.UpdatePosition();
        }

        public void HidePartiallyOccludedText()
        {
            double begin = GetGridlineStart();
            var major = getMajorTickSpacing();

            int leftMostTick = ((int)(begin / major)) * major;
            if (begin < 0)
            {
                leftMostTick -= major;
            }

            // get left most textblock..
            var textBlock = textBlocks.Where(it => it.Text == leftMostTick.ToString()).FirstOrDefault();
            foreach(var textBlockIt in textBlocks)
            {
                textBlockIt.Visibility = Visibility.Visible;
            }
            if (textBlock != null)
            {
                var textBlockStart = Canvas.GetLeft(textBlock);
                // rel to start of gridline
                var distPoint = this.canvas.TranslatePoint(new Point(textBlockStart, textBlockStart), MainCanvas.overlayCanvas);
                double dist = distPoint.X;
                if (this.IsVertical)
                {
                    dist = distPoint.Y;
                }
                if (dist < (28 - 4))
                {
                    textBlock.Visibility = Visibility.Hidden;
                }
            }
        }

        public void UpdateForPan()
        {
            this.UpdateTranslation();
            this.UpdateTickmarks();
            this.HidePartiallyOccludedText();
        }
    }
}
