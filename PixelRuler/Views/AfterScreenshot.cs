using PixelRuler.Common;
using PixelRuler.CustomControls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;
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
                    switch (e.Key)
                    {
                        case Key.A:
                            contextMenu.IsOpen = false;
                            action(AfterScreenshotAction.ViewInPixelRulerWindow, null);
                            break;
                        case Key.P:
                            contextMenu.IsOpen = false;
                            action(AfterScreenshotAction.Pin, null);
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

                var menuItemPixelRuler = new MenuItemCustom() { Header = UiUtils.CreateTextBlock("View"), InputGestureText = "Enter" };
                menuItemPixelRuler.Click += (object sender, RoutedEventArgs e) => { action(AfterScreenshotAction.ViewInPixelRulerWindow, null); };
                var pixelRulerIcon = new System.Windows.Controls.Image() { Width = 16, Height = 16 };
                pixelRulerIcon.SetResourceReference(System.Windows.Controls.Image.SourceProperty, "di_Layer_1");
                menuItemPixelRuler.SetValue(Wpf.Ui.Controls.MenuItem.IconProperty, pixelRulerIcon);


                var menuItemPin = new MenuItemCustom() { Header = UiUtils.CreateTextBlock("Pin"), InputGestureText = "P" };
                menuItemPin.Click += (object sender, RoutedEventArgs e) => { action(AfterScreenshotAction.Pin, null); };
                var pinIcon = new SymbolIcon(SymbolRegular.Pin24);
                menuItemPin.SetValue(Wpf.Ui.Controls.MenuItem.IconProperty, pinIcon);

                var menuItemClose = new MenuItemCustom() { Header = UiUtils.CreateTextBlock("Cancel"), InputGestureText = "Escape" };
                menuItemClose.SetResourceReference(Wpf.Ui.Controls.MenuItem.IconProperty, "CloseIcon");
                menuItemClose.Click += (object sender, RoutedEventArgs e) => { action(AfterScreenshotAction.Cancel, null); };

                var menuItemCopy = new MenuItemCustom() { Header = UiUtils.CreateTextBlock("Copy"), InputGestureText = "C" };
                menuItemCopy.SetResourceReference(Wpf.Ui.Controls.MenuItem.IconProperty, "CopyButtonPathProper");
                menuItemCopy.Click += (object sender, RoutedEventArgs e) => { action(AfterScreenshotAction.Copy, null); };

                var menuItemSaveAs = new MenuItemCustom() { Header = UiUtils.CreateTextBlock("Save As"), InputGestureText = "Ctrl+S" };
                menuItemSaveAs.SetValue(Wpf.Ui.Controls.MenuItem.IconProperty, UiUtils.CreateFontIcon(UiConsts.SaveAsSegoeMDL2)); //new SymbolIcon { Symbol = SymbolRegular.Save24 });
                menuItemSaveAs.Click += (object sender, RoutedEventArgs e) => { action(AfterScreenshotAction.SaveAs, null); };

                var menuItemSave = new MenuItemCustom() { Header = UiUtils.CreateTextBlock("Save"), InputGestureText = "S" };
                menuItemSave.SetValue(Wpf.Ui.Controls.MenuItem.IconProperty, UiUtils.CreateFontIcon("\xE74E")); //new SymbolIcon { Symbol = SymbolRegular.Save24 });
                menuItemSave.Click += (object sender, RoutedEventArgs e) => { action(AfterScreenshotAction.Save, settings.DefaultPathSaveInfo); };


                contextMenu.Items.Add(menuItemPixelRuler);
                contextMenu.Items.Add(menuItemPin);
                contextMenu.Items.Add(menuItemCopy);
                contextMenu.Items.Add(menuItemSave);
                contextMenu.Items.Add(menuItemSaveAs);
                contextMenu.Items.Add(menuItemClose);

                int i = 0;
                foreach (var saveDest in settings.AdditionalPathSaveInfos)
                {
                    i++;
                    var menuItem = new MenuItemCustom() { Header = UiUtils.CreateTextBlock(saveDest.DisplayName?.SanitizeUnderscores()), InputGestureText = $"{i}", Icon = new SymbolIcon(saveDest.Icon) };
                    menuItem.Click += (object sender, RoutedEventArgs e) => { action(AfterScreenshotAction.Save, saveDest); };
                    contextMenu.Items.Add(menuItem);
                }

                int j = 0;
                foreach (var cmdTarget in settings.CommandTargetInfos)
                {
                    j++;
                    var menuItem = new MenuItemCustom() { Header = UiUtils.CreateTextBlock(cmdTarget.DisplayName?.SanitizeUnderscores()), InputGestureText = $"{i}", Icon = new SymbolIcon(cmdTarget.Icon) };
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
        Copy = 6,
    }
}
