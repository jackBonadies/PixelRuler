using System.Windows.Controls;
using System.Windows.Input;

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
