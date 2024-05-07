using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            var dpi = this.GetDpi();
            this.LayoutTransform = new ScaleTransform(dpi, dpi);
        }

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

        public int Has2Dim
        {
            get { return (int)GetValue(Has2DimProperty); }
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
