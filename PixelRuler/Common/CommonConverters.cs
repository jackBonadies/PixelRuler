﻿using PixelRuler.Models;
using PixelRuler.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace PixelRuler
{
    public class DrawingColorToWpfBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is System.Drawing.Color color)
            {
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
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

    public class PointPositionStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is System.Windows.Point pt)
            {
                return $"{pt.X}, {pt.Y}px";
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

    public class WidthHeightDisplayConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double width = (double)values[0];
            double height = (double)values[1];
            if (width > 0 && height > 0)
            {
                return $"size: {width} × {height}px";
            }
            else if (width > 0)
            {
                return $"size: {width}px";
            }
            else if (height > 0)
            {
                return $"size: {height}px";
            }
            else
            {
                return $"size: 0px";
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new Exception("One way converter");
        }
    }

    public class PathTokenDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var pathSaveInfoToken = value as PathSaveInfoToken;
            if(pathSaveInfoToken == null)
            {
                return DependencyProperty.UnsetValue;
            }

            if(string.IsNullOrEmpty(pathSaveInfoToken.FormatSpecifierName))
            {
                return $"{{{pathSaveInfoToken.TokenName}}}";
            }
            return $"{{{pathSaveInfoToken.TokenName}:{pathSaveInfoToken.FormatSpecifierName}}}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("One way");
        }
    }

    public class DebugConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("One way");
        }
    }

    public class DisplayKeysMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Key key = (Key)values[0];
            ModifierKeys modifierKeys = (ModifierKeys)values[1];
            bool pendingCase = values.Length > 2 && values[2] is PendingShortcutInfo;
            return DisplayKeysHelper.GetDisplayKeys(key, modifierKeys, pendingCase);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new Exception("One way converter");
        }
    }

    public class DisplayKeysConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ShortcutInfo shortcutInfo)
            {
                return DisplayKeysHelper.GetDisplayKeys(shortcutInfo.Key, shortcutInfo.Modifiers);
            }
            throw new Exception("Unexpected type for DisplayKeysConverter");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("One way");
        }
    }

    public class DoubleNaNZeroBlankConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double.NaN || value is 0.0)
            {
                return string.Empty;
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("One way");
        }
    }

    public class DoubleShouldShowPlaceholderConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double doubleVal)
            {
                if (doubleVal < 0)
                {
                    // if -1 then show placeholder
                    return parameter.ToString();
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("One way");
        }
    }

    public class ColorConverter : IValueConverter
    {
        /// <summary>
        /// Drawing Color to WPF Media Color
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameterFactor"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public object Convert(object value, Type targetType, object parameterFactor, System.Globalization.CultureInfo culture)
        {
            System.Drawing.Color sysDrawingColor = (value as ColorAnnotationsBundle).AnnotationColor;
            if (parameterFactor is string factorStr)
            {
                var factor = double.Parse(factorStr);
                sysDrawingColor = sysDrawingColor.Times(factor);
                
            }
            return new SolidColorBrush(sysDrawingColor.ConvertToWpfColor());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((SolidColorBrush)value).Color.ToWinformColor();
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

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool bVal)
            {
                if(bVal)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
            throw new Exception("Unexpected type in BoolToVisibilityConverter");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("One way converter");
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

    public class EnumToOptionsCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<string> enumDisplayLabels = new List<string>();
            foreach (var item in Enum.GetValues(value.GetType()))
            {
                enumDisplayLabels.Add(((Enum)item).GetDisplayLabel());

            }
            return enumDisplayLabels;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("One way converter");
        }
    }

    public class EnumToOptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Enum)value).GetDisplayLabel();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is string displayName)
            {
                foreach (var item in Enum.GetValues(targetType))
                {
                    if (((Enum)item).GetDisplayLabel() == displayName)
                    {
                        return item;
                    }
                }
                throw new Exception("Enum not found");
            }
            throw new Exception("Unexpected Type");
        }
    }
}