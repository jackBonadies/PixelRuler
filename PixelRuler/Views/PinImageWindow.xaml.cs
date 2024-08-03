using PixelRuler.Common;
using PixelRuler.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

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
            this.SourceInitialized += PinImageWindow_SourceInitialized;
            this.DataContextChanged += PinImageWindow_DataContextChanged;
            this.MouseDown += PinImageWindow_PreviewMouseDown;
            this.MouseMove += PinImageWindow_PreviewMouseMove;
            this.MouseUp += PinImageWindow_PreviewMouseUp;
            this.LostFocus += PinImageWindow_LostFocus;
            this.PreviewKeyUp += PinImageWindow_PreviewKeyUp;

            this.MouseEnter += PinImageWindow_MouseEnter;
            this.MouseLeave += PinImageWindow_MouseLeave;
        }

        private void PinImageWindow_LostFocus(object sender, RoutedEventArgs e)
        {
            this.Cursor = null;
        }

        private void PinImageWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            var storyboard = gripNotch.Resources["MoveBorderStoryboard"] as Storyboard;
            storyboard.Stop();

            storyboard = buttonPanel.Resources["buttonPanelFadeOut"] as Storyboard;
            storyboard.SetValue(Storyboard.TargetProperty, buttonPanel);
            storyboard.Begin();
            if (this.IsFocused)
            {

            }

            this.Cursor = null;
        }

        private void PinImageWindow_MouseEnter(object sender, MouseEventArgs e)
        {
            var storyboard = gripNotch.Resources["MoveBorderStoryboard"] as Storyboard;
            storyboard.Begin();

            storyboard = buttonPanel.Resources["buttonPanelFadeIn"] as Storyboard;
            storyboard.SetValue(Storyboard.TargetProperty, buttonPanel);
            storyboard.Begin();
        }


        private void PinImageWindow_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!isMoveKey(e.ChangedButton))
            {
                return;
            }
            if (useCustomMovement(e.ChangedButton))
            {
                isCustomMoving = false;
                this.ReleaseMouseCapture();
            }
            this.Cursor = null;
        }

        private void PinImageWindow_SourceInitialized(object? sender, EventArgs e)
        {
            var dpi = this.GetDpi();
            var scale = 1 / dpi;
            reverseImageDpi.ScaleX = scale;
            reverseImageDpi.ScaleY = scale;
        }

        private void PinImageWindow_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!isCustomMoving)
            {
                return;
            }
            var pt = e.GetPosition(this);
            pt = this.PointToScreen(pt);

            var deltaX = (pt - startMovePos).X;
            var deltaY = (pt - startMovePos).Y;

            //startMovePos = new Point(startMovePos.X + deltaX , startMovePos.Y + deltaY);
            var dpi = this.GetDpi();
            this.Left = startLeft + deltaX / dpi;
            this.Top = startTop + deltaY / dpi;
        }

        private bool isMoveKey(MouseButton mouseButton)
        {
            return mouseButton == MouseButton.Left || mouseButton == MouseButton.Middle;
        }

        private bool useCustomMovement(MouseButton mouseButton)
        {
            return mouseButton != MouseButton.Left;
        }

        private void PinImageWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isMoveKey(e.ChangedButton))
            {
                return;
            }

            this.Cursor = Cursors.SizeAll;
            if (useCustomMovement(e.ChangedButton))
            {
                this.CaptureMouse();
                startMovePos = e.GetPosition(this);
                startMovePos = this.PointToScreen(startMovePos);
                startLeft = this.Left;
                startTop = this.Top;
                isCustomMoving = true;
            }
            else
            {
                this.DragMove();
            }
        }

        double startLeft;
        double startTop;
        /// <summary>
        /// Native DragMove kicks off windows MoveWindow loop which is lower latency then handling
        ///   at our level.  But also does not allow one to move the top of the window offscreen.
        /// </summary>
        bool isCustomMoving = false;
        Point startMovePos;

        public PinViewModel? ViewModel
        {
            get
            {
                return this.DataContext as PinViewModel;
            }
            set
            {
                if (this.DataContext != value)
                {
                    this.DataContext = value;
                    if (this.ViewModel != null)
                    {
                        this.ViewModel.CloseCommand = new RelayCommand((object? o) => this.Close());
                        this.ViewModel.MinimizeCommand = new RelayCommand((object? o) => this.WindowState = WindowState.Minimized);
                    }
                }
            }
        }

        private void PinImageWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var img = this.ViewModel.MainViewModel.Image;
            mainImage.Width = img.Width;
            mainImage.Height = img.Height;
            rectGeom.Rect = new Rect(0, 0, img.Width, img.Height);
        }

        private void PinImageWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.OemPlus)
            {
                set_zoom(2);
            }
            else if (e.Key == Key.OemMinus)
            {
                set_zoom(.5);
            }
        }


        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private SizerEnum getSides(Border border, Point pt)
        {
            bool? left = null;
            bool? top = null;
            int margin = 40;
            if (pt.X < margin)
            {
                left = true;
            }
            if (pt.X > border.ActualWidth - margin)
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

            return (left, top) switch
            {
                (null, true) => SizerEnum.TopCenter,
                (null, false) => SizerEnum.BottomCenter,
                (true, null) => SizerEnum.CenterLeft,
                (false, null) => SizerEnum.CenterRight,
                (false, true) => SizerEnum.TopRight,
                (false, false) => SizerEnum.BottomRight,
                (true, true) => SizerEnum.TopLeft,
                (true, false) => SizerEnum.BottomLeft,
                _ => throw new Exception("Unexpected Case")
            };
        }

        private void SetCursor(Border border, SizerEnum sizer)
        {
            var x = sizer.GetXFlag();
            var y = sizer.GetYFlag();
            if (x == SizerPosX.Centered)
            {
                border.Cursor = Cursors.SizeNS;
            }
            else if (y == SizerPosY.Centered)
            {
                border.Cursor = Cursors.SizeWE;
            }
            else if ((int)x == (int)y)
            {
                border.Cursor = Cursors.SizeNESW;
            }
            else
            {
                border.Cursor = Cursors.SizeNWSE;
            }
        }

        private void SetBorderCursor(Border border, MouseEventArgs e)
        {
            var pt = e.GetPosition(border);
            SizerEnum sizer = getSides(border, pt);
            SetCursor(border, sizer);
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Width = 1000;
            var border = sender as Border;
            SetBorderCursor(border, e);
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (isResizing)
            {
                var t = System.Windows.Input.Mouse.GetPosition(this);
                var newPoint = e.GetPosition(this);
                newPoint = this.PointToScreen(newPoint);
                var deltaPoint = newPoint - sizeStartPoint;

                var ratioX = deltaPoint.X / sizeOrigWidth;
                var ratioY = deltaPoint.Y / sizeOrigHeight;
                var ratio = Math.Max(Math.Abs(ratioX), Math.Abs(ratioY));

                bool useX = false;
                if (sizingFrom.IsCorner())
                {
                    if (Math.Abs(ratioX) > Math.Abs(ratioY))
                    {
                        useX = true;
                    }
                    else
                    {
                        useX = false;
                    }
                }
                else if (sizingFrom.GetXFlag() == SizerPosX.Centered)
                {
                    useX = false;
                }
                else if (sizingFrom.GetYFlag() == SizerPosY.Centered)
                {
                    useX = true;
                }
                else
                {
                    throw new Exception();
                }

                if (!useX && sizingFrom.IsTop())
                {
                    ratio = -ratioY;
                }
                else if (useX && sizingFrom.IsLeft())
                {
                    ratio = -ratioX;
                }
                else if (useX)
                {
                    ratio = ratioX;
                }
                else
                {
                    ratio = ratioY;
                }

                deltaPoint = new Vector(ratio * sizeOrigWidth, ratio * sizeOrigHeight); 

                if (sizingFrom.IsLeft())
                {
                    //deltaPoint = new Vector(deltaPoint.X, deltaPoint.Y);
                    this.Left = sizeOrigLeft - deltaPoint.X / this.GetDpi();
                }
                if (sizingFrom.IsTop())
                {
                    //deltaPoint = new Vector(deltaPoint.X, -deltaPoint.Y);
                    this.Top = sizeOrigTop - deltaPoint.Y / this.GetDpi();
                }
                
                setPinWindowSize(sizeOrigWidth + deltaPoint.X, sizeOrigHeight + deltaPoint.Y);
            }
            else
            {
                SetBorderCursor(gripBorder, e);
            }
        }

        private void setPinWindowSize(double newWidth, double newHeight)
        {
            mainImage.Width = newWidth;
            mainImage.Height = newHeight;
            rectGeom.Rect = new Rect(0, 0, newWidth, newHeight);
        }

        bool isResizing = false;
        SizerEnum sizingFrom;
        double sizeOrigLeft;
        double sizeOrigTop;
        double sizeOrigWidth;
        double sizeOrigHeight;
        Point sizeStartPoint;
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pt = e.GetPosition(gripBorder);
            sizingFrom = getSides(gripBorder, pt);
            isResizing = true;
            var startPoint = e.GetPosition(this);
            sizeStartPoint = this.PointToScreen(startPoint);
            this.sizeOrigHeight = this.mainImage.Height;
            this.sizeOrigWidth = this.mainImage.Width;
            this.sizeOrigLeft = this.Left;
            this.sizeOrigTop = this.Top;
            if (!gripBorder.CaptureMouse())
            {

            }
            e.Handled = true;
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isResizing = false;
            gripBorder.ReleaseMouseCapture();
        }

        private void doAnimation()
        {
            double durationSeconds = .2;
            var s = new Storyboard();
            var ca = new ColorAnimation()
            {
                Duration = TimeSpan.FromSeconds(durationSeconds),
                To = System.Windows.Media.Color.FromRgb(255, 255, 255),
                //EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut, Power = 2 },
                AutoReverse = true,
            };
            Storyboard.SetTargetProperty(ca, new PropertyPath("BorderBrush.Color"));
            Storyboard.SetTarget(ca, mainBorder);
            s.Children.Add(ca);
            s.Begin();
        }

        private void pulseAnimation()
        {
            double durationSeconds = .13;
            double extent = 30;
            var s = new Storyboard();

            var widthRatio = (mainBorder.ActualWidth + extent) / mainBorder.ActualWidth;
            var heightRatio = (mainBorder.ActualHeight + extent) / mainBorder.ActualHeight;

            var d1 = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(durationSeconds),
                From = 1,
                To = widthRatio,
                EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut, Power = 2 },
                AutoReverse = true,
            };
            Storyboard.SetTargetProperty(d1, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTarget(d1, mainBorder);
            s.Children.Add(d1);

            var d2 = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(durationSeconds),
                From = 1,
                To = heightRatio,
                EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut, Power = 2 },
                AutoReverse = true,
            };
            Storyboard.SetTargetProperty(d2, new PropertyPath("RenderTransform.ScaleY"));
            Storyboard.SetTarget(d2, mainBorder);
            s.Children.Add(d2);

            s.Begin();
        }

        private void mainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //doAnimation();
        }

        private void mainBorder_Loaded(object sender, RoutedEventArgs e)
        {
            doAnimation();
        }

        private double current_zoom = 1;

        private void set_zoom(double new_zoom)
        {
            var newWidth = this.ViewModel.MainViewModel.Image.Width * new_zoom;
            var newHeight = this.ViewModel.MainViewModel.Image.Height * new_zoom;

            var screen = this.GetRelevantScreen();
            if ((newWidth < minPinWidth && newWidth < this.ViewModel.MainViewModel.Image.Width) || 
                (newWidth > screen.Bounds.Width && newWidth > this.ViewModel.MainViewModel.Image.Width) ||
                (newHeight < minPinHeight && newHeight < this.ViewModel.MainViewModel.Image.Height) || 
                (newHeight > screen.Bounds.Height && newHeight > this.ViewModel.MainViewModel.Image.Height))
            {
                return;
            }

            var zoomAmount = new_zoom / current_zoom;
            current_zoom = new_zoom;

            mainImage.Width = newWidth;
            mainImage.Height = newHeight;
            rectGeom.Rect = new Rect(0, 0, newWidth, newHeight);

            var pointToKeepAtLocation = System.Windows.Input.Mouse.GetPosition(this);

            var newX = zoomAmount * pointToKeepAtLocation.X;
            var oldX = pointToKeepAtLocation.X;

            var newY = zoomAmount * pointToKeepAtLocation.Y;
            var oldY = pointToKeepAtLocation.Y;

            var diffToCorrectX = newX - oldX;
            var diffToCorrectY = newY - oldY;

            // too much flicker...
            this.Left -= diffToCorrectX;
            this.Top -= diffToCorrectY;

            // resize window 
            // WPF uses DirectX and effectively double buffers for all its rendering
            //   but when you resize a window DX requires recreating the device context 
            //zoomAmountText.
            var size = Math.Min(mainImage.Width, mainImage.Height);
            var ratio = Math.Max(size / 250, 1);
            zoomAmountBlurb.LayoutTransform = new ScaleTransform(ratio, ratio);
            zoomAmountText.Text = $"{current_zoom * 100}%";
            (zoomAmountBlurb.Resources["fadeInOutStory"] as Storyboard).Begin();
        }

        private int minPinWidth = 100;
        private int minPinHeight = 70;

        MouseWheelAccumulator mouseWheelAccumulator = new();
        private void mainBorder_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var zoomAmount = mouseWheelAccumulator.Zoom(e.Delta);
            if (zoomAmount == 0)
            {
                return;
            }

            if (zoomAmount < 1)
            {
                set_zoom(current_zoom / 2);
            }
            else
            {
                set_zoom(current_zoom * 2);
            }

        }
    }
}
