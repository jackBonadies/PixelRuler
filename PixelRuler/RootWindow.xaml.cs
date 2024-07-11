using CommunityToolkit.Mvvm.ComponentModel;
using PixelRuler.ViewModels;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace PixelRuler
{



    /// <summary>
    /// Interaction logic for RootWindow.xaml
    /// </summary>
    public partial class RootWindow : Window
    {
        public RootWindow(RootViewModel rootViewModel)
        {
            InitializeComponent();

            this.RootViewModel = rootViewModel;
            this.DataContext = rootViewModel;
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.WindowState = WindowState.Normal; // better than minimized bc no lower left flicker
            this.ShowActivated = false;
            this.Top = int.MinValue;
            this.Left = int.MinValue;
            this.ShowInTaskbar = false;
            this.DataContextChanged += RootWindow_DataContextChanged;
            this.notifyIcon.Menu.DataContext = rootViewModel;
            this.notifyIcon.Menu.IsVisibleChanged += Menu_IsVisibleChanged;
            this.notifyIcon.FocusOnLeftClick = true;
            this.notifyIcon.LeftClick += NotifyIcon_LeftClick;
            this.notifyIcon.LeftDoubleClick += NotifyIcon_LeftDoubleClick;
            this.Loaded += RootWindow_Loaded;
        }

        private void NotifyIcon_LeftDoubleClick([System.Diagnostics.CodeAnalysis.NotNull] Wpf.Ui.Tray.Controls.NotifyIcon sender, RoutedEventArgs e)
        {
            this.notifyIcon.Menu.IsOpen = true;
        }

        private void NotifyIcon_LeftClick([System.Diagnostics.CodeAnalysis.NotNull] Wpf.Ui.Tray.Controls.NotifyIcon sender, RoutedEventArgs e)
        {
            bringAllWindowsToFront();
        }

        private void bringAllWindowsToFront()
        {
            foreach (var window in Application.Current.Windows)
            {
                if (window is Window win)
                {
                    win.Activate();
                }
            }
        }

        private void RootWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void Menu_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // TODO: header text binding only updates on explicit
            //   MenuItem.GetBindingExpression(MenuItem.HeaderProperty).UpdateTarget() call
            // This hack fixes that.
            this.notifyIcon.Menu.DataContext = null;
            this.notifyIcon.Menu.DataContext = this.DataContext;
        }

        public RootViewModel RootViewModel { get; init; }

        private void RootWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.notifyIcon.Menu.DataContext = this.DataContext;
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private HwndSource _source;

        public SettingsViewModel SettingsViewModel
        {
            get
            {
                return this.RootViewModel.Settings;
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);
            var handle = new WindowInteropHelper(this).Handle;
            // this is a good time to do it
            ThemeManager.UpdateForThemeChanged(this.SettingsViewModel.DayNightMode);
            this.SettingsViewModel.ShortcutChanged += Settings_ShortcutChanged;
            this.SettingsViewModel.GlobalShortcutsEnabledChanged += Settings_GlobalShortcutsEnabledChanged;

            RegisterHotKeys();
        }

        private void Settings_GlobalShortcutsEnabledChanged(object? sender, bool hotkeysEnabled)
        {
            if (hotkeysEnabled)
            {
                RegisterHotKeys();
            }
            else
            {
                UnregisterHotKeys();
            }
        }

        private void Settings_ShortcutChanged(object? sender, ShortcutInfo e)
        {
            ReregisterShortcut(e);
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
            if (this.SettingsViewModel.GlobalShortcutsEnabled)
            {
                foreach (var shortcut in this.SettingsViewModel.GlobalShortcuts)
                {
                    RegisterShortcut(shortcut);
                }
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
                    shortcut.Status = RegistrationStatus.FailedRegistration;
                }
                else
                {
                    shortcut.Status = RegistrationStatus.SuccessfulRegistration;
                }
            }
        }

        private void UnregisterHotKeys()
        {
            var helper = new WindowInteropHelper(this);
            foreach (var shortcut in this.SettingsViewModel.GlobalShortcuts)
            {
                shortcut.Status = RegistrationStatus.Unregistered;
                NativeMethods.UnregisterHotKey(helper.Handle, shortcut.HotKeyId);
            }
        }

        private void ReregisterShortcut(ShortcutInfo shortcut)
        {
            var helper = new WindowInteropHelper(this);
            NativeMethods.UnregisterHotKey(helper.Handle, shortcut.HotKeyId);
            RegisterShortcut(shortcut);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY)
            {
                switch (wParam.ToInt32())
                {
                    case App.FULLSCREEN_HOTKEY_ID:
                        App.NewFullscreenshotLogic(true);
                        handled = true;
                        break;
                    case App.WINDOWED_HOTKEY_ID:
                        App.EnterScreenshotTool(OverlayMode.Window, true);
                        handled = true;
                        break;
                    case App.REGION_WINDOWED_HOTKEY_ID:
                        App.EnterScreenshotTool(OverlayMode.WindowAndRegionRect, true);
                        handled = true;
                        break;
                    case App.QUICK_MEASURE_HOTKEY_ID:
                        App.EnterScreenshotTool(OverlayMode.QuickMeasure, true);
                        handled = true;
                        break;
                    case App.QUICK_COLOR_HOTKEY_ID:
                        App.EnterScreenshotTool(OverlayMode.QuickColor, true);
                        handled = true;
                        break;
                }
            }
            return IntPtr.Zero;
        }
    }
}
