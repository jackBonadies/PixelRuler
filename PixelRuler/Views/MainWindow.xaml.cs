﻿using System;
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
using PixelRuler.Views;
using Microsoft.Win32;
using PixelRuler.Models;

namespace PixelRuler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ThemeWindow
    {
        protected override void OnClosed(EventArgs e)
        {
            HookUpUICommands(false);
            this.ViewModel.Cleanup();
            this.mainCanvas.Bind(false);
            base.OnClosed(e);
        }

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
            HookUpUICommands(true);

            this.Loaded += MainWindow_Loaded;
            this.IsVisibleChanged += MainWindow_IsVisibleChanged;


            this.KeyDown += MainWindow_KeyDown;
            this.KeyUp += MainWindow_KeyUp;
            //this.notifyIcon.Menu.DataContext = prvm;

            var handle = new WindowInteropHelper(this).Handle;

            this.SizeChanged += MainWindow_SizeChanged;
        }

        private void HookUpUICommands(bool bind)
        {
            if(bind)
            {
                this.ViewModel.CloseWindowCommand = new RelayCommandFull((object? o) => { this.Close(); }, Key.W, ModifierKeys.Control, "Close Window");
                this.ViewModel.NewScreenshotFullCommand = new RelayCommandFull((object? o) => { NewFullScreenshot(false); }, Key.N, ModifierKeys.Control | ModifierKeys.Shift, "New Full Screenshot");
                this.ViewModel.NewScreenshotRegionCommand = new RelayCommandFull((object? o) => { NewWindowedScreenshot(OverlayMode.WindowAndRegionRect, false); }, Key.N, ModifierKeys.Control, "New Region Screenshot");
                this.ViewModel.CopyCanvasContents = new RelayCommandFull((object? o) => { CopyContents(); }, Key.C, ModifierKeys.Control, "Copy Elements");
                this.ViewModel.PasteCanvasContents = new RelayCommandFull((object? o) => { this.mainCanvas.PasteCopiedData(); }, Key.V, ModifierKeys.Control, "Paste Elements");
            }
            else
            {
                this.ViewModel.CloseWindowCommand = null!;
                this.ViewModel.NewScreenshotFullCommand = null!;
                this.ViewModel.NewScreenshotRegionCommand = null!;
                this.ViewModel.CopyCanvasContents = null!;
                this.ViewModel.PasteCanvasContents = null!;
            }
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
            await Task.Delay(this.ViewModel.Settings.ScreenshotDelayMs);
            var wsw = new WindowSelectionWindow(mode, this.ViewModel.Settings);
            var res = wsw.ShowDialog();
            Bitmap bmp = null;

            if (res is not true)
            {
                return false;
            }


            if(wsw.AfterScreenshotValue is AfterScreenshotAction.Cancel)
            {
                return false;
            }

            var snippedImage = wsw.ViewModel.CropImage(wsw.SelectedRectCanvas);
            this.ViewModel.SetImage(snippedImage, wsw.ScreenshotInfo);

            if (res is true)
            {
                // transform SelectedRectWin to ImageCoords
                // i.e. the time the actual screenshot was taken.
                mainCanvas.SetImage(this.ViewModel.ImageSource);
            }

            if(wsw.AfterScreenshotValue is AfterScreenshotAction.SaveAs)
            {
                this.ViewModel.SaveAs();
                return false;
            }
            
            // string savedPath 
            if (wsw.AfterScreenshotValue is AfterScreenshotAction.Save)
            {
                string fname = string.Empty;
                if (wsw.AfterScreenshotAdditionalArg is PathSaveInfo pathSaveInfo)
                {
                    if(this.ViewModel.ScreenshotInfo == null)
                    {
                        throw new InvalidOperationException("Missing Screenshot Info");
                    }
                    if(this.ViewModel.Image == null)
                    {
                        throw new InvalidOperationException("Missing Image");
                    }

                    fname = pathSaveInfo.SaveImage(this.ViewModel.Image, this.ViewModel.ScreenshotInfo.Value);
                }
                else
                {
                    throw new Exception("Unexpected Arg for Save");
                }
                return false;
            }

            if (wsw.AfterScreenshotValue is AfterScreenshotAction.CommandTarget)
            {
                if (wsw.AfterScreenshotAdditionalArg is CommandTargetInfo cmdTargetInfo)
                {
                    if(this.ViewModel.ScreenshotInfo == null)
                    {
                        throw new InvalidOperationException("Missing Screenshot Info");
                    }
                    if(this.ViewModel.Image == null)
                    {
                        throw new InvalidOperationException("Missing Image");
                    }

                    string fname = this.ViewModel.Settings.DefaultPathSaveInfo.SaveImage(this.ViewModel.Image, this.ViewModel.ScreenshotInfo.Value);
                    cmdTargetInfo.Execute(fname);
                }
                else
                {
                    throw new Exception("Unexpected Arg for Command Target");
                }
                return false;
            }

            

            //if(wsw.AfterScreenshotValue is AfterScreenshotAction.Pin)
            //{
                var pinWindow = new PinImageWindow();
                pinWindow.ViewModel = new PinViewModel(this.ViewModel);
                pinWindow.Show();
            //}

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
            this.ViewModel.SetImage(bmp, new ScreenshotInfo());
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
            App.ShowSettingsWindowSingleInstance(this.ViewModel.Settings);
        }
    }
}
