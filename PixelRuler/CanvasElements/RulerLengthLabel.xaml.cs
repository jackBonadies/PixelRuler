using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for BoundingBoxLabel.xaml
    /// </summary>
    public partial class RulerLengthLabel : UserControl, INotifyPropertyChanged
    {
        public RulerLengthLabel()
        {
            InitializeComponent();

            this.Loaded += BoundingBoxLabel_Loaded;
            // we need to perform dpi scaling here bc our parent undid dpi scaling

            this.DataContext = this;
        }

        private void BoundingBoxLabel_Loaded(object sender, RoutedEventArgs e)
        {
            var dpi = this.GetDpi();
            this.LayoutTransform = new ScaleTransform(dpi, dpi);
        }

        /// <summary>
        /// Extent
        /// </summary>
        public int Extent
        {
            get { return (int)GetValue(ExtentProperty); }
            set
            {
                SetValue(ExtentProperty, value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Dependency property
        /// </summary>
        public static readonly DependencyProperty ExtentProperty =
            DependencyProperty.Register("Extent", typeof(int), typeof(RulerLengthLabel), new PropertyMetadata(12));

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(name));
        }

    }
}
