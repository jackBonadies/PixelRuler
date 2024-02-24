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
        private double scale;
        private int startCoor;
        private int endCoor;

        public Gridline()
        {
            InitializeComponent();
            SetTickmarks(1);
            SetBorder();
            canvas.Width = 20000;
            this.Cursor = Cursors.Hand;
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
        }
        public void SetZoom(double scale)
        {
            SetTickmarks(scale);
        }

        private void SetTickmarks(double scale)
        {
            int majorTickSpacing = (int)(100 / scale);
            if(scale == 8)
            {
                majorTickSpacing = 10;
            }
            if (scale == 16)
            {
                // cant do 4 (since 50 / 4)
                majorTickSpacing = 5;
            }
            int minorTickInterval = 5;

            this.canvas.Children.Clear();
            SetBorder();
            int minorTickSpacing = majorTickSpacing / minorTickInterval;
            int curVal = -1920;
            while(curVal < 1920*2)
            {
                if(curVal % minorTickSpacing == 0)
                {
                    var curValLoc = curVal * scale;
                    var line = new Line()
                    {
                        X1 = curValLoc + 10000,
                        X2 = curValLoc + 10000,
                        Stroke = new SolidColorBrush(Color.FromRgb(0x90, 0x90, 0x90)),
                        StrokeThickness = 1,
                    };

                    if(curVal % majorTickSpacing == 0)
                    {
                        var txtBlock = new TextBlock()
                        {
                            Text = curVal.ToString(),
                            Foreground = new SolidColorBrush(Color.FromRgb(0x90, 0x90, 0x90)), 
                        };
                        canvas.Children.Add(txtBlock);
                        Canvas.SetLeft(txtBlock, curValLoc + 5 + 10000);
                        Canvas.SetTop(txtBlock, 0);
                        line.Y1 = 0;
                        line.Y2 = 30;
                    }
                    else if((curVal * 2) % majorTickSpacing == 0)
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

        public void UpdateLinesForRange(double x1, double x2)
        {
        }
    }
}
