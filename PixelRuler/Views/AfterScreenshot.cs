using PixelRuler.Common;
using PixelRuler.CustomControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;
using Wpf.Ui.Markup;
using WpfScreenHelper;

namespace PixelRuler.Views
{
    public static class AfterScreenshot
    {
        public static void ShowOptions(
            UIElement owner, 
            SettingsViewModel settings, 
            Action<AfterScreenshotAction, object?> action,
            double dpiOverride = -1)
        {
            System.Windows.Controls.ContextMenu CreateContextMenu()
            {
                void ContextMenu_ContextMenuClosing(object sender, ContextMenuEventArgs e)
                {
                    action(AfterScreenshotAction.Cancel, null);
                }


                var contextMenu = new System.Windows.Controls.ContextMenu();
                void ContextMenu_PreviewKeyDown(object sender, KeyEventArgs e)
                {
                    switch(e.Key)
                    {
                        case Key.A:
                            contextMenu.IsOpen = false;
                            action(AfterScreenshotAction.ViewInPixelRulerWindow, null);
                            break;
                        case Key.S:
                            contextMenu.IsOpen = false;
                            action(AfterScreenshotAction.SaveAs, null);
                            break;
                        case Key.D1:
                            contextMenu.IsOpen = false;
                            action(AfterScreenshotAction.ViewInPixelRulerWindow, null);
                            break;
                    }
                }
                var cmd = new RelayCommand((object? o) => action(AfterScreenshotAction.ViewInPixelRulerWindow, null));
                contextMenu.ContextMenuClosing += ContextMenu_ContextMenuClosing;
                contextMenu.PreviewKeyDown += ContextMenu_PreviewKeyDown;

                var menuItem1 = new MenuItemCustom() { Header = UiUtils.CreateTextBlock("View"), InputGestureText="Enter" };
                menuItem1.Click += (object sender, RoutedEventArgs e) => { action(AfterScreenshotAction.ViewInPixelRulerWindow, null); };
                var pixelRulerIcon = new System.Windows.Controls.Image() { Width = 16, Height = 16 };
                pixelRulerIcon.SetResourceReference(System.Windows.Controls.Image.SourceProperty, "di_Layer_1");
                menuItem1.SetValue(Wpf.Ui.Controls.MenuItem.IconProperty, pixelRulerIcon);

                var menuItem2 = new MenuItemCustom() { Header = UiUtils.CreateTextBlock("Cancel"), InputGestureText="Escape" };
                menuItem2.Click += (object sender, RoutedEventArgs e) => { action(AfterScreenshotAction.Cancel, null); };
                var menuItem3 = new MenuItemCustom() { Header = UiUtils.CreateTextBlock("Save As"), InputGestureText="Ctrl+S" };
                menuItem3.SetValue(Wpf.Ui.Controls.MenuItem.IconProperty, UiUtils.CreateFontIcon("\xE792")); //new SymbolIcon { Symbol = SymbolRegular.Save24 });

                menuItem3.Click += (object sender, RoutedEventArgs e) => { action(AfterScreenshotAction.SaveAs, null); };
                var menuItem4 = new MenuItemCustom() { Header = UiUtils.CreateTextBlock("Save"), InputGestureText="S" };
                menuItem4.SetValue(Wpf.Ui.Controls.MenuItem.IconProperty, UiUtils.CreateFontIcon("\xE74E")); //new SymbolIcon { Symbol = SymbolRegular.Save24 });
                menuItem4.Click += (object sender, RoutedEventArgs e) => { action(AfterScreenshotAction.Save, settings.DefaultPathSaveInfo); };

                contextMenu.Items.Add(menuItem1);
                contextMenu.Items.Add(menuItem2);
                contextMenu.Items.Add(menuItem3);
                contextMenu.Items.Add(menuItem4);

                int i = 0;
                foreach(var saveDest in settings.AdditionalPathSaveInfos)
                {
                    i++;
                    var menuItem = new MenuItemCustom() { Header = UiUtils.CreateTextBlock(saveDest.DisplayName?.SanitizeUnderscores()), InputGestureText = $"{i}" };
                    menuItem.Click += (object sender, RoutedEventArgs e) => { action(AfterScreenshotAction.Save, saveDest); };
                    contextMenu.Items.Add(menuItem);
                }

                int j = 0;
                foreach(var cmdTarget in settings.CommandTargetInfos)
                {
                    j++;
                    var menuItem = new MenuItemCustom() { Header = UiUtils.CreateTextBlock(cmdTarget.DisplayName?.SanitizeUnderscores()), InputGestureText = $"{i}" };
                    menuItem.Click += (object sender, RoutedEventArgs e) => { action(AfterScreenshotAction.CommandTarget, cmdTarget); };
                    contextMenu.Items.Add(menuItem);
                }

                return contextMenu;
            }
            var contextMenu = CreateContextMenu();

            contextMenu.Placement = PlacementMode.MousePoint; 
            // if owner is set to screenshotwindow, context menu will always have the leftmost screens dpi..
            // if not set it will always have primary screen dpi
            //contextMenu.PlacementTarget = owner;

            // since we have multiple screens with different dpis try to make that screens dpi
            if (Screen.PrimaryScreen.ScaleFactor != dpiOverride)
            {
                contextMenu.RenderTransform = new ScaleTransform()
                {
                    ScaleX = dpiOverride / Screen.PrimaryScreen.ScaleFactor,
                    ScaleY = dpiOverride / Screen.PrimaryScreen.ScaleFactor,
                };
            }

            contextMenu.IsOpen = true;
        }
    }

    public enum AfterScreenshotAction
    {
        Cancel = 0,
        ViewInPixelRulerWindow = 1,
        Save = 2,
        SaveAs = 3,
        Pin = 4,
        CommandTarget = 5,
    }
}
