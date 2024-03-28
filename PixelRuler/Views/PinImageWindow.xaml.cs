using PixelRuler.Common;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PixelRuler.Views
{
    public class PinViewModel : ViewModelBase
    {
        public PinViewModel(PixelRulerViewModel prvm)
        {
            this.MainViewModel = prvm;
        }


        public RelayCommand CloseCommand { get; set; }

        public PixelRulerViewModel MainViewModel { get; set; }

    }

    /// <summary>
    /// Interaction logic for PinImageWindow.xaml
    /// </summary>
    public partial class PinImageWindow : Window
    {
        public PinImageWindow()
        {
            InitializeComponent();
            this.DataContextChanged += PinImageWindow_DataContextChanged;
            this.MouseLeftButtonDown += PinImageWindow_PreviewMouseDown;
            this.MouseMove += PinImageWindow_PreviewMouseMove;
            this.MouseLeftButtonUp += PinImageWindow_PreviewMouseUp;

            this.MouseEnter += PinImageWindow_MouseEnter;
            this.MouseLeave += PinImageWindow_MouseLeave;
        }

        private void PinImageWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            var storyboard = gripBorder.Resources["MoveBorderStoryboard"] as Storyboard;
            storyboard.Stop();

            storyboard = closeButton.Resources["buttonFadeOut"] as Storyboard;
            storyboard.SetValue(Storyboard.TargetProperty, closeButton);
            storyboard.Begin();
        }

        private void PinImageWindow_MouseEnter(object sender, MouseEventArgs e)
        {
            var storyboard = gripBorder.Resources["MoveBorderStoryboard"] as Storyboard;
            storyboard.Begin();

            storyboard = closeButton.Resources["buttonFadeIn"] as Storyboard;
            storyboard.SetValue(Storyboard.TargetProperty, closeButton);
            storyboard.Begin();
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

        public PinViewModel? ViewModel
        {
            get
            {
                return this.DataContext as PinViewModel;
            }
            set
            {
                if(this.DataContext != value)
                {
                    this.DataContext = value;
                    if(this.ViewModel != null)
                    {
                        this.ViewModel.CloseCommand = new RelayCommand((object? o) => this.Close());
                    }
                }
            }
        }

        private void PinImageWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var img = (this.DataContext as PinViewModel).MainViewModel.Image;
            rectGeom.Rect = new Rect(0, 0, img.Width, img.Height);
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private (bool? left, bool? top) getSides(Border border, Point pt)
        {
            bool? left = null;
            bool? top = null;
            int margin = 40;
            if (pt.X < margin)
            {
                left = true;
            }
            if(pt.X > border.ActualWidth - margin)
            {
                left = false;
            }

            if (pt.Y < margin)
            {
                top = true;
            }
            if (pt.Y > border.ActualHeight - margin)
            {
                top = false;
            }
            return (left, top);
        }

        private void SetCursor(Border border, bool? left, bool? top)
        {
            if(left is null)
            {
                border.Cursor = Cursors.SizeNS;
            }
            else if(top is null)
            {
                border.Cursor = Cursors.SizeWE;
            }
            else if(left == top)
            {
                border.Cursor = Cursors.SizeNWSE;
            }
            else
            {
                border.Cursor = Cursors.SizeNESW;
            }
        }

        private void SetBorderCursor(Border border, MouseEventArgs e)
        {
            var pt = e.GetPosition(border);
            var (left, top) = getSides(border, pt);
            SetCursor(border, left, top);
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Width = 1000;
            var border = sender as Border;
            SetBorderCursor(border, e);
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if(isResizing)
            {
                var newPoint = e.GetPosition(this);
                newPoint = this.PointToScreen(newPoint);
                var deltaPoint = newPoint - sizeStartPoint;
                this.Width = sizeOrigWidth + deltaPoint.X;
                this.Height = sizeOrigHeight + deltaPoint.Y;
                this.Background = new SolidColorBrush(Colors.Red);
            }
            else
            {
                var border = sender as Border;
                SetBorderCursor(border, e);
            }
        }

        bool isResizing = false;
        bool? sizeFromLeft = null;
        bool? sizeFromTop = null;
        double sizeOrigLeft;
        double sizeOrigTop;
        double sizeOrigWidth;
        double sizeOrigHeight;
        Point sizeStartPoint;
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pt = e.GetPosition(border);
            (sizeFromLeft, sizeFromTop) = getSides(border, pt);
            isResizing = true;
            var startPoint = e.GetPosition(this);
            sizeStartPoint = this.PointToScreen(startPoint);
            this.sizeOrigHeight = this.Height;
            this.sizeOrigWidth = this.Width;
            this.sizeOrigLeft = this.Left;
            this.sizeOrigTop = this.Top;
            this.border.CaptureMouse();
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isResizing = false;

        }
    }
}
