using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Media;

namespace PixelRuler
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public SettingsViewModel()
        {
            fullscreenScreenshotShortcut = new ShortcutInfo(
                "Fullscreen Screenshot",
                Properties.Settings.Default.GlobalShortcutFullscreenKey,
                Properties.Settings.Default.GlobalShortcutFullscreenModifiers);

            fullscreenScreenshotShortcut = new ShortcutInfo(
                "Fullscreen Screenshot",
                Properties.Settings.Default.GlobalShortcutFullscreenKey,
                Properties.Settings.Default.GlobalShortcutFullscreenModifiers);


        }

        public bool StartAtSystemStartup
        {
            get
            {
                return Properties.Settings.Default.StartAtWindowsStartup;
            }
            set
            {
                if (Properties.Settings.Default.StartAtWindowsStartup != value)
                {
                    Properties.Settings.Default.StartAtWindowsStartup = value;
                    UpdateForWindowsStartupChanged();
                    OnPropertyChanged();
                }
            }
        }

        private const string PixelRulerStartup = "PixelRulerStartup";

        private void UpdateForWindowsStartupChanged()
        {
            RegistryKey runSubKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (Properties.Settings.Default.StartAtWindowsStartup)
            {
                //var exeLocation = System.Reflection.Assembly.GetExecutingAssembly().Location; // gives .dll
                var exeLocation = Process.GetCurrentProcess().MainModule.FileName;
                var cmdLineOption = "--background";
                runSubKey.SetValue(PixelRulerStartup, $"{exeLocation} {cmdLineOption}");
            }
            else
            {
                runSubKey.DeleteValue(PixelRulerStartup);
            }
        }

        public OnLaunchAction LaunchStartupAction
        {
            get
            {
                return (OnLaunchAction)Properties.Settings.Default.LaunchStartupAction;
            }
            set
            {
                if (Properties.Settings.Default.LaunchStartupAction != (int)value)
                {
                    Properties.Settings.Default.LaunchStartupAction = (int)value;
                    OnPropertyChanged();
                }
            }
        }

        public Tool DefaultTool
        {
            get
            {
                return (Tool)Properties.Settings.Default.DefaultTool;
            }
            set
            {
                if (Properties.Settings.Default.DefaultTool != (int)value)
                {
                    Properties.Settings.Default.DefaultTool = (int)value;
                    OnPropertyChanged();
                }
            }
        }

        public DayNightMode DayNightMode
        {
            get
            {
                return (DayNightMode)Properties.Settings.Default.DayNightMode;
            }
            set
            {
                if (Properties.Settings.Default.DayNightMode != (int)value)
                {
                    ThemeManager.UpdateForThemeChanged(value);
                    Properties.Settings.Default.DayNightMode = (int)value;
                    OnPropertyChanged();
                }
            }
        }



        public bool CloseToTray
        {
            get
            {
                return Properties.Settings.Default.CloseToTray;
            }
            set
            {
                if (Properties.Settings.Default.CloseToTray != value)
                {
                    Properties.Settings.Default.CloseToTray = value;
                    UpdateCloseToTrayChanged();
                    OnPropertyChanged();
                }
            }
        }

        private void UpdateCloseToTrayChanged()
        {

            if (this.CloseToTray)
            {
                App.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            }
            else
            {
                App.Current.ShutdownMode = ShutdownMode.OnLastWindowClose;
            }
        }


        public ColorAnnotationsBundle AnnotationsColor
        {
            get
            {
                return getColorBundle(Properties.Settings.Default.AnnotationColor);
            }
            set
            {
                if (Properties.Settings.Default.AnnotationColor != value.Key)
                {
                    Properties.Settings.Default.AnnotationColor = value.Key;
                    SetAnnotationColorState();
                    OnPropertyChanged();
                }
            }
        }

        private ColorAnnotationsBundle getColorBundle(string key)
        {
            var colorBundle = AvailableAnnotationsColors.Where(it => it.Key == Properties.Settings.Default.AnnotationColor);
            if(colorBundle.Any())
            {
                return colorBundle.First();
            }
            else
            {
                return AvailableAnnotationsColors[0];
            }
        }

        private void SetAnnotationColorState()
        {
            var colorBundle = getColorBundle(Properties.Settings.Default.AnnotationColor);
            App.Current.Resources["AnnotationColor"] = new SolidColorBrush(colorBundle.AnnotationColor.ConvertToWpfColor());
            App.Current.Resources["AnnotationColorText"] = new SolidColorBrush(colorBundle.AnnotationColorText.ConvertToWpfColor());
            App.Current.Resources["AnnotationColorLabelBackground"] = new SolidColorBrush(colorBundle.LabelColorBackground.ConvertToWpfColor());
        }

        public ColorAnnotationsBundle[] AvailableAnnotationsColors
        {
            get; set;
        } = new ColorAnnotationsBundle[]
        {
            new ColorAnnotationsBundle("Red","#FF2A1A".ToWinFormColorFromRgbHex(), "#FF2A1A".ToWinFormColorFromRgbHex()),
            //"#EE4B2B".ToWinFormColorFromRgbHex(),
            //"#FF5733".ToWinFormColorFromRgbHex(),
            //"#FF0000".ToWinFormColorFromRgbHex(),
            new ColorAnnotationsBundle("Green","#2BEE4B".ToWinFormColorFromRgbHex()),
            new ColorAnnotationsBundle("Blue","#4B2BEE".ToWinFormColorFromRgbHex()),
            new ColorAnnotationsBundle("Purple","#BC13FE".ToWinFormColorFromRgbHex()),
            new ColorAnnotationsBundle("Orange","#FF9E3D".ToWinFormColorFromRgbHex()),
            new ColorAnnotationsBundle("White","#FFFFFF".ToWinFormColorFromRgbHex(), System.Drawing.Color.Black),
            new ColorAnnotationsBundle("Black","#000000".ToWinFormColorFromRgbHex()),
        };

        public bool GlobalShortcutsEnabled
        {
            get
            {
                return Properties.Settings.Default.GlobalShortcutsEnabled;
            }
            set
            {
                if (Properties.Settings.Default.GlobalShortcutsEnabled != value)
                {
                    Properties.Settings.Default.GlobalShortcutsEnabled = value;
                    // GLOBAL SHORTCUT CHANGED
                    OnPropertyChanged();
                }
            }
        }

        private ShortcutInfo fullscreenScreenshotShortcut;
        public ShortcutInfo FullscreenScreenshotShortcut
        {
            get
            {
                return fullscreenScreenshotShortcut;
            }
            set
            {
                if(fullscreenScreenshotShortcut != value)
                {
                    fullscreenScreenshotShortcut = value;
                    //TODO shortcut changed.
                    OnPropertyChanged();
                }
            }

        }

        private ShortcutInfo windowedScreenshotShortcut;
        public ShortcutInfo WindowedScreenshotShortcut
        {
            get
            {
                return windowedScreenshotShortcut;
            }
            set
            {
                if (windowedScreenshotShortcut != value)
                {
                    windowedScreenshotShortcut = value;
                    //TODO shortcut changed.
                    OnPropertyChanged();
                }
            }
        }

        //public System.Windows.Input.Key GlobalStartupKey
        //{
        //    get
        //    {
        //        return (System.Windows.Input.Key)Properties.Settings.Default.GlobalShortcutFullscreenKey;
        //    }
        //    set
        //    {
        //        if ((System.Windows.Input.Key)Properties.Settings.Default.GlobalShortcutFullscreenKey != value)
        //        {
        //            Properties.Settings.Default.GlobalShortcutFullscreenKey = (int)value;
        //            // GLOBAL SHORTCUT CHANGED
        //            OnPropertyChanged();
        //        }
        //    }
        //}

        //public System.Windows.Input.ModifierKeys GlobalStartupModifiers
        //{
        //    get
        //    {
        //        return (System.Windows.Input.ModifierKeys)Properties.Settings.Default.GlobalShortcutFullscreenModifiers;
        //    }
        //    set
        //    {
        //        if ((System.Windows.Input.ModifierKeys)Properties.Settings.Default.GlobalShortcutFullscreenModifiers != value)
        //        {
        //            Properties.Settings.Default.GlobalShortcutFullscreenKey = (int)value;
        //            // GLOBAL SHORTCUT CHANGED
        //            OnPropertyChanged();
        //        }
        //    }
        //}

        public string VersionDisplayName
        {
            get
            {
                try
                {
                    return Windows.ApplicationModel.Package.Current.Id.Version.ToString();
                }
                catch(Exception)
                {
                    return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                }
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            Properties.Settings.Default.Save();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void SetState()
        {
            SetAnnotationColorState();
            ThemeManager.UpdateForThemeChanged(this.DayNightMode);
            this.UpdateCloseToTrayChanged();

        }
    }
}