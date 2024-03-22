using System;
using System.Collections.Generic;
using Drawing = System.Drawing;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Policy;
using System.Windows.Interop;
using System.IO;
using System.Xml.Linq;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Drawing;
using PixelRuler;
using System.Reflection;
using WpfScreenHelper;
using PixelRuler.Common;

namespace PixelRuler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ThemeWindow
    {
        public MainWindow(PixelRulerViewModel prvm)
        {

            this.DataContext = prvm;

            InitializeComponent();

            // basically we need to .Show() a window without actually showing it
            //   for the notifyicon
            //if(backgrounded)
            //{
            //    this.WindowStartupLocation = WindowStartupLocation.Manual;
            //    this.WindowState = WindowState.Normal; // better than minimized bc no lower left flicker
            //    this.ShowActivated = false;
            //    this.Top = int.MinValue;
            //    this.Left = int.MinValue;
            //    this.ShowInTaskbar = false;
            //}

            this.Loaded += MainWindow_Loaded;
            this.IsVisibleChanged += MainWindow_IsVisibleChanged;

            this.ViewModel.CloseWindowCommand = new RelayCommandFull((object? o) => { this.Close(); }, Key.W, ModifierKeys.Control, "Close Window");
            this.ViewModel.NewScreenshotFullCommand = new RelayCommandFull((object? o) => { NewWindowedScreenshot(OverlayMode.Window, false); }, Key.N, ModifierKeys.Control, "New Full Screenshot");
            this.ViewModel.CopyCanvasContents = new RelayCommandFull((object? o) => { CopyContents(); }, Key.C, ModifierKeys.Control, "Copy Elements");
            this.ViewModel.PasteCanvasContents = new RelayCommandFull((object? o) => { this.mainCanvas.PasteCopiedData(); }, Key.V, ModifierKeys.Control, "Paste Elements");

            this.KeyDown += MainWindow_KeyDown;
            this.KeyUp += MainWindow_KeyUp;
            //this.notifyIcon.Menu.DataContext = prvm;

            var handle = new WindowInteropHelper(this).Handle;

            this.SizeChanged += MainWindow_SizeChanged;

        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //var deltaX = (e.NewSize.Width - e.PreviousSize.Width);
            //var deltaY = (e.NewSize.Height - e.PreviousSize.Height);
            //var tt = this.mainCanvas.innerCanvas.GetTranslateTransform();
            //tt.X += deltaX / 2;
            //tt.Y += deltaY / 2;
        }

        private void MainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private void CopyContents()
        {
            if(!this.mainCanvas.CopySelectedData())
            {
                this.ViewModel.CopyRawImageToClipboard();
            }
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Space)
            {
                mainCanvas.HideZoomBox();
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.G)
            {
                (mainCanvas.DataContext as PixelRulerViewModel).ShowGridLines =
                    !(mainCanvas.DataContext as PixelRulerViewModel).ShowGridLines;
            }
#if DEBUG
            if(e.Key == Key.D)
            {
                var el = VisualTreeHelper.HitTest(this, Mouse.GetPosition(this));
                System.Diagnostics.Debugger.Break();
            }
            else if(e.Key == Key.P)
            {
                RedrawTitleBar();
                mainCanvas.innerCanvas.Background = new SolidColorBrush(Colors.Blue);
                this.Activate();
                var rect = new Rect(0, -500, 1000, 1000);
                var drawingVisual = new DrawingVisual();
                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {
                    VisualBrush visualBrush = new VisualBrush(mainCanvas.innerCanvas)
                    {
                        Viewbox = rect,
                        ViewboxUnits = BrushMappingMode.Absolute
                    };
                    drawingContext.DrawRectangle(visualBrush, null, new Rect(0, 0, rect.Width, rect.Height));
                }

                var renderTargetBitmap = new RenderTargetBitmap(
                    //(int)mainCanvas.innerCanvas.ActualWidth,  // Use the width of the region
                    //(int)mainCanvas.innerCanvas.ActualHeight, // Use the height of the region
                    (int)rect.Width,
                    (int)rect.Height,
                    96, // DPI X
                    96, // DPI Y
                    PixelFormats.Pbgra32); // Pixel format

                // Render the DrawingVisual containing the cropped region
                renderTargetBitmap.Render(drawingVisual);

                var pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                using (var fileStream = new FileStream(@"C:\tmp\innercanvas.png", FileMode.Create))
                {
                    pngEncoder.Save(fileStream);
                }

                Clipboard.SetImage(renderTargetBitmap);


            }
#endif
            // todo move to canvas??
            if (e.Key == this.ViewModel.Settings.ZoomBoxQuickZoomKey)
            {
                mainCanvas.ShowZoomBox();
            }
        }

        private PixelRulerViewModel ViewModel
        {
            get
            {
                var prvm = DataContext as PixelRulerViewModel;
                if (prvm == null)
                {
                    throw new Exception("No View Model on Main Canvas");
                }
                return prvm;
            }
        }


        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            SetupForDpi();
            base.OnDpiChanged(oldDpi, newDpi);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            this.RedrawTitleBar();
            base.OnSourceInitialized(e);
        }


        private void SetupForDpi()
        {
            var dpi = this.GetDpi();

            // basically undo the TransformToDevice transform so that 100% zoom has 1 pixel : 1 pixel
            mainCanvas.LayoutTransform = new ScaleTransform(1 / dpi, 1 / dpi);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetupForDpi();
            mainCanvas.Panning += MainCanvas_Panning;
        }

        private void MainCanvas_Panning(object? sender, EventArgs e)
        {
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if(Properties.Settings.Default.CloseToTray)
            {
                // only 1 instance needs to close to tray otherwise you have
                //   several tray notification icons as you open and close 
                //   instances.
                this.Hide(); // i.e. so taskbar stays up
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        public void SetImage(string imagePath)
        {
            var bmp = System.Drawing.Bitmap.FromFile(imagePath);
            this.ViewModel.Image = bmp as System.Drawing.Bitmap;
            mainCanvas.SetImage(this.ViewModel.ImageSource);
        }

        public async Task<bool> NewWindowedScreenshot(OverlayMode mode, bool newWindow)
        {
            this.Hide();
            var wsw = new WindowSelectionWindow(mode, this.ViewModel.Settings);
            var res = wsw.ShowDialog();
            Bitmap bmp = null;

            if (res is not true)
            {
                return false;
            }

            if(res is true)
            {
                bmp = UiUtils.CaptureScreen(wsw.SelectedRectWin);
                this.ViewModel.Image = bmp;
                mainCanvas.SetImage(this.ViewModel.ImageSource);
            }

            if(res is true && newWindow)
            {
                if(wsw.SelectedRectCanvas.Width * 1.3 > WpfScreenHelper.Screen.PrimaryScreen.Bounds.Width && 
                   wsw.SelectedRectCanvas.Height  * 1.3 > WpfScreenHelper.Screen.PrimaryScreen.Bounds.Height)
                {
                    this.WindowState = WindowState.Maximized;
                    this.Width = WpfScreenHelper.Screen.PrimaryScreen.Bounds.Width * .75;
                    this.Height = WpfScreenHelper.Screen.PrimaryScreen.Bounds.Height * .75;
                }
                else
                {
                    // set reasonable size
                    // Left, Top, Width, Height are all dpi independent values, must translate. 
                    // should be close to where the snip was done (if applicable)
                    // should have some min size
                    // no part of it should be offscreen
                    // if close to max then maximize.
                    Rect workArea = SystemParameters.WorkArea;
                    //WpfScreenHelper.Screen.PrimaryScreen.WorkingArea
                    var dpiScaleFactor = wsw.Dpi;
                    this.Left = wsw.SelectedRectCanvas.Left / dpiScaleFactor - 60;
                    this.Top = wsw.SelectedRectCanvas.Top / dpiScaleFactor - 60;
                    this.Width = Math.Max(wsw.SelectedRectCanvas.Width / dpiScaleFactor + 120, 730);
                    this.Height = Math.Max(wsw.SelectedRectCanvas.Height / dpiScaleFactor + 120, 515);
                    this.WindowStartupLocation = WindowStartupLocation.Manual;
                    this.WindowState = WindowState.Normal;
                }
            }

            this.Show();
            this.Activate();
            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
            }
            return res != null ? res.Value : false;
        }

        public async void NewFullScreenshot(bool newWindow)
        {
            Bitmap bmp = null;
            if (!newWindow)
            {
                this.Hide();
                await Task.Delay(200);
                await Task.Run(new Action(async () =>
                {
                //    await Task.Delay(1000);
                    bmp = UiUtils.CaptureScreen();
                })).ConfigureAwait(true);
            }
            else
            {
                bmp = UiUtils.CaptureScreen();
            }
            this.ViewModel.Image = bmp;
            mainCanvas.SetImage(this.ViewModel.ImageSource);
            this.Show();
            this.Activate();
            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
                // it will also preserve any Maximized windows
            }
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(this.ViewModel);
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

    }
}
