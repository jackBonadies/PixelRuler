using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class BoundingBoxLabel : UserControl, INotifyPropertyChanged
    {
        public BoundingBoxLabel()
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
        /// Bounding Box Width
        /// </summary>
        public int BoundingBoxWidth
        {
            get { return (int)GetValue(BoundingBoxWidthProperty); }
            set
            {
                SetValue(BoundingBoxWidthProperty, value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Dependency property
        /// </summary>
        public static readonly DependencyProperty BoundingBoxWidthProperty =
            DependencyProperty.Register("BoundingBoxWidth", typeof(int), typeof(BoundingBoxLabel), new PropertyMetadata(12));

        /// <summary>
        /// Bounding Box Height
        /// </summary>
        public int BoundingBoxHeight
        {
            get { return (int)GetValue(BoundingBoxHeightProperty); }
            set
            {
                SetValue(BoundingBoxHeightProperty, value);
                OnPropertyChanged();
            }
        }

        ///// <summary>
        ///// Width
        ///// </summary>
        //public int Dim1
        //{
        //    get { return (int)GetValue(Dim1Property); }
        //    set
        //    {
        //        SetValue(Dim1Property, value);
        //        OnPropertyChanged();
        //    }
        //}

        ///// <summary>
        ///// Height
        ///// </summary>
        //public int Dim2
        //{
        //    get { return (int)GetValue(Dim2Property); }
        //    set
        //    {
        //        SetValue(Dim2Property, value);
        //        OnPropertyChanged();
        //    }
        //}

        //public int Has2Dim
        //{
        //    get { return (int)GetValue(Has2DimProperty); }
        //    set
        //    {
        //        SetValue(Has2DimProperty, value);
        //        OnPropertyChanged();
        //    }
        //}

        ///// <summary>
        ///// Dependency property
        ///// </summary>
        //public static readonly DependencyProperty Dim1Property =
        //    DependencyProperty.Register("Dim1", typeof(int), typeof(LengthLabel), new PropertyMetadata(12));

        ///// <summary>
        ///// Dependency property
        ///// </summary>
        //public static readonly DependencyProperty Dim2Property =
        //    DependencyProperty.Register("Dim2", typeof(int), typeof(LengthLabel), new PropertyMetadata(12));


        ///// <summary>
        ///// Dependency property
        ///// </summary>
        //public static readonly DependencyProperty Has2DimProperty =
        //    DependencyProperty.Register("Has2Dim", typeof(bool), typeof(LengthLabel), new PropertyMetadata(true));

        /// <summary>
        /// Dependency property
        /// </summary>
        public static readonly DependencyProperty BoundingBoxHeightProperty =
            DependencyProperty.Register("BoundingBoxHeight", typeof(int), typeof(BoundingBoxLabel), new PropertyMetadata(14));

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
    public class IntToStringConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
