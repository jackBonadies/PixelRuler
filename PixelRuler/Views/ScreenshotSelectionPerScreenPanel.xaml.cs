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
        Storyboard enterTransform;
        Storyboard leaveTransform;
        public ScreenshotSelectionPerScreenPanel(double scaleFactor)
        {
            InitializeComponent();
            enterTransform = this.Resources["enterTransform"] as Storyboard ?? throw new NullReferenceException("Missing Enter Transform Storyboard");
            leaveTransform = this.Resources["leaveTransform"] as Storyboard ?? throw new NullReferenceException("Missing Leave Transform Storyboard");
            ScaleFactor = scaleFactor;
        }

        public double ScaleFactor { get; private set; }

        public bool IsMouseEnteredVirtual
        {
            get; set;
        }

        public Rect Bounds { get; set; }

        internal void HandleMouse(Point pos)
        {
            bool inside = Bounds.Contains(pos);
            if (!IsMouseEnteredVirtual && inside)
            {
                IsMouseEnteredVirtual = true;
                this.enterTransform.Begin();
            }
            else if(IsMouseEnteredVirtual && !inside)
            {
                IsMouseEnteredVirtual = false;
                this.leaveTransform.Begin();
            }
        }
    }
}
