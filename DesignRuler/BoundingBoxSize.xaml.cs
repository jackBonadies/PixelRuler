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

namespace DesignRuler
{
    /// <summary>
    /// Interaction logic for BoundingBoxLabel.xaml
    /// </summary>
    public partial class BoundingBoxLabel : UserControl
    {
        public BoundingBoxLabel()
        {
            InitializeComponent();
            this.DataContext = this;
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
                //TODO
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
                //TODO
            }
        }

        /// <summary>
        /// Dependency property
        /// </summary>
        public static readonly DependencyProperty BoundingBoxHeightProperty =
            DependencyProperty.Register("BoundingBoxHeight", typeof(int), typeof(BoundingBoxLabel), new PropertyMetadata(14));


    }
    public class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
