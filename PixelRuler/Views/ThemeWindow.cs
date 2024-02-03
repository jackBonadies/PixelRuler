using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Wpf.Ui.Appearance;

namespace PixelRuler
{
    public class ThemeManager
    {
        public static event EventHandler<DayNightMode>? ThemeChanged;
        public static void UpdateForThemeChanged(DayNightMode effectiveMode)
        {
            if (effectiveMode == DayNightMode.FollowSystem)
            {
                var sysTheme = SystemThemeManager.GetCachedSystemTheme();
                if(sysTheme == SystemTheme.Dark)
                {
                    effectiveMode = DayNightMode.ForceNight;
                }
                else
                {
                    effectiveMode = DayNightMode.ForceDay;
                }
            }
            ThemeChanged?.Invoke(null, effectiveMode);
            UpdateResourceDictionaries(effectiveMode);
        }

        private static void UpdateResourceDictionaries(DayNightMode effectiveMode)
        {
            Wpf.Ui.Appearance.ApplicationTheme wpfUiTheme = Wpf.Ui.Appearance.ApplicationTheme.Light;
            if (effectiveMode == DayNightMode.ForceNight)
            {
                wpfUiTheme = Wpf.Ui.Appearance.ApplicationTheme.Dark;
            }

            var resourceDictionaries1 = App.Current.Resources.MergedDictionaries.OfType<Wpf.Ui.Markup.ThemesDictionary>();
            foreach(var resource in resourceDictionaries1)
            {
                resource.Theme = wpfUiTheme;
            }

            var resourceDictionaries2 = App.Current.Resources.MergedDictionaries.OfType<ThemesDictionary>();
            foreach (var resource in resourceDictionaries2)
            {
                resource.Theme = wpfUiTheme;
            }
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr,
            ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
        {
            if (IsWindows10OrGreater(17763))
            {
                var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                if (IsWindows10OrGreater(18985))
                {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                }

                int useImmersiveDarkMode = enabled ? 1 : 0;
                return DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
            }

            return false;
        }

        private static bool IsWindows10OrGreater(int build = -1)
        {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
        }
    }

    public class ThemeWindow : Window
    {
        public ThemeWindow()
        {
            ThemeManager.ThemeChanged += OnThemeChanged;
            this.DataContextChanged += ThemeWindow_DataContextChanged;
            this.Loaded += ThemeWindow_Loaded;
        }

        private void ThemeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetThemeFromDataContext();
            //var hwnd = new WindowInteropHelper(this).Handle;
            //ThemeManager.UseImmersiveDarkMode(hwnd, true);
        }

        private void SetThemeFromDataContext()
        {
            switch (this.DataContext)
            {
                case PixelRulerViewModel prvm:
                    OnThemeChanged(this, prvm.Settings.DayNightMode);
                    break;
                case SettingsViewModel svm:
                    OnThemeChanged(this, svm.DayNightMode);
                    break;
            }
        }

        private void ThemeWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //SetThemeFromDataContext();
        }

        protected override void OnClosed(EventArgs e)
        {
            ThemeManager.ThemeChanged -= OnThemeChanged;
            base.OnClosed(e);
        }

        private void OnThemeChanged(object? sender, DayNightMode e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            ThemeManager.UseImmersiveDarkMode(hwnd, e == DayNightMode.ForceNight);
            RedrawTitleBar();
        }


        public void RedrawTitleBar()
        {
            var windowInteropHelper = new WindowInteropHelper(this);
            NativeMethods.SendMessage(windowInteropHelper.Handle, NativeMethods.WM_NCPAINT, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
