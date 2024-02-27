using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Shapes;

namespace PixelRuler
{
    /// <summary>
    /// Interaction logic for RootWindow.xaml
    /// </summary>
    public partial class RootWindow : Window
    {
        public RootWindow()
        {
            InitializeComponent();
        }

        private HwndSource _source;

        public SettingsViewModel SettingsViewModel { get; internal set; }

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
