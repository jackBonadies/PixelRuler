using System.Windows.Controls;
using System.Windows.Input;

namespace PixelRuler.CanvasElements
{
    /// <summary>
    /// Interaction logic for CloseButtonCanvas.xaml
    /// </summary>
    public partial class CloseButtonView : UserControl
    {
        public CloseButtonView()
        {
            InitializeComponent();
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Opacity = 1;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Opacity = .6;
        }
    }
}
