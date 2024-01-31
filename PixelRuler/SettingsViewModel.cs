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

        public System.Drawing.Color AnnotationsColor
        {
            get
            {
                return Properties.Settings.Default.AnnotationColor;
            }
            set
            {
                if(Properties.Settings.Default.AnnotationColor != value)
                {
                    Properties.Settings.Default.AnnotationColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool GlobalShortcutsEnabled
        {
            get
            {
                return Properties.Settings.Default.GlobalShortcutEnabled;
            }
            set
            {
                if (Properties.Settings.Default.GlobalShortcutEnabled != value)
                {
                    Properties.Settings.Default.GlobalShortcutEnabled = value;
                    // GLOBAL SHORTCUT CHANGED
                    OnPropertyChanged();
                }
            }
        }

        public System.Windows.Input.Key GlobalStartupKey
        {
            get
            {
                return (System.Windows.Input.Key)Properties.Settings.Default.GlobalShortcutKey;
            }
            set
            {
                if ((System.Windows.Input.Key)Properties.Settings.Default.GlobalShortcutKey != value)
                {
                    Properties.Settings.Default.GlobalShortcutKey = (int)value;
                    // GLOBAL SHORTCUT CHANGED
                    OnPropertyChanged();
                }
            }
        }

        public System.Windows.Input.ModifierKeys GlobalStartupModifiers
        {
            get
            {
                return (System.Windows.Input.ModifierKeys)Properties.Settings.Default.GlobalShortcutModifiers;
            }
            set
            {
                if ((System.Windows.Input.ModifierKeys)Properties.Settings.Default.GlobalShortcutModifiers != value)
                {
                    Properties.Settings.Default.GlobalShortcutKey = (int)value;
                    // GLOBAL SHORTCUT CHANGED
                    OnPropertyChanged();
                }
            }
        }

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
            if (App.Current.Resources["AnnotationColor"] is SolidColorBrush brush)
            {
                //brush.Color = Properties.Settings.Default.AnnotationColor.ConvertToWpfColor();
            }
            this.UpdateCloseToTrayChanged();

        }
    }
}