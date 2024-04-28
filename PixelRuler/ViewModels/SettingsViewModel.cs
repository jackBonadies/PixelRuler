using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using PixelRuler.Models;
using PixelRuler.Properties;
using PixelRuler.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Windows.Networking.Vpn;
using Wpf.Ui.Controls;

namespace PixelRuler
{
    public partial class SettingsViewModel : ObservableObject
    {
        public SettingsViewModel()
        {
            FullscreenScreenshotShortcut = new ShortcutInfo(
                "Fullscreen Screenshot",
                App.FULLSCREEN_HOTKEY_ID,
                (Key)Properties.Settings.Default.GlobalShortcutFullscreenKey,
                (ModifierKeys)Properties.Settings.Default.GlobalShortcutFullscreenModifiers,
                Key.PrintScreen,
                ModifierKeys.Control | ModifierKeys.Shift);

            WindowedScreenshotShortcut = new ShortcutInfo(
                "Window Screenshot",
                App.WINDOWED_HOTKEY_ID,
                (Key)Properties.Settings.Default.GlobalShortcutWindowKey,
                (ModifierKeys)Properties.Settings.Default.GlobalShortcutWindowModifiers,
                Key.PrintScreen,
                ModifierKeys.Shift);

            WindowedRegionScreenshotShortcut = new ShortcutInfo(
                "Region Screenshot",
                App.REGION_WINDOWED_HOTKEY_ID,
                (Key)Properties.Settings.Default.GlobalShortcutRegionKey,
                (ModifierKeys)Properties.Settings.Default.GlobalShortcutRegionModifiers,
                Key.PrintScreen,
                ModifierKeys.Shift);

            QuickMeasureShortcut = new ShortcutInfo(
                "Quick Measure",
                App.QUICK_MEASURE_HOTKEY_ID,
                (Key)Properties.Settings.Default.GlobalShortcutQuickMeasureKey,
                (ModifierKeys)Properties.Settings.Default.GlobalShortcutQuickMeasureModifiers,
                Key.M,
                ModifierKeys.Windows | ModifierKeys.Shift);

            QuickColorShortcut = new ShortcutInfo(
                "Quick Color",
                App.QUICK_COLOR_HOTKEY_ID,
                (Key)Properties.Settings.Default.GlobalShortcutQuickColorKey,
                (ModifierKeys)Properties.Settings.Default.GlobalShortcutQuickColorModifiers,
                Key.C,
                ModifierKeys.Windows | ModifierKeys.Shift);

            //GlobalShortcuts.Add(WindowedScreenshotShortcut);
            GlobalShortcuts.Add(WindowedRegionScreenshotShortcut);
            GlobalShortcuts.Add(FullscreenScreenshotShortcut);
            GlobalShortcuts.Add(QuickMeasureShortcut);
            GlobalShortcuts.Add(QuickColorShortcut);

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

            editSavePathInfoCommand = new RelayCommand((object? o) =>
            {
                if(o is PathSaveInfo pathSaveInfo)
                {
                    EditSavePathCommandEvent?.Invoke(this, (pathSaveInfo, false));
                }
                else
                {
                    throw new Exception("Unexpected Type");
                }
            });

            addSavePathInfoCommand = new RelayCommand((object? o) =>
            {
                var num = this.AdditionalPathSaveInfos.Count + 1;
                var customPathSaveInfo = new PathSaveInfo($"Custom Path {num}", App.DefaultSavePath, "MyScreenshot {datetime:yyyy_MM_dd_HHmmss}", "png");
                EditSavePathCommandEvent?.Invoke(this, (customPathSaveInfo, true));
            });

            addCommandTargetCommand = new RelayCommand((object? o) =>
            {
                var num = this.CommandTargetInfos.Count + 1;
                var customPathSaveInfo = new CommandTargetInfo($"My Command {num}", "", "\"{filename}\"");
                EditCommandTargetCommandEvent?.Invoke(this, (customPathSaveInfo, true));
            });

            editCommandTargetCommand = new RelayCommand((object? o) =>
            {
                if(o is CommandTargetInfo cmdTargetInfo)
                {
                    EditCommandTargetCommandEvent?.Invoke(this, (cmdTargetInfo, false));
                }
                else
                {
                    throw new Exception("Unexpected Type");
                }
            });

            RestorePathInfos();
            RestoreCommandTargets();
        }

