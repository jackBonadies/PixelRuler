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
using System.Windows.Shapes;

namespace PixelRuler.Views
{
    /// <summary>
    /// Interaction logic for PinImageWindow.xaml
    /// </summary>
    public partial class PinImageWindow : Window
    {
        public PinImageWindow()
        {
            InitializeComponent();
            this.DataContextChanged += PinImageWindow_DataContextChanged;
            this.PreviewMouseDown += PinImageWindow_PreviewMouseDown;
            this.PreviewMouseMove += PinImageWindow_PreviewMouseMove;
            this.PreviewMouseUp += PinImageWindow_PreviewMouseUp;
        }

        /// <summary>
        /// Native DragMove kicks off windows MoveWindow loop which is lower latency then handling
        ///   at our level.  But also does not allow one to move the top of the window offscreen.
        /// </summary>
        private bool _useCustomMovement = false;

        private void PinImageWindow_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if(_useCustomMovement)
            {
                isMoving = false;
                this.ReleaseMouseCapture();
            }
        }

        private void PinImageWindow_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if(_useCustomMovement)
            {
                if(!isMoving)
                {
                    return;
                }
                var pt = e.GetPosition(this);
                pt = this.PointToScreen(pt); 

                var deltaX = (pt - startMovePos).X * .8;
                var deltaY = (pt - startMovePos).Y * .8;
                //startMovePos = new Point(startMovePos.X + deltaX , startMovePos.Y + deltaY);
                this.Left = startLeft + deltaX;
                this.Top = startTop + deltaY;
            }
        }

        private void PinImageWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(_useCustomMovement)
            {
                this.CaptureMouse();
                startMovePos = e.GetPosition(this);
                startMovePos = this.PointToScreen(startMovePos); 
                startLeft = this.Left;
                startTop = this.Top;
                isMoving = true;
            }
            else
            {
                this.DragMove();
            }
        }

        double startLeft;
        double startTop;
        bool isMoving = false;
        Point startMovePos;

        private void PinImageWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var img = (this.DataContext as PixelRulerViewModel).Image;
            rectGeom.Rect = new Rect(0, 0, img.Width, img.Height);
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {

        }
    }
}
