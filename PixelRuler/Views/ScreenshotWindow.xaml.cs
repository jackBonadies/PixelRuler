using PixelRuler.CanvasElements;
using PixelRuler.Common;
using PixelRuler.Models;
using PixelRuler.ViewModels;
using PixelRuler.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Data.Text;

namespace PixelRuler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WindowSelectionWindow : Window
    {
        // TODO simple viewmodel for mode and rectangle

        Rect fullBounds;
        public ScreenshotWindowViewModel ViewModel { get; private set; }

        public WindowSelectionWindow(OverlayMode mode, SettingsViewModel settings)
        {
            InitializeComponent();
            this.Loaded += WindowSelectionWindow_Loaded;
            this.SourceInitialized += WindowSelectionWindow_SourceInitialized;
            this.WindowState = WindowState.Normal;
            this.ShowInTaskbar = false;

            this.fullBounds = UiUtils.GetFullBounds(WpfScreenHelper.Screen.AllScreens);

            var scaleFactor = WpfScreenHelper.Screen.PrimaryScreen.ScaleFactor;

            this.Top = fullBounds.Top / scaleFactor;
            this.Left = fullBounds.Left / scaleFactor;
            this.Width = fullBounds.Width / scaleFactor / 1;
            this.Height = fullBounds.Height / scaleFactor / 1;
            this.WindowStyle = WindowStyle.None;
            this.Topmost = true;
            this.AllowsTransparency = true;
            this.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            this.Loaded += WindowSelectionWindow_Loaded;
            this.SourceInitialized += WindowSelectionWindow_SourceInitialized;
            this.PreviewMouseMove += WindowSelectionWindow_PreviewMouseMove;

            this.SizeChanged += WindowSelectionWindow_SizeChanged;
            this.PreviewKeyDown += WindowSelectionWindow_PreviewKeyDown;
            this.KeyDown += WindowSelectionWindow_KeyDown;
            this.MouseUp += WindowSelectionWindow_MouseUp;
            this.MouseDown += WindowSelectionWindow_MouseDown;

            var imageCoorFullBounds = TranslateFromWindowsCoordinatesToImageCoordinates(fullBounds);
            this.blurRectGeometry.Rect = imageCoorFullBounds; // offset i.e. it needs to start at 0..
            this.rectSelectionOutline.Width = 0;
            this.rectSelectionOutline.Height = 0;

            ViewModel = new ScreenshotWindowViewModel(settings);
            ViewModel.FullscreenScreenshotMode = true;
            ViewModel.Mode = mode;

            double xOffset = -fullBounds.Left;
            double yOffset = -fullBounds.Top;

            foreach(var screen in WpfScreenHelper.Screen.AllScreens)
            {
                // cannot use WpfBounds bc it will scale for that screens DPI. 
                // but our window will scale our DPI (which may be different than
                // the particular screen)

                // we still do per screen dpi scaling.  so if the window dpi is 1.5 but 
                //   a secondary monitor is 1 then scale things down.
                var individualScale = screen.ScaleFactor / scaleFactor;

                var left = (screen.Bounds.Left + xOffset) / scaleFactor;
                var top = (screen.WpfBounds.Top + yOffset) / scaleFactor;
                var perScreenPanel = new ScreenshotSelectionPerScreenPanel(screen.ScaleFactor)
                {
                    Width = (screen.Bounds.Width / scaleFactor) / individualScale,
                    Height = (screen.Bounds.Height / scaleFactor) / individualScale,
                    Margin = new Thickness(left, top, 0, 0),
                };
                perScreenPanel.Bounds = new Rect(left, top, screen.Bounds.Width / scaleFactor, screen.Bounds.Height / scaleFactor);
                perScreenPanel.perScreenDpiScaleTransform.ScaleX = individualScale;
                perScreenPanel.perScreenDpiScaleTransform.ScaleY = individualScale;
                PerScreenPanels.Add(perScreenPanel);
                this.mainContent.Children.Add(perScreenPanel);
            }
            setForMode();
            CaptureImage = UiUtils.CaptureScreen(UiUtils.GetFullBounds(WpfScreenHelper.Screen.AllScreens));
        }

        /// <summary>
        /// Image always starts at 0,0. But windows coordinates might start negative
        ///   i.e. if there is a seconary monitor to the left of the primary
        /// </summary>
        /// <returns></returns>
        private Rect TranslateFromWindowsCoordinatesToImageCoordinates(Rect windowsCoordinates)
        {
            windowsCoordinates.Offset(new Vector(-fullBounds.Left, -fullBounds.Top));
            return windowsCoordinates;
        }

        private System.Drawing.Bitmap CaptureImage;

        public List<ScreenshotSelectionPerScreenPanel> PerScreenPanels { get; init; } = new List<ScreenshotSelectionPerScreenPanel>();

        private void setForMode()
        {
            if (ViewModel.IsToolMode())
            {
                overlayCanvas.Visibility = Visibility.Collapsed;
            }
            else if (ViewModel.IsInWindowSelection())
            {
                overlayCanvas.Visibility = Visibility.Visible;
            }

            if (ViewModel.Mode.IsSelectWindow())
            {
                blurBackground.Visibility = Visibility.Visible;
                blurBackground.Fill = new SolidColorBrush(Color.FromArgb(0x80, 0, 0, 0));
            }

            if (ViewModel.Mode.IsSelectRegion())
            {
                blurBackground.Visibility = Visibility.Visible;
                blurBackground.Fill = new SolidColorBrush(Color.FromArgb(0x30, 0, 0, 0));

                horzIndicator.X1 = 0;
                horzIndicator.X2 = fullBounds.Right - fullBounds.Left;
                horzIndicator.Y1 = horzIndicator.Y2 = 300;

                vertIndicator.X1 = vertIndicator.X2 = 300;
                vertIndicator.Y1 = 0;
                vertIndicator.Y2 = fullBounds.Bottom - fullBounds.Top;
            }

            if (ViewModel.Mode == OverlayMode.QuickMeasure)
            {
                blurBackground.Visibility = Visibility.Collapsed;
            }
            else if (ViewModel.Mode == OverlayMode.QuickColor)
            {
                blurBackground.Visibility = Visibility.Collapsed;
            }
        }

        private void WindowSelectionWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void WindowSelectionWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void SetupScreenshowWindowViewModel(ScreenshotWindowViewModel viewModel)
        {
            viewModel.CloseWindowCommand = new RelayCommandFull((object? o) => this.Close(), Key.Escape, ModifierKeys.None, "Close");
        }

        private void WindowSelectionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
            // basically undo the TransformToDevice transform so that 100% zoom has 1 pixel : 1 pixel
            var dpi = this.GetDpi();
            mainCanvas.LayoutTransform = new ScaleTransform(1 / dpi, 1 / dpi);

            SetupScreenshowWindowViewModel(ViewModel);
            this.DataContext = ViewModel;
            mainCanvas.DataContext = ViewModel;
            this.ViewModel.SetImage(CaptureImage, new ScreenshotInfo());
            mainCanvas.SetImage(ViewModel.ImageSource);

            Dpi = this.GetDpi();


        }

        /// <summary>
        /// Windows rect, this is in screen coordinates
        /// i.e. if second monitor is to the left of first, the leftmost
        ///      point will be 0,0 in canvas coords, but -1920 in screen coords
        /// </summary>
        public Rect SelectedRectWinCoordinates
        {
            get; private set;
        }

        public ScreenshotInfo ScreenshotInfo
        {
            get
            {
                return new ScreenshotInfo()
                {
                    ProcessName = this.ProcessName,
                    WindowTitle = this.WindowTitle,
                    Width = (int)this.SelectedRectWinCoordinates.Width,
                    Height = (int)this.SelectedRectWinCoordinates.Height,
                    DateTime = DateTime.Now
                };
            }
        }

        public string ProcessName { get; set; }
        public string WindowTitle { get; set; }

        private Rect SelectedRegionOverlayCoordinates { get; set; }

        /// <summary>
        /// Image Coodinate Rect where 0,0 is leftmost point on virtual screen.
        /// </summary>
        private Rect selectionRegionImageCoordinates;
        public Rect SelectedRegionImageCoordinates
        {
            get
            {
                return selectionRegionImageCoordinates;
            }
            set
            {
                if (selectionRegionImageCoordinates != value)
                {
                    selectionRegionImageCoordinates = value;
                    value.Offset(fullBounds.Left, fullBounds.Top);
                    SelectedRectWinCoordinates = value;
                }
            }
        }
        private bool dragging = false;
        private Point startPoint;

        private void playScreenshotAnimation(bool windowMode)
        {
            double durationSeconds = .1;

            var cameraFlash = new Storyboard();
            var flashBrush = this.Resources["FlashBrush"] as SolidColorBrush;
            var animation = new DoubleAnimation
            {
                From = 0.0,
                To = .2,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                AutoReverse = true,
            };

            flashBrush.BeginAnimation(SolidColorBrush.OpacityProperty, animation);


            if (windowMode)
            {
                var s = new Storyboard();

                var widthRatio = (rect.Width + 10) / rect.Width;
                var heightRatio = (rect.Height + 10) / rect.Height;

                var d1 = new DoubleAnimation()
                {
                    Duration = TimeSpan.FromSeconds(durationSeconds),
                    From = 1,
                    To = widthRatio,
                    EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut, Power = 2 },
                    AutoReverse = true,
                };
                Storyboard.SetTargetProperty(d1, new PropertyPath("RenderTransform.ScaleX"));
                Storyboard.SetTarget(d1, rect);
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
                Storyboard.SetTarget(d2, rect);
                s.Children.Add(d2);

                var finalVal = rect.StrokeDashOffset;
                rect.BeginAnimation(Rectangle.StrokeDashOffsetProperty, null);
                rect.StrokeDashOffset = finalVal;

                s.Begin();
            }
            else
            {
                var s = new Storyboard();

                var d1 = new DoubleAnimation()
                {
                    Duration = TimeSpan.FromSeconds(durationSeconds),
                    From = 1,
                    To = 6,
                    EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut, Power = 2 },
                    AutoReverse = true,
                };
                Storyboard.SetTargetProperty(d1, new PropertyPath("StrokeThickness"));
                Storyboard.SetTarget(d1, rectSelectionOutline);
                s.Children.Add(d1);

                s.Begin();
            }
        }

        /// <summary>
        /// Perform dpi to get UI scaling value
        /// </summary>
        /// <returns></returns>
        private Point ForwardDpi(Point pt)
        {
            var dpi = this.GetDpi();
            return new Point(pt.X * dpi, pt.Y * dpi);
        }

        /// <summary>
        /// The canvas is dpi aware so reverse to get pixel : pixel
        /// </summary>
        /// <returns></returns>
        private Point ReverseDpi(Point pt)
        {
            var dpi = this.GetDpi();
            return new Point(pt.X / dpi, pt.Y / dpi);
        }

        /// <summary>
        /// The canvas is dpi aware so reverse to get pixel : pixel
        /// </summary>
        /// <returns></returns>
        private Rect ReverseDpi(Rect rect)
        {
            var dpi = this.GetDpi();
            rect = new Rect(
                (rect.Left) / 1.5,
                (rect.Top) / 1.5,
                (rect.Right - rect.Left) / 1.5,
                (rect.Bottom - rect.Top) / 1.5);
            return rect;
        }

        private void WindowSelectionWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            //var s = this.Resources["scaleInOut"] as Storyboard;
            //foreach(var child in s.Children)
            //{
            //    Storyboard.SetTarget(child, rect);
            //}
            //s.Begin();
            dragging = true;

            startPoint = roundToZoomCanvasPixel(e);
            startPoint = ReverseDpi(startPoint);
            innerRectGeometry.Rect = new Rect(startPoint, startPoint);
            horzIndicator.Visibility = Visibility.Collapsed;
            vertIndicator.Visibility = Visibility.Collapsed;
        }

        private void WindowSelectionWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel.Mode == OverlayMode.QuickMeasure || ViewModel.Mode == OverlayMode.QuickColor)
            {
                return;
            }

            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            if (ViewModel.Mode == OverlayMode.Window || ViewModel.Mode == OverlayMode.WindowAndRegionRect && !regionOnlyMode)
            {
                (SelectedRegionImageCoordinates, SelectedRegionOverlayCoordinates, ProcessName, WindowTitle) = SelectWindowUnderCursor();
                playScreenshotAnimation(true);
            }
            else
            {
                var startPt = ForwardDpi(startPoint);
                var imageStartPt = UiUtils.RoundPoint(this.mainCanvas.TranslatePoint(startPt, this.mainCanvas.innerCanvas));
                var imageEndPt = UiUtils.RoundPoint(this.mainCanvas.TranslatePoint(e.GetPosition(this.mainCanvas), this.mainCanvas.innerCanvas));
                imageStartPt.Offset(-10000, -10000);
                imageEndPt.Offset(-10000, -10000);
                SelectedRegionImageCoordinates = new Rect(imageStartPt, imageEndPt);
                playScreenshotAnimation(false);
            }

            void AfterScreenshot(AfterScreenshotAction a, object? additionalArg)
            {
                this.AfterScreenshotValue = a;
                this.AfterScreenshotAdditionalArg = additionalArg;
                this.DialogResult = true;
                this.Close();
            }

            if (System.Windows.Input.Keyboard.IsKeyDown(this.ViewModel.Settings.PromptKey))
            {
                double dpiToUse = -1;
                foreach (var screenPanel in PerScreenPanels)
                {
                    if (screenPanel.IsMouseWithinBounds(e))
                    {
                        dpiToUse = screenPanel.ScaleFactor;
                    }
                }

                Debug.Assert(dpiToUse != -1, "Failed to get DPI");

                Views.AfterScreenshot.ShowOptions(this.overlayCanvas, this.ViewModel.Settings, AfterScreenshot, dpiToUse);
            }
            else
            {
                AfterScreenshot(AfterScreenshotAction.ViewInPixelRulerWindow, null);
            }
        }

        public AfterScreenshotAction AfterScreenshotValue { get; set; }
        public object? AfterScreenshotAdditionalArg { get; set; }

        private void WindowSelectionWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.DialogResult = false;
                this.Close();
            }

            if (e.Key == (this.DataContext as PixelRulerViewModel).Settings.ZoomBoxQuickZoomKey)
            {
                this.mainCanvas.ShowZoomBox();
            }
        }

        private Point roundToZoomCanvasPixel(MouseEventArgs e)
        {
            var pt = e.GetPosition(this.mainCanvas.innerCanvas);
            pt = UiUtils.RoundPoint(pt);
            pt = this.mainCanvas.innerCanvas.TranslatePoint(pt, this.mainCanvas);
            return pt;
        }

        private void WindowSelectionWindow_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            foreach(var perScreenPanel in PerScreenPanels)
            {
                perScreenPanel.HandleMouse(pos);
            }

            if (ViewModel.Mode == OverlayMode.QuickMeasure || ViewModel.Mode == OverlayMode.QuickColor)
            {
                return;
            }

            if (ViewModel.Mode.IsSelectWindow() && !regionOnlyMode)
            {
                (SelectedRegionImageCoordinates, SelectedRegionOverlayCoordinates, ProcessName, WindowTitle) = SelectWindowUnderCursor();
            }

            if(ViewModel.Mode.IsSelectRegion())
            {
                var pt = roundToZoomCanvasPixel(e);
                SetCursorIndicator(pt);
                if (dragging)
                {
                    // its possible this gets called but the mouse position doesnt actually move
                    if (!regionOnlyMode)
                    {
                        // if we have not yet entered region only mode check if we have moved
                        var newPt = e.GetPosition(this.mainCanvas);
                        var res = startPoint - newPt;
                        bool moved = Math.Abs(res.X) >= 1 || Math.Abs(res.Y) > 1;
                        if (!moved)
                        {
                            return;
                        }
                    }
                    EnterRegionOnlyMode();
                    var endPt = roundToZoomCanvasPixel(e);
                    endPt = ReverseDpi(endPt);
                    innerRectGeometry.Rect = new Rect(startPoint, endPt);
                    var minX = Math.Min(innerRectGeometry.Rect.Left, innerRectGeometry.Rect.Right);
                    var maxX = Math.Max(innerRectGeometry.Rect.Left, innerRectGeometry.Rect.Right);
                    var minY = Math.Min(innerRectGeometry.Rect.Top, innerRectGeometry.Rect.Bottom);
                    var maxY = Math.Max(innerRectGeometry.Rect.Top, innerRectGeometry.Rect.Bottom);
                    Canvas.SetLeft(rectSelectionOutline, minX);
                    Canvas.SetTop(rectSelectionOutline, minY);
                    rectSelectionOutline.Width = maxX - minX + 1;
                    rectSelectionOutline.Height = maxY - minY + 1;
                }
            }
        }

        private bool regionOnlyMode = false;
        private void EnterRegionOnlyMode()
        {
            if(regionOnlyMode)
            {
                return;
            }
            regionOnlyMode = true;
            rect.Visibility = Visibility.Collapsed;
        }

        private void SetCursorIndicator(Point point)
        {
            point = ReverseDpi(point);
            vertIndicator.X1 = vertIndicator.X2 = (int)Math.Round(point.X);
            horzIndicator.Y1 = horzIndicator.Y2 = (int)Math.Round(point.Y);
            horzIndicator.StrokeDashOffset = point.X; 
            vertIndicator.StrokeDashOffset = point.Y; 
        }

        /// <summary>
        /// Given a screenpoint on the canvas, transform it to the corresponding screenpoint 
        ///   in windows (non zoomed / translated space)
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        private Point TransformPointFromZoomCanvas(Point pt)
        {
            var s = this.mainCanvas.CanvasScaleTransform;
            var t = this.mainCanvas.CanvasTranslateTransform;
            pt = new Point(pt.X * this.Dpi, pt.Y * this.Dpi);
            var ptX = (pt.X - (t.X + 10000 * s.ScaleX)) / s.ScaleX;
            var ptY = (pt.Y - (t.Y + 10000 * s.ScaleY)) / s.ScaleY;
            return new Point(ptX, ptY);
        }

        /// <summary>
        /// Given a screenpoint on the canvas, transform it to the corresponding screenpoint 
        ///   in windows (non zoomed / translated space)
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        private Rect TransfromRectFromOverlayCoordinatesToZoomCanvas(Rect rect)
        {
            var s = this.mainCanvas.CanvasScaleTransform;
            var t = this.mainCanvas.CanvasTranslateTransform;

            var x = rect.Left;
            var y = rect.Top;

            var newWidth = rect.Width * s.ScaleX;
            var newHeight = rect.Height * s.ScaleY;

            var offsetX = t.X + 10000 * s.ScaleX;
            offsetX /= 1.5;
            var offsetY = t.Y + 10000 * s.ScaleY;
            offsetY /= 1.5;

            return new Rect(x * s.ScaleX + offsetX, y * s.ScaleY + offsetY, newWidth, newHeight);
        }

        private (Rect, Rect, string, string) SelectWindowUnderCursor()
        {
            var pt = System.Windows.Input.Mouse.GetPosition(this);
            pt = TransformPointFromZoomCanvas(pt);
            pt.Offset(fullBounds.Left, fullBounds.Top);

            var windowUnderCursorHwnd = NativeHelpers.GetWindowUnderPointExcludingOwn(new NativeMethods.POINT((int)pt.X, (int)pt.Y), new WindowInteropHelper(this).Handle);

            NativeMethods.DwmGetWindowAttribute(windowUnderCursorHwnd, (int)NativeMethods.DwmWindowAttribute.DWMWA_EXTENDED_FRAME_BOUNDS, out NativeMethods.RECT rect12, Marshal.SizeOf(typeof(NativeMethods.RECT)));

            string process_name = NativeHelpers.GetProcessNameFromWindowHandle(windowUnderCursorHwnd);
            string window_title = NativeHelpers.GetWindowTitle(windowUnderCursorHwnd);

            var wpfRectOffset = new Rect(rect12.Left - this.fullBounds.Left, rect12.Top - this.fullBounds.Top, rect12.Right - rect12.Left, rect12.Bottom - rect12.Top);
            var wpfRect = ReverseDpi(wpfRectOffset);
            wpfRect = TransfromRectFromOverlayCoordinatesToZoomCanvas(wpfRect);

            Canvas.SetLeft(this.rect, wpfRect.Left);
            Canvas.SetTop(this.rect, wpfRect.Top);
            rect.Width = wpfRect.Width;
            rect.Height = wpfRect.Height;
            innerRectGeometry.Rect = wpfRect;

            return (wpfRectOffset, wpfRect, process_name, window_title);
        }

        private void MainWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void WindowSelectionWindow_SourceInitialized(object? sender, EventArgs e)
        {
        }

        public double Dpi { get; private set; }
    }
}
