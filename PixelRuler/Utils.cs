using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace PixelRuler
{
    public class ColorDisplayColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is System.Drawing.Color color)
            {
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, color.R, color.G, color.B));
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("One way converter");
        }
    }

    public class ColorFormatStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is System.Drawing.Color color)
            {
                return formatColor(color);
            }
            else
            {
                return string.Empty;
            }
        }

        private string formatColor(System.Drawing.Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("One way converter");
        }
    }

    public class PercentFormatStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double)
            {
                return formatZoom((double)value);
            }
            else if (value is double[] doubleArray)
            {
                return doubleArray.Select(it => formatZoom(it));
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

    public class EnumToBoolCheckedStickyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? parameterString = parameter as string;
            if (parameterString == null || value == null)
            {
                throw new Exception("Missing Parameter");
            }

            if (Enum.IsDefined(value.GetType(), value) == false)
            {
                throw new Exception("Bad Enum Parameter");
            }

            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? parameterString = parameter as string;
            if (parameterString == null)
            {
                throw new Exception("Missing Parameter");
            }

            // never "uncheck" i.e. whether true or false we set to this
            return Enum.Parse(targetType, parameterString);
        }
    }

    //public static class ImageBehavior
    //{
    //    public static readonly DependencyProperty UpdatePositioningOnSourceChangeProperty = DependencyProperty.RegisterAttached(
    //        "UpdatePositioningOnSourceChange",
    //        typeof(bool),
    //        typeof(ImageBehavior),
    //        new PropertyMetadata(false, OnUpdatePositioningOnSourceChanged_Changed));

    //    public static bool GetUpdatePositioningOnSourceChange(DependencyObject obj) => (bool)obj.GetValue(UpdatePositioningOnSourceChangeProperty);
    //    public static void SetUpdatePositioningOnSourceChange(DependencyObject obj, bool value) => obj.SetValue(UpdatePositioningOnSourceChangeProperty, value);

    //    private static void OnUpdatePositioningOnSourceChanged_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        if (d is Image image)
    //        {
    //            if ((bool)e.NewValue)
    //            {
    //                image.SourceUpdated += OnImageSourceUpdated;
    //            }
    //            else
    //            {
    //                image.SourceUpdated -= OnImageSourceUpdated;
    //            }
    //        }
    //    }

    //    private static void OnImageSourceUpdated(object sender, DataTransferEventArgs e)
    //    {
    //        if (sender is Image image && image.Source != null)
    //        {
    //            SetImageLocation(image.Parent as Canvas, image);
    //        }
    //    }



    //}
}
