using PixelRuler.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PixelRuler.Views
{
    /// <summary>
    /// Interaction logic for PathInfoEdit.xaml
    /// </summary>
    public partial class PathInfoEditView : UserControl
    {
        public PathInfoEditView()
        {
            InitializeComponent();
            this.DataContextChanged += PathInfoEditView_DataContextChanged;
        }

        private void PathInfoEditView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //(this.DataContext as PathInfoEditViewModel).EditIconClicked += PathInfoEditView_EditIconClicked;
            (this.DataContext as PathInfoEditViewModel).IconViewModel.OnNewIconSelected += NewIconSelected;

        }

        private void PathInfoEditView_EditIconClicked(object? sender, EventArgs e)
        {
        }

        private void NewIconSelected(object? sender, EventArgs e)
        {
            var toggleButton = VisualTreeHelper.GetChild(this.iconShowEdit, 0) as ToggleButton;
            toggleButton.IsChecked = false;
        }
    }
}
