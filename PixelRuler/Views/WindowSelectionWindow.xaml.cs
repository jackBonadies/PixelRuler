using PixelRuler.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

        ScreenshotMode mode;
        Rect fullBounds;

        public WindowSelectionWindow(ScreenshotMode mode)
        {
            InitializeComponent();
            this.mode = mode;
            this.Loaded += WindowSelectionWindow_Loaded;
            this.SourceInitialized += WindowSelectionWindow_SourceInitialized;
            this.WindowState = WindowState.Normal;

            this.fullBounds = UiUtils.GetFullBounds(WpfScreenHelper.Screen.AllScreens);
            this.Top = fullBounds.Top;
            this.Left = fullBounds.Left / 1.5; //TODO
            this.Width = fullBounds.Width;
            this.Height = fullBounds.Height;
            this.WindowStyle = WindowStyle.None;
            this.Topmost = true;
            this.AllowsTransparency = true;
            this.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            this.PreviewMouseMove += WindowSelectionWindow_PreviewMouseMove;
            this.KeyDown += WindowSelectionWindow_KeyDown;
            this.MouseUp += WindowSelectionWindow_MouseUp;
            this.MouseDown += WindowSelectionWindow_MouseDown;

            this.blurRectGeometry.Rect = fullBounds; // offset i.e. it needs to start at 0..
            this.rectSelectionOutline.Width = 0;
            this.rectSelectionOutline.Height = 0;

            setForMode();
        }


        private void setForMode()
        {
            if(mode == ScreenshotMode.Window)
            {
                blurBackground.Fill = new SolidColorBrush(Color.FromArgb(0x80, 0, 0, 0));
            }
            else
            {
                blurBackground.Fill = new SolidColorBrush(Color.FromArgb(0x30, 0, 0, 0));

                horzIndicator.X1 = fullBounds.Left;
                horzIndicator.X2 = fullBounds.Right;
                horzIndicator.Y1 = horzIndicator.Y2 = 300;

                vertIndicator.X1 = vertIndicator.X2 = 300;
                vertIndicator.Y1 = fullBounds.Top;
                vertIndicator.Y2 = fullBounds.Bottom;
            }
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
                if(selectedRectCanvas != value)
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
            dragging = true;
            startPoint = UiUtils.RoundPoint(e.GetPosition(this.canv));
            innerRectGeometry.Rect = new Rect(startPoint, startPoint);
            horzIndicator.Visibility = Visibility.Collapsed;
            vertIndicator.Visibility = Visibility.Collapsed;
        }

        private void WindowSelectionWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(mode == ScreenshotMode.Window)
            {
                SelectedRectCanvas = SelectWindowUnderCursor();
            }
            else
            {
                SelectedRectCanvas = new Rect(startPoint, UiUtils.RoundPoint(e.GetPosition(this.canv)));
            }

            this.DialogResult = true;
            this.Close();
        }

        private void WindowSelectionWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.DialogResult = false;
                this.Close();
            }
        }

        private void WindowSelectionWindow_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if(mode == ScreenshotMode.Window)
            {
                SelectWindowUnderCursor();
            }
            else
            {
                SetCursorIndicator(e.GetPosition(this.canv));
                if(dragging)
                {
                    innerRectGeometry.Rect = new Rect(startPoint, e.GetPosition(this.canv));
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

        private void SetCursorIndicator(Point point)
        {
            vertIndicator.X1 = vertIndicator.X2 = (int)Math.Round(point.X);
            horzIndicator.Y1 = horzIndicator.Y2 = (int)Math.Round(point.Y);
            //horzIndicator.StrokeDashOffset = point.X; 
            //vertIndicator.StrokeDashOffset = point.Y; 
        }

        private Rect SelectWindowUnderCursor()
        {
            var windowUnderCursorHwnd = NativeHelpers.GetWindowUnderPointExcludingOwn(new WindowInteropHelper(this).Handle);
            
            //NativeMethods.GetWindowRect(windowUnderCursorHwnd, out NativeMethods.RECT rectWin); // includes too much window chrome..

            NativeMethods.DwmGetWindowAttribute(windowUnderCursorHwnd, (int)NativeMethods.DwmWindowAttribute.DWMWA_EXTENDED_FRAME_BOUNDS, out NativeMethods.RECT rect12, Marshal.SizeOf(typeof(NativeMethods.RECT)));

            var procname = NativeHelpers.GetProcessNameFromWindowHandle(windowUnderCursorHwnd);

            System.Diagnostics.Trace.WriteLine($"ProcName {procname}");
            //System.Diagnostics.Trace.WriteLine($"Title {title}");

            Canvas.SetLeft(this.rect, rect12.Left - this.fullBounds.Left);
            Canvas.SetTop(this.rect, rect12.Top - this.fullBounds.Top);
            rect.Width = rect12.Right - rect12.Left;
            rect.Height = rect12.Bottom - rect12.Top;
            var wpfRect = new Rect(rect12.Left - this.fullBounds.Left, rect12.Top - this.fullBounds.Top, rect12.Right - rect12.Left, rect12.Bottom - rect12.Top);
            innerRectGeometry.Rect = wpfRect;

            return wpfRect;
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

        private void WindowSelectionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Dpi = this.GetDpi();
        }
    }
}
