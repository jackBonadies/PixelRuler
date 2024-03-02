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
using static System.Formats.Asn1.AsnWriter;

namespace PixelRuler.Views
{
    /// <summary>
    /// Interaction logic for Gridline.xaml
    /// </summary>
    public partial class Gridline : UserControl
    {
        public double Scale { get; private set; }
        private int startCoor = int.MaxValue;
        private int endCoor = int.MaxValue;

        public bool IsVertical
        {
            get { return (bool)GetValue(IsVerticalProperty); }
            set { SetValue(IsVerticalProperty, value); }
        }

        public static readonly DependencyProperty IsVerticalProperty
            = DependencyProperty.Register(
                  "IsVertical",
                  typeof(bool),
                  typeof(Gridline),
                  new PropertyMetadata(false)
              );

        public MainCanvas MainCanvas { get; set; }

        public Gridline()
        {
            InitializeComponent();

            //SetTickmarks(1);
            this.Cursor = Cursors.Hand;
            this.Loaded += Gridline_Loaded;
        }

        private void Gridline_Loaded(object sender, RoutedEventArgs e)
        {
            SetBorder();
            canvas.Width = 20000;
        }

        private void SetBorder()
        {
            var line = new Line()
            {
                X1 = 0,
                X2 = 20000,
                Y1 = 0,
                Y2 = 0,
                Stroke = new SolidColorBrush(Color.FromRgb(0x90, 0x90, 0x90)),
                StrokeThickness = 1,
            };
            this.canvas.Children.Add(line);
            Canvas.SetLeft(line, 0);
            Canvas.SetTop(line, 0);
            Canvas.SetZIndex(line, 10);

            line = new Line()
            {
                X1 = 0,
                X2 = 20000,
                Y1 = 30,
                Y2 = 30,
                Stroke = new SolidColorBrush(Color.FromRgb(0x90, 0x90, 0x90)),
                StrokeThickness = 1,
            };
            this.canvas.Children.Add(line);
            Canvas.SetLeft(line, 0);
            Canvas.SetTop(line, 0);
            Canvas.SetZIndex(line, 10);
        }

        public void SetZoom(double scale)
        {
            if(this.Scale == scale)
            {
                return;
            }

            startCoor = int.MaxValue;
            endCoor = int.MaxValue;

            this.Scale = scale;

            this.canvas.Children.Clear();
            SetBorder();
            UpdateTickmarks();
            SetGuidelineTicks();
        }

        private void SetGuidelineTicks()
        {
            foreach(var tick in guideLineTicks)
            {
                tick.AddToGridline();
            }
        }

        private void AddTickmarksForRange(int start, int end)
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
            int minorTickInterval = 5;


            majorTickSpacing = Math.Max(majorTickSpacing, 5);
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
                        StrokeThickness = .8,
                        SnapsToDevicePixels = true,
                        UseLayoutRounding = true
                    };

                    if (curVal % majorTickSpacing == 0)
                    {
                        var txtBlock = new TextBlock()
                        {
                            Text = curVal.ToString(),
                            Foreground = new SolidColorBrush(Color.FromRgb(0x90, 0x90, 0x90)),
                        };
                        if(this.IsVertical)
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
                        Canvas.SetLeft(txtBlock, curValLoc + 5 + 10000);
                        Canvas.SetTop(txtBlock, 0);
                        line.Y1 = 0;
                        line.Y2 = 30;
                    }
                    else if ((curVal * 2) % majorTickSpacing == 0)
                    {
                        line.Y1 = 14;
                        line.Y2 = 30;
                    }
                    else
                    {
                        line.Y1 = 18;
                        line.Y2 = 30;
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
                this.translation.X = tt.Y + st.ScaleX * 10000 + 30;
            }
            else
            {
                this.translation.X = tt.X + st.ScaleX * 10000 + 30;
            }
        }

        public void UpdateTickmarks()
        {
            UpdateTickmarks(GetRange());
        }

        private void UpdateTickmarks((int start, int end) newRange)
        {
            if(startCoor == int.MaxValue || endCoor == int.MaxValue)
            {
                AddTickmarksForRange(newRange.start, newRange.end);
                startCoor = newRange.start;
                endCoor = newRange.end;
            }
            else
            {
                if(newRange.start < startCoor)
                {
                    AddTickmarksForRange(newRange.start, startCoor);
                    startCoor = newRange.start;
                }
                if(endCoor < newRange.end)
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
            if(this.IsVertical)
            {
                start = (int)startPoint.Y;
                end = (int)endPont.Y;
            }
            return (start - 1, end + 1);
        }

        private List<GuidelineTick> guideLineTicks = new List<GuidelineTick>();
        internal void AddTick(GuidelineTick gridLineTick)
        {
            guideLineTicks.Add(gridLineTick);
            gridLineTick.AddToGridline();
        }
    }
}
