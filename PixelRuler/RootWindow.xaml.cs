using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PixelRuler
{
    public enum ScreenshotMode
    {
        Window = 0,
        RegionRect = 1,
    }

    // TODO own class
    public class RootViewModel : INotifyPropertyChanged
    {
        public RootViewModel(SettingsViewModel? settingsViewModel = null) 
        {
            Settings = settingsViewModel;
            this.NewScreenshotFullCommand = new RelayCommandFull((object? o) => { App.NewFullscreenshotLogic(this.Settings, true); }, Settings.FullscreenScreenshotShortcut, "New Full Screenshot");
            this.NewScreenshotWindowedCommand = new RelayCommandFull((object? o) => { App.NewScreenshotRegionLogic(this.Settings, ScreenshotMode.Window, true); }, Settings.WindowedScreenshotShortcut, "New Windowed Screenshot");
            this.NewScreenshotRegionCommand = new RelayCommandFull((object? o) => { App.NewScreenshotRegionLogic(this.Settings, ScreenshotMode.RegionRect, true); }, Key.N, ModifierKeys.Control, "New Region Screenshot");
        }

        public RelayCommandFull NewScreenshotRegionCommand { get; init; }
        public RelayCommandFull NewScreenshotWindowedCommand { get; init; }
        public RelayCommandFull NewScreenshotFullCommand { get; init; } 
        public SettingsViewModel Settings { get; set; }


        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }


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
                var fullscreenShortcut = this.SettingsViewModel.FullscreenScreenshotShortcut;
                RegisterShortcut(fullscreenShortcut);
                var windowedShortcut = this.SettingsViewModel.WindowedScreenshotShortcut;
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
            this.SettingsViewModel.FullscreenScreenshotShortcut.Status = RegistrationStatus.Unregistered;
            NativeMethods.UnregisterHotKey(helper.Handle, this.SettingsViewModel.FullscreenScreenshotShortcut.HotKeyId);
            this.SettingsViewModel.WindowedScreenshotShortcut.Status = RegistrationStatus.Unregistered;
            NativeMethods.UnregisterHotKey(helper.Handle, this.SettingsViewModel.WindowedScreenshotShortcut.HotKeyId);
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
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case App.FULLSCREEN_HOTKEY_ID:
                            //NewFullScreenshot(true);
                            handled = true;
                            break;
                        case App.WINDOWED_HOTKEY_ID:
                            //NewWindowedScreenshot();
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }
    }
}
