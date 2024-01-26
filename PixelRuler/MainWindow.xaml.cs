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

namespace PixelRuler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            mainCanvas.ScaleChanged += ScaleChanged;

            this.DataContext = new PixelRulerViewModel();

            var screenshot = CaptureScreen();
            this.ViewModel.Image = screenshot;
            mainCanvas.SetImage(this.ViewModel.ImageSource);
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
            var screenBounds = new Drawing.Size(3840, 2160);//System.Windows.Forms.Screen.PrimaryScreen.Bounds;
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
            var helper = new WindowInteropHelper(this);
            const uint VK_F10 = 0x79;
            const uint MOD_CTRL = 0x0002;
            if (!RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_CTRL, VK_F10))
            {
                // handle error
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
            // do stuff
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

        public void ScaleChanged(object? sender, double e)
        {
            this.ViewModel.CurrentZoom = (e * 100);
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItemNewScreenshot_Click(object sender, RoutedEventArgs e)
        {

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide(); // i.e. so taskbar stays up
            e.Cancel = true;
            base.OnClosing(e);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Bitmap bmp = null;
            await Task.Run(new Action(() =>
            {
                bmp = CaptureScreen();
            })).ConfigureAwait(true);
            BitmapSource? image = null;
            this.ViewModel.Image = bmp;
            mainCanvas.SetImage(this.ViewModel.ImageSource);
            this.Show();

        }
    }
}