        public void DeletePathItem(PathSaveInfo pathSaveInfo)
        {
            this.AdditionalPathSaveInfos.Remove(pathSaveInfo);
            OnPropertyChanged(nameof(AdditionalPathSaveInfos));
        }

        private void RestoreDefaultPathInfo()
        {
            var defaultPathInfoString = Properties.Settings.Default.DefaultPathInfo;

            if(string.IsNullOrEmpty(defaultPathInfoString))
            {
                DefaultPathSaveInfo = new PathSaveInfo("Default", App.DefaultSavePath, "Screenshot {datetime:yyyy_MM_dd_HHmmss}", "png");
            }
            else
            {
                DefaultPathSaveInfo = JsonSerializer.Deserialize(defaultPathInfoString, typeof(PathSaveInfo)) as PathSaveInfo;
                ArgumentNullException.ThrowIfNull(DefaultPathSaveInfo);
            }
            DefaultPathSaveInfo.IsDefault = true;
        }

        private void RestoreAdditionalPathInfos()
        {
            var additionalPathInfosString = Properties.Settings.Default.AdditionalPathInfos;

            if (string.IsNullOrEmpty(additionalPathInfosString))
            {
                AdditionalPathSaveInfos = new ObservableCollection<PathSaveInfo>()
                {
                    new PathSaveInfo("UI Examples", "%USERPROFILE%\\Pictures\\UI_Examples", "Screenshot {datetime:yyyy_MM_dd_HHmmss}", "png", false),
                };
            }
            else
            {
                AdditionalPathSaveInfos = new ObservableCollection<PathSaveInfo>(JsonSerializer.Deserialize(additionalPathInfosString, typeof(List<PathSaveInfo>)) as List<PathSaveInfo>);
            }
        }
        private void RestoreCommandTargets()
        {
            var commandTargetsString = Properties.Settings.Default.CommandTargets;

            if (string.IsNullOrEmpty(commandTargetsString))
            {
                CommandTargetInfos = new ObservableCollection<CommandTargetInfo>()
                {
                    new CommandTargetInfo("Paint", "mspaint", "\"{filename}\"")
                };
            }
            else
            {
                CommandTargetInfos = new ObservableCollection<CommandTargetInfo>(JsonSerializer.Deserialize(commandTargetsString, typeof(List<CommandTargetInfo>)) as List<CommandTargetInfo>);
            }
        }

        private void RestorePathInfos()
        {
            RestoreDefaultPathInfo();
            RestoreAdditionalPathInfos();
        }

        private void SaveDefaultPathInfo()
        {
            var pathSaveInfoString = JsonSerializer.Serialize(DefaultPathSaveInfo, typeof(PathSaveInfo));
            Properties.Settings.Default.DefaultPathInfo = pathSaveInfoString;
            Properties.Settings.Default.Save();
        }

        private void SaveAdditionalPathInfos()
        {
            var additionalPathSaveInfosList = AdditionalPathSaveInfos.ToList();
            var pathSaveInfoString = JsonSerializer.Serialize(additionalPathSaveInfosList, typeof(List<PathSaveInfo>));
            Properties.Settings.Default.AdditionalPathInfos = pathSaveInfoString;
            Properties.Settings.Default.Save();
        }

        private void SavePathInfos()
        {
            SaveDefaultPathInfo();
            SaveAdditionalPathInfos();
        }

        private void SaveCommandInfos()
        {
            var additionalPathSaveInfosList = CommandTargetInfos.ToList();
            var pathSaveInfoString = JsonSerializer.Serialize(additionalPathSaveInfosList, typeof(List<CommandTargetInfo>));
            Properties.Settings.Default.CommandTargets = pathSaveInfoString;
            Properties.Settings.Default.Save();
        }

        public Key ZoomBoxQuickZoomKey { get; set; } = Key.Space;

        public ObservableCollection<ShortcutInfo> GlobalShortcuts { get; set; } = new ObservableCollection<ShortcutInfo>();


        public event EventHandler<ShortcutInfo> EditShortcutCommandEvent;

        public event EventHandler<(PathSaveInfo, bool)> EditSavePathCommandEvent;
        public event EventHandler<(CommandTargetInfo, bool)> EditCommandTargetCommandEvent;

        [ObservableProperty]
        private RelayCommand editShortcutCommand;

        [ObservableProperty]
        private RelayCommand editSavePathInfoCommand;

        [ObservableProperty]
        private RelayCommand addSavePathInfoCommand;

