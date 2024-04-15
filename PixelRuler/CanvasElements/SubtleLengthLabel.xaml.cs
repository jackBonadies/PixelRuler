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

namespace PixelRuler.CanvasElements
{
    /// <summary>
    /// Interaction logic for SubtleLengthLabel.xaml
    /// </summary>
    public partial class SubtleLengthLabel : UserControl
    {
        public SubtleLengthLabel()
        {
            InitializeComponent();
        }

        public static DependencyProperty LengthProperty =
            DependencyProperty.Register("LengthProperty", typeof(int), typeof(SubtleLengthLabel), new PropertyMetadata(0));

        public int Length
        {
            get 
            { 
                return (int)GetValue(LengthProperty); 
            }
            set
            {
                SetValue(LengthProperty, value);
            }
        }
    }
}
