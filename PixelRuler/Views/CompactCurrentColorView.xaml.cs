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
    /// Interaction logic for CompactCurrentColorView.xaml
    /// </summary>
    public partial class CompactCurrentColorView : UserControl
    {
        public CompactCurrentColorView(MainCanvas owningCanvas, PixelRulerViewModel mainViewModel)
        {
            InitializeComponent();
            this.owningCanvas = owningCanvas;
            this.Loaded += CompactCurrentColorView_Loaded;
        }

        MainCanvas owningCanvas;


        public void OnOverlayCanvasMouseMove()
        {
            SetPositions();
        }

        public void Show(MouseButtonEventArgs e)
        {
            this.Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void CompactCurrentColorView_Loaded(object sender, RoutedEventArgs e)
        {
            SetupForDpi();
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            SetupForDpi();
            base.OnDpiChanged(oldDpi, newDpi);
        }

        private void SetupForDpi()
        {
            var dpi = this.GetDpi();
            this.LayoutTransform = new ScaleTransform(dpi, dpi);
        }

        private void SetPositions()
        {
            Point innerCanvasLocation = default;
            Point overlayCanvasLocation = default;

            innerCanvasLocation = Mouse.GetPosition(owningCanvas.innerCanvas);
            overlayCanvasLocation = Mouse.GetPosition(owningCanvas.overlayCanvas);
            var translatedPoint1 = owningCanvas.innerCanvas.TranslatePoint(new Point(10200, 10000), this);


            bool floatXPos = false;
            bool floatYPos = false;

            this.Measure(new Size(double.MaxValue, double.MaxValue));
            var boxWidth = this.DesiredSize.Width;
            var boxHeight = this.DesiredSize.Height;

            SizerPosX boxOffsetX = SizerPosX.Right;
            SizerPosY boxOffsetY = SizerPosY.Below;


            var offsetX = 20;
            var offsetY = 8;
            if (boxOffsetX is SizerPosX.Centered)
            {
                Canvas.SetLeft(this, overlayCanvasLocation.X - boxWidth / 2 + offsetX);
            }
            else if (boxOffsetX is SizerPosX.Left)
            {
                Canvas.SetLeft(this, overlayCanvasLocation.X - boxWidth - offsetX);
            }
            else if (boxOffsetX is SizerPosX.Right)
            {
                Canvas.SetLeft(this, overlayCanvasLocation.X + offsetX);
            }


            if (boxOffsetY is SizerPosY.Centered)
            {
                //Canvas.SetTop(this, overlayCanvasLocation.Y - boxHeight / 2 - ZoomFactor / 2.0);
                Canvas.SetTop(this, overlayCanvasLocation.Y - boxHeight / 2);
            }
            else if (boxOffsetY is SizerPosY.Above)
            {
                Canvas.SetTop(this, overlayCanvasLocation.Y - boxHeight - offsetY);
            }
            else if (boxOffsetY is SizerPosY.Below)
            {
                Canvas.SetTop(this, overlayCanvasLocation.Y + offsetY);// - outerBorder.ActualHeight / 2 + 156);
            }
        }

    }
}
