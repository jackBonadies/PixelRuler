using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using Wpf.Ui.Controls;

namespace PixelRuler.Views
{
    public partial class IconViewModel : ObservableObject
    {
        [ObservableProperty]
        public string filterText;

        public List<SymbolRegular> SymbolsAll { get; set; } = GetAllIcons();

        [ObservableProperty]
        SymbolRegular currentIcon;

        [RelayCommand]
        void NewIconSelected(object? args)
        {
            CurrentIcon = (SymbolRegular)args;
            OnNewIconSelected?.Invoke(this, EventArgs.Empty); 
        }
        public event EventHandler? OnNewIconSelected;

        public static List<SymbolRegular> GetAllIcons()
        {
            var icons24 = new List<SymbolRegular>();
            foreach (var enum1 in Enum.GetValues(typeof(SymbolRegular)))
            {
                if (enum1.ToString().EndsWith("24"))
                {
                    icons24.Add((SymbolRegular)enum1);
                }
            }
            return icons24;
        }
    }
    public class FilterIconsMaxLimitConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var iconList = values[0] as IEnumerable<SymbolRegular>;
            var filter = values[1] as string;
            if (iconList == null)
            {
                return DependencyProperty.UnsetValue;
            }
            return iconList.Where(it => it.ToString().Contains(filter ?? "", StringComparison.InvariantCultureIgnoreCase)).Take(100);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EmptyStringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is string stringValue)
            {
                bool invert = parameter is "Invert";
                bool shouldShow = !string.IsNullOrEmpty(stringValue);
                if (invert)
                {
                    shouldShow = !shouldShow;
                }
                if (shouldShow)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IconListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for IconSelectionControl.xaml
    /// </summary>
    public partial class IconSelectionControl : UserControl
    {
        public IconSelectionControl()
        {
            InitializeComponent();
        }

        private void ListBox_Selected(object sender, RoutedEventArgs e)
        {
            (this.DataContext as IconViewModel).NewIconSelectedCommand.Execute(sender);
            IconSelectedCommand.Execute(null);
        }

        /// <summary>
        ///     The DependencyProperty for IconSelectedCommand
        /// </summary>
        public static readonly DependencyProperty IconSelectedCommandProperty =
                DependencyProperty.Register(
                        "IconSelectedCommand",
                        typeof(ICommand),
                        typeof(IconSelectionControl),
                        new FrameworkPropertyMetadata((ICommand)null));

        public ICommand IconSelectedCommand
        {
            get
            {
                return (ICommand)GetValue(IconSelectedCommandProperty);
            }
            set
            {
                SetValue(IconSelectedCommandProperty, value);
            }
        }

        private void ListBox_Selected_1(object sender, RoutedEventArgs e)
        {

        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (this.DataContext as IconViewModel).NewIconSelectedCommand.Execute(sender);
        }


        //private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        //{
        //    e.
        //}
    }
}
