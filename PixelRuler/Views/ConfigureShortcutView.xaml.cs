using PixelRuler.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PixelRuler
{
    /// <summary>
    /// Interaction logic for ConfigureShortcutView.xaml
    /// </summary>
    public partial class ConfigureShortcutView : UserControl
    {
        public ConfigureShortcutView(PendingShortcutInfo pendingShortcutInfo)
        {
            InitializeComponent();
            this.pendingShortcutInfo = pendingShortcutInfo;
            this.DataContext = pendingShortcutInfo;
            this.Focusable = true;
            this.MouseUp += ConfigureShortcutView_MouseUp;
            this.LostFocus += ConfigureShortcutView_LostFocus;
            this.Focus();
        }

        private void ConfigureShortcutView_LostFocus(object sender, RoutedEventArgs e)
        {
            keysDown.Clear();
        }

        private void ConfigureShortcutView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
        }

        PendingShortcutInfo pendingShortcutInfo;

        private HashSet<Key> keysDown = new HashSet<Key>();

        /// <summary>
        /// Cannot handle prntscreen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (keysDown.Count == 0)
            {
                pendingShortcutInfo.Key = Key.None;
                pendingShortcutInfo.Modifiers = ModifierKeys.None;
            }

            bool relevantKey = false;
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                pendingShortcutInfo.Modifiers |= ModifierKeys.Control;
                relevantKey = true;
            }
            else if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                pendingShortcutInfo.Modifiers |= ModifierKeys.Shift;
                relevantKey = true;
            }
            else if (e.Key == Key.LWin || e.Key == Key.RWin)
            {
                pendingShortcutInfo.Modifiers |= ModifierKeys.Windows;
                relevantKey = true;
            }
            else if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            {
                pendingShortcutInfo.Modifiers |= ModifierKeys.Alt;
                relevantKey = true;
            }
            else if (e.Key == Key.System)
            {
                if (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt)
                {
                    pendingShortcutInfo.Modifiers |= ModifierKeys.Alt;
                    keysDown.Add(e.SystemKey);
                }
            }
            else
            {
                pendingShortcutInfo.Key = e.Key;
                relevantKey = true;
            }

            if (relevantKey)
            {
                keysDown.Add(e.Key);
            }

            e.Handled = true;
        }

        private void UserControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.PrintScreen)
            {
                // printscreen is on keyup only
                // a low level hook is maybe too intrusive, so this hack
                //   (where we unfortunately cannot show printscreen until
                //    keyup is maybe better)
                pendingShortcutInfo.Key = e.Key;
            }
            else if (e.Key == Key.System)
            {
                if (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt)
                {
                    keysDown.Remove(e.SystemKey);
                }
            }
            else
            {
                keysDown.Remove(e.Key);
            }

            e.Handled = true;
        }
    }
}
