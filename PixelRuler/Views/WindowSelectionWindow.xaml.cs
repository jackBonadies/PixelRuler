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

        public WindowSelectionWindow(ScreenshotMode mode)
        {
            InitializeComponent();
            this.mode = mode;
            this.Loaded += WindowSelectionWindow_Loaded;
            this.SourceInitialized += WindowSelectionWindow_SourceInitialized;
            this.WindowState = WindowState.Normal;
            var fullBounds = UiUtils.GetFullBounds(WpfScreenHelper.Screen.AllScreens);
            this.Top = fullBounds.Top;
            this.Left = fullBounds.Left;
            this.Width = fullBounds.Width;
            this.Height = fullBounds.Height;
            this.WindowStyle = WindowStyle.None;
            this.Topmost = true;
            this.AllowsTransparency = true;
            this.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            this.PreviewMouseMove += WindowSelectionWindow_PreviewMouseMove;
            this.KeyDown += WindowSelectionWindow_KeyDown;
            this.MouseUp += WindowSelectionWindow_MouseUp;

            horzIndicator.X1 = fullBounds.Left;
            horzIndicator.X2 = fullBounds.Right;
            horzIndicator.Y1 = horzIndicator.Y2 = 300;

            vertIndicator.X1 = vertIndicator.X2 = 300;
            vertIndicator.Y1 = fullBounds.Top;
            vertIndicator.Y2 = fullBounds.Bottom;

            blurRect.Rect = new Rect(0, 0, this.Width, this.Height);
        }

        public Rect SelectedRect { get; set; }

        private void WindowSelectionWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SelectedRect = SelectWindowUnderCursor();

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
                SetCursorIndicator(e.GetPosition(this));
            }
        }

        private void SetCursorIndicator(Point point)
        {
            vertIndicator.X1 = vertIndicator.X2 = point.X;
            horzIndicator.Y1 = horzIndicator.Y2 = point.Y;
        }

        private Rect SelectWindowUnderCursor()
        {
            var windowUnderCursorHwnd = NativeHelpers.GetWindowUnderPointExcludingOwn(new WindowInteropHelper(this).Handle);
            
            //NativeMethods.GetWindowRect(windowUnderCursorHwnd, out NativeMethods.RECT rectWin); // includes too much window chrome..

            NativeMethods.DwmGetWindowAttribute(windowUnderCursorHwnd, (int)NativeMethods.DwmWindowAttribute.DWMWA_EXTENDED_FRAME_BOUNDS, out NativeMethods.RECT rect12, Marshal.SizeOf(typeof(NativeMethods.RECT)));

            var procname = NativeHelpers.GetProcessNameFromWindowHandle(windowUnderCursorHwnd);

            System.Diagnostics.Trace.WriteLine($"ProcName {procname}");
            //System.Diagnostics.Trace.WriteLine($"Title {title}");

            Canvas.SetLeft(this.rect, rect12.Left - this.Left);
            Canvas.SetTop(this.rect, rect12.Top - this.Top);
            rect.Width = rect12.Right - rect12.Left;
            rect.Height = rect12.Bottom - rect12.Top;
            var wpfRect = new Rect(rect12.Left - this.Left, rect12.Top - this.Top, rect12.Right - rect12.Left, rect12.Bottom - rect12.Top);
            innerRect.Rect = wpfRect;

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

        private void WindowSelectionWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
