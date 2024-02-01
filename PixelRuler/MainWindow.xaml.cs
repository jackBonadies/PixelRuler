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
            this.ViewModel.NewScreenshotFullCommand = new RelayCommandFull((object? o) => { NewFullScreenshot(true); }, Key.N, ModifierKeys.Control, "New Full Screenshot");

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

        public static Drawing.Bitmap CaptureScreen()
        {

            var primaryScreen = System.Windows.Forms.Screen.PrimaryScreen;
            if(primaryScreen == null)
            {
                throw new Exception("Primary Screen is null");
            }
            int pixelWidth = primaryScreen.Bounds.Width; 
            int pixelHeight = primaryScreen.Bounds.Height; 

            var screenBounds = new Drawing.Size(pixelWidth, pixelHeight);//System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            var screenshot = new Drawing.Bitmap(screenBounds.Width, screenBounds.Height);// PixelFormat.Format32bppArgb);
            using (Drawing.Graphics g = Drawing.Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(0, 0, 0, 0, screenBounds, Drawing.CopyPixelOperation.SourceCopy);
            }
            return screenshot;
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);
        }


        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey(
            [In] IntPtr hWnd,
            [In] int id,
            [In] uint fsModifiers,
            [In] uint vk);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);

        private HwndSource _source;
        private const int HOTKEY_ID = 9000;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);

            RegisterHotKey();



            //icon.ShowBalloonTip(5000, "Title", "Text", System.Windows.Forms.ToolTipIcon.Info);
            //icon.Click += nIcon_Click;
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            _source = null;
            UnregisterHotKey();
            base.OnClosed(e);
        }

        private void RegisterHotKey()
        {
            if(this.ViewModel.Settings.GlobalShortcutsEnabled)
            {
                var helper = new WindowInteropHelper(this);
                uint key = (uint)KeyInterop.VirtualKeyFromKey(this.ViewModel.Settings.GlobalStartupKey);
                //const uint MOD_CTRL = 0x0002;
                //const uint MOD_SHIFT = 0x0004;
                uint mods = (uint)this.ViewModel.Settings.GlobalStartupModifiers;
                if (!RegisterHotKey(helper.Handle, HOTKEY_ID, mods, key))
                {
                    // handle error
                }
            }
        }

        private void UnregisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            UnregisterHotKey(helper.Handle, HOTKEY_ID);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            OnHotKeyPressed();
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void OnHotKeyPressed()
        {
            this.NewFullScreenshot(true);
        }

        private void Hotkey_Pressed(object? sender, HandledEventArgs e)
        {
            
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
            BitmapSource? image = null;
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
