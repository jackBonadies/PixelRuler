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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PixelRuler.Views
{
    /// <summary>
    /// Interaction logic for ScreenshotSelectionPerScreenPanel.xaml
    /// </summary>
    public partial class ScreenshotSelectionPerScreenPanel : UserControl
    {
        Storyboard enterAnimationPanel;
        Storyboard leaveAnimationPanel;
        Storyboard enterAnimationHelp;
        Storyboard leaveAnimationHelp;
        public ScreenshotSelectionPerScreenPanel(double scaleFactor)
        {
            InitializeComponent();
            enterAnimationPanel = this.Resources["enterAnimationPanel"] as Storyboard ?? throw new NullReferenceException("Missing Enter Transform Storyboard");
            leaveAnimationPanel = this.Resources["leaveAnimationPanel"] as Storyboard ?? throw new NullReferenceException("Missing Leave Transform Storyboard");
            enterAnimationHelp = this.Resources["enterAnimationHelp"] as Storyboard ?? throw new NullReferenceException("Missing Enter Transform Storyboard");
            leaveAnimationHelp = this.Resources["leaveAnimationHelp"] as Storyboard ?? throw new NullReferenceException("Missing Leave Transform Storyboard");
            ScaleFactor = scaleFactor;
        }

        public double ScaleFactor { get; private set; }

        public bool IsMouseEnteredVirtual
        {
            get; set;
        }

        public Rect Bounds { get; set; }

        private void enterVirtualScreenAnimation()
        {
            this.enterAnimationPanel.Begin();
            this.enterAnimationHelp.Begin();
        }

        private void leaveVirtualScreenAnimation()
        {
            this.leaveAnimationPanel.Begin();
            this.leaveAnimationHelp.Begin();
        }

        internal void HandleMouse(Point pos)
        {
            bool inside = Bounds.Contains(pos);
            if (!IsMouseEnteredVirtual && inside)
            {
                IsMouseEnteredVirtual = true;
                enterVirtualScreenAnimation();
            }
            else if(IsMouseEnteredVirtual && !inside)
            {
                IsMouseEnteredVirtual = false;
                leaveVirtualScreenAnimation();
            }
        }
    }
}
