using PixelRuler.CustomControls;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public static void ShowOptions(UIElement owner, Action<AfterScreenshotAction> action)
        {
            System.Windows.Controls.ContextMenu CreateContextMenu()
            {
                void MenuItem1_Click(object sender, RoutedEventArgs e)
                {
                    action(AfterScreenshotAction.ViewInPixelRulerWindow);
                }

                void MenuItem2_Click(object sender, RoutedEventArgs e)
                {
                    action(AfterScreenshotAction.Cancel);
                }

                void ContextMenu_ContextMenuClosing(object sender, ContextMenuEventArgs e)
                {
                    action(AfterScreenshotAction.Cancel);
                }


                var contextMenu = new System.Windows.Controls.ContextMenu();
                void ContextMenu_PreviewKeyDown(object sender, KeyEventArgs e)
                {
                    switch(e.Key)
                    {
                        case Key.A:
                            contextMenu.IsOpen = false;
                            action(AfterScreenshotAction.ViewInPixelRulerWindow);
                            break;
                        case Key.S:
                            contextMenu.IsOpen = false;
                            action(AfterScreenshotAction.SaveAs);
                            break;
                        case Key.D1:
                            contextMenu.IsOpen = false;
                            action(AfterScreenshotAction.ViewInPixelRulerWindow);
                            break;
                    }
                }
                var cmd = new RelayCommand((object? o) => action(AfterScreenshotAction.ViewInPixelRulerWindow));
                contextMenu.ContextMenuClosing += ContextMenu_ContextMenuClosing;
                contextMenu.PreviewKeyDown += ContextMenu_PreviewKeyDown;

                var menuItem1 = new MenuItemCustom() { Header = "View", InputGestureText="Enter" };
                menuItem1.Click += MenuItem1_Click; 
                var menuItem2 = new MenuItemCustom() { Header = "Cancel", InputGestureText="Escape" };
                menuItem2.Click += MenuItem2_Click;

                contextMenu.Items.Add(menuItem1);
                contextMenu.Items.Add(menuItem2);

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
        SaveAs = 2,
    }
}
