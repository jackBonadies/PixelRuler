using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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

    public class RelayCommandFull : RelayCommand
    {
        private static readonly Dictionary<ModifierKeys, string> modifierKeysToText = new Dictionary<ModifierKeys, string>()
            {
                {ModifierKeys.None, ""},
                {ModifierKeys.Control, "Ctrl+"},
                {ModifierKeys.Shift, "Shift+"},
                {ModifierKeys.Control|ModifierKeys.Shift, "Ctrl+Shift+"},
                {ModifierKeys.Control|ModifierKeys.Alt, "Ctrl+Alt+"},
                {ModifierKeys.Control|ModifierKeys.Shift|ModifierKeys.Alt, "Ctrl+Shift+Alt+"},
                {ModifierKeys.Windows, "Win+"}
            };

        public RelayCommandFull(Action<object?> action, Key key, ModifierKeys modifiers, string toolTipText) : base(action)
        {
            this.key = key;
            this.modifiers = modifiers;
            this.toolTipTextBase = toolTipText;
        }

        private Key key;
        public Key Key
        {
            get
            {
                return this.key;
            }
            set
            {
                this.key = value;
                OnPropertyChanged();
                OnPropertyChanged("ToolTipTextFull");
            }
        }

        private ModifierKeys modifiers;
        public ModifierKeys Modifiers
        {
            get
            {
                return this.modifiers;
            }
            set
            {
                this.modifiers = value;
                OnPropertyChanged();
                OnPropertyChanged("ToolTipTextFull");
            }
        }

        private string toolTipTextBase;
        public string ToolTipTextBase
        {
            get
            {
                return this.toolTipTextBase;
            }
            set
            {
                this.toolTipTextBase = value;
                OnPropertyChanged();
                OnPropertyChanged("ToolTipTextFull");
            }
        }

        public string ToolTipTextFull
        {
            get
            {
                if (key != Key.None)
                {
                    var modifiersText = modifierKeysToText[modifiers];
                    return $"{toolTipTextBase} ({modifiersText}{key})";
                }
                return toolTipTextBase;
            }
        }
    }

    public class RelayCommand : ICommand
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private Action<object?> action;
        public RelayCommand(Action<object?> action)
        {
            this.action = action;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return canExecute;
        }

        private bool canExecute = true;

        public void SetCanExecute(bool value)
        {
            if (canExecute != value)
            {
                canExecute = value;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Execute(object? parameter)
        {
            action.Invoke(parameter);
        }

        public void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
