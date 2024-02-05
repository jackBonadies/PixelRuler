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
    /// Interaction logic for ShortcutInfoView.xaml
    /// </summary>
    public partial class ShortcutInfoView : UserControl
    {
        public ShortcutInfoView()
        {
            InitializeComponent();
        }
    }

    public class DesignTimeShortcutInfo : ShortcutInfo
    {
        public DesignTimeShortcutInfo() : base("Fullscreen Screenshot", 0, Key.Q, ModifierKeys.Control | ModifierKeys.Shift, Key.P, ModifierKeys.Control | ModifierKeys.Shift)
        {
        }
    }
}