        [ObservableProperty]
        private RelayCommand? deleteSavePathInfoCommand;

        [ObservableProperty]
        private RelayCommand editCommandTargetCommand;

        [ObservableProperty]
        private RelayCommand addCommandTargetCommand;

        [ObservableProperty]
        private RelayCommand deleteCommandTargetCommand;

        [ObservableProperty]
        private RelayCommand clearShortcutCommand;

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
                    OnPropertyChangedAndSave();
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
                    OnPropertyChangedAndSave();
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
                    OnPropertyChangedAndSave();
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
                    OnPropertyChangedAndSave();
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
                    OnPropertyChangedAndSave();
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
                    OnPropertyChangedAndSave();
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

        [ObservableProperty]
        private PathSaveInfo defaultPathSaveInfo;

        public ObservableCollection<PathSaveInfo> AdditionalPathSaveInfos { get; set; }

        public ObservableCollection<CommandTargetInfo> CommandTargetInfos { get; set; }

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
                    OnPropertyChangedAndSave();
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
                    fullscreenScreenshotShortcut.PropertyChanged += (object o, PropertyChangedEventArgs e) => { OnPropertyChangedAndSave(nameof(FullscreenScreenshotShortcut)); };
                    fullscreenScreenshotShortcut.PropertyChanged += FullscreenScreenshotShortcut_PropertyChanged;
                    //TODO shortcut changed.
                    OnPropertyChangedAndSave();
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
                    windowedScreenshotShortcut.PropertyChanged += (object o, PropertyChangedEventArgs e) => { OnPropertyChangedAndSave(nameof(WindowedScreenshotShortcut)); };
                    windowedScreenshotShortcut.PropertyChanged += WindowedScreenshotShortcut_PropertyChanged;
                    OnPropertyChangedAndSave();
                }
            }
        }

        private ShortcutInfo windowedRegionScreenshotShortcut;
        public ShortcutInfo WindowedRegionScreenshotShortcut
        {
            get
            {
                return windowedRegionScreenshotShortcut;
            }
            set
            {
                if (windowedRegionScreenshotShortcut != value)
                {
                    windowedRegionScreenshotShortcut = value;
                    windowedRegionScreenshotShortcut.PropertyChanged += (object o, PropertyChangedEventArgs e) => { OnPropertyChangedAndSave(nameof(WindowedRegionScreenshotShortcut)); };
                    windowedRegionScreenshotShortcut.PropertyChanged += WindowedRegionScreenshotShortcut_PropertyChanged;
                    OnPropertyChangedAndSave();
                }
            }
        }

        private ShortcutInfo quickMeasureShortcut;
        public ShortcutInfo QuickMeasureShortcut
        {
            get
            {
                return quickMeasureShortcut;
            }
            set
            {
                if (quickMeasureShortcut != value)
                {
                    quickMeasureShortcut = value;
                    quickMeasureShortcut.PropertyChanged += (object o, PropertyChangedEventArgs e) => { OnPropertyChangedAndSave(nameof(QuickMeasureShortcut)); };
                    quickMeasureShortcut.PropertyChanged += QuickMeasureShortcut_PropertyChanged;
                    OnPropertyChangedAndSave();
                }
            }
        }

        private ShortcutInfo quickColorShortcut;
        public ShortcutInfo QuickColorShortcut
        {
            get
            {
                return quickColorShortcut;
            }
            set
            {
                if (quickColorShortcut != value)
                {
                    quickColorShortcut = value;
                    quickColorShortcut.PropertyChanged += (object o, PropertyChangedEventArgs e) => { OnPropertyChangedAndSave(nameof(QuickColorShortcut)); };
                    quickColorShortcut.PropertyChanged += QuickColorShortcutShortcut_PropertyChanged;
                    OnPropertyChangedAndSave();
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

        private void QuickColorShortcutShortcut_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Properties.Settings.Default.GlobalShortcutRegionKey = (int)WindowedRegionScreenshotShortcut.Key;
            Properties.Settings.Default.GlobalShortcutRegionModifiers = (int)WindowedRegionScreenshotShortcut.Modifiers;
            Properties.Settings.Default.Save();
            ShortcutChanged_Invoke(sender);
        }


        private void QuickMeasureShortcut_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Properties.Settings.Default.GlobalShortcutQuickMeasureKey = (int)QuickMeasureShortcut.Key;
            Properties.Settings.Default.GlobalShortcutQuickMeasureModifiers = (int)QuickMeasureShortcut.Modifiers;
            Properties.Settings.Default.Save();
            ShortcutChanged_Invoke(sender);
        }

        private void WindowedRegionScreenshotShortcut_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Properties.Settings.Default.GlobalShortcutRegionKey = (int)WindowedRegionScreenshotShortcut.Key;
            Properties.Settings.Default.GlobalShortcutRegionModifiers = (int)WindowedRegionScreenshotShortcut.Modifiers;
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
        //            OnPropertyChangedAndSave();
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
        //            OnPropertyChangedAndSave();
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
                catch (Exception)
                {
                    return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                }
            }
        }

        private void OnPropertyChangedAndSave([CallerMemberName] string? name = null)
        {
            Properties.Settings.Default.Save();
            OnPropertyChanged(name);
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void SetState()
        {
            SetAnnotationColorState();
            ThemeManager.UpdateForThemeChanged(this.DayNightMode);
            this.UpdateCloseToTrayChanged();

        }

        public void AddCommandTargetInfo(CommandTargetInfo pending)
        {
            this.CommandTargetInfos.Add(pending);
            OnPropertyChanged(nameof(CommandTargetInfos));
            SaveCommandInfos();
        }

        public void UpdateCommandTargetInfo(CommandTargetInfo cmdTargetInfo, CommandTargetInfo pending)
        {
            for (int i = 0; i < CommandTargetInfos.Count; i++)
            {
                if (this.CommandTargetInfos[i] == cmdTargetInfo)
                {
                    this.CommandTargetInfos[i] = pending;
                    OnPropertyChanged(nameof(CommandTargetInfos));
                }
            }
            SaveCommandInfos();
        }

        public void AddPathInfo(PathSaveInfo pathSaveInfo)
        {
            this.AdditionalPathSaveInfos.Add(pathSaveInfo);
            OnPropertyChanged(nameof(AdditionalPathSaveInfos));
            SavePathInfos();
        }

        public void UpdatePathInfo(PathSaveInfo pathSaveInfo, PathSaveInfo pending)
        {
            if (this.DefaultPathSaveInfo == pathSaveInfo)
            {
                this.DefaultPathSaveInfo = pending;
                OnPropertyChanged(nameof(DefaultPathSaveInfo));
            }
            else
            {
                for (int i = 0; i < AdditionalPathSaveInfos.Count; i++)
                {
                    if (this.AdditionalPathSaveInfos[i] == pathSaveInfo)
                    {
                        this.AdditionalPathSaveInfos[i] = pending;
                        OnPropertyChanged(nameof(AdditionalPathSaveInfos));
                    }
                }
            }
            SavePathInfos();
        }

        public ScreenshotSelectionViewModel ScreenshotSelectionViewModel { get; set; } = new ScreenshotSelectionViewModel();
        public ZoomViewModel ZoomViewModel { get; set; } = new ZoomViewModel();
        public Key PromptKey { get; private set; } = Key.LeftShift;
        public int ScreenshotDelayMs { get; private set; } = 100;

        [ObservableProperty]
        private ColorFormatMode colorFormatMode;

        [ObservableProperty]
        private QuickColorMode quickColorMode; 

        public bool IsQuickColorAutoCopy
        {
            get
            {
                return QuickColorMode == QuickColorMode.AutoCopyAndClose || QuickColorMode == QuickColorMode.AutoCopyMany;
            }
        }

    }

    public partial class ScreenshotSelectionViewModel : ObservableObject
    {
        [ObservableProperty]
        bool screenshotHelpOn;
        [ObservableProperty]
        bool zoomBoxLocation; // TODO
        [ObservableProperty]
        bool zoomBoxOn; // TODO

        public ScreenshotSelectionViewModel()
        {
            this.PropertyChanged += ScreenshotSelectionViewModel_PropertyChanged;
        }

        private void ScreenshotSelectionViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(ScreenshotHelpOn))
            {
                ScreenshotHelpOnChanged?.Invoke(sender, this.ScreenshotHelpOn);
            }
        }

        public event EventHandler<bool>? ScreenshotHelpOnChanged;
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

        public int BorderThickness { get; set; } = 3;
    }

    public enum ColorFormatMode
    {
        Hex = 0,
    }

    public enum QuickColorMode
    {
        ColorTrayCopyExplicit = 0,
        AutoCopyMany = 1,
        AutoCopyAndClose = 2
    }
}