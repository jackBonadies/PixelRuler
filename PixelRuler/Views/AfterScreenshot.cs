using PixelRuler.CustomControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace PixelRuler.Views
{
    public static class AfterScreenshot
    {
        public static void ShowOptions(UIElement owner, SettingsViewModel settings, Action<AfterScreenshotAction, object?> action)
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

                var menuItem1 = new MenuItemCustom() { Header = "View", InputGestureText="Enter" };
                menuItem1.Click += (object sender, RoutedEventArgs e) => { action(AfterScreenshotAction.ViewInPixelRulerWindow, null); };
                var menuItem2 = new MenuItemCustom() { Header = "Cancel", InputGestureText="Escape" };
                menuItem2.Click += (object sender, RoutedEventArgs e) => { action(AfterScreenshotAction.Cancel, null); };
                var menuItem3 = new MenuItemCustom() { Header = "Save As", InputGestureText="Ctrl+S" };
                menuItem3.Click += (object sender, RoutedEventArgs e) => { action(AfterScreenshotAction.SaveAs, null); };
                var menuItem4 = new MenuItemCustom() { Header = "Save", InputGestureText="S" };
                menuItem4.Click += (object sender, RoutedEventArgs e) => { action(AfterScreenshotAction.Save, null); };

                contextMenu.Items.Add(menuItem1);
                contextMenu.Items.Add(menuItem2);
                contextMenu.Items.Add(menuItem3);
                contextMenu.Items.Add(menuItem4);

                int i = 0;
                foreach(var saveDest in settings.AdditionalPathSaveInfos)
                {
                    i++;
                    var menuItem = new MenuItemCustom() { Header = saveDest.DisplayName, InputGestureText=$"{i}" };
                    menuItem.Click += (object sender, RoutedEventArgs e) => { action(AfterScreenshotAction.Save, i); };
                    contextMenu.Items.Add(menuItem);
                }

                return contextMenu;
            }
            var contextMenu = CreateContextMenu();

            contextMenu.Placement = PlacementMode.MousePoint; 
            contextMenu.PlacementTarget = owner;

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
    }
}
