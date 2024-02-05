using PixelRuler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            this.Focus();
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
            if(keysDown.Count == 0)
            {
                pendingShortcutInfo.Key = Key.None;
                pendingShortcutInfo.Modifiers = ModifierKeys.None;
            }

            bool relevantKey = false;
            if(e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                pendingShortcutInfo.Modifiers |= ModifierKeys.Control;
                relevantKey = true;
            }
            else if(e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                pendingShortcutInfo.Modifiers |= ModifierKeys.Shift;
                relevantKey = true;
            }
            else if(e.Key == Key.LWin || e.Key == Key.RWin)
            {
                pendingShortcutInfo.Modifiers |= ModifierKeys.Windows;
                relevantKey = true;
            }
            else if(e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            {
                pendingShortcutInfo.Modifiers |= ModifierKeys.Alt;
                relevantKey = true;
            }
            else if(e.Key == Key.System)
            {
                if(e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt)
                {
                    pendingShortcutInfo.Modifiers |= ModifierKeys.Alt;
                    keysDown.Add(e.SystemKey);
                }
            }
            else
            {
                pendingShortcutInfo.Key = e.Key;// 
                relevantKey = true;
            }

            if(relevantKey)
            {
                keysDown.Add(e.Key);
            }

            e.Handled = true;
        }

        private void UserControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            keysDown.Remove(e.Key);

            e.Handled = true;
        }
    }
}
