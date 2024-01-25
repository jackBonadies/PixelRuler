using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PixelRuler
{
    public class PercentFormatStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double)
            {
                return formatZoom((double)value);
            }
            else if (value is double[])
            {
                return (value as double[]).Select(it => formatZoom(it));
            }
            else
            {
                throw new Exception("Unexpected type for PercentFormatStringConverter");
            }
        }

        private string formatZoom(double zoom)
        {
            return $"{zoom.ToString()}%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return double.Parse(((string)value).Replace("%", ""));
        }
    }

}
