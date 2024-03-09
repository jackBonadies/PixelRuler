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
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace PixelRuler
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public SettingsViewModel()
        {
            FullscreenScreenshotShortcut = new ShortcutInfo(
                "Fullscreen Screenshot",
                App.FULLSCREEN_HOTKEY_ID,
                (Key)Properties.Settings.Default.GlobalShortcutFullscreenKey,
                (ModifierKeys)Properties.Settings.Default.GlobalShortcutFullscreenModifiers,
                Key.P,
                ModifierKeys.Control | ModifierKeys.Shift);

            WindowedScreenshotShortcut = new ShortcutInfo(
                "Window Screenshot",
                App.WINDOWED_HOTKEY_ID,
                (Key)Properties.Settings.Default.GlobalShortcutWindowKey,
                (ModifierKeys)Properties.Settings.Default.GlobalShortcutWindowModifiers,
                Key.W,
                ModifierKeys.Control | ModifierKeys.Shift);

            clearShortcutCommand = new RelayCommand((object? o) =>
            {
                if(o is ShortcutInfo shortcutInfo)
                {
                    shortcutInfo.Clear();
                }
                else
                {
                    throw new Exception("Unexpected Type");
                }
            });

            editShortcutCommand = new RelayCommand((object? o) =>
            {
                if(o is ShortcutInfo shortcutInfo)
                {
                    EditShortcutCommandEvent?.Invoke(this, shortcutInfo);
                }
                else
                {
                    throw new Exception("Unexpected Type");
                }
            });
        }

        public event EventHandler<ShortcutInfo> EditShortcutCommandEvent;

        private RelayCommand editShortcutCommand;
        public RelayCommand EditShortcutCommand
        {
            get
            {
                return editShortcutCommand;
            }
            set
            {
                if(editShortcutCommand != value)
                {
                    editShortcutCommand = value;
                }
            }
        }

        private RelayCommand clearShortcutCommand;
        public RelayCommand ClearShortcutCommand
        {
            get
            {
                return clearShortcutCommand;
            }
            set
            {
                if(clearShortcutCommand != value)
                {
                    clearShortcutCommand = value;
                }
            }
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
                    GlobalShortcutsEnabledChanged?.Invoke(this, value);
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
                    fullscreenScreenshotShortcut.PropertyChanged += (object o, PropertyChangedEventArgs e) => { OnPropertyChanged(nameof(FullscreenScreenshotShortcut)); };
                    fullscreenScreenshotShortcut.PropertyChanged += FullscreenScreenshotShortcut_PropertyChanged;
                    //TODO shortcut changed.
                    OnPropertyChanged();
                }
            }
        }

        public event EventHandler<bool> GlobalShortcutsEnabledChanged;

        public event EventHandler<ShortcutInfo> ShortcutChanged;

        private void FullscreenScreenshotShortcut_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Properties.Settings.Default.GlobalShortcutFullscreenKey = (int)FullscreenScreenshotShortcut.Key;
            Properties.Settings.Default.GlobalShortcutFullscreenModifiers = (int)FullscreenScreenshotShortcut.Modifiers;
            Properties.Settings.Default.Save();
            ShortcutChanged_Invoke(sender);
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
                    windowedScreenshotShortcut.PropertyChanged += (object o, PropertyChangedEventArgs e) => { OnPropertyChanged(nameof(WindowedScreenshotShortcut)); };
                    windowedScreenshotShortcut.PropertyChanged += WindowedScreenshotShortcut_PropertyChanged;
                    OnPropertyChanged();
                }
            }
        }

        private void ShortcutChanged_Invoke(object? sender)
        {
            if (sender is ShortcutInfo shortcutInfo)
            {
                ShortcutChanged?.Invoke(sender, sender as ShortcutInfo);
            }
            else
            {
                throw new System.Exception("Unexpected shortcut changed type");
            }
        }

        private void WindowedScreenshotShortcut_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Properties.Settings.Default.GlobalShortcutWindowKey = (int)WindowedScreenshotShortcut.Key;
            Properties.Settings.Default.GlobalShortcutWindowModifiers = (int)WindowedScreenshotShortcut.Modifiers;
            Properties.Settings.Default.Save();
            ShortcutChanged_Invoke(sender);
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

        public ZoomViewModel ZoomViewModel { get; set; } = new ZoomViewModel();
    }

    public enum ZoomMode
    {
        Fixed = 0,
        Relative = 1,
    }

    // todo: own file. + make actual viewmodel
    public class ZoomViewModel
    {
        public double ZoomFactor { get; set; } = 8;
        /// <summary>
        /// Zoom mode - relative to current canvas zoom or absolute
        /// </summary>
        public ZoomMode ZoomMode { get; set; } = ZoomMode.Relative;
        /// <summary>
        /// Never zoom beyond
        /// </summary>
        public double ZoomLimitEffectiveZoom { get; set; } = 32;
    }
}