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

            this.Top = fullBounds.Top;
            this.Left = fullBounds.Left / scaleFactor;
            this.Width = fullBounds.Width / scaleFactor;
            this.Height = fullBounds.Height / scaleFactor;
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

            var copyFullBounds = fullBounds;
            copyFullBounds.Offset(-fullBounds.Left, -fullBounds.Top);
            this.blurRectGeometry.Rect = copyFullBounds; // offset i.e. it needs to start at 0..
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
                var perScreenPanel = new ScreenshotSelectionPerScreenPanel()
                {
                    Width = (screen.Bounds.Width / scaleFactor) / individualScale,
                    Height = (screen.Bounds.Height / scaleFactor) / individualScale,
                    Margin = new Thickness(left, top, 0, 0),
                };
                perScreenPanel.perScreenDpiScaleTransform.ScaleX = individualScale;
                perScreenPanel.perScreenDpiScaleTransform.ScaleY = individualScale;
                this.mainContent.Children.Add(perScreenPanel);
            }
            setForMode();
        }



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

            var bmp = UiUtils.CaptureScreen(UiUtils.GetFullBounds(WpfScreenHelper.Screen.AllScreens));

            SetupScreenshowWindowViewModel(ViewModel);
            this.DataContext = ViewModel;
            mainCanvas.DataContext = ViewModel;
            this.ViewModel.SetImage(bmp, new ScreenshotInfo());
            mainCanvas.SetImage(ViewModel.ImageSource);

            Dpi = this.GetDpi();


        }

        /// <summary>
        /// Windows rect, this is in screen coordinates
        /// i.e. if second monitor is to the left of first, the leftmost
        ///      point will be 0,0 in canvas coords, but -1920 in screen coords
        /// </summary>
        public Rect SelectedRectWin
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
                    Width = (int)this.SelectedRectWin.Width,
                    Height = (int)this.SelectedRectWin.Height,
                    DateTime = DateTime.Now
                };
            }
        }

        public string ProcessName { get; set; }
        public string WindowTitle { get; set; }

        /// <summary>
        /// Canvas Rect where 0,0 is leftmost point on virtual screen.
        /// </summary>
        private Rect selectedRectCanvas;
        public Rect SelectedRectCanvas
        {
            get
            {
                return selectedRectCanvas;
            }
            set
            {
                if (selectedRectCanvas != value)
                {
                    selectedRectCanvas = value;
                    value.Offset(fullBounds.Left, fullBounds.Top);
                    SelectedRectWin = value;
                }
            }
        }
        private bool dragging = false;
        private Point startPoint;

        private void WindowSelectionWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            dragging = true;
            startPoint = UiUtils.RoundPoint(e.GetPosition(this.mainCanvas));
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
                (SelectedRectCanvas, ProcessName, WindowTitle) = SelectWindowUnderCursor();
            }
            else
            {
                SelectedRectCanvas = new Rect(startPoint, UiUtils.RoundPoint(e.GetPosition(this.mainCanvas)));
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
                Views.AfterScreenshot.ShowOptions(this, this.ViewModel.Settings, AfterScreenshot);
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

        private void WindowSelectionWindow_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (ViewModel.Mode == OverlayMode.QuickMeasure || ViewModel.Mode == OverlayMode.QuickColor)
            {
                return;
            }

            if (ViewModel.Mode.IsSelectWindow())
            {
                (SelectedRectCanvas, ProcessName, WindowTitle) = SelectWindowUnderCursor();
            }

            if(ViewModel.Mode.IsSelectRegion())
            {
                SetCursorIndicator(e.GetPosition(this.mainCanvas));
                if (dragging)
                {
                    EnterRegionOnlyMode();
                    innerRectGeometry.Rect = new Rect(startPoint, e.GetPosition(this.mainCanvas));
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
            vertIndicator.X1 = vertIndicator.X2 = (int)Math.Round(point.X);
            horzIndicator.Y1 = horzIndicator.Y2 = (int)Math.Round(point.Y);
            //horzIndicator.StrokeDashOffset = point.X; 
            //vertIndicator.StrokeDashOffset = point.Y; 
        }

        private (Rect, string, string) SelectWindowUnderCursor()
        {
            var windowUnderCursorHwnd = NativeHelpers.GetWindowUnderPointExcludingOwn(new WindowInteropHelper(this).Handle);

            //NativeMethods.GetWindowRect(windowUnderCursorHwnd, out NativeMethods.RECT rectWin); // includes too much window chrome..

            NativeMethods.DwmGetWindowAttribute(windowUnderCursorHwnd, (int)NativeMethods.DwmWindowAttribute.DWMWA_EXTENDED_FRAME_BOUNDS, out NativeMethods.RECT rect12, Marshal.SizeOf(typeof(NativeMethods.RECT)));

            string process_name = NativeHelpers.GetProcessNameFromWindowHandle(windowUnderCursorHwnd);
            string window_title = NativeHelpers.GetWindowTitle(windowUnderCursorHwnd);

            Canvas.SetLeft(this.rect, rect12.Left - this.fullBounds.Left);
            Canvas.SetTop(this.rect, rect12.Top - this.fullBounds.Top);
            rect.Width = rect12.Right - rect12.Left;
            rect.Height = rect12.Bottom - rect12.Top;
            var wpfRect = new Rect(rect12.Left - this.fullBounds.Left, rect12.Top - this.fullBounds.Top, rect12.Right - rect12.Left, rect12.Bottom - rect12.Top);
            innerRectGeometry.Rect = wpfRect;

            return (wpfRect, process_name, window_title);
        }

        private void MainWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void WindowSelectionWindow_SourceInitialized(object? sender, EventArgs e)
        {
            var dpi = this.GetDpi();
            var scale = 1 / dpi;
            reverseDpiTransform.ScaleX = scale;
            reverseDpiTransform.ScaleY = scale;
        }

        public double Dpi { get; private set; }
    }
}
