using System.Windows;
using System.Windows.Controls;

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
