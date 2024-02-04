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

            this.Loaded += MainWindow_Loaded;

            this.ViewModel.CloseWindowCommand = new RelayCommandFull((object? o) => { this.Close(); }, Key.W, ModifierKeys.Control, "Close Window");
            this.ViewModel.NewScreenshotFullCommand = new RelayCommandFull((object? o) => { NewWindowedScreenshot(); }, Key.N, ModifierKeys.Control, "New Full Screenshot");

            this.KeyDown += MainWindow_KeyDown;

            var handle = new WindowInteropHelper(this).Handle;

        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
#if DEBUG
            if(e.Key == Key.D)
            {
                var el = VisualTreeHelper.HitTest(this, Mouse.GetPosition(this));
                System.Diagnostics.Debugger.Break();
            }
            else if(e.Key == Key.P)
            {
                RedrawTitleBar();
                this.Activate();
            }
#endif
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

        public static Drawing.Bitmap CaptureScreen(Rect? bounds = null)
        {
            if(bounds == null)
            {
                var primaryScreen = System.Windows.Forms.Screen.PrimaryScreen;
                if (primaryScreen == null)
                {
                    throw new Exception("Primary Screen is null");
                }
                int pixelWidth = primaryScreen.Bounds.Width;
                int pixelHeight = primaryScreen.Bounds.Height;
                bounds = new Rect(0, 0, pixelWidth, pixelHeight);
            }

            var boundsVal = bounds.Value;

            var screenBounds = new Drawing.Size((int)boundsVal.Width, (int)boundsVal.Height);//System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            var screenshot = new Drawing.Bitmap(screenBounds.Width, screenBounds.Height);// PixelFormat.Format32bppArgb);
            using (Drawing.Graphics g = Drawing.Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen((int)boundsVal.X, (int)boundsVal.Y, 0, 0, screenBounds, Drawing.CopyPixelOperation.SourceCopy);
            }
            return screenshot;
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);
        }

        private HwndSource _source;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);
            var handle = new WindowInteropHelper(this).Handle;
            // this is a good time to do it
            ThemeManager.UpdateForThemeChanged(this.ViewModel.Settings.DayNightMode);
            RedrawTitleBar();

            RegisterHotKeys();
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            _source = null;
            UnregisterHotKeys();
            base.OnClosed(e);
        }

        private void RegisterHotKeys()
        {
            if(this.ViewModel.Settings.GlobalShortcutsEnabled)
            {
                var fullscreenShortcut = this.ViewModel.Settings.FullscreenScreenshotShortcut;
                RegisterShortcut(fullscreenShortcut);
                var windowedShortcut = this.ViewModel.Settings.WindowedScreenshotShortcut;
                RegisterShortcut(windowedShortcut);
            }
        }

        public void RegisterShortcut(ShortcutInfo shortcut)
        {
            if (shortcut.IsValid)
            {
                var helper = new WindowInteropHelper(this);
                uint key = (uint)KeyInterop.VirtualKeyFromKey(shortcut.Key);
                //const uint MOD_CTRL = 0x0002;
                //const uint MOD_SHIFT = 0x0004;
                uint mods = (uint)shortcut.Modifiers;
                if (!NativeMethods.RegisterHotKey(helper.Handle, shortcut.HotKeyId, mods, key))
                {
                    // TODO handle error
                }
            }
        }

        private void UnregisterHotKeys()
        {
            var helper = new WindowInteropHelper(this);
            NativeMethods.UnregisterHotKey(helper.Handle, this.ViewModel.Settings.FullscreenScreenshotShortcut.HotKeyId);
            NativeMethods.UnregisterHotKey(helper.Handle, this.ViewModel.Settings.WindowedScreenshotShortcut.HotKeyId);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case App.FULLSCREEN_HOTKEY_ID:
                            NewFullScreenshot(true);
                            handled = true;
                            break;
                        case App.WINDOWED_HOTKEY_ID:
                            NewWindowedScreenshot();
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var dpi = this.GetDpi();

            // basically undo the TransformToDevice transform so that 100% zoom has 1 pixel : 1 pixel
            mainCanvas.LayoutTransform = new ScaleTransform(1 / dpi, 1 / dpi);
        }


        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if(Properties.Settings.Default.CloseToTray)
            {
                this.Hide(); // i.e. so taskbar stays up
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        public async void NewWindowedScreenshot()
        {
            this.Hide();
            var wsw = new WindowSelectionWindow();
            var res = wsw.ShowDialog();
            Bitmap bmp = null;
            if(res is true)
            {
                bmp = CaptureScreen(wsw.SelectedRect);
                this.ViewModel.Image = bmp;
                mainCanvas.SetImage(this.ViewModel.ImageSource);
            }

            this.Show();
            this.Activate();
            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
            }
        }

        public async void NewFullScreenshot(bool alreadyRunning)
        {
            Bitmap bmp = null;
            if (alreadyRunning)
            {
                this.Hide();
                await Task.Delay(200);
                await Task.Run(new Action(async () =>
                {
                //    await Task.Delay(1000);
                    bmp = CaptureScreen();
                })).ConfigureAwait(true);
            }
            else
            {
                bmp = CaptureScreen();
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

        private void notifyIcon_Initialized(object sender, EventArgs e)
        {

        }

        private void notifyIcon_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }
    }
}
