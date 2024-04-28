using PixelRuler.CanvasElements;
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
    public partial class LengthLabel : UserControl, INotifyPropertyChanged, IDimensionProvider
    {
        public LengthLabel()
        {
            InitializeComponent();

            this.Loaded += BoundingBoxLabel_Loaded;
            // we need to perform dpi scaling here bc our parent undid dpi scaling

            this.DataContext = this;
        }

        private void BoundingBoxLabel_Loaded(object sender, RoutedEventArgs e)
        {
            var scale = ScaleOverride == -1 ? this.GetDpi() : ScaleOverride;
            this.LayoutTransform = new ScaleTransform(scale, scale);
        }

        public double ScaleOverride { get; set; } = -1;

        /// <summary>
        /// Extent
        /// </summary>
        public int Dim1
        {
            get { return (int)GetValue(Dim1Property); }
            set
            {
                SetValue(Dim1Property, value);
                OnPropertyChanged();
            }
        }

        public int Dim2
        {
            get { return (int)GetValue(Dim2Property); }
            set
            {
                SetValue(Dim2Property, value);
                OnPropertyChanged();
            }
        }

        public bool Has2Dim
        {
            get { return (bool)GetValue(Has2DimProperty); }
            set
            {
                SetValue(Has2DimProperty, value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Dependency property
        /// </summary>
        public static readonly DependencyProperty Dim1Property =
            DependencyProperty.Register("Dim1", typeof(int), typeof(LengthLabel), new PropertyMetadata(12));

        /// <summary>
        /// Dependency property
        /// </summary>
        public static readonly DependencyProperty Dim2Property =
            DependencyProperty.Register("Dim2", typeof(int), typeof(LengthLabel), new PropertyMetadata(12));


        /// <summary>
        /// Dependency property
        /// </summary>
        public static readonly DependencyProperty Has2DimProperty =
            DependencyProperty.Register("Has2Dim", typeof(bool), typeof(LengthLabel), new PropertyMetadata(false));

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
