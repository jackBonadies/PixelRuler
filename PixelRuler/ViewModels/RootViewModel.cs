using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Uwp.Notifications;
using PixelRuler.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Notifications;

namespace PixelRuler.ViewModels
{
    public partial class RootViewModel : ObservableObject
    {
        public RootViewModel(SettingsViewModel? settingsViewModel = null)
        {
            Settings = settingsViewModel;
            this.NewScreenshotFullCommand = new RelayCommandFull((object? o) => { App.NewFullscreenshotLogic(true); }, Settings.FullscreenScreenshotShortcut, "New Full Screenshot");
            this.NewScreenshotWindowedCommand = new RelayCommandFull((object? o) => { App.EnterScreenshotTool(OverlayMode.Window, true); }, Settings.WindowedScreenshotShortcut, "New Windowed Screenshot");
            this.NewScreenshotRegionCommand = new RelayCommandFull((object? o) => { App.EnterScreenshotTool(OverlayMode.WindowAndRegionRect, true); }, Settings.WindowedRegionScreenshotShortcut, "New Region Screenshot");
            this.QuickMeasureCommand = new RelayCommandFull((object? o) => { App.EnterScreenshotTool(OverlayMode.QuickMeasure, true); }, Settings.QuickMeasureShortcut, "Quick Measure");
            this.QuickColorCommand = new RelayCommandFull((object? o) => { App.EnterScreenshotTool(OverlayMode.QuickColor, true); }, Settings.QuickColorShortcut, "Quick Color");
            this.SettingsCommand = new RelayCommandFull((object? o) => { App.ShowSettingsWindowSingleInstance(); }, Key.None, ModifierKeys.None, "Settings");
        }


        public RelayCommandFull QuickMeasureCommand { get; init; }
        public RelayCommandFull QuickColorCommand { get; init; }
        public RelayCommandFull NewScreenshotRegionCommand { get; init; }
        public RelayCommandFull NewScreenshotWindowedCommand { get; init; }
        public RelayCommandFull NewScreenshotFullCommand { get; init; }
        public RelayCommandFull SettingsCommand { get; init; }
        public SettingsViewModel Settings { get; set; }

        public void ShowStartupToast()
        {

            bool shortcutAvailable = false;
            string shortcutText = string.Empty;
            ShortcutInfo[] shortcutsInPreferedOrder = new ShortcutInfo[]
            {
                this.Settings.WindowedRegionScreenshotShortcut,
                this.Settings.FullscreenScreenshotShortcut,
                this.Settings.QuickMeasureShortcut,
                this.Settings.QuickColorShortcut,
            };

            foreach(var shortcutIt in shortcutsInPreferedOrder)
            {
                if (shortcutIt.Status == RegistrationStatus.SuccessfulRegistration)
                {
                    shortcutAvailable = true;
                    shortcutText = KeyboardHelper.GetShortcutLabel(
                        shortcutIt.Modifiers,
                        shortcutIt.Key);
                    break;
                }
            }

            string toGetStarted = "Right click tray icon to get started";
            if (shortcutAvailable)
            {
                toGetStarted = $"Right click tray icon or press {shortcutText} to get started.";
            }

            new ToastContentBuilder()
                .AddText("Pixel Ruler is Running", AdaptiveTextStyle.Header)
                .AddText(toGetStarted)
                .SetToastDuration(ToastDuration.Short)
                .Show(toast => 
                {
                    //toast.ExpirationTime = DateTime.Now;
                    toast.ExpiresOnReboot = true;
                    ToastHelper.ConfigureToast(toast, true, (ToastArguments args) =>
                    {
                        NewScreenshotRegionCommand.Execute(null);
                    });
                });
        }
    }

    public static class OverlayModeExt
    {
        public static bool IsSelectRegion(this OverlayMode mode)
        {
            return mode == OverlayMode.RegionRect || mode == OverlayMode.WindowAndRegionRect;
        }

        public static bool IsSelectWindow(this OverlayMode mode)
        {
            return mode == OverlayMode.Window || mode == OverlayMode.WindowAndRegionRect;
        }
    }


    public enum OverlayMode
    {
        None = -1,
        Window = 0,
        RegionRect = 1,
        QuickMeasure = 2,
        QuickColor = 3,
        WindowAndRegionRect = 4,
    }
}
